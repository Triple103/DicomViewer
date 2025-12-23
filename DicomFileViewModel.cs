using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewer
{
     public partial class DicomFileViewModel : ObservableObject
    {
        [ObservableProperty] public string fileName;

        [ObservableProperty] public int fileIndex;

        public DicomFileViewModel(UnpackedDicom dicom,int fileIndex)
        {
            this.fileName = dicom.Name;
            this.fileIndex = fileIndex;
        }


    }
}
