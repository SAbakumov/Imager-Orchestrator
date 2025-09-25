from typing import Dict, Any , List


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
        if usinghardwareautofocus==1:
            self.usinghardwareautofocus = True
        if usinghardwareautofocus==0:
            self.usinghardwareautofocus = False
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
