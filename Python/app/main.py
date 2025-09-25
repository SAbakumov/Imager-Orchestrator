# main.py
from fastapi import FastAPI, Request,  Body 
from fastapi.responses import JSONResponse, HTMLResponse
from fastapi.middleware.cors import CORSMiddleware

from app.routers.processing_router import router
from app.core.statemanagement.state import State

import asyncio, threading
import uvicorn
import traceback
import msgpack
import app.core.processing_functions
import app.core.user_classes.accumulate_stage_loop
import app.core.io_functions


from app.core.imagecache.image_cache import LocalImageCache
from fastapi import FastAPI, WebSocket
from app.core.user_classes.merge_channels import channel_merger
from imagedata.image_data_handler import datarouter
from imagedata.image_data_handler import image_provider

app = FastAPI()
app.include_router(router, prefix="/api")
app.include_router(datarouter, prefix="/imagedata")


def run_server():
    asyncio.run(LocalImageCache.start())

thread = threading.Thread(target=run_server, daemon=True)
thread.start()




@app.exception_handler(Exception)
async def exception_handler(request: Request, exc: Exception):
    tb_str = "".join(traceback.format_exception(type(exc), exc, exc.__traceback__))
    
    return JSONResponse(
        status_code=500,
        content={
            "detail": "Internal Server Error",
            "error": str(exc),
            "traceback": tb_str
        }
    )

@app.post("/set_jobid")
async def set_jobid(job: str = Body(..., embed=True)):
    LocalImageCache.current_job_id = job


@app.delete("/clear_key/{keyval}")
def clear_key( keyval: str):
    LocalImageCache.clear_key(keyval)
    return {'status':'success'}


@app.get("/purge_state")
async def clear_state(dagid: str):
    State.reset_program(dagid)

@app.get("/purge_all")
async def clear_all_state(dagid: str):
    State.reset()

@app.get("/api/get_nodes")
def get_processing_nodes():

    nodelist =  []
    for route in app.routes:
        if "/processing/" in route.path and "get_info" not in route.path :
            nodelist.append(route.path)
        if "/io/" in route.path and "get_info" not in route.path :
            nodelist.append(route.path)           
        
    return nodelist



@app.get("/")
def root():
    return {"message": "FastAPI with router is working!"}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8080, reload=True)