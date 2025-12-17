using FellowOakDicom;
using System.Windows.Media;

namespace DICOM_HPF5Viewer
{
    class MainLogic
    {
        FileLoader fileLoader;
        DicomLogic dicomLogic;

        //Loaded files
        private LinkedList<UnpackedDicom> Dicoms = new LinkedList<UnpackedDicom>();

        //Events
        public event Action<ImageSource>? ImageChange;

        // Constructeur
        public MainLogic()
        {
            dicomLogic = new DicomLogic();
            fileLoader = new FileLoader();
        }

        public void DicomLoad()
        {
            DicomFile file = fileLoader.LoadDicom();
            Dicoms.AddFirst(dicomLogic.UnpackDicom(file));
            ImageChange?.Invoke(Dicoms.First().GetBitmap());
        }

        public async Task Run()
        {
            while (true) { await Task.Delay(100); }
        }


    }
}
