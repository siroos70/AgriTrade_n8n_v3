"""GEO-SURVEY v2.0 - shared Data Model: survey_feature.py

Pure-Python implementation of the survey feature data model.

Coordinate convention:
    X = Easting
    Y = Northing

This module is intentionally free of any QGIS, QField, PyQt, or
database dependency.
"""
from __future__ import annotations

import json
import uuid
from dataclasses import dataclass, field
from datetime import date, datetime
from enum import Enum
from typing import Any, Dict, List, Optional, Tuple

from ..coordinate_rules import Point, polygon_area
from ..feature_codes import FeatureCode, GeometryType


class MediaAttachmentType(str, Enum):
    """Supported media attachment types."""

    PHOTO = "PHOTO"
    VIDEO = "VIDEO"
    AUDIO = "AUDIO"
    DOCUMENT = "DOCUMENT"

    def __str__(self) -> str:
        return self.value


class FeatureGeometry:
    """Geometry of a survey feature.

    Coordinate convention:
        X = Easting
        Y = Northing

    Supported geometry types: POINT, LINE, POLYGON.
    """

    __slots__ = ("geometry_type", "coordinates", "_hash")

    def __init__(self, geometry_type, coordinates) -> None:
        if isinstance(geometry_type, GeometryType):
            geom_type = geometry_type
        elif isinstance(geometry_type, str):
            geom_type = GeometryType(geometry_type)
        else:
            raise TypeError(
                "geometry_type must be GeometryType or str, got "
                f"{type(geometry_type).__name__}"
            )

        coords = self._normalize_coordinates(geom_type, coordinates)
        self._validate(geom_type, coords)

        object.__setattr__(self, "geometry_type", geom_type)
        object.__setattr__(self, "coordinates", coords)
        object.__setattr__(self, "_hash", None)

    @staticmethod
    def _normalize_coordinates(geom_type, coordinates) -> List[Point]:
        if coordinates is None:
            raise ValueError("coordinates must not be None")
        if isinstance(coordinates, Point):
            return [coordinates]
        if isinstance(coordinates, (list, tuple)):
            pts = []
            for item in coordinates:
                if isinstance(item, Point):
                    pts.append(item)
                elif isinstance(item, (list, tuple)) and len(item) == 2:
                    pts.append(Point(float(item[0]), float(item[1])))
                else:
                    raise TypeError(
                        "coordinate items must be Point or (x, y) pairs"
                    )
            return pts
        raise TypeError(
            "coordinates must be Point, list of Point, or list of (x, y) pairs"
        )

    @staticmethod
    def _validate(geom_type, coords: List[Point]) -> None:
        if not coords:
            raise ValueError("coordinates must contain at least one point")
        if geom_type == GeometryType.POINT and len(coords) != 1:
            raise ValueError("POINT geometry requires exactly one coordinate")
        if geom_type == GeometryType.LINE and len(coords) < 2:
            raise ValueError("LINE geometry requires at least two coordinates")
        if geom_type == GeometryType.POLYGON and len(coords) < 3:
            raise ValueError("POLYGON geometry requires at least three coordinates")

    def __setattr__(self, key, value):
        raise AttributeError("FeatureGeometry is immutable")

    @property
    def is_empty(self) -> bool:
        return len(self.coordinates) == 0

    @property
    def point(self) -> Optional[Point]:
        if self.geometry_type == GeometryType.POINT:
            return self.coordinates[0]
        return None

    @property
    def area_m2(self) -> float:
        if self.geometry_type == GeometryType.POLYGON:
            return polygon_area(self.coordinates)
        return 0.0

    def to_dict(self) -> Dict[str, Any]:
        return {
            "geometry_type": str(self.geometry_type),
            "coordinates": [[p.x, p.y] for p in self.coordinates],
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "FeatureGeometry":
        return cls(
            geometry_type=str(data["geometry_type"]),
            coordinates=[(p[0], p[1]) for p in data["coordinates"]],
        )

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, FeatureGeometry):
            return NotImplemented
        return (
            self.geometry_type == other.geometry_type
            and self.coordinates == other.coordinates
        )

    def __hash__(self) -> int:
        if object.__getattribute__(self, "_hash") is None:
            h = hash(self.geometry_type)
            for p in self.coordinates:
                h = hash((h, p))
            object.__setattr__(self, "_hash", h)
        return object.__getattribute__(self, "_hash")

    def __repr__(self) -> str:
        return (f"FeatureGeometry(geometry_type={self.geometry_type!r}, "
                f"coordinates={self.coordinates!r})")

