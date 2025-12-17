using System.Windows;
using System.Windows.Media;

namespace DICOM_HPF5Viewer
{
    public partial class MainWindow : Window
    {
        //Parametres
        private MainLogic mainLogik;


        // Constructeur
        public MainWindow()
        {
            InitializeComponent();
            mainLogik = new MainLogic();

            //Abonnements
            mainLogik.ImageChange += HandleImageChange;
        }

        public async void WindowContentRendered(object sender, RoutedEventArgs e)
        {
            await mainLogik.Run();
        }



        // Imput Handlers
        private void FileExpl(object sender, RoutedEventArgs e)
        {
            mainLogik.DicomLoad();
        }

        // Event Handlers
        private void HandleImageChange(ImageSource img)
        {
            MainImg.Source = img;
        }

    }
}