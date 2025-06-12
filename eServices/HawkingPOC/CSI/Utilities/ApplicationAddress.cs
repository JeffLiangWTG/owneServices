using System;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Utilities
{
    public class ApplicationAddress
    {
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; } = "latest";

        public override string ToString()
        {
            return $"{ApplicationName}:{ApplicationVersion}";
        }

        public static implicit operator StringValues(ApplicationAddress address)
        {
            return new StringValues(address.ToString());
        }

        public static ApplicationAddress Parse(string address)
        {
            var splitPos = address.IndexOf(':');
            if (splitPos >= 0)
            {
                return new ApplicationAddress
                {
                    ApplicationName = address.Remove(splitPos),
                    ApplicationVersion = address.Substring(splitPos + 1)
                };
            }
            else
            {
                return new ApplicationAddress
                {
                    ApplicationName = address
                };
            }
        }
    }
}
