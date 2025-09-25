
class ImageProviderOutput:
    def __init__(self, input_path):
        self.input_path = input_path

    def set_jobid(self,jobid):
        return f'fromprovider::{jobid}_{self.input_path}'


class ElementProviderOutput:
    def __init__(self, element):
        self.element = element

