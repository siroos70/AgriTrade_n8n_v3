"""GEO-SURVEY v2.0 - shared specifications.

Pure-Python shared specifications used across the application.
"""
from __future__ import annotations

from dataclasses import dataclass, field
from typing import Dict, List


@dataclass(frozen=True)
class Specification:
    """A named specification document."""

    name: str
    version: str
    description: str = ""
    rules: List[str] = field(default_factory=list)

    def __post_init__(self) -> None:
        if not self.name:
            raise ValueError("name must be non-empty")
        if not self.version:
            raise ValueError("version must be non-empty")

    def to_dict(self) -> Dict[str, object]:
        return {
            "name": self.name,
            "version": self.version,
            "description": self.description,
            "rules": list(self.rules),
        }

    @classmethod
    def from_dict(cls, data: Dict[str, object]) -> "Specification":
        return cls(
            name=str(data["name"]),
            version=str(data["version"]),
            description=str(data.get("description", "")),
            rules=[str(r) for r in data.get("rules", [])],
        )


__all__ = ["Specification"]
