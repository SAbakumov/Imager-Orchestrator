import app
import numpy as np
import redis

from app.core.measurements.measurementelementsutils import XYStagePosition
from app.core.imagecache.image_cache import LocalImageCache
from typing import List, Dict




class ImageDataProvider:
    def __init__(self):
        self.image_cache = LocalImageCache.cache
        self.image_data : Dict[str, np.ndarray] = {}
        self.position_data : Dict[str, np.ndarray] = {}
        self.imsizes = {}

    def retrieve_image_data(self, file_name):
        self.image_data[file_name] = self.image_cache.get_image(file_name)
        self.position_data[file_name] = self.image_cache.get_image(file_name)

        # if img_cache is not None:
        #     im_data = np.frombuffer(img_cache ,dtype=np.uint16)

        #     metadata =self.image_cache.hgetall(f"{file_name}_size")
        #     position =self.image_cache.hgetall(f"{file_name}_position")

        #     width = int(metadata[b'nrows'])
        #     height = int(metadata[b'ncols'])

        #     pos_x = float(position[b'x'])
        #     pos_y = float(position[b'y'])
        #     pos_z = float(position[b'z'])
        #     hardwareafenabled = float(position[b'usinghardwareautofocus'])
        #     offset = float(position[b'offset'])
            
        #     stage_position = XYStagePosition('',pos_x, pos_y,pos_z,hardwareautofocusoffset=offset,usinghardwareautofocus=hardwareafenabled)

        #     self.image_data[file_name] = np.reshape(im_data,shape=(width, height))
        #     self.position_data[file_name] = stage_position



            
            # self.image_data[file_name].set_xy_positions(stage_position) 


    def clear_all_image_data(self, process_id):
        for image_key in self.image_data.keys():
            if process_id in image_key:
                self.image_data.pop(image_key)

