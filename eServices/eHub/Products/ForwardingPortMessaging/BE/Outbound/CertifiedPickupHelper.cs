using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    public static class CertifiedPickupHelper
    {
        public static string GetCertifiedPickupBodyJsonString(XmlDocument outboxMessage)
        {
            var bodyXmlList = outboxMessage.GetElementsByTagName("Body");
            if (bodyXmlList.Count == 0)
            {
                throw new ArgumentException("outboxMessage must contain body element");
            }
            var bodyXml = bodyXmlList[0];
            var jsonString = JsonConvert.SerializeXmlNode(
                bodyXml,
                Newtonsoft.Json.Formatting.None,
                true);
            return jsonString;
        }

		public static int ConvertToIntWithDefaultValue(string originalString, int defaultValue)
		{
			int number;
			if (int.TryParse(originalString, out number))
			{
				return number;
			}
			return defaultValue;
		}
    }
}
