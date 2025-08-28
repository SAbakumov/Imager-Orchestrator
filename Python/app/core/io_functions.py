import numpy as np 

from app.core.dag_types import *
from app.decorators.node_decorator import ionode
from app.core.io_provider import ImageProviderOutput, ElementProviderOutput
from imagedata.image_data_handler import image_provider
from scipy.ndimage import gaussian_filter
import skimage as ski
import tifffile





@ionode(input=[], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": AcquisitionName("","Input Acquisition")
            },
            {
             "type": DetectorName("","Input Detector")
            },
        ]               
    },
    isinputnode = True,
    isoutputnode = False, 
    )
async def live_input(jobid, acq_name, det_name):
    image_provider.concatenate_image_data(f'{jobid}_{acq_name}_{det_name}')
    return  ImageProviderOutput(f'{acq_name}_{det_name}')


@ionode(input=[MeasurementElement], output=[], params= 
    {
        "Parameters": [
        ]               
    },
    isinputnode = False,
    isoutputnode = True,
    )
async def measurement_output(jobid, measurement_element):
    return  ElementProviderOutput(measurement_element)
