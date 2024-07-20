using SharpDX.Direct3D11;
using System.Windows;
using WledLightbox.Render;

namespace WledLightbox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        var mainVm = new MainViewModel() { };
        DataContext = mainVm;
        InitializeComponent();


        Content = new Sample2DRenderer(mainVm.Capture);

        //renderBox.Caputre = mainVm.Capture;
        //Capture = new DesktopDuplicator();

    }
}
