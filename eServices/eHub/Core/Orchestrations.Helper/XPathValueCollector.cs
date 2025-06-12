using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using System.Xml;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	public class XPathValueCollector
	{
		public XPathValueCollector(params string[] xPaths)
		{
			this.xPaths = xPaths;
		}

		public List<Tuple<int, string, string>> Collect(string input)
		{
			var bytes = Encoding.UTF8.GetBytes(input);

			using (var virtualStream = new VirtualStream())
			{
				virtualStream.Write(bytes, 0, bytes.Length);
				return Collect(virtualStream);
			}
		}

		public List<Tuple<int, string, string>> Collect(VirtualStream virtualStream)
		{
			virtualStream.Position = 0;
			XPathCollection xPathCollection = new XPathCollection();
			foreach (var xPath in xPaths) xPathCollection.Add(new XPathExpression(xPath));
			ValueMutator valueMutator = new ValueMutator(this.CallBack);
			var xPathMutatorStream = new XPathMutatorStream(virtualStream, xPathCollection, valueMutator);
			using (var reader = new XmlTextReader(xPathMutatorStream)) while (reader.Read()) ;
			return result;
		}

		void CallBack(int matchIdx, XPathExpression matchExpr, string origVal, ref string finalVal)
		{
			result.Add(new Tuple<int, string, string>(matchIdx, matchExpr.ToString(), origVal));
		}

		readonly string[] xPaths;
		List<Tuple<int, string, string>> result = new List<Tuple<int, string, string>>();
	}
}
