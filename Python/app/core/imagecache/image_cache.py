import redis 
import msgpack , numpy as np
import socket , struct
import threading

from app.core.measurements.measurementelementsutils import XYStagePosition

class RedisImageCache:
    def __init__(self):
        # self.cache = redis.Redis(host='dagorchestrator-redis-1', port=6379, decode_responses=False)
        self.cache = redis.Redis(host='localhost', port=6379, decode_responses=False)


ImageCache = RedisImageCache()


class ImageCacheLocal:
    def __init__(self):
        self.cache = {}
        self.current_job_id = None
        self.isconnected = False
        self._lock = threading.Lock()
        self.image_data = {}
        self.position_data = {}

    def get(self, key):
        try: 
            result = self.cache[key]
            return  result

        except Exception as e:
            print(f'No key:{key} found in local cache')
            
    def clear_key(self, key_val):
        with self._lock:
            self.image_data.pop(key_val, None)
            self.cache.pop(key_val, None)
            self.position_data.pop(key_val, None)

       
    def recv_exact(self, sock :socket, size):
        buffer = bytearray(size)
        view = memoryview(buffer)
        bytes_received = 0
        while bytes_received < size:
            n = sock.recv_into(view[bytes_received:])
            if n == 0:
                raise ConnectionError("Connection closed")
            bytes_received += n
        return buffer
    



    async def start(self):

        HOST = "127.0.0.1"
        PORT = 8401

        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as self.server:
            self.server.setsockopt(socket.SOL_SOCKET, socket.SO_RCVBUF, 8 * 1024 * 1024)
            self.server.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 8 * 1024 * 1024)


            self.server.bind((HOST, PORT))
            self.server.listen()
            print(f"Server listening on {HOST}:{PORT}")

            while True: 
                conn, addr = self.server.accept()
                self.isconnected = True
                while self.isconnected:
                    try:

                        jobid_bytes = self.recv_exact(conn, 36)
                        guid = jobid_bytes.decode("utf-8")
                        conn.sendall(b"GUID_RECEIVED")

                        size_bytes = self.recv_exact(conn, 4)
                        size = struct.unpack("!I", size_bytes)[0]
                        conn.sendall(b"SIZE_RECEIVED")

                        payload = self.recv_exact(conn, size)
                        unpacker = msgpack.Unpacker(raw=False)
                        unpacker.feed(payload)

                        for unpacked_images in unpacker:
                                image_data = np.frombuffer( unpacked_images['message']['data']['imagedata'],
                                                        dtype=np.uint16)
                                width = unpacked_images['message']['data']['ncols']
                                height = unpacked_images['message']['data']['nrows']

                                acq_name = unpacked_images['message']['metadata']['acquisitiontype']
                                det_name = unpacked_images['message']['data']['detectorname']


                                image_data = np.reshape(image_data,shape=(width, height))   
                                xy_pos = XYStagePosition.from_cache(unpacked_images['message']
                                                        ['metadata']['stageposition'])

                                self.image_data[f'{guid}_{acq_name}_{det_name}'] = image_data
                                self.position_data[f'{guid}_{acq_name}_{det_name}'] = xy_pos
                                break
                            # except: 
                            #     continue


                        #     LocalImageCache.set(unpacked_images, LocalImageCache.current_job_id)
                        conn.sendall(b"DATA_RECEIVED")


                    except Exception as e:
                        self.isconnected = False
                        print(e)

    

LocalImageCache = ImageCacheLocal()
