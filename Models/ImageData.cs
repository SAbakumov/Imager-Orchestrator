using MessagePack;

namespace DagOrchestrator.Models
{
    public class ImageData
    {
    }

    [MessagePackObject]
    public class MessagePackData
    {
        [Key("data")]
        public MessagePackImageData data { get; set; }

        [Key("index")]
        public int index { get; set; }

        [Key("metadata")]
        public ImageMetadata metadata { get; set; }

        [Key("type")]
        public string type { get; set; }
    }

    [MessagePackObject]
    public class MessagePackImageData
    {
        [Key("detectorname")]
        public string detectorname { get; set; }

        [Key("imagedata")]
        public byte[] imagedata { get; set; }

        [Key("ncols")]
        public int ncols { get; set; }

        [Key("nrows")]
        public int nrows { get; set; }

        [Key("numtype")]
        public int numtype { get; set; }

        [Key("timestamp")]
        public float timestamp { get; set; }
    }

    [MessagePackObject]
    public class ImageMetadata
    {
        [Key("acquisitiontype")]
        public string acquisitiontype { get; set; }

        [Key("stageposition")]
        public MessagePackStagePosition stageposition { get; set; }
    }

    [MessagePackObject]
    public class MessagePackStagePosition
    {
        [Key("hardwareautofocusoffset")]
        public int hardwareautofocusoffset { get; set; }

        [Key("usinghardwareautofocus")]
        public bool usinghardwareautofocus { get; set; }

        [Key("x")]
        public float x { get; set; }

        [Key("y")]
        public float y { get; set; }

        [Key("z")]
        public float z { get; set; }
    }
}
