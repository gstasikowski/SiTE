using System.Windows;

namespace SiTE
{
    /// <summary>
    /// Interaction logic for MainWindowTemp.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool isDoneLoading = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isDoneLoading)
            {
                var temp = Logic.Refs.viewControl.PageViewModels[0] as Views.EditorView;
                e.Cancel = !temp.AreUnsavedChangesHandled();
            }
        }
    }
}
