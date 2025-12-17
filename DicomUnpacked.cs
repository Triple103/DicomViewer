using FellowOakDicom;
using System.Buffers.Binary;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace DICOM_HPF5Viewer
{
    public class UnpackedDicom
    {
        public DicomDataset AllTags { get; private set; }
        //Paramètres propres au file
        public int ImgHeight { get; set; }
        public int ImgWidth { get; set; }
        public int NumberOfFrames { get; private set; }
        public int BitsAllocated { get; private set; }
        public int SamplesPerPx { get; private set; }
        public int PixelRepresentation { get; private set; }
        public PixelFormat Format { get; private set; }
        public bool IsEnhanced { get; private set; }

        //Paramètres pouvant varier d'une image a l'autre ou être généraux
        public double? RescaleSlopeGen { get; set; }
        public double? RescaleInterceptGen { get; set; }
        public double BaseWindowWidthGen { get; set; }
        public double BaseWindowCenterGen { get; set; }



        public List<ImageData> imageDataList = new List<ImageData>();

        //Constructor
        public UnpackedDicom(DicomFile dicom, PixelFormat format)
        {
            var dataset = dicom.Dataset;
            ImgHeight = dataset.GetSingleValue<int>(DicomTag.Rows);
            ImgWidth = dataset.GetSingleValue<int>(DicomTag.Columns);
            NumberOfFrames = dataset.GetSingleValueOrDefault<int>(DicomTag.NumberOfFrames, 1);
            BitsAllocated = dataset.GetSingleValue<int>(DicomTag.BitsAllocated);
            SamplesPerPx = dataset.GetSingleValue<int>(DicomTag.SamplesPerPixel);
            PixelRepresentation = dataset.GetSingleValue<int>(DicomTag.PixelRepresentation);
            Format = format;

            // Check if enhanced dicom :
            bool hasSharedFG = dataset.Contains(DicomTag.SharedFunctionalGroupsSequence);
            bool hasPerFrameFG = dataset.Contains(DicomTag.PerFrameFunctionalGroupsSequence);
            IsEnhanced = hasSharedFG || hasPerFrameFG;

            //Frame-variable parameters are in ImageData. Default values are marked as [...]Gen
            BaseWindowWidthGen = dataset.GetSingleValueOrDefault<double>(DicomTag.WindowWidth, 6442450942);
            BaseWindowCenterGen = dataset.GetSingleValueOrDefault<double>(DicomTag.WindowCenter, 1073741824);
            //If no window specified, general mapping will go from lowest int32 to highest uint32 to avoid out of range.

            if (IsEnhanced)
            {
                var sharedFGSequence = dataset.GetSequence(DicomTag.SharedFunctionalGroupsSequence);
                var sharedFG = sharedFGSequence?.Items.FirstOrDefault();
                if (sharedFG != null)
                {
                    BaseWindowWidthGen = sharedFG.GetSingleValueOrDefault<double>(DicomTag.WindowWidth, BaseWindowWidthGen);
                    BaseWindowCenterGen = sharedFG.GetSingleValueOrDefault<double>(DicomTag.WindowCenter, BaseWindowCenterGen);
                }
            }

            AllTags = new DicomDataset(dataset);
            AllTags.Remove(DicomTag.PixelData);
        }

        public BitmapSource GetBitmap(
            int slice = 0,
            double? windowCenter = null,
            double? windowWidth = null,
            Func<double[], double?, double?, uint[]>? windower = null)
        //RGB windowing not supported yet
        {
            int stride = 4 * ((ImgWidth * Format.BitsPerPixel / 8 + 3) / 4); //Width*BytesPerPx padded to multiple of 4
                                                                             //Confirmé: DicomLogic nous renvoie les bonnes valeurs dans le bon ordre. Le reste est un problème d'affichage, de windowing, et de bitmap.Create

            //Uses frame-specific window center and width when no user specified mapping exists
            windower ??= LinWindower;
            uint[] windowed = windower(
                imageDataList[slice].Data,
                windowCenter ?? imageDataList[slice].BaseWindowCenter,
                windowWidth ?? imageDataList[slice].BaseWindowWidth
                );

            WriteableBitmap bitmap = new WriteableBitmap(BitmapSource.Create(
            ImgWidth,
            ImgHeight,
            96.0,
            96.0,
            Format,
            null,
            LilEndian(windowed),
            stride
            ));

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var fs = File.Create("debug.png"))
            {
                encoder.Save(fs);
            }

            return bitmap;
        }


        //Windowers
        public uint[] LinWindower(double[] data, double? windowCenter = null, double? windowWidth = null)
        {//If there is no center specified by user or frame, uses general window center and width
            double wC = windowCenter ?? BaseWindowCenterGen;
            double wW = windowWidth ?? BaseWindowWidthGen;
            uint[] clamped = new uint[data.Length];

            double windowScale = Math.Pow(2, Format.BitsPerPixel) - 1;

            double UpperBound = wC + (wW / 2);
            double LowerBound = wC - (wW / 2);
            for (int i = 0; i < data.Length; i++)
            {
                var x = Math.Clamp(data[i], LowerBound, UpperBound);
                clamped[i] = (uint)(windowScale * (x - LowerBound) / (UpperBound - LowerBound));
            }
            return clamped;
        }

        //
        public byte[] LilEndian(uint[] intdata)
        {
            Byte[] output = new Byte[intdata.Length * Format.BitsPerPixel / 8];

            for (int i = 0; i < intdata.Length; i++)
            {
                var inMyInt = new Byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(inMyInt, intdata[i]);
                for (int j = 0; j < (Format.BitsPerPixel/8); j++)
                { output[(2 * i) + j] = inMyInt[j]; }
            }
            return output;
        }

    }
}