class FieldValue:
    """A typed field value.

    Supported types: string, integer, float, boolean, date, datetime, null.
    """

    __slots__ = ("data_type", "value", "_hash")

    def __init__(self, data_type: str, value: Any = None) -> None:
        if data_type not in {"string", "integer", "float", "boolean",
                              "date", "datetime", "null"}:
            raise ValueError(f"unsupported data_type: {data_type!r}")
        coerced = self._coerce(data_type, value)
        object.__setattr__(self, "data_type", data_type)
        object.__setattr__(self, "value", coerced)
        object.__setattr__(self, "_hash", None)

    @staticmethod
    def _coerce(data_type: str, value: Any) -> Any:
        if data_type == "null":
            return None
        if value is None:
            raise ValueError(f"value must not be None for data_type {data_type!r}")
        if data_type == "string":
            if not isinstance(value, str):
                raise TypeError("string value must be a str")
            return value
        if data_type == "integer":
            if isinstance(value, bool) or not isinstance(value, int):
                raise TypeError("integer value must be an int")
            return value
        if data_type == "float":
            if isinstance(value, bool):
                raise TypeError("float value must not be a bool")
            return float(value)
        if data_type == "boolean":
            if not isinstance(value, bool):
                raise TypeError("boolean value must be a bool")
            return value
        if data_type == "date":
            if isinstance(value, datetime):
                return value.date()
            if isinstance(value, date):
                return value
            if isinstance(value, str):
                return date.fromisoformat(value)
            raise TypeError("date value must be date, datetime, or ISO str")
        if data_type == "datetime":
            if isinstance(value, datetime):
                return value
            if isinstance(value, date):
                return datetime(value.year, value.month, value.day)
            if isinstance(value, str):
                return datetime.fromisoformat(value)
            raise TypeError("datetime value must be datetime, date, or ISO str")
        raise ValueError(f"unsupported data_type: {data_type!r}")

    def __setattr__(self, key, value):
        raise AttributeError("FieldValue is immutable")

    @property
    def is_null(self) -> bool:
        return self.data_type == "null"

    def to_dict(self) -> Dict[str, Any]:
        if self.data_type == "date" and isinstance(self.value, date):
            return {"data_type": self.data_type, "value": self.value.isoformat()}
        if self.data_type == "datetime" and isinstance(self.value, datetime):
            return {"data_type": self.data_type, "value": self.value.isoformat()}
        return {"data_type": self.data_type, "value": self.value}

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "FieldValue":
        return cls(data_type=str(data["data_type"]), value=data.get("value"))

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, FieldValue):
            return NotImplemented
        return self.data_type == other.data_type and self.value == other.value

    def __hash__(self) -> int:
        if object.__getattribute__(self, "_hash") is None:
            h = hash(self.data_type)
            if isinstance(self.value, (date, datetime)):
                h = hash((h, self.value.isoformat()))
            else:
                h = hash((h, self.value))
            object.__setattr__(self, "_hash", h)
        return object.__getattribute__(self, "_hash")

    def __repr__(self) -> str:
        return f"FieldValue(data_type={self.data_type!r}, value={self.value!r})"


