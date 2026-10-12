"""GEO-SURVEY v2.0 - shared Data Model: field_form.py

Pure-Python implementation of the field form data model.

This module is intentionally free of any QGIS, QField, PyQt, or
database dependency.
"""
from __future__ import annotations

import json
import uuid
from typing import Any, Dict, List, Optional

from ..feature_codes import FieldDefinition
from .survey_feature import FieldValue


class FieldForm:
    """A reusable survey form definition.

    Properties:
        form_id
        name
        title
        description
        fields
    """

    __slots__ = ("form_id", "name", "title", "description", "fields", "_hash")

    def __init__(self, form_id: Optional[str], name: str, title: str,
                 description: str = "", fields: Optional[List[FieldDefinition]] = None) -> None:
        if not form_id or not isinstance(form_id, str):
            raise ValueError("form_id must be a non-empty string")
        if not name or not isinstance(name, str):
            raise ValueError("name must be a non-empty string")
        if not title or not isinstance(title, str):
            raise ValueError("title must be a non-empty string")

        object.__setattr__(self, "form_id", form_id)
        object.__setattr__(self, "name", name)
        object.__setattr__(self, "title", title)
        object.__setattr__(self, "description", description or "")
        object.__setattr__(self, "fields", list(fields) if fields else [])
        object.__setattr__(self, "_hash", None)

    def __setattr__(self, key, value):
        raise AttributeError("FieldForm is immutable")

    def add_field(self, field_def: FieldDefinition) -> None:
        if not isinstance(field_def, FieldDefinition):
            raise TypeError("expected FieldDefinition")
        if any(f.field_id == field_def.field_id for f in self.fields):
            raise ValueError(f"duplicate field_id: {field_def.field_id!r}")
        self.fields.append(field_def)

    def remove_field(self, field_id: str) -> bool:
        for i, f in enumerate(self.fields):
            if f.field_id == field_id:
                del self.fields[i]
                return True
        return False

    def get_field(self, field_id: str) -> Optional[FieldDefinition]:
        for f in self.fields:
            if f.field_id == field_id:
                return f
        return None

    def has_field(self, field_id: str) -> bool:
        return any(f.field_id == field_id for f in self.fields)

    def validate_values(self, values: Dict[str, Any]) -> List[str]:
        """Validate a values mapping against the form definition."""
        errors: List[str] = []
        for f in self.fields:
            present = f.field_id in values
            if f.required and not present:
                errors.append(f"required field missing: {f.field_id!r}")
                continue
            if not present:
                continue
            try:
                FieldValue(f.data_type, values[f.field_id])
            except (TypeError, ValueError) as exc:
                errors.append(f"field {f.field_id!r}: {exc}")
        return errors

    def is_valid_values(self, values: Dict[str, Any]) -> bool:
        return not self.validate_values(values)

    def to_dict(self) -> Dict[str, Any]:
        return {
            "form_id": self.form_id,
            "name": self.name,
            "title": self.title,
            "description": self.description,
            "fields": [f.to_dict() for f in self.fields],
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "FieldForm":
        return cls(
            form_id=str(data["form_id"]),
            name=str(data["name"]),
            title=str(data["title"]),
            description=str(data.get("description", "")),
            fields=[FieldDefinition.from_dict(f) for f in data.get("fields", [])],
        )

    def to_json(self) -> str:
        return json.dumps(self.to_dict(), ensure_ascii=False, sort_keys=True)

    @classmethod
    def from_json(cls, text: str) -> "FieldForm":
        return cls.from_dict(json.loads(text))

    @classmethod
    def new(cls, name: str, title: str,
            description: str = "",
            fields: Optional[List[FieldDefinition]] = None) -> "FieldForm":
        return cls(
            form_id=str(uuid.uuid4()),
            name=name,
            title=title,
            description=description,
            fields=fields,
        )

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, FieldForm):
            return NotImplemented
        return self.to_dict() == other.to_dict()

    def __hash__(self) -> int:
        if object.__getattribute__(self, "_hash") is None:
            object.__setattr__(self, "_hash", hash(self.form_id))
        return object.__getattribute__(self, "_hash")

    def __repr__(self) -> str:
        return (f"FieldForm(form_id={self.form_id!r}, name={self.name!r}, "
                f"fields={len(self.fields)})")


class FieldFormManager:
    """Registry of FieldForm instances.

    Duplicate IDs raise a deterministic ValueError on register().
    """

    def __init__(self) -> None:
        self._forms: Dict[str, FieldForm] = {}

    def register(self, form: FieldForm) -> None:
        if not isinstance(form, FieldForm):
            raise TypeError("expected FieldForm")
        if form.form_id in self._forms:
            raise ValueError(f"form already registered: {form.form_id!r}")
        self._forms[form.form_id] = form

    def register_or_replace(self, form: FieldForm) -> None:
        if not isinstance(form, FieldForm):
            raise TypeError("expected FieldForm")
        self._forms[form.form_id] = form

    def get(self, form_id: str) -> Optional[FieldForm]:
        return self._forms.get(form_id)

    def remove(self, form_id: str) -> bool:
        if form_id in self._forms:
            del self._forms[form_id]
            return True
        return False

    def list(self) -> List[FieldForm]:
        return list(self._forms.values())

    def exists(self, form_id: str) -> bool:
        return form_id in self._forms

    def __contains__(self, form_id: object) -> bool:
        return isinstance(form_id, str) and form_id in self._forms

    def __len__(self) -> int:
        return len(self._forms)

    def __iter__(self):
        return iter(self._forms.values())

    def clear(self) -> None:
        self._forms.clear()

    def __repr__(self) -> str:
        return f"FieldFormManager(forms={len(self._forms)})"


__all__ = [
    "FieldForm",
    "FieldFormManager",
]
