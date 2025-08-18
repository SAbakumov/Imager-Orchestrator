import numpy as np 
import tifffile as TiffFile

class MMFProcessor:

    @staticmethod 
    def parse_array_to_mmf(numpy_array: np.ndarray, path: str):
        mmf_map = np.memmap(path, dtype=numpy_array.dtype, mode='w+', shape=numpy_array.shape)
        mmf_map[:] = numpy_array[:]
        mmf_map.flush()

    @staticmethod
    def load_array_from_mmf(path: str):
        images = []
        with TiffFile.TiffFile(path) as tif:
            for page in tif.pages:
                images.append(page.asarray().astype(dtype=np.float32))

        # fp = np.memmap(path, mode='r')
        return images
