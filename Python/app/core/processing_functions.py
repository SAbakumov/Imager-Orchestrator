import numpy as np 

from app.core.dag_types import *
from app.decorators.node_decorator import node 







@node(input=[Image2D], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": Scalar(10, "Value to add")
            },
            {
             "type": Categoric(["Option 1", "Option 2", "Option 3"], "Option 1","Category to select")
            }  ]                       
    })
async def add_scalar(input_arr1, input_scalar, option_value):
    output_arr = await (input_arr1 + input_scalar)
    return output_arr



@node(input=[Image2D], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": Scalar(10,"Value to add")
            },
        ]               
    })
async def multiply(input_arr, input_scalar):
    output_arr = await (input_arr*input_scalar)
    return output_arr


@node(input=[Image2D,Image2D], output=[Image2D], params= 
    {
        "Parameters": [
            {
             "type": Categoric(["Add", "Subtract", "Multiply"], "Add","Operation")
            }  ]    
                       
    })
async def image_arithmetic(input_arr1,input_arr2, input_option):
  
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



@node(input=[Image2D,Image2D], output=[Image2D,Image2D], params= 
    {
        "Parameters": [
            {
             "type": Categoric(["Option 1", "Option 2", "Option 3"], "Option 1","Category to select")
            },
            {
             "type": Scalar(100,"Value to subtract")
            }  ]    
                       
    })
async def filter(input_arr, input_option, input_value):
    if input_option=="Option 1":
        output_arr = input_arr 
    else:
        output_arr = input_arr -input_value
    return output_arr

