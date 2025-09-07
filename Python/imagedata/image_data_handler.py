from fastapi import APIRouter, Request, Depends
from imagedata.image_data_provider import ImageDataProvider
from fastapi import Query

import asyncio
import numpy as np

datarouter = APIRouter()
image_provider = ImageDataProvider()


def get_image_provider() -> ImageDataProvider:
    return image_provider



@datarouter.post("/set_data")
async def append_image(
    request: Request,
    process_id: str = Query(...),
    acqname: str = Query(...),
    detname: str = Query(...),
    detindex: int = Query(...),
    width: int = Query(None),
    height: int = Query(None),
    provider: ImageDataProvider = Depends(get_image_provider)
):
    file_name = f"{process_id}_{acqname}_{detname}"
    body_data = await request.body()
    provider.add_image_data(file_name, body_data, detindex, width=width, height=height)

    return {"status": "received"}


@datarouter.get("/clear_all_data")
async def clear_all_image_data(
    request: Request,
    process_id: str = Query(...),
    provider: ImageDataProvider = Depends(get_image_provider)
):
    provider.clear_image_data(process_id)

    return {"status": "data cleared"}

# @datarouter.post("/{process_id}/start")
# async def submit_all(detindex:int, 
#                     provider: ImageDataProvider = Depends(get_image_provider)
# ):
#     return {"status": "submitted"}