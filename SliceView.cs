using CommunityToolkit.Mvvm.ComponentModel;
using System.Buffers.Binary;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DicomViewer
{
    public partial class SliceModel : ObservableObject
    {

        [ObservableProperty] ImageSource? image;
        [ObservableProperty] int sliceIndex;

        Func<UnpackedDicom, int, double?, double?, uint[]> Windower { get; set; }

        public SliceModel(UnpackedDicom dicom, int index)
        {
            Windower = DicomLogic.LinGreyscaleWindower; //Allows to change windower setting
            sliceIndex = index;


            image = GetBitmap(dicom, index);

        }



        public BitmapSource GetBitmap(UnpackedDicom dicom, int index)
        //RGB windowing not supported yet
        {




            uint[] source = GetImageSource(dicom);
            PixelFormat format = DicomLogic.FormatSolver(dicom);
            int stride = 4 * ((dicom.ImgWidth * dicom.BytesPerPx + 3) / 4); //Width*BytesPerPx padded to multiple of 4


            WriteableBitmap bitmap = new WriteableBitmap(BitmapSource.Create(dicom.ImgWidth,
                                                                            dicom.ImgHeight,
                                                                            96.0, 96.0,
                                                                            format,
                                                                            null,
                                                                            LilEndian(source, dicom.BytesPerPx),
                                                                            stride));


            return bitmap;
        }


        public uint[] GetImageSource(UnpackedDicom dicom)
        {
            var slice = dicom.imageDataList[SliceIndex];
            var output = new uint[slice.Data.Length];
            FieldInfo? field = dicom.Format.GetType().GetField(dicom.Format.ToString());
            if (field != null && field.IsDefined(typeof(RequiresWindowingAttribute)))
            {
                output = Windower(dicom, SliceIndex, null, null);
            }
            //if (field != null && field.IsDefined(typeof(RequiresConversionAttribute)))
            //{
            //Pass for now
            //}
            else { /*DicomLogic/WindowlessFormatsHandler*/}
            ;
            return output;
        }


        //Transform uint[] to byte[] of appropriate format
        public byte[] LilEndian(uint[] intdata, int bytesPerPx)
        {
            Byte[] output = new Byte[intdata.Length * bytesPerPx];

            for (int i = 0; i < intdata.Length; i++)
            {
                var inMyInt = new Byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(inMyInt, intdata[i]);
                for (int j = 0; j < bytesPerPx; j++)
                { output[(2 * i) + j] = inMyInt[j]; }
            }
            return output;
        }
    }
}
