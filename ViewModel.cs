using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace DicomViewer
{
    public partial class ViewModel : ObservableObject
    {
        private MainLogic mainLogick { get; }

        [ObservableProperty] public SliceModel? currentSlice;
        [ObservableProperty] public ObservableCollection<SliceModel> listSlices;

        [ObservableProperty] private DicomFileViewModel? selectedDicom;
        [ObservableProperty] private ObservableCollection<DicomFileViewModel> loadedDicomViews;


        //Commandes
        public ICommand OpenNewFile { get; }

        public ViewModel()
        {
            loadedDicomViews = new();
            listSlices = new();
            mainLogick = new MainLogic();

            //Commandes
            OpenNewFile = new RelayCommand(OpenFile);

            //Abonnements
            mainLogick.NewDicom += AddDicomView;
        }



        //Inputs
        public void OpenFile()
        {
            mainLogick.DicomLoad();
        }

        partial void OnSelectedDicomChanged(DicomFileViewModel self)
        {
            if (self.fileIndex < 0)
                return;

            ListSlices.Clear();
            var dicom = mainLogick.Dicoms[self.fileIndex];
            for (int i = 0; i < dicom.imageDataList.Count; i++)
            {
                var slice = new SliceModel(dicom, i);
                ListSlices.Add(slice);
            }
            CurrentSlice = ListSlices[0];
        }



        //Outputs
        private void AddDicomView(UnpackedDicom dicomFile)
        {
            var dicomView = new DicomFileViewModel(dicomFile, mainLogick.Dicoms.Count-1);
            LoadedDicomViews.Add(dicomView);

            if (mainLogick.Dicoms.Count != LoadedDicomViews.Count)
            {
                Console.WriteLine("Erreurs d'indexation des viewmodels dicoms");
            }

            SelectedDicom=LoadedDicomViews[LoadedDicomViews.Count-1];
        }


    }
}
