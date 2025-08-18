import numpy as np
import os
from abc import ABC, abstractmethod
from typing import Any
from pathlib import Path
from pydantic import BaseModel
from app.utils.mmf_processor import MMFProcessor
from app.utils.array_utils import NPYArrayIO
from app.utils.tif_utils import TIFFStackLoader

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
# 2D Image type
# -------------------------------
class Image2D(ArrayType):
    ndim: int = 2
    dtype: str = "float32"
    image_dir: str
    image_shape: list | None = None

    def __init__(self, image_dir: str):
        super().__init__(image_dir=image_dir)
        self.image_dir = Path(self.image_dir)

    def load_data(self) -> np.ndarray:
        if not self.image_dir.exists():
            raise FileNotFoundError(f"{self.image_dir} does not exist.")
        if self.image_dir.suffix.lower() in [".tif", ".tiff"]:
            loader = TIFFStackLoader(self.image_dir)
            stack = loader.load_stack()
            if stack.ndim == 3 and stack.shape[0] == 1:
                return stack[0]  # single-page TIFF
            elif stack.ndim == 3:
                raise ValueError("Expected single 2D image, got multi-page TIFF")
            return stack
        elif self.image_dir.suffix.lower() == ".npy":
            return np.load(self.image_dir)
        else:
            raise ValueError(f"Unsupported file type: {self.image_dir.suffix}")

    @staticmethod
    def serialize() -> dict:
        return {"datatype": "Image2D", "image_dir": ""}

    @staticmethod
    def set_result(image: np.ndarray, name: str) -> None:
        out_path = os.path.join(os.path.abspath("temp_data"),f"{name}.npy")
        npy_io = NPYArrayIO(out_path)
        npy_io.save(image)
        return {"datatype": "Image2D", "image_dir": out_path }



# -------------------------------
# 3D Volume type
# -------------------------------
class Volume3D(ArrayType):
    ndim: int = 3
    dtype: str = "float32"
    image_shape: list
    image_type: str
    image_dir: str

    def __init__(self, image_shape: list, image_type: str, image_dir: str):
        self.image_shape = image_shape
        self.image_type = image_type
        self.image_dir = image_dir

    def load_data(self) -> np.ndarray:
        return MMFProcessor.load_array_from_mmf(self.image_dir)


# -------------------------------
# Other types
# -------------------------------
class MMFPath:
    dtype: str
    image_dir: str

    def __init__(self, image_dir: str):
        self.image_dir = image_dir

    def load_data(self) -> str:
        return self.image_dir


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
data_types = {
    "Image2D": Image2D,
    "Volume3D": Volume3D,
    "MMFPath": MMFPath,
    "Scalar": Scalar,
    "Categoric": Categoric,
    "Text": Text,
    "Image2DPath": Image2DPath}