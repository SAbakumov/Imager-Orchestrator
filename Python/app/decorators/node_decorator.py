from typing import List, Optional
from fastapi import Request
from app.core.processing_nodes import NodeInput
from app.core.dag_types import data_types, IOType
from app.routers.processing_router import router

def node(input: Optional[List] = None, output: Optional[List[IOType]] = None, params: Optional[dict] = None):
    def decorator(func):
        async def endpoint(request: Request):
            node_input = NodeInput(**await request.json())

            argvals = [
                data_types[input_val.input_type](**input_val.input_params.input_json_params).load_data()
                for input_val in node_input.input
            ]

            results = await func(*argvals)
            if not isinstance(results, tuple):
                results = [results]
            return [
                datatype.set_result(result, f"{node_input.node_id}.out{out_id}")
                for out_id, (datatype, result) in enumerate(zip(output or [], results))
            ]

        async def endpoint_info():
            return {
                "Input": [x.serialize() for x in input or []],
                "Output": [x.serialize() for x in output or []],
                "Parameters": [param["type"].serialize() for param in (params or {}).get("Parameters", [])]
            }

        router.add_api_route(f'/processing/{func.__name__}', endpoint=endpoint, methods=["POST"])
        router.add_api_route(f'/processing/{func.__name__}/get_info', endpoint=endpoint_info, methods=["GET"])

        return func  # Important: return original function

    return decorator