using System.Windows;
using System.Windows.Controls;

namespace DicomViewer.UserControls
{
    /// <summary>
    /// Interaction logic for FileControl.xaml
    /// </summary>
    public partial class FileControl : UserControl
    {
        public FileControl()
        {
            InitializeComponent();


            this.Loaded += MyDicomListControl_Loaded;
        }




        private void MyDicomListControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
                Console.WriteLine($"UserControl DataContext type: {this.DataContext.GetType().FullName}");
            else
                Console.WriteLine("UserControl DataContext is null");
        }



    }

}
