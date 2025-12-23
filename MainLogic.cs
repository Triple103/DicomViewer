using FellowOakDicom;

namespace DicomViewer
{
    public class MainLogic
    {
        FileLoader fileLoader;
        DicomLogic dicomLogic;

        //Loaded files
        public List<UnpackedDicom> Dicoms { get; private set; } = new List<UnpackedDicom>();

        //Events
        public event Action<UnpackedDicom>? NewDicom;

        // Constructeur
        public MainLogic()
        {
            dicomLogic = new DicomLogic();
            fileLoader = new FileLoader();
        }

        public void DicomLoad()
        {
            (DicomFile? file, string? name)[] loaded = fileLoader.LoadDicom();
            if (loaded[0].file != null)
            {
                for (int i = 0; i < loaded.Length; i++)
                {
                    Dicoms.Add(DicomLogic.UnpackDicom(loaded[i].file, loaded[i].name));
                    NewDicom?.Invoke(Dicoms.Last());
                }
            }
        }




    }
}
