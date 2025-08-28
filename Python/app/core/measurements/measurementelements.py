from app.core.measurements.measurementelementbase import MeasurementType

class WaitForTime(MeasurementType):
    def __init__(self, wait_period):
        self.measurement_name = "Wait"
        self.element_type = "wait"
        self.wait_period = wait_period  # default duration

    def serialize(self) -> dict:
        return {"datatype": "MeasurementElement",
            "elementproperties": {"elementtype": self.element_type,
                                 "duration": self.wait_period}
        }
