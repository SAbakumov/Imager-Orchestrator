from app.core.measurements.measurementelementbase import MeasurementType
from typing import Dict, Any , List
import json

class WaitForTime(MeasurementType):
    def __init__(self, wait_period):
        self.measurement_name = "Wait"
        self.element_type = "wait"
        self.wait_period = wait_period  # default duration

    def serialize(self) -> dict:
        return {"datatype": "MeasurementElement",
            "elementproperties": {"elementtype": self.element_type,
                                 "duration": self.wait_period}
        }
    
class DoTimes:
    def __init__(self, ntotal) -> None:
        self.type: str = "dotimesdecision"
        self.ntotal: int = ntotal

    def serialize(self) -> Dict[str, Any]:
        return {
            "datatype": "MeasurementElement",
            "decision": {
                "type": self.type,
                "ntotal": self.ntotal
            },
            "decisiontype": "dotimesdecision"

        }


class RelativeStageLoop(MeasurementType):
    def __init__(
        self, 
        dx: float, dy: float, dz: float, 
        nNegX: int, nNegY: int, nNegZ: int, 
        nPosX: int, nPosY: int, nPosZ: int, 
        returntostartingposition: bool
    ) -> None:
        self.dx: float = dx
        self.dy: float = dy
        self.dz: float = dz
        
        self.nNegX: int = nNegX
        self.nNegY: int = nNegY
        self.nNegZ: int = nNegZ
        
        self.nPosX: int = nPosX
        self.nPosY: int = nPosY
        self.nPosZ: int = nPosZ
        self.type: str = "relativestageloopdecision"

        self.returntostartingposition: bool = returntostartingposition

    def serialize(self) -> Dict[str, Any]:
        return {
            "datatype": "MeasurementElement",
            "decision": {
                "type": self.type,
                "params": {
                    "deltax": self.dx,
                    "deltay": self.dy,
                    "deltaz": self.dz,
                    "additionalplanesx": [self.nNegX, self.nPosX],
                    "additionalplanesy": [self.nNegY, self.nPosY],
                    "additionalplanesz": [self.nNegZ, self.nPosZ],
                    "returntostartingposition": self.returntostartingposition
                }
            },
            "decisiontype": "relativestageloopdecision"

        }


class XYStagePosition:
    def __init__(
        self, 
        name: str, 
        x: float, 
        y: float, 
        z: float, 
        usinghardwareautofocus: bool = False, 
        hardwareautofocusoffset: float = 0.0
    ) -> None:
        self.name: str = name
        self.x: float = x
        self.y: float = y
        self.z: float = z
        self.usinghardwareautofocus: bool = usinghardwareautofocus
        self.hardwareautofocusoffset: float = hardwareautofocusoffset

    @classmethod
    def from_cache(cls, data: Dict[str, Any]) -> "XYStagePosition":
     
        name = data.get("name", "")
        coordinates = data.get("coordinates", {})
        x = coordinates.get("x", 0.0)
        y = coordinates.get("y", 0.0)
        z = coordinates.get("z", 0.0)
        usinghardwareautofocus = coordinates.get("usinghardwareautofocus", False)
        hardwareautofocusoffset = coordinates.get("hardwareautofocusoffset", 0.0)
        return cls(name, x, y, z, usinghardwareautofocus, hardwareautofocusoffset)


    def serialize(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "coordinates": {
                "x": self.x,
                "y": self.y,
                "z": self.z,
                "usinghardwareautofocus": self.usinghardwareautofocus,
                "hardwareautofocusoffset": self.hardwareautofocusoffset
            }
        }


class StageLoop:
    def __init__(self) -> None:
        self.positions: List[XYStagePosition] = list()
        self.type: str = "stageloopdecision"

    def append_stage_position(self, position: XYStagePosition):
        self.positions.append(position)

    def remove_stage_position(self, position: XYStagePosition):
        self.positions = [pos for pos in self.positions if pos.name != position.name]

    def serialize(self) -> Dict[str, Any]:
        return {
            "datatype": "MeasurementElement",
            "decision": {
                "type": self.type,
                "positions": [pos.serialize() for pos in self.positions]
            },
            "decisiontype" : "stageloopdecision"
        }


