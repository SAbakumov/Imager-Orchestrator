import numpy as np 

from app.core.measurements.measurementelements import WaitForTime
from app.core.dag_types import *
from app.decorators.node_decorator import node 
from scipy.ndimage import gaussian_filter
from app.core.user_classes.merge_channels import channel_merger

import skimage as ski
import tifffile
import asyncio


@node(input=[Image2D,Image2D], output=[MeasurementElement], params= 
    {
        "Parameters": [ ]                     
    })
def combine_channels(ch1, ch2):

    channel_merger.set_channels(ch1,ch2)

    return WaitForTime(10)



@node(input=[Image2D], output=[MeasurementElement], params= 
    {
        "Parameters": []               
    })
def set_wait_time(input_arr):
    return WaitForTime(10)


@node(input=[], output=[], params= 
    {
        "Parameters": [
        ]               
    })
def calc_best_position():

    return 



@node(input=[Image2D], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": Scalar(10,"Multiply by")
            },
        ]               
    })
def multiply(input_arr, input_scalar):
    output_arr = (input_arr*input_scalar)
    return output_arr


@node(input=[Image2D,Image2D], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": Categoric(["Add", "Subtract", "Multiply"], "Add","Operation")
            }  ]    
                       
    })
def image_arithmetic(input_arr1,input_arr2, input_option):
  
    img1 = np.array(input_arr1)
    img2 = np.array(input_arr2)

    if img1.shape != img2.shape:
        raise ValueError("Input images must have the same shape")

    match input_option:
        case "Add":
            result = img1 + img2
        case "Subtract":
            result = img1 - img2
        case "Multiply":
            result = img1 * img2
        case _:
            raise ValueError(f"Unsupported operation: {input_option}")
    return result


@node(input=[Image2D], output=[Image2D], params= 
    {
        "Parameters": [ ]                     
    })
def calculate_mean(data):
    # mean_all = np.mean(data)
    mean_all = 10
    print(10)
    return mean_all






@node(input=[Image2D], output=[], params= 
    {
        "Parameters": [ ]                     
    })
def write_tif_to_file(data):
    tifffile.imwrite('output2.tif', data)
    return []


@node(input=[Image2D], output=[Image2D], params= 
    {
        "Parameters": [ ]                     
    })
def test_exception(data):
    raise Exception("Unhandled exception from Python")
    return []


@node(input=[], output=[Image2D], params= 
    {
        "Parameters": [
                {
                "type": Image2DPath("")
            },
         ]                     
    })
def read_tif(path):
    loader = TIFFStackLoader(path)
    stack = loader.load_stack()
    if stack.ndim == 3 and stack.shape[0] == 1:
        return stack[0]  # single-page TIFF
    elif stack.ndim == 3:
        raise ValueError("Expected single 2D image, got multi-page TIFF")
    return stack




@node(
    input=[Image2D],
    output=[Image2D],
    params={
        "Parameters": [
            {
                "type": Categoric(
                    ["Gaussian Blur", "Sharpen", "Edge Detection", "Invert"],
                    "Gaussian Blur",
                    "Filter"
                )
            },
            {
                "type": Scalar(0.0,  "Sigma: used only for Gaussian Blur")
            }
        ]
    }
)
def image_filter(image_array, selected_filter, sigma=1.0):
    img = np.array(image_array)

    match selected_filter:
        case "Gaussian Blur":
            # Sigma can be a scalar
            result = gaussian_filter(img, sigma=sigma)
        case "Sharpen":
            blurred = gaussian_filter(img, sigma=1.0)
            result = img + (img - blurred)
        case "Edge Detection":
            result = ski.filters.sobel(img)
        case "Invert":
            result = ski.util.invert(img)
        case _:
            raise ValueError(f"Unsupported filter: {selected_filter}")

    return result