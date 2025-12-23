using FellowOakDicom;

namespace DicomViewer
{
    public class UnpackedDicom
    {
        public DicomDataset AllTags { get; private set; }
        //Paramètres propres au file
        public string Name { get; private set; }
        public int ImgHeight { get; set; }
        public int ImgWidth { get; set; }
        public int NumberOfFrames { get; private set; }
        public int BytesAllocated { get; private set; }
        public int SamplesPerPx { get; set; }
        public int BytesPerPx { get; set; }
        public bool IsEnhanced { get; private set; }
        public SupportedFormats Format { get; private set; }
        public Func<(double[], double?, double?)>? BaseWindower { get; private set; }

        //Paramètres pouvant varier d'une image a l'autre ou être généraux
        public double? RescaleSlopeGen { get; set; }
        public double? RescaleInterceptGen { get; set; }
        public double BaseWindowWidthGen { get; set; }
        public double BaseWindowCenterGen { get; set; }



        public List<ImageData> imageDataList = new List<ImageData>();

        //Constructor
        public UnpackedDicom(DicomFile dicom, string name)
        {
            var dataset = dicom.Dataset;
            Name = name;

            ImgHeight = dataset.GetSingleValue<int>(DicomTag.Rows);
            ImgWidth = dataset.GetSingleValue<int>(DicomTag.Columns);
            NumberOfFrames = dataset.GetSingleValueOrDefault<int>(DicomTag.NumberOfFrames, 1);
            BytesAllocated = dataset.GetSingleValue<int>(DicomTag.BitsAllocated) / 8;
            SamplesPerPx = dataset.GetSingleValue<int>(DicomTag.SamplesPerPixel);
            BytesPerPx = BytesAllocated * SamplesPerPx;

            AllTags = new DicomDataset(dataset);
            AllTags.Remove(DicomTag.PixelData);

            BaseWindowSetter(dataset);
            Format = DicomLogic.FormatAttributor(this);

        }

        private void BaseWindowSetter(DicomDataset dataset)
        {
            // Check if enhanced dicom :
            bool hasSharedFG = dataset.Contains(DicomTag.SharedFunctionalGroupsSequence);
            bool hasPerFrameFG = dataset.Contains(DicomTag.PerFrameFunctionalGroupsSequence);
            IsEnhanced = hasSharedFG || hasPerFrameFG;

            //Frame-variable parameters are in ImageData. Default values are marked as [...]Gen
            BaseWindowWidthGen = dataset.GetSingleValueOrDefault<double>(DicomTag.WindowWidth, 6442450942);
            BaseWindowCenterGen = dataset.GetSingleValueOrDefault<double>(DicomTag.WindowCenter, 1073741824);
            //If no window specified, general mapping will go from lowest int32 to highest uint32 to avoid out of range. It intentionally sucks.
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
        }



    }





}

