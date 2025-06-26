import numpy as np

from pydantic import BaseModel
from app.utils.mmf_processor import MMFProcessor

class ArrayType:
    ndim: int = None  
    dtype: str = "float32"


    def __repr__(self):
        return f"{self.__class__.__name__}(ndim={self.ndim}, dtype={self.dtype.__name__})"

    def validate(self, arr: np.ndarray):
        npdtype = np.dtype(self.dtype)

        if arr.ndim != self.ndim:
            raise ValueError(f"Expected {self.ndim}D array, got {arr.ndim}D")
        if arr.dtype != npdtype:
            raise ValueError(f"Expected dtype {self.dtype}, got {arr.dtype}")
        
    def load_data(self, path, input_shape):
        return MMFProcessor.load_array_from_mmf(path,shape = input_shape,input_type = self.dtype )
        



class Image2D(ArrayType):
    ndim: int = 2
    dtype: str = "float32" 
    image_shape: list 
    image_type: str 
    image_dir: str
    
    def __init__(self, image_shape, image_type, image_dir ):
        self.image_shape = image_shape
        self.image_type = image_type 
        self.image_dir =  image_dir

    def load_data(self):
            return MMFProcessor.load_array_from_mmf(self.image_dir,shape = self.image_shape,input_type = self.dtype )  

    @staticmethod  
    def serialize():
        return {"datatype": "Image2D",
                "image_dir": ""}




class Volume3D(ArrayType):
    ndim: int = 3
    dtype: str = "float32"
    image_shape: list 
    image_type: str 
    image_dir: str

    def __init__(self, image_shape, image_type, image_dir ):
        self.image_shape = image_shape
        self.image_type = image_type 
        self.image_dir =  image_dir

    def load_data(self):
        return MMFProcessor.load_array_from_mmf(self.image_dir,shape = self.image_shape,input_type = self.dtype )

class MMFPath:
    dtype: str 
    image_dir:str 

    def __init__(self, image_dir):
        self.image_dir = image_dir 
    
    def load_data(self):
        return self.image_dir

class Scalar:
    dtype: np.float32

    def __init__(self,value):
        self.value = value

    def load_data(self):
        return self.value
    
    def serialize(self):
        return {"datatype": "Scalar",
                "value": self.value}

class Categoric:
    dtype: list

    def __init__(self, options, selectedvalue):
        self.options = options 
        self.selectedvalue = selectedvalue 
    
    def load_data(self):
        return self.selectedvalue
    
    def serialize(self):
        return {"datatype": "Categoric",
                "options": self.options,
                "selectedvalue": self.selectedvalue}

data_types = {
    "Image2D": Image2D,
    "Volume3D": Volume3D,
    "MMFPath": MMFPath,
    "Scalar": Scalar,
}
