from abc import ABC, abstractmethod



class MeasurementType(ABC):
    """Abstract base class for all measurement types."""

    @abstractmethod
    def serialize(self) -> dict:
        """Serialize measurement data into a dictionary."""
        pass