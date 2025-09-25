import sys
import copy

class StateMeta(type):
    _registry = {}
    _originals = {}
    _initializers = []
    _program_id = ""
    _var_register = {}

    def add(cls, data):
        if callable(data):
            cls._initializers.append(data)
            value_dict = data()
            if not isinstance(value_dict, dict):
                raise TypeError("Initializer function must return a dict")
        elif isinstance(data, dict):
            cls._initializers.append(lambda d=data: d)
            value_dict = data
        else:
            raise TypeError("State.add expects dict or function returning dict")

        for k, v in value_dict.items():
            cls._registry[k] = v
            cls._originals[k] = copy.deepcopy(v)
            sys.modules[__name__].__dict__[k] = v

    @classmethod
    def set_program_id(cls,dagid):
        if dagid not in cls._var_register.keys():
            cls._var_register[dagid] = []
        cls._program_id = dagid


    def reset_program(cls, program_id=None):


        if program_id is None:
            program_id = cls._program_id

        if not program_id or program_id not in cls._var_register:
            return  

        vars_to_reset = cls._var_register[program_id]

        for k in vars_to_reset:
            if k in cls._originals:
                cls._registry[k] = copy.deepcopy(cls._originals[k])
                sys.modules[__name__].__dict__[k] = cls._registry[k]

        cls._var_register[program_id] = []

    def reset(cls, *vars_to_reset):

        keys = vars_to_reset or cls._originals.keys()
        cls._var_register.clear()
        for k in keys:
            if k in cls._originals:
                cls._registry[k] = copy.deepcopy(cls._originals[k])
                sys.modules[__name__].__dict__[k] = cls._registry[k]
            else:
                raise KeyError(f"No such state variable: {k}")

        if not vars_to_reset:
            for init in cls._initializers:
                try:
                    val = init()
                    if isinstance(val, dict):
                        for k, v in val.items():
                            if k not in cls._originals:
                                cls._registry[k] = v
                                cls._originals[k] = copy.deepcopy(v)
                                sys.modules[__name__].__dict__[k] = v
                except Exception:
                    pass

    def __getattr__(cls, name):
        if name in cls._registry:
            if cls._program_id!="":
                if name not in cls._var_register[cls._program_id]:
                    cls._var_register[cls._program_id].append(name)
            return cls._registry[name]
        

        raise AttributeError(f"No such state variable: {name}")

    def __setattr__(cls, name, value):
        cls._registry[name] = value
        sys.modules[__name__].__dict__[name] = value
        if cls._program_id!="":
            if name not in cls._var_register[cls._program_id]:
                cls._var_register[cls._program_id].append(name)

class State(metaclass=StateMeta):
    pass


