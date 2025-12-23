namespace DicomViewer
{
    public class ImageData
    {
        public double[] Data { get; set; }
        public double? BaseWindowCenter { get; set; }
        public double? BaseWindowWidth { get; set; }

        //Constructeur
        public ImageData(double[] data, double? center, double? width)
        {
            Data = data;
            BaseWindowCenter = center;
            BaseWindowWidth = width;
        }

    }
}
