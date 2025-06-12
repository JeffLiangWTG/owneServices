using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	abstract class PromotedValue
	{
		public string Find(IBaseMessage message, string propertyName)
		{
			return Find(message.BodyPart.GetOriginalDataStream(), propertyName);
		}

		public string Find(Stream messageStream, string propertyName)
		{
			return FindByXPath(messageStream, promotedValueCollection[propertyName]);
		}

		public static string FindByXPath(Stream messageStream, string xPath)
		{
			string value = null;
			try
			{
				messageStream.Position = 0;
				var xnav = new XPathDocument(new XmlTextReader(messageStream)).CreateNavigator();
				var node = xnav.SelectSingleNode(xPath);
				if (node != null)
					value = node.TypedValue as string;
				messageStream.Position = 0;
			}
			catch { }

			return value;
		}

		#region Implementation

		readonly Dictionary<string, string> promotedValueCollection;

		protected PromotedValue()
		{
			promotedValueCollection = new Dictionary<string, string>();
		}

		protected void Add(string value, string xPath)
		{
			promotedValueCollection.Add(value, xPath);
		}

		#endregion
	}
}
