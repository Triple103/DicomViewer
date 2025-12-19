using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DicomViewer
{
    public partial class SliceModel : ObservableObject
    {
        
        [ObservableProperty] ImageSource image;
        [ObservableProperty] int sliceIndex;

        public SliceModel(UnpackedDicom dicom,int index)
        { 
            image = dicom.GetBitmap(index);
            sliceIndex = index;
        }
    }
}
