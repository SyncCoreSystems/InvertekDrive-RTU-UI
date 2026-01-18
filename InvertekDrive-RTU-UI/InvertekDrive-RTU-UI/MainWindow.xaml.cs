using System.Windows;
using InvertekDrive_RTU_UI.ViewModel;

namespace InvertekDrive_RTU_UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainViewModel();
    }
}