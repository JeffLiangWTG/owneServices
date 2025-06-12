using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Resources;
using System.IO;
using System.Reflection;

namespace eservices.ehub.products.OceanCarrierMessaging
{
    public class APERAK2UInterchangeInclude
    {
        const string userCSharpNamespaceName = "http://schemas.microsoft.com/BizTalk/2003/userCSharp";
        const string XslContentResourceFilePath = "APERAK2UInterchangeInclude.xsl";

        Stream xslContentStream;
        public Stream XslContent
        {
            get
            {
                if (xslContentStream == null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    xslContentStream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + XslContentResourceFilePath);
                }

                return xslContentStream;
            }
        }

        public string UserCSharpNamespaceName => userCSharpNamespaceName;

        public string ConvertDateToString(string dateValue)
        {
            return DateTime.ParseExact(dateValue, new[] { "yyyyMMdd", "yyyyMMddHHmm" }, null, System.Globalization.DateTimeStyles.None).ToString("s");
        }

        public string GetDocumentName(string input)
        {
            switch (input)
            {
                case "IFTMIN":
                    return "Shipping Instruction";

                case "IFTMBF":
                    return "Booking Request";

                case "VERMAS":
                    return "Verified Gross Container Weight";

                default:
                    return "";
            }
        }

        public string GetResponse(string input)
        {
            switch (input)
            {
                case "ACCEPTED":
                case "A":
                    return "ACCEPTED";

                case "R":
                    return "REJECTED";

                case "E":
                    return "ACCEPTED IN INTERCHANGE LEVEL WITH ERRORS/WARNING";

                default:
                    return "";
            }
        }

        public string GetReferenceType(string input)
        {
            switch (input)
            {
                case "BN":
                    return "Carrier Booking Number";

                case "BM":
                    return "Bill of lading number";

                case "FF":
                    return "Freight forwarder's reference number";

                default:
                    return "";
            }
        }
    }
}