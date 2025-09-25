from dataclasses import dataclass
from typing import Optional
import msgpack
import msgpack_numpy as m  # Optional, if you want numpy support for image arrays

m.patch()  # Enable numpy array support if needed


@dataclass
class MessagePackStagePosition:
    hardwareautofocusoffset: int
    usinghardwareautofocus: bool
    x: float
    y: float
    z: float


@dataclass
class ImageMetadata:
    acquisitiontype: str
    detectionindex: int
    nimageswithdetectionindex: int
    stageposition: MessagePackStagePosition


@dataclass
class MessagePackImageData:
    detectorname: str
    imagedata: bytes  # raw image bytes
    ncols: int
    nrows: int
    numtype: int
    timestamp: float


@dataclass
class MessagePackMessage:
    data: MessagePackImageData
    metadata: ImageMetadata
    type: str


@dataclass
class MessagePackData:
    index: int
    message: MessagePackMessage


# Optional placeholder class like your C# ImageData
class ImageData:
    pass
