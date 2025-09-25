
from typing import Dict, Any , List
from app.core.dag_types import MeasurementElementProperties
from app.core.measurements.measurementelementsutils import XYStagePosition
import json


    
class DoTimes():
    def __init__(self, ntotal) -> None:
        self.type: str = "dotimesdecision"
        self.ntotal: int = ntotal

    def serialize(self) -> Dict[str, Any]:
        return {
                "type": self.type,
                "ntotal": self.ntotal
        }
    
    @classmethod
    def from_properties(cls, properties : MeasurementElementProperties) -> "DoTimes":
        return cls(properties.element_parameters["ntotal"])

    @property 
    def properties(self):
        return DoTimesProperties.from_dict({   
                "type": self.type,
                "ntotal": self.ntotal })


class DoTimesProperties(MeasurementElementProperties):

    @staticmethod
    def serialize() -> dict:
        return {
            "datatype": "MeasurementElementProperties",
            "image_dir": "",
            "elementtype": "DoTimes"
        }    







class TimeLapse():
    def __init__(self, ntotal, timedelta):
        self.ntotal = ntotal 
        self.timedelta = timedelta 
        self.type : str = "timelapsedecision"
    
    def serialize(self) -> Dict[str, Any]:
        return {
                "type": self.type,
                "ntotal": self.ntotal, 
                "timedelta": self.timedelta          
        }
    






class RelativeStageLoop():
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
        }
         
        
    
    @classmethod
    def from_properties(cls, properties : MeasurementElementProperties) -> "RelativeStageLoop":
        return cls(properties.element_parameters["params"]["deltax"],
                   properties.element_parameters["params"]["deltay"],
                   properties.element_parameters["params"]["deltaz"],
                   
                                      properties.element_parameters["params"]["additionalplanesx"][0],
                   properties.element_parameters["params"]["additionalplanesx"][1],

                                      properties.element_parameters["params"]["additionalplanesy"][0],
                   properties.element_parameters["params"]["additionalplanesy"][1],
                   
                                      properties.element_parameters["params"]["additionalplanesz"][0],
                   properties.element_parameters["params"]["additionalplanesz"][1],
                   properties.element_parameters["params"]["returntostartingposition"])

    @property 
    def properties(self) -> "RelativeStageLoopProperties":
        return DoTimesProperties.from_dict({   
                "type": self.type,
                "params": {
                    "deltax": self.dx,
                    "deltay": self.dy,
                    "deltaz": self.dz,
                    "additionalplanesx": [self.nNegX, self.nPosX],
                    "additionalplanesy": [self.nNegY, self.nPosY],
                    "additionalplanesz": [self.nNegZ, self.nPosZ],
                    "returntostartingposition": self.returntostartingposition
                } })
    

class RelativeStageLoopProperties(MeasurementElementProperties):

    @staticmethod
    def serialize() -> dict:
        return {
            "datatype": "MeasurementElementProperties",
            "image_dir": "",
            "elementtype": "RelativeStageLoop"
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
                "type": self.type,
                "positions": [pos.serialize() for pos in self.positions]
        }
    
    @classmethod
    def from_properties(cls, properties : MeasurementElementProperties) -> "StageLoop":
        stage_loop = cls()
        for xy_pos in properties.element_parameters['element_parameters']["positions"]:
            stage_loop.append_stage_position(XYStagePosition(xy_pos["name"],
                                                             xy_pos[ "coordinates"]["x"],
                                                             xy_pos[ "coordinates"]["y"],
                                                             xy_pos[ "coordinates"]["z"],
                                                             xy_pos[ "coordinates"]["usinghardwareautofocus"],
                                                             xy_pos[ "coordinates"]["hardwareautofocusoffset"]))
        return stage_loop

    @property 
    def properties(self):
        return StageLoopProperties.from_dict({   
                "type": self.type,
                "positions": [pos.serialize() for pos in self.positions] })


class StageLoopProperties(MeasurementElementProperties):

    @staticmethod
    def serialize() -> dict:
        return {
            "datatype": "MeasurementElementProperties",
            "image_dir": "",
            "elementtype": "StageLoop"
        }    



