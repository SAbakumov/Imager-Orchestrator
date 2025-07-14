import numpy as np 

from fastapi import  Request
from app.core.processing_nodes import NodeInput
from app.core.dag_types import data_types
from app.routers.processing_router import router


def node(input=None, output=None, params=None):
    def decorator(func):
        async def endpoint(request: Request):
            json_input = await request.json()
            node_input = NodeInput(**json_input)
            

            argvals = []
            for input_vals in node_input.input:
                data_class = data_types[input_vals.input_type](**input_vals.input_params.input_json_params)
                argvals.append(data_class.load_data())

            result = await func(*argvals)

            return {"result": result}
     
        async def endpoint_info():
            info = {}

            info["Input"] = [x.serialize() for x in input]
            info["Output"] = [x.serialize() for x in output]
            info["Parameters"] = []

            for param in params["Parameters"]:
                info["Parameters"].append(param["type"].serialize())
                



            return info
        


        router.add_api_route(f'/processing/{func.__name__}',endpoint = endpoint, methods=["POST"])
        router.add_api_route(f'/processing/{func.__name__}/get_info',endpoint = endpoint_info, methods=["GET"])

    return decorator
        

"""
Structure of the json payload:
{
    node_id: HASH_NODE_ID,
    input: [
    {
        input_dir: mmf/path/to/dir,
        input_type: Type (Image2D, Image3D)
        input_shape: [int, int...]
    },
    {
        input_dir: mmf/path/to/dir,
        input_type: Type (Image2D, Image3D)
        input_shape: [int, int...]
    },
    ...
    ]
}

"""

