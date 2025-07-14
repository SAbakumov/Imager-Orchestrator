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
async def add_scalar(input_arr1, input_scalar):
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

