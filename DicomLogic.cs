using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Render;
using System.Windows.Media;

namespace DicomViewer
{
    internal class DicomLogic
    {

        public static UnpackedDicom UnpackDicom(DicomFile dicom, string nameDicom)
        {
            //Working objects
            var dataset = dicom.Dataset;
            var pixelData = DicomPixelData.Create(dataset);
            UnpackedDicom UDicom = new UnpackedDicom(dicom, nameDicom);

            var dicomImg = new DicomImage(dataset);
            var img = dicomImg.RenderImage();
            Console.WriteLine(img.GetType());

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

        //Pixel format handling
        public static SupportedFormats FormatAttributor(UnpackedDicom dicom)
        {
            var forma = dicom.AllTags.GetSingleValue<string>(DicomTag.PhotometricInterpretation);
            return (dicom.BytesAllocated, forma) switch
            {
                (1, "MONOCHROME2") => SupportedFormats.Gray8,
                (2, "MONOCHROME2") => SupportedFormats.Gray16,
            };
        }

        public static PixelFormat FormatSolver(UnpackedDicom dicom)
        {
            return dicom.Format switch
            {
                SupportedFormats.Gray8 => PixelFormats.Gray8,
                SupportedFormats.Gray16 => PixelFormats.Gray16,
                SupportedFormats.Gray32 => PixelFormats.Gray32Float,
            };
        }

        //Windowers
        public static uint[] LinGreyscaleWindower(UnpackedDicom dicom, int indexSlice,
                                                double? windowCenter = null, double? windowWidth = null)
        {
            ImageData slice = dicom.imageDataList[indexSlice];
            double wC = windowCenter ?? slice.BaseWindowCenter ?? dicom.BaseWindowCenterGen -0.5; // -0.5 and +1 from DICOM norm
            double wW = windowWidth ?? slice.BaseWindowWidth ?? dicom.BaseWindowWidthGen - 1;
            uint[] clamped = new uint[slice.Data.Length];

            double windowScale = Math.Pow(2, (dicom.BytesPerPx*8)) - 1; // Car BitmapSource.Create est sensible a PixelFormat.BitsPerPixel

            double UpperBound = wC + (wW / 2);
            double LowerBound = wC - (wW / 2);
            for (int i = 0; i < slice.Data.Length; i++)
            {
                var x = Math.Clamp(slice.Data[i], LowerBound, UpperBound);
                clamped[i] = (uint)(windowScale * (x - LowerBound) / (UpperBound - LowerBound));
            }
            return clamped;
        }



        /*public uint[] WindowlessFormats(UnpackedDicom dicom, int indexSlice)
        {
            var data = dicom.imageDataList[indexSlice].Data;
            dicom.Format switch
            {
                SupportedFormats.YCbCr_FULL_422 =>
            };
            
            var output = new uint[data.Length];
            for (int i=0, i< data.Length,int++)
            {
                output[i]= (uint)data[i];
            return

        }*/



    }




}
