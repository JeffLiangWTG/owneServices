using System;

namespace Hawking.CSI.Utilities
{
    public class UriInfo
    {
        public UriInfo(string uriString)
        {
            var uri = new Uri(uriString);
            User = Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]);
            Password = Uri.UnescapeDataString(uri.UserInfo.Split(':')[1]);
            Host = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            if (int.TryParse(uri.GetComponents(UriComponents.Port, UriFormat.Unescaped), out int port))
                Port = port;
            Path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
        }

        public string Schema { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
    }


}