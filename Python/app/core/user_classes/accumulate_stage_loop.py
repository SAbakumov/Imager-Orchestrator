from app.core.measurements.measurementelements import *
from app.core.dag_types import *
from app.decorators.node_decorator import *
from app.core.statemanagement.state import *




    
State.add( {'accumulated_stage_loop': StageLoop(), 
            'counter': 0})


@node(input=[Image2D], output=[StageLoopProperties], params= 
    {
        "Parameters": []               
    },
    )
def add_stage_pos(image : Image2D):
    if State.counter%2 ==0:
        State.accumulated_stage_loop.append_stage_position(image.xy_pos)
    State.counter+=1
    return  State.accumulated_stage_loop.properties



