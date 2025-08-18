import numpy as np
from pathlib import Path
import tifffile

class TIFFStackLoader:
    """Load a multi-page TIFF stack into a NumPy array."""

    def __init__(self, tiff_path: str):
        self.tiff_path = Path(tiff_path)
        if not self.tiff_path.exists():
            raise FileNotFoundError(f"TIFF file {self.tiff_path} does not exist.")

    def load_stack(self) -> np.ndarray:
        """Load all pages of the TIFF stack into a 3D array."""
        with tifffile.TiffFile(self.tiff_path) as tif:
            pages = [page.asarray() for page in tif.pages]
        return np.stack(pages, axis=0).astype(dtype=np.float32)
