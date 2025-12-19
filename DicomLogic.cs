using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Render;
using System.Windows.Media;

namespace DicomViewer
{
    internal class DicomLogic
    {



        //Dictionnaire pour gérer les formats d'image Dicom

        public static readonly Dictionary
        <(int? samplePerPixel, int? bitsAllocated, int pixelRepresentation, string? photometric), PixelFormat>
        FormatHandler = new Dictionary
        <(int? samplePerPixel, int? bitsAllocated, int pixelRepresentation, string? photometric), PixelFormat>
        {
                { (1, 8, 0, "MONOCHROME2"),  PixelFormats.Gray8},
                { (1, 16, 0, "MONOCHROME2"), PixelFormats.Gray16},
                { (3, 8, 0, "RGB"),          PixelFormats.Rgb24 },
                { (3, 16, 0, "RGB"),         PixelFormats.Rgb48 }
        };

        public PixelFormat DicomFormatReader(DicomDataset dataset)
        {
            int? samplesPerPixel = dataset.GetSingleValueOrDefault<ushort?>(DicomTag.SamplesPerPixel, null);
            int? bitsAllocated = dataset.GetSingleValueOrDefault<ushort?>(DicomTag.BitsAllocated, null);
            int pixelRepresentation = dataset.GetSingleValueOrDefault<ushort>(DicomTag.PixelRepresentation, 0);
            string? photometric = dataset.GetSingleValueOrDefault<string?>(DicomTag.PhotometricInterpretation, null);

            var key = (samplesPerPixel, bitsAllocated, pixelRepresentation, photometric);
            PixelFormat format = FormatHandler[key];
            return (format);

        }

        public UnpackedDicom UnpackDicom(DicomFile dicom,string nameDicom)
        {
            //Working objects
            var dataset = dicom.Dataset;
            var pixelData = DicomPixelData.Create(dataset);

            //Get general image parameters
            PixelFormat pixelFormat = DicomFormatReader(dataset);
            UnpackedDicom UDicom = new UnpackedDicom(dicom, pixelFormat,nameDicom);

            //initialisation si possible des paramètres potentiellement changeants d'une frame a l'autre

            double? RescaleSlopeGen = dataset.GetSingleValueOrDefault<double?>(DicomTag.RescaleSlope, null);
            double? RescaleInterceptGen = dataset.GetSingleValueOrDefault<double?>(DicomTag.RescaleIntercept, null);
            if (UDicom.IsEnhanced)
            {
                var sharedFG = dataset.GetSequence(DicomTag.SharedFunctionalGroupsSequence).Items.FirstOrDefault();
                {
                    RescaleSlopeGen = sharedFG.GetSingleValueOrDefault<double?>(DicomTag.RescaleSlope, RescaleSlopeGen);
                    RescaleInterceptGen = sharedFG.GetSingleValueOrDefault<double?>(DicomTag.RescaleIntercept, RescaleInterceptGen);
                }
            }

            //For every frame
            for (int frame = 0; frame < UDicom.NumberOfFrames; frame++)
            {
                //default needs to be reset at every frame
                double? rescaleSlope = RescaleSlopeGen;         //We won't need rescale once real values are converted
                double? rescaleIntercept = RescaleInterceptGen;
                double? windowWidth = null;                     //General windowWidth values are used if none specific exist
                double? windowCenter = null;

                if (UDicom.IsEnhanced)
                {
                    var perFrameFG = dataset.GetSequence(DicomTag.PerFrameFunctionalGroupsSequence)?.Items[frame];
                    if (perFrameFG != null)
                    {
                        rescaleSlope = perFrameFG.GetSingleValueOrDefault<double?>(DicomTag.RescaleSlope, rescaleSlope);
                        rescaleIntercept = perFrameFG.GetSingleValueOrDefault<double?>(DicomTag.RescaleIntercept, rescaleIntercept);
                        windowWidth = perFrameFG.GetSingleValueOrDefault<double?>(DicomTag.WindowWidth, windowWidth);
                        windowCenter = perFrameFG.GetSingleValueOrDefault<double?>(DicomTag.WindowCenter, windowCenter);
                    }
                }

                IPixelData RawData = PixelDataFactory.Create(pixelData, frame);

                var data = new double[UDicom.ImgWidth * UDicom.ImgHeight];
                int n = 0;
                for (int y = 0; y < UDicom.ImgHeight; y++)
                {
                    for (int x = 0; x < UDicom.ImgWidth; x++)
                    {
                        data[n++] = RawData.GetPixel(x, y);
                    }
                }

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (rescaleSlope ?? 1) * data[i] + (rescaleIntercept ?? 0);
                }

                UDicom.imageDataList.Add(new ImageData(data, windowCenter, windowWidth));
            }

            return UDicom;
        }


    }



}
