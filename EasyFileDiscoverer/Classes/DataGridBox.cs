using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EasyFileDiscoverer
{
   public class DataGridBox
    {
        public BitmapSource fileitemIcon { get; set; }
        public string fileitemText { get; set; }
        public string fileitemPath { get; internal set; }

    }
}