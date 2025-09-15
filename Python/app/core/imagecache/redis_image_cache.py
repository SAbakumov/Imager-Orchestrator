import redis 

class RedisImageCache:
    def __init__(self):
        self.cache = redis.Redis(host='dagorchestrator-redis-1', port=6379, decode_responses=False)
        # self.cache = redis.Redis(host='localhost', port=6379, decode_responses=False)


ImageCache = RedisImageCache()
