using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using System.Xml;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	public class XPathValueUpdaterStatic
	{
		public static XmlDocument Update(XmlDocument xmlDocument, string xPath, string value)
		{
			return new XPathValueUpdater(xmlDocument, xPath, value).Update();
		}
	}

	public class XPathValueUpdater
	{
		public XPathValueUpdater(XmlDocument xmlDocument, string xPath, string value)
		{
			this.xmlDocument = xmlDocument;
			this.xPath = xPath;
			this.value = value;
		}

		readonly XmlDocument xmlDocument;
		readonly string xPath;
		readonly string value;

		public XmlDocument Update()
		{
			using (VirtualStream virtualStream = new VirtualStream())
			{
				xmlDocument.Save(virtualStream);
				virtualStream.Position = 0;
				var result = new XmlDocument();
				result.Load(new XPathMutatorStream(virtualStream, new XPathCollection() { new XPathExpression(xPath) }, new ValueMutator(this.SetValue)));
				return result;
			}
		}

		void SetValue(int matchIdx, XPathExpression matchExpr, string origVal, ref string finalVal)
		{
			finalVal = value;
		}
	}
}
