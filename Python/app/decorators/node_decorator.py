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

            info["input"] = [x.serialize() for x in input]
            info["output"] = [x.serialize() for x in output]
            info["params"] = []

            for param in params["params"]:
                info["params"].append(param["type"].serialize())
                



            return {"node_info": info}
        


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

