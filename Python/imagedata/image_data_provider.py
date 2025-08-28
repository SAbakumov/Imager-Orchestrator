import app
import numpy as np


class ImageDataProvider:
    def __init__(self):
        self.image_data = {}  # {file_name: bytearray()}
        self.imsizes    = {}

        self.max_image_index = 0
        self.min_image_index = 0
        self.image_val = 0
    def add_image_data(self, file_name: str, data: bytes, max_index:int, width:int, height:int):
        if file_name not in self.image_data:
            self.image_data[file_name] = []

            
        self.image_data[file_name].append(data)
        self.imsizes[file_name] = tuple([width,height])


    def concatenate_image_data(self, file_name):
        arrays = [np.frombuffer(chunk, dtype=np.uint16) for chunk in self.image_data[file_name]]
        stack_size = len(arrays)
        width, height = self.imsizes[file_name]
        # Concatenate them efficiently
        self.image_data[file_name] = np.concatenate(arrays)
        self.image_data[file_name] = np.reshape(self.image_data[file_name],shape=(stack_size, width, height))
