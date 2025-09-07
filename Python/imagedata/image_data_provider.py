import app
import numpy as np
import redis

from app.core.imagecache.redis_image_cache import ImageCache


class ImageDataProvider:
    def __init__(self):
        self.image_cache = ImageCache.cache
        self.image_data = {}

 
    def add_image_data(self, file_name: str, data: bytes, max_index:int, width:int, height:int):
        if file_name not in self.image_data:
            self.image_data[file_name] = []

            
        self.image_data[file_name].append(data)
        self.imsizes[file_name] = tuple([width,height])


    def retrieve_image_data(self, file_name):
        im_data = np.frombuffer(self.image_cache.get(file_name),dtype=np.uint16)

        metadata =self.image_cache.hgetall(f"{file_name}_size")

        width = int(metadata[b'nrows'])
        height = int(metadata[b'ncols'])


        self.image_data[file_name] = np.reshape(im_data,shape=(width, height))

    def clear_all_image_data(self, process_id):
        for image_key in self.image_data.keys():
            if process_id in image_key:
                self.image_data.pop(image_key)