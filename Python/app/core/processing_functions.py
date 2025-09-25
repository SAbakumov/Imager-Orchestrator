import numpy as np 

from app.core.dag_types import *
from app.core.measurements.measurementelements import *
from app.decorators.node_decorator import node 
from scipy.ndimage import gaussian_filter
from app.core.user_classes.merge_channels import channel_merger

import skimage as ski








@node(input=[Image2D,Image2D], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": Categoric(["Add", "Subtract", "Multiply"], "Add", "Operation")
            }  ]    
                       
    })
def image_arithmetic(input_arr1: Image2D, input_arr2 : Image2D, input_option):
  
    img1 = np.array(input_arr1.imdata)
    img2 = np.array(input_arr2.imdata)

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
    return Image2D.from_array(result)


@node(input=[Image2D], output=[Image2D], params= 
    {
        "Parameters": [ ]                     
    })
def calculate_mean(data: Image2D):
    mean_all = np.mean(data.imdata)
    data.imdata = mean_all
    return data



@node(input=[Image2D], output=[DoTimesProperties], params= 
    {
        "Parameters": [ ]                     
    })
def return_dotimes(data: Image2D):

    loop = DoTimes(20)

    return loop.properties



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
def image_filter(image_array: Image2D, selected_filter, sigma=1.0):
    img = np.array(image_array.imdata)

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

    return Image2D.from_array(result)