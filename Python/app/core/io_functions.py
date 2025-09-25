import numpy as np 

from app.core.dag_types import *
from app.decorators.node_decorator import ionode
from app.core.io_provider import ImageProviderOutput, ElementProviderOutput
from imagedata.image_data_handler import image_provider
from scipy.ndimage import gaussian_filter
from app.core.user_classes.merge_channels import channel_merger
from app.core.measurements.measurementelements import *

import skimage as ski
import tifffile





@ionode(input=[StageLoopProperties], output=[], params= 
    {
        "Parameters": []               
    },
    isinputnode = False,
    isoutputnode = True,
    islazynode = True,
    )
def stageloop_decision(dotimes_properties : StageLoopProperties):
    stageloop_decision = StageLoop.from_properties(dotimes_properties)
    return  ElementProviderOutput(stageloop_decision)




@ionode(input=[DoTimesProperties], output=[], params= 
    {
        "Parameters": []               
    },
    isinputnode = False,
    isoutputnode = True,
    islazynode = True,
    )
def dotimes_decision(dotimes_properties : DoTimesProperties):
    dotimes_decision = DoTimes.from_properties(dotimes_properties)
    return  ElementProviderOutput(dotimes_decision)