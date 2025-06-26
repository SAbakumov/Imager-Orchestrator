# main.py
from fastapi import FastAPI
from app.routers.processing_router import router
import uvicorn


import plotly.express as px
import numpy as np 
import app.core.processing_functions


app = FastAPI()
app.include_router(router, prefix="/api")




@app.get("/api/get_nodes")
def get_processing_nodes():

    nodelist =  {"processing_nodes" : []}
    for route in app.routes:
        if "/processing/" in route.path and "get_info" not in route.path:
            nodelist["processing_nodes"].append(route.path)
        
    return nodelist


for route in app.routes:
    print(f"{route.path} [{','.join(route.methods)}]")

@app.get("/")
def root():
    return {"message": "FastAPI with router is working!"}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8080, reload=True)