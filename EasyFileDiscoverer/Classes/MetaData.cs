using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyFileDiscoverer 
{
  public  class MetaData
    {
        public DateTime fileCreation { get; set; }
        public DateTime fileModified { get; set; }
        public string fileName { get; set; }
        public string fileLoc { get; set; }
        public string fileLength { get; set; }
        public string fileType { get; set; }
    }
}

