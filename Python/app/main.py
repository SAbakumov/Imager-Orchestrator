# main.py
from fastapi import FastAPI, Request
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from app.routers.processing_router import router

import uvicorn
import time
import asyncio
import traceback
import plotly.express as px
import numpy as np 
import app.core.processing_functions
import app.core.io_functions

from imagedata.image_data_handler import datarouter
from imagedata.image_data_handler import image_provider

app = FastAPI()
app.include_router(router, prefix="/api")
app.include_router(datarouter, prefix="/imagedata")


@app.get("/ping")
async def ping():
    return {"status": "ok"}

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


@app.post("/api/test")
async def test_endpoint(request: Request):
    start = time.time()
    data = await request.json()  # read JSON payload
    end = time.time()
    elapsed = end - start
    return {"received": data, "elapsed_seconds": elapsed}


@app.get("/api/get_nodes")
def get_processing_nodes():

    nodelist =  []
    for route in app.routes:
        if "/processing/" in route.path and "get_info" not in route.path :
            nodelist.append(route.path)
        if "/io/" in route.path and "get_info" not in route.path :
            nodelist.append(route.path)           
        
    return nodelist


for route in app.routes:
    print(f"{route.path} [{','.join(route.methods)}]")

@app.get("/")
def root():
    return {"message": "FastAPI with router is working!"}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8080, reload=True)