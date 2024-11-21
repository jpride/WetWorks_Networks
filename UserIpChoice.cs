using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WetWorks_NetWorks
{
    /// <summary>
    /// A Class to represent a User IP Scheme choice button on the UI
    /// </summary>
     class UserIpChoice
    {
        public string UiContent {  get; set; }
        public string IpAddress { get; set; }
        public string NetShellString { get; set; }

        public UserIpChoice(string content, string ipAddress, string netShellString)
        {
            UiContent = content;
            IpAddress = ipAddress;
            NetShellString = netShellString;
        }
    }
}