@dataclass(frozen=True)
class MediaAttachment:
    """Metadata-only media attachment."""

    media_id: str
    media_type: MediaAttachmentType
    file_name: str
    file_path: str
    created_at: datetime
    description: str = ""

    def __post_init__(self) -> None:
        if not self.media_id or not isinstance(self.media_id, str):
            raise ValueError("media_id must be a non-empty string")
        if not isinstance(self.media_type, MediaAttachmentType):
            object.__setattr__(self, "media_type", MediaAttachmentType(self.media_type))
        if not self.file_name or not isinstance(self.file_name, str):
            raise ValueError("file_name must be a non-empty string")
        if not self.file_path or not isinstance(self.file_path, str):
            raise ValueError("file_path must be a non-empty string")
        if not isinstance(self.created_at, datetime):
            raise TypeError("created_at must be a datetime")
        if self.description is None:
            object.__setattr__(self, "description", "")

    def to_dict(self) -> Dict[str, Any]:
        return {
            "media_id": self.media_id,
            "media_type": str(self.media_type),
            "file_name": self.file_name,
            "file_path": self.file_path,
            "created_at": self.created_at.isoformat(),
            "description": self.description,
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "MediaAttachment":
        return cls(
            media_id=str(data["media_id"]),
            media_type=MediaAttachmentType(str(data["media_type"])),
            file_name=str(data["file_name"]),
            file_path=str(data["file_path"]),
            created_at=datetime.fromisoformat(str(data["created_at"])),
            description=str(data.get("description", "")),
        )

    def __repr__(self) -> str:
        return (f"MediaAttachment(media_id={self.media_id!r}, "
                f"media_type={self.media_type})")

class SurveyFeature:
    """A single survey feature.

    Properties:
        feature_id
        feature_code
        geometry
        attributes
        media
        metadata
        created_at
        updated_at
    """

    __slots__ = (
        "feature_id", "feature_code", "geometry", "attributes",
        "media", "metadata", "created_at", "updated_at", "_hash",
    )

    def __init__(self, feature_id: Optional[str], feature_code: str,
                 geometry: FeatureGeometry, attributes: Optional[Dict[str, Any]] = None,
                 media: Optional[List[MediaAttachment]] = None,
                 metadata: Optional[Dict[str, Any]] = None,
                 created_at: Optional[datetime] = None,
                 updated_at: Optional[datetime] = None) -> None:
        if not feature_id or not isinstance(feature_id, str):
            raise ValueError("feature_id must be a non-empty string")
        if not feature_code or not isinstance(feature_code, str):
            raise ValueError("feature_code must be a non-empty string")
        if not isinstance(geometry, FeatureGeometry):
            raise TypeError("geometry must be a FeatureGeometry")

        now = datetime.now()
        object.__setattr__(self, "feature_id", feature_id)
        object.__setattr__(self, "feature_code", feature_code)
        object.__setattr__(self, "geometry", geometry)
        object.__setattr__(self, "attributes", dict(attributes) if attributes else {})
        object.__setattr__(self, "media", list(media) if media else [])
        object.__setattr__(self, "metadata", dict(metadata) if metadata else {})
        object.__setattr__(self, "created_at", created_at if created_at is not None else now)
        object.__setattr__(self, "updated_at", updated_at if updated_at is not None else now)
        object.__setattr__(self, "_hash", None)

    def __setattr__(self, key, value):
        if key in {"updated_at"}:
            object.__setattr__(self, key, value)
            return
        raise AttributeError(
            "SurveyFeature is immutable; use update_* methods or create a new feature"
        )

    def touch(self) -> None:
        object.__setattr__(self, "updated_at", datetime.now())

    def add_media(self, attachment: MediaAttachment) -> None:
        if not isinstance(attachment, MediaAttachment):
            raise TypeError("expected MediaAttachment")
        self.media.append(attachment)
        self.touch()

    def remove_media(self, media_id: str) -> bool:
        for i, item in enumerate(self.media):
            if item.media_id == media_id:
                del self.media[i]
                self.touch()
                return True
        return False

    def set_attribute(self, key: str, value: Any) -> None:
        if not isinstance(key, str) or not key:
            raise ValueError("attribute key must be a non-empty string")
        self.attributes[key] = value
        self.touch()

    def remove_attribute(self, key: str) -> bool:
        if key in self.attributes:
            del self.attributes[key]
            self.touch()
            return True
        return False

    def validate(self, feature_code: Optional[FeatureCode] = None) -> List[str]:
        """Return a list of validation errors (empty means valid)."""
        errors: List[str] = []
        if not self.feature_id:
            errors.append("feature_id is required")
        if not self.feature_code:
            errors.append("feature_code is required")
        if not isinstance(self.geometry, FeatureGeometry):
            errors.append("geometry must be a FeatureGeometry")
        else:
            if self.geometry.is_empty:
                errors.append("geometry must not be empty")

        if feature_code is not None:
            if self.feature_code != feature_code.code:
                errors.append(
                    f"feature_code mismatch: {self.feature_code!r} != {feature_code.code!r}"
                )
            if self.geometry.geometry_type != feature_code.geometry_type:
                errors.append(
                    "geometry_type mismatch: "
                    f"{self.geometry.geometry_type} != {feature_code.geometry_type}"
                )

        if self.created_at is not None and self.updated_at is not None:
            if self.updated_at < self.created_at:
                errors.append("updated_at must not be earlier than created_at")

        for item in self.media:
            if not isinstance(item, MediaAttachment):
                errors.append("media items must be MediaAttachment instances")
                break

        return errors

    def is_valid(self, feature_code: Optional[FeatureCode] = None) -> bool:
        return not self.validate(feature_code=feature_code)

    def to_dict(self) -> Dict[str, Any]:
        return {
            "feature_id": self.feature_id,
            "feature_code": self.feature_code,
            "geometry": self.geometry.to_dict(),
            "attributes": dict(self.attributes),
            "media": [m.to_dict() for m in self.media],
            "metadata": dict(self.metadata),
            "created_at": self.created_at.isoformat(),
            "updated_at": self.updated_at.isoformat(),
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "SurveyFeature":
        return cls(
            feature_id=str(data["feature_id"]),
            feature_code=str(data["feature_code"]),
            geometry=FeatureGeometry.from_dict(data["geometry"]),
            attributes=dict(data.get("attributes", {})),
            media=[MediaAttachment.from_dict(m) for m in data.get("media", [])],
            metadata=dict(data.get("metadata", {})),
            created_at=datetime.fromisoformat(str(data["created_at"])),
            updated_at=datetime.fromisoformat(str(data["updated_at"])),
        )

    def to_json(self) -> str:
        return json.dumps(self.to_dict(), ensure_ascii=False, sort_keys=True)

    @classmethod
    def from_json(cls, text: str) -> "SurveyFeature":
        return cls.from_dict(json.loads(text))

    @classmethod
    def new(cls, feature_code: str, geometry: FeatureGeometry,
            feature_id: Optional[str] = None,
            attributes: Optional[Dict[str, Any]] = None,
            metadata: Optional[Dict[str, Any]] = None) -> "SurveyFeature":
        return cls(
            feature_id=feature_id or str(uuid.uuid4()),
            feature_code=feature_code,
            geometry=geometry,
            attributes=attributes,
            metadata=metadata,
        )

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, SurveyFeature):
            return NotImplemented
        return self.to_dict() == other.to_dict()

    def __hash__(self) -> int:
        if object.__getattribute__(self, "_hash") is None:
            object.__setattr__(self, "_hash", hash(self.feature_id))
        return object.__getattribute__(self, "_hash")

    def __repr__(self) -> str:
        return (f"SurveyFeature(feature_id={self.feature_id!r}, "
                f"feature_code={self.feature_code!r}, "
                f"geometry_type={self.geometry.geometry_type})")


__all__ = [
    "MediaAttachmentType",
    "FeatureGeometry",
    "FieldValue",
    "MediaAttachment",
    "SurveyFeature",
]
