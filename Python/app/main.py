# main.py
from fastapi import FastAPI, Request
from app.routers.processing_router import router
import uvicorn
import time


import plotly.express as px
import numpy as np 
import app.core.processing_functions


app = FastAPI()
app.include_router(router, prefix="/api")


@app.get("/ping")
async def ping():
    return {"status": "ok"}

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
        if "/processing/" in route.path and "get_info" not in route.path:
            nodelist.append(route.path)
        
    return nodelist


for route in app.routes:
    print(f"{route.path} [{','.join(route.methods)}]")

@app.get("/")
def root():
    return {"message": "FastAPI with router is working!"}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8080, reload=True)