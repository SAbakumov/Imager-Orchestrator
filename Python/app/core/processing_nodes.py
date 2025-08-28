from pydantic import BaseModel
from typing import List, Literal, Dict, Any

class InputParams(BaseModel):
    input_json_params: Dict[str, Any]


class InputItem(BaseModel):
    input_type: Literal["Image2D", 
                        "MeasurementElement",
                        "Volume3D", 
                        "MMFPath",
                        "numeric", 
                        "Scalar", 
                        "Categoric",
                        "Text",
                        "Image2DPath",
                        "AcquisitionName",
                        "DetectorName",
                        "JobID"]
    input_params: InputParams


class NodeInput(BaseModel):
    node_id: str
    input: List[InputItem]


class IoInput(BaseModel):
    node_id: str
    job_id: str
    input: List[InputItem]

