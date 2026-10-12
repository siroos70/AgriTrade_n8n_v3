"""GEO-SURVEY v2.0 - shared coordinate rules.

Pure-Python coordinate helpers.

Coordinate convention:
    X = Easting
    Y = Northing

This module is intentionally free of any QGIS, QField, PyQt, or
database dependency so that it can be imported by every layer of the
application.
"""
from __future__ import annotations

import math
from dataclasses import dataclass
from typing import Iterable, Tuple

TWO_PI = 2.0 * math.pi
HALF_PI = math.pi / 2.0


@dataclass(frozen=True)
class Point:
    """Immutable 2D coordinate.

    X = Easting
    Y = Northing
    """

    x: float
    y: float

    def __post_init__(self) -> None:
        object.__setattr__(self, "x", float(self.x))
        object.__setattr__(self, "y", float(self.y))

    def __iter__(self):
        return iter((self.x, self.y))

    def to_tuple(self) -> Tuple[float, float]:
        return (self.x, self.y)

    def distance_to(self, other: "Point") -> float:
        return math.hypot(self.x - other.x, self.y - other.y)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Point):
            return NotImplemented
        return math.isclose(self.x, other.x) and math.isclose(self.y, other.y)

    def __hash__(self) -> int:
        return hash((round(self.x, 9), round(self.y, 9)))

    def __repr__(self) -> str:
        return f"Point(x={self.x!r}, y={self.y!r})"


def bearing(p1: Point, p2: Point) -> float:
    """Return the forward bearing (degrees, 0..360) from p1 to p2."""
    dx = p2.x - p1.x
    dy = p2.y - p1.y
    angle = math.degrees(math.atan2(dx, dy))
    return (angle + 360.0) % 360.0


def distance(p1: Point, p2: Point) -> float:
    """Euclidean distance between two points."""
    return math.hypot(p2.x - p1.x, p2.y - p1.y)


def polar(origin: Point, bearing_deg: float, distance_m: float) -> Point:
    """Return the point reached by travelling `distance_m` from `origin`
    along `bearing_deg` (degrees, 0..360)."""
    theta = math.radians(bearing_deg)
    return Point(
        origin.x + distance_m * math.sin(theta),
        origin.y + distance_m * math.cos(theta),
    )


def inverse_bearing(p1: Point, p2: Point) -> float:
    """Return the back bearing (degrees, 0..360) from p2 to p1."""
    return (bearing(p2, p1) + 180.0) % 360.0


def dms_to_decimal(degrees: int, minutes: int, seconds: float,
                   direction: str = "N") -> float:
    """Convert degrees/minutes/seconds to a signed decimal value."""
    if minutes < 0 or seconds < 0:
        raise ValueError("minutes and seconds must be non-negative")
    value = float(degrees) + minutes / 60.0 + seconds / 3600.0
    direction = direction.strip().upper()
    if direction in ("S", "W"):
        value = -value
    return value


def decimal_to_dms(value: float) -> Tuple[int, int, float, str]:
    """Convert a signed decimal value to (degrees, minutes, seconds, direction)."""
    direction = "N" if value >= 0 else "S"
    abs_value = abs(value)
    degrees = int(abs_value)
    minutes_full = (abs_value - degrees) * 60.0
    minutes = int(minutes_full)
    seconds = (minutes_full - minutes) * 60.0
    return degrees, minutes, seconds, direction


def format_bearing(bearing_deg: float) -> str:
    """Format a bearing as a 3-digit zero-padded string."""
    return f"{bearing_deg % 360.0:03.1f}"


def format_distance(distance_m: float) -> str:
    """Format a distance with a sensible unit suffix."""
    if distance_m >= 1000.0:
        return f"{distance_m / 1000.0:.3f} km"
    return f"{distance_m:.2f} m"


def format_area(area_m2: float) -> str:
    """Format an area with a sensible unit suffix."""
    if area_m2 >= 1_000_000.0:
        return f"{area_m2 / 1_000_000.0:.3f} km2"
    if area_m2 >= 10000.0:
        return f"{area_m2 / 10000.0:.3f} ha"
    return f"{area_m2:.2f} m2"


def polygon_area(points: Iterable[Point]) -> float:
    """Shoelace area of a closed polygon (m2)."""
    coords = [(p.x, p.y) for p in points]
    if len(coords) < 3:
        return 0.0
    if coords[0] != coords[-1]:
        coords.append(coords[0])
    s = 0.0
    for i in range(len(coords) - 1):
        x1, y1 = coords[i]
        x2, y2 = coords[i + 1]
        s += x1 * y2 - x2 * y1
    return abs(s) / 2.0


__all__ = [
    "Point",
    "bearing",
    "distance",
    "polar",
    "inverse_bearing",
    "dms_to_decimal",
    "decimal_to_dms",
    "format_bearing",
    "format_distance",
    "format_area",
    "polygon_area",
]
