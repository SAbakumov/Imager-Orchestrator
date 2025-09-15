from fastapi import APIRouter, Request, Depends
from imagedata.image_data_provider import ImageDataProvider
from fastapi import Query

import asyncio
import numpy as np

datarouter = APIRouter()
image_provider = ImageDataProvider()


def get_image_provider() -> ImageDataProvider:
    return image_provider





@datarouter.get("/clear_all_data")
async def clear_all_image_data(
    request: Request,
    process_id: str = Query(...),
    provider: ImageDataProvider = Depends(get_image_provider)
):
    provider.clear_all_image_data(process_id)

    return {"status": "data cleared"}

# @datarouter.post("/{process_id}/start")
# async def submit_all(detindex:int, 
#                     provider: ImageDataProvider = Depends(get_image_provider)
# ):
#     return {"status": "submitted"}