using FellowOakDicom;
using Microsoft.Win32;
using System.IO;


namespace DicomViewer
{
    class FileLoader
    {
        public (DicomFile?, string?)[] LoadDicom()
        {
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var openFileDialog = new OpenFileDialog();
            {
                openFileDialog.InitialDirectory = userDir;
                openFileDialog.Filter = "DICOM files (*.dcm)|*.dcm|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.Multiselect = true;
                openFileDialog.RestoreDirectory = true;
            }

            if (openFileDialog.ShowDialog() == true)
            {
                var outpute = new (DicomFile, string)[openFileDialog.FileNames.Count()];
                for (int i=0;i<openFileDialog.FileNames.Count();i++)
                {
                    var filePath = openFileDialog.FileNames[i];
                    var name = Path.GetFileName(filePath);
                    DicomFile dicom = DicomFile.Open(filePath);
                    outpute[i]=(dicom,name);
                }

                return outpute;

            }
            else { return [(null, null)]; }
        }
    }
}