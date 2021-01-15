using System;

namespace ImageResizeSample
{
    public record PictureInfoModel
    {
        public int ProductId { get; set; }
        public int ModelId { get; set; }
        public int Sequence { get; set; }
        public string PathName { get; set; }
        public string FileName { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string OutputName => $"U_{ProductId}_{Sequence}.jpg";
    }
}