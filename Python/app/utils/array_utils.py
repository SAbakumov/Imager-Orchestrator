import numpy as np
from pathlib import Path
from typing import Optional


class NPYArrayIO:
    """
    Utility class to save and load NumPy arrays in .npy format.
    Automatically preserves dtype and shape.
    """

    def __init__(self, filepath: str | Path):
        """
        Initialize the NPYArrayIO instance.

        Parameters:
            filepath (str | Path): Path to the .npy file.
        """
        self.filepath = Path(filepath)

    def save(self, array: np.ndarray, overwrite: bool = True) -> None:
        """
        Save a NumPy array to disk.

        Parameters:
            array (np.ndarray): The array to save.
            overwrite (bool): Whether to overwrite if the file already exists.

        Raises:
            FileExistsError: If the file exists and overwrite is False.
        """
        if self.filepath.exists() and not overwrite:
            raise FileExistsError(f"File {self.filepath} already exists.")
        np.save(self.filepath, array)

    def load(self, mmap_mode: Optional[str] = None) -> np.ndarray:
        """
        Load a NumPy array from disk.

        Parameters:
            mmap_mode (Optional[str]): If set, returns a memory-mapped array.
                                       Options: 'r', 'r+', 'w+', 'c'

        Returns:
            np.ndarray: Loaded array.
        """
        if not self.filepath.exists():
            raise FileNotFoundError(f"File {self.filepath} does not exist.")
        return np.load(self.filepath, mmap_mode=mmap_mode)

    def exists(self) -> bool:
        """Return True if the file exists on disk."""
        return self.filepath.exists()

    def delete(self) -> None:
        """Delete the file if it exists."""
        if self.filepath.exists():
            self.filepath.unlink()
