using System;
using System.Collections.Generic;
using System.Text;

namespace EAPClientManager
{
    internal class GetConfigOutput
    {
        public string EquipmentName;
        public string ModelName;
        public string WorkPath;
        public string SummaryFilePath;
        //public string ManagerURI = "http://172.20.97.207";
        public string ManagerURI = "http://172.16.204.220";
        public DateTime dateTime = DateTime.Now;
    }
}
