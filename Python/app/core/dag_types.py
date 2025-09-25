import numpy as np
import os , io , json
from abc import ABC, abstractmethod
from typing import Any, List, Dict
from pathlib import Path
from pydantic import BaseModel
from app.core.imagecache.image_cache import LocalImageCache


from app.core.measurements.measurementelementsutils import XYStagePosition
from app.utils.mmf_processor import MMFProcessor
from app.core.imagecache.image_cache import ImageCache

# -------------------------------
# Base IO types
# -------------------------------
class IOType(BaseModel, ABC):
    """Abstract base class for all I/O data types."""

    @abstractmethod
    def load_data(self, *args, **kwargs) -> Any:
        ...

    @staticmethod
    def serialize(*args, **kwargs) -> dict:
        ...

    @abstractmethod
    def set_result(self, value: Any, node_id: str, *args, **kwargs) -> None:
        ...


class ArrayType(IOType):
    """Base class for array-like data types (2D or 3D)."""

    ndim: int | None = None
    dtype: str = "float32"

    def __repr__(self) -> str:
        return f"{self.__class__.__name__}(ndim={self.ndim}, dtype={self.dtype})"

    def validate(self, arr: np.ndarray) -> None:
        npdtype = np.dtype(self.dtype)
        if arr.ndim != self.ndim:
            raise ValueError(f"Expected {self.ndim}D array, got {arr.ndim}D")
        if arr.dtype != npdtype:
            raise ValueError(f"Expected dtype {self.dtype}, got {arr.dtype}")

    def load_data(self, path: str, input_shape: tuple[int, ...]) -> np.ndarray:
        return MMFProcessor.load_array_from_mmf(path, shape=input_shape, input_type=self.dtype)



class Image2DPath():
    image_dir: str
    image_shape: list | None = None

    def __init__(self, image_dir: str):
        self.image_dir = Path(image_dir)

    def load_data(self) -> Path:
        return self.image_dir

    @staticmethod
    def serialize() -> dict:
        return {"datatype": "Image2DPath", "image_dir": ""}

# -------------------------------
# MeasurementElementProperties type
# -------------------------------
class MeasurementElementProperties(IOType):
    image_dir: str
    element_parameters: Dict = {}

 
    @classmethod
    def from_dict(cls, element_parameters :dict) -> "MeasurementElementProperties":
        return cls(image_dir="", element_parameters=element_parameters)

    def set_measurement_element(self,measurement_element: dict):
        self.element_parameters = measurement_element
    
    def load_data(self) -> "MeasurementElementProperties":
        data = LocalImageCache.cache[self.image_dir]
        return data


    @staticmethod
    def serialize() -> dict:
        return {"datatype": "MeasurementElementProperties",
                "image_dir": ""}

    @staticmethod
    def set_result(measurement_element : "MeasurementElementProperties", name: str) -> dict:    
        out_path_data =  f"{name}.npy"
        LocalImageCache.cache[out_path_data] = measurement_element

        return {"datatype": "MeasurementElementProperties",
                "image_dir":  out_path_data}







   
# -------------------------------
# 2D Image type
# -------------------------------
class Image2D(ArrayType):
    ndim: int = 2
    dtype: str = "float32"
    image_dir: str
    image_shape: list | None = None
    image_cache: Any = None  # make it optional
    imdata : np.ndarray = None
    xy_pos:  Any = None

    model_config = {
        "arbitrary_types_allowed": True
    }

    def __init__(self, image_dir: str):
        super().__init__(image_dir=image_dir)
        prefix = image_dir.split("::")[0]

        self.image_dir = image_dir
        self.image_cache = LocalImageCache
        self.imdata = np.zeros(0)
        self.xy_pos = list()

    @classmethod
    def from_array(cls, imdata:np.ndarray) -> "Image2D":
        image_data = Image2D("")
        image_data.imdata = imdata 
        return image_data


    def load_data(self) -> "Image2D":

        prefix = self.image_dir.split("::")[0]
        if prefix == "fromprovider":
            array_im_data = LocalImageCache.image_data[self.image_dir.split("::")[1]]
            image2Ddata = Image2D.from_array(array_im_data)
            image2Ddata.set_xy_positions(LocalImageCache.position_data[self.image_dir.split("::")[1]])
            return image2Ddata
            

        elif self.image_dir.split('.')[-1] == "npy":
            image_key = self.image_dir.rsplit('.', 1)[0]
            image = LocalImageCache.cache[self.image_dir]

            data_position = LocalImageCache.cache[f'{image_key}_position']

            image2Ddata = Image2D.from_array(image)
            if len(data_position)!=0:
            
                image2Ddata.set_xy_positions(data_position)

            return image2Ddata
        else:
            raise ValueError(f"Unsupported file type: {self.image_dir.suffix}")
        

    def set_xy_positions(self, xy_pos : XYStagePosition):
        self.xy_pos = xy_pos

    @staticmethod
    def serialize() -> dict:
        return {"datatype": "Image2D", "image_dir": ""}

    @staticmethod
    def set_result(image : "Image2D", name: str) -> None:
        out_path_data =  f"{name}.npy"
        out_path_position = f"{name}_position"
        

        LocalImageCache.cache[out_path_data] =  image.imdata
        LocalImageCache.cache[out_path_position] = image.xy_pos
        
        return {"datatype": "Image2D", "image_dir": out_path_data  }







class AcquisitionName:
    dtype: str
    def __init__(self, value: str, name: str):
        self.value = value
        self.name = name

    def load_data(self) -> str:
        return self.value
    
    def serialize(self) -> dict:
        return {"datatype": "AcquisitionName", "ImagePath": self.value, "name": self.name}
    
class DetectorName:
    dtype: str
    def __init__(self, value: str, name: str):
        self.value = value
        self.name = name

    def load_data(self) -> str:
        return self.value
    
    def serialize(self) -> dict:
        return {"datatype": "DetectorName", "ImagePath": self.value, "name": self.name}


class Scalar:
    dtype: np.float32

    def __init__(self, value: float, name: str):
        self.value = value
        self.name = name

    def load_data(self) -> float:
        return self.value

    def serialize(self) -> dict:
        return {"datatype": "Scalar", "value": self.value, "name": self.name}


class Text:
    dtype: str

    def __init__(self, value: float, name: str):
        self.value = value
        self.name = name

    def load_data(self) -> float:
        return self.value

    def serialize(self) -> dict:
        return {"datatype": "Text", "value": self.value, "name": self.name}



class Categoric:
    dtype: list

    def __init__(self, options: list, selectedvalue: Any, name: str):
        self.options = options
        self.selectedvalue = selectedvalue
        self.name = name

    def load_data(self) -> Any:
        return self.selectedvalue

    def serialize(self) -> dict:
        return {
            "datatype": "Categoric",
            "options": self.options,
            "selectedvalue": self.selectedvalue,
            "name": self.name
        }


# -------------------------------
# Data types mapping
# -------------------------------
data_types  = {
    "Image2D": Image2D,
    "MeasurementElementProperties": MeasurementElementProperties,
    "Scalar": Scalar,
    "Categoric": Categoric,
    "Text": Text,
    "Image2DPath": Image2DPath,
    "AcquisitionName": AcquisitionName,
    "DetectorName": DetectorName}

