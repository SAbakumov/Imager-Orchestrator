import plotly.graph_objects as go
import numpy as np

class MergeChannels:
    def __init__(self):
        self.channel1 = None 
        self.channel2 = None  
        self.fig = None

    def _normalize(self, arr):
        arr = arr.astype(float)
        return np.uint8(255 * (arr - np.min(arr)) / (np.ptp(arr) + 1e-8))

    def set_channels(self, ch1, ch2):
        ch1 = np.uint8((ch1 - ch1.min()) /256)
        ch2 = np.uint8((ch2 - ch2.min()) / 256).T

        self.channel1 = ch1
        self.channel2 = ch2

        rgb_image = np.zeros((ch1.shape[0], ch1.shape[1], 3), dtype=np.uint8)
        rgb_image[..., 0] = ch1
        rgb_image[..., 1] = ch2

        if self.fig is not None and len(self.fig.data) > 0:
            self.fig.data[0].z = rgb_image
        else:
            self.fig = go.Figure(go.Image(z=rgb_image))
            self.fig.update_layout(
                title="Merged Channels (Red + Green)",
                xaxis=dict(showticklabels=False),
                yaxis=dict(showticklabels=False)
            )

    def get_html(self):
        # Return the figure as self-contained HTML
        if self.fig is not None:
            return self.fig.to_html(full_html=True)
        else:
            return "<html><body><h3>No image set yet</h3></body></html>"


channel_merger = MergeChannels()
