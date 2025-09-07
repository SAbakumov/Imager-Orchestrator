import numpy as np 

from app.core.dag_types import *
from app.decorators.node_decorator import ionode
from app.core.io_provider import ImageProviderOutput, ElementProviderOutput
from imagedata.image_data_handler import image_provider
from scipy.ndimage import gaussian_filter
from app.core.user_classes.merge_channels import channel_merger


import skimage as ski
import tifffile


@ionode(input=[MeasurementElement], output=[], params= 
    {
        "Parameters": [] ,
    },
    islazynode= False,
    isinputnode = False,
    isoutputnode = True                   
    )

def plot_channels(jobid,measurement_element):
    channel_merger.get_html()
    return  ElementProviderOutput(measurement_element)




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
def live_input(jobid, acq_name, det_name):
    image_provider.retrieve_image_data(f'{jobid}_{acq_name}_{det_name}')
    return  ImageProviderOutput(f'{acq_name}_{det_name}')




@ionode(input=[MeasurementElement], output=[], params= 
    {
        "Parameters": [
        ]               
    },
    isinputnode = False,
    isoutputnode = True,
    )
def measurement_output(jobid, measurement_element):
    return  ElementProviderOutput(measurement_element)
