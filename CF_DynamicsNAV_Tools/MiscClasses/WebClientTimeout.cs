using System;
using System.Text;
using System.Net;

namespace CF_DynamicsNAV_Tools
{
    public class WebClientTimeOut : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = TimeOut;
            return webRequest;
        }
        public int TimeOut { set; get; }
    }
}
