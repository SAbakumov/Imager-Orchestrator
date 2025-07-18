import numpy as np 


class MMFProcessor:

    @staticmethod 
    def parse_array_to_mmf(numpy_array: np.ndarray, path: str):
        mmf_map = np.memmap(path, dtype=numpy_array.dtype, mode='w+', shape=numpy_array.shape)
        mmf_map[:] = numpy_array[:]
        mmf_map.flush()

    @staticmethod
    def load_array_from_mmf(path: str, shape: list, input_type: str):
        fp = np.memmap(path, dtype=input_type, mode='r', shape=shape)
        return fp
