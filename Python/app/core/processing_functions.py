import numpy as np 

from app.core.dag_types import *
from app.decorators.node_decorator import node 







@node(input=[Image2D], output=[Image2D], params= 
    {
        "params": [
            {
             "name": "Value to add",
             "type": Scalar(10)
            },
            {
             "name": "Category to select",
             "type": Categoric(["Option 1", "Option 2", "Option 3"], "Option 1")
            }  ]                       
    })
async def add_scalar(input_arr1, input_scalar):
    output_arr = await (input_arr1 + input_scalar)
    return output_arr

# @node(input=[MMFPath], output = None)
# async def picasso_localize(input_path):
#     args = SimpleNamespace(
#         files="testdata.raw",
#         fit_method="mle",
#         box_side_length=7,
#         gradient=5000,
#         baseline=0,
#         sensitivity=1,
#         gain=1,
#         qe=1,
#         roi=None,
#         drift=100,
#     )


#     picasso_localizer._localize(args)