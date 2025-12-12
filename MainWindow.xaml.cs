using System.Windows;
using QualisulCameraApp.ViewModels;

namespace QualisulCameraApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext is already set in XAML, but this is fine too if we wanted DI later.
            // But wait, in XAML I did <vm:MainViewModel/> so it instantiates a new one.
            // If I want to access it here I should cast.
        }
    }
}