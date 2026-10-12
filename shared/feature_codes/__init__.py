"""GEO-SURVEY v2.0 - shared feature codes.

Canonical definitions reused by the Data Model.

This module is intentionally free of any QGIS, QField, PyQt, or
database dependency.
"""
from __future__ import annotations

from dataclasses import dataclass, field
from enum import Enum
from typing import Dict, List, Optional


class GeometryType(str, Enum):
    """Supported geometry types for a survey feature."""

    POINT = "POINT"
    LINE = "LINE"
    POLYGON = "POLYGON"

    def __str__(self) -> str:
        return self.value


class FieldDefinition:
    """Definition of a single field on a form."""

    __slots__ = (
        "field_id", "name", "label", "data_type",
        "required", "default_value", "description",
    )

    def __init__(self, field_id: str, name: str, label: str,
                 data_type: str, required: bool = False,
                 default_value=None, description: str = "") -> None:
        if not field_id or not isinstance(field_id, str):
            raise ValueError("field_id must be a non-empty string")
        if not name or not isinstance(name, str):
            raise ValueError("name must be a non-empty string")
        if not label or not isinstance(label, str):
            raise ValueError("label must be a non-empty string")
        if data_type not in {"string", "integer", "float", "boolean",
                              "date", "datetime", "null"}:
            raise ValueError(f"unsupported data_type: {data_type!r}")
        object.__setattr__(self, "field_id", field_id)
        object.__setattr__(self, "name", name)
        object.__setattr__(self, "label", label)
        object.__setattr__(self, "data_type", data_type)
        object.__setattr__(self, "required", bool(required))
        object.__setattr__(self, "default_value", default_value)
        object.__setattr__(self, "description", description or "")

    def __setattr__(self, key, value):
        raise AttributeError("FieldDefinition is immutable")

    def __repr__(self) -> str:
        return (f"FieldDefinition(field_id={self.field_id!r}, "
                f"name={self.name!r}, data_type={self.data_type!r})")

    def to_dict(self) -> Dict[str, object]:
        return {
            "field_id": self.field_id,
            "name": self.name,
            "label": self.label,
            "data_type": self.data_type,
            "required": self.required,
            "default_value": self.default_value,
            "description": self.description,
        }

    @classmethod
    def from_dict(cls, data: Dict[str, object]) -> "FieldDefinition":
        return cls(
            field_id=str(data["field_id"]),
            name=str(data["name"]),
            label=str(data["label"]),
            data_type=str(data["data_type"]),
            required=bool(data.get("required", False)),
            default_value=data.get("default_value"),
            description=str(data.get("description", "")),
        )


@dataclass(frozen=True)
class FeatureCode:
    """A registered survey feature code."""

    code: str
    name: str
    geometry_type: GeometryType
    description: str = ""
    fields: List[FieldDefinition] = field(default_factory=list)

    def __post_init__(self) -> None:
        if not self.code or not isinstance(self.code, str):
            raise ValueError("code must be a non-empty string")
        if not self.name or not isinstance(self.name, str):
            raise ValueError("name must be a non-empty string")
        if not isinstance(self.geometry_type, GeometryType):
            object.__setattr__(self, "geometry_type", GeometryType(self.geometry_type))

    def __repr__(self) -> str:
        return (f"FeatureCode(code={self.code!r}, name={self.name!r}, "
                f"geometry_type={self.geometry_type})")

    def to_dict(self) -> Dict[str, object]:
        return {
            "code": self.code,
            "name": self.name,
            "geometry_type": str(self.geometry_type),
            "description": self.description,
            "fields": [f.to_dict() for f in self.fields],
        }

    @classmethod
    def from_dict(cls, data: Dict[str, object]) -> "FeatureCode":
        return cls(
            code=str(data["code"]),
            name=str(data["name"]),
            geometry_type=GeometryType(str(data["geometry_type"])),
            description=str(data.get("description", "")),
            fields=[FieldDefinition.from_dict(f) for f in data.get("fields", [])],
        )


class FeatureCodeRegistry:
    """In-memory registry of feature codes.

    Deterministic ordering is guaranteed by insertion order.
    """

    def __init__(self) -> None:
        self._codes: Dict[str, FeatureCode] = {}

    def register(self, code: FeatureCode) -> None:
        if not isinstance(code, FeatureCode):
            raise TypeError("expected FeatureCode")
        if code.code in self._codes:
            raise ValueError(f"feature code already registered: {code.code!r}")
        self._codes[code.code] = code

    def get(self, code: str) -> Optional[FeatureCode]:
        return self._codes.get(code)

    def remove(self, code: str) -> bool:
        if code in self._codes:
            del self._codes[code]
            return True
        return False

    def list(self) -> List[FeatureCode]:
        return list(self._codes.values())

    def exists(self, code: str) -> bool:
        return code in self._codes

    def __contains__(self, code: object) -> bool:
        return isinstance(code, str) and code in self._codes

    def __len__(self) -> int:
        return len(self._codes)

    def __iter__(self):
        return iter(self._codes.values())


class FeatureCodeManager:
    """High-level manager wrapping a FeatureCodeRegistry."""

    def __init__(self, registry: Optional[FeatureCodeRegistry] = None) -> None:
        self._registry = registry if registry is not None else FeatureCodeRegistry()

    @property
    def registry(self) -> FeatureCodeRegistry:
        return self._registry

    def register(self, code: FeatureCode) -> None:
        self._registry.register(code)

    def get(self, code: str) -> Optional[FeatureCode]:
        return self._registry.get(code)

    def remove(self, code: str) -> bool:
        return self._registry.remove(code)

    def list(self) -> List[FeatureCode]:
        return self._registry.list()

    def exists(self, code: str) -> bool:
        return self._registry.exists(code)


__all__ = [
    "GeometryType",
    "FieldDefinition",
    "FeatureCode",
    "FeatureCodeRegistry",
    "FeatureCodeManager",
]
