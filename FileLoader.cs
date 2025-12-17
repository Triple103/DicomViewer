using FellowOakDicom;
using Microsoft.Win32;


namespace DICOM_HPF5Viewer
{
    class FileLoader
    {
        public DicomFile LoadDicom()
        {
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            {
                openFileDialog.InitialDirectory = userDir;
                openFileDialog.Filter = "DICOM files (*.dcm)|*.dcm|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.Multiselect = false;
                openFileDialog.RestoreDirectory = true;
            }
            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                //filePath = openFileDialog.FileName; Pas besoin du path pour l'instant
                var stream = openFileDialog.OpenFile();
                DicomFile dicom = DicomFile.Open(stream);
                return dicom;

            }
            else { return null; }
        }
    }
}
