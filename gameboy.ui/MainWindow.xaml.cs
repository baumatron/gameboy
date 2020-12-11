using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GameBoy;

namespace GameBoy.ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void InitializeGameboy()
        {

        }

        public void UpdateFrame()
        {
            // Is this what I need? https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.bitmapsource?view=netcore-3.1
            // Maybe compositor should just fill a simple buffer and then this should do the windows specific image conversion to present it?
            // Or is this all wrong and I just need to figure out how to render this using direct3d and a swap chain panel?
            display.Source = gameboy._compositor.GrabFrame();
        }

        GameBoy gameboy = new GameBoy();
    }
}
