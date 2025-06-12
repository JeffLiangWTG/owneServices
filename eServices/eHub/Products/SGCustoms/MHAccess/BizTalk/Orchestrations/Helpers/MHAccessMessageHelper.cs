using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class MHAccessMessageHelper
	{
		public static void AppendEdifactFlatFileContent(string accountID, string senderID, string recipientID, string UNBApplicationRef, string edifactFlatFileContent, string messageTrackingID, ref string mHAccessMessage)
		{
			if (string.IsNullOrEmpty(edifactFlatFileContent)) return;

			var interchangeSender = accountID.Substring(0, 4) + "." + accountID;
			var interchangeRecipient = (recipientID == "SGCustomsTest") ? "PRET1.PRET001" : "PRE1.PRE1001";
			var UNB = System.String.Format(@"UNB+UNOA:2+{0}:ZZ+{1}:ZZ+20${{DateTimeAndInterchangeControlref}}++{2}'", interchangeSender, interchangeRecipient, UNBApplicationRef);
			mHAccessMessage += "UNA:+.? '" + System.Text.RegularExpressions.Regex.Replace(edifactFlatFileContent, @"^UNB\+UNOB\:1\+BTS\-SENDER\:ZZZ\+RECEIVE-PARTNER\:ZZZ\+(?<DateTimeAndInterchangeControlref>\d{6}\:\d{4}\+\d{1,8})(\+\+\+\+0\+\+0)?\'", UNB);
		}

        public static XmlDocument SortAIRAEPMessages(XmlDocument receivedMessages)
        {
            XDocument xDoc;
            using (var nodeReader = new XmlNodeReader(receivedMessages))
            {
                nodeReader.MoveToContent();
                xDoc = XDocument.Load(nodeReader);
            }

			XNamespace ns0 = "http://cargowise.com/ehub/products/sgcustoms/MHAccessGateway";
			XNamespace ns1 = "http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09";

			var xElementOriginal = xDoc.Element(ns0 + "retrieveMessagesResponse").Element(ns1 + "response").Element(ns1 + "messages");
			XElement xElementSorted = new XElement(ns1 + "messages",
                                from message in xDoc.Element(ns0 + "retrieveMessagesResponse").Element(ns1 + "response").Element(ns1 + "messages").Elements(ns1 + "message")
                                let index = (int)message.Value.IndexOf("GIR+02")
                                orderby index
                                select message);
            xElementOriginal.ReplaceWith(xElementSorted);

            var xmlSortedDocument = new XmlDocument();
            using (var xmlReader = xDoc.CreateReader())
            {
                xmlSortedDocument.Load(xmlReader);
            }
            return xmlSortedDocument;
        }

    }
}