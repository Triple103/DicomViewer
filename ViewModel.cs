using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DicomViewer
{
#nullable enable
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


        // UI Reactions
        partial void OnSelectedDicomChanged(DicomFileViewModel? value)
        {
            if (value == null) { return; }
            if (value.fileIndex < 0) { return; }

            ListSlices.Clear();
            var dicom = mainLogick.Dicoms[value.fileIndex];
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
            var dicomView = new DicomFileViewModel(dicomFile, mainLogick.Dicoms.Count - 1);
            LoadedDicomViews.Add(dicomView);

            /*if (mainLogick.Dicoms.Count != LoadedDicomViews.Count)
            {
                Console.WriteLine("Erreurs d'indexation des viewmodels dicoms");
            }*/

            SelectedDicom = LoadedDicomViews[LoadedDicomViews.Count - 1];
        }


    }
}
