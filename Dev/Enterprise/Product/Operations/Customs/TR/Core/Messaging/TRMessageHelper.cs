using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace Enterprise.Customs.TR.Messaging
{
	public static class TRMessageHelper
	{
		public static ZString GetNodeXml(ZString messageText, ZString nodeName)
		{
			var result = ZString.Empty;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
				xnm.AddNamespace((NoResString)"x", XmlNamespace);
				var node = xmlDocument.SelectSingleNode(nodeName, xnm);
				result = node?.OuterXml;
			}
			return result;
		}

		public static ZString GetNodeValue(ZString messageText, ZString nodeName, string xmlNamespace = XmlNamespace)
		{
			var result = ZString.Empty;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
				xnm.AddNamespace((NoResString)"x", xmlNamespace);
				var node = xmlDocument.SelectSingleNode(nodeName, xnm);
				result = node?.InnerText;
			}
			return result;
		}

		public static ZString GetNodeValue(ZString messageText, IEnumerable<ZString> nodeNameList, ZString innerNodeName)
		{
			var nodeList = GetInnerNode(messageText, nodeNameList, innerNodeName);
			var node = ZString.Empty;
			if (nodeList != null)
			{
				node = nodeList[0]?.InnerText;
			}
			return node;
		}

		public static ZString GetNodeValue(XmlDocument xmlDocument, IEnumerable<ZString> nodeNameList, ZString innerNodeName)
		{
			var nodeList = GetInnerNode(nodeNameList, innerNodeName, xmlDocument);
			var node = ZString.Empty;
			if (nodeList != null)
			{
				node = nodeList[0]?.InnerText;
			}
			return node;
		}

		public static ZString GetNodeValue(ZString messageText, ZString nodeName, ZString[] namespaceList)
		{
			var result = ZString.Empty;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
				for (int i = 0; i < namespaceList.Length; i++)
				{
					xnm.AddNamespace(ZString.AlphabeticCharacters.ElementAt(i).ToString(), namespaceList.ElementAt(i));
				}
				var node = xmlDocument.SelectSingleNode(nodeName, xnm);
				result = node?.InnerText;
			}
			return result;
		}

		public static IEnumerable<string> GetNodeValues(ZString messageText, IEnumerable<ZString> parentNodeNames, ZString nodeName)
		{
			var result = new List<string>();

			var nodes = GetInnerNode(messageText, parentNodeNames, nodeName).Cast<XmlNode>();
			foreach (var node in nodes)
			{
				result.Add(node.FirstChild.Value);
			}

			return result;
		}

		static XmlNodeList GetInnerNode(ZString messageText, IEnumerable<ZString> nodeNameList, ZString innerNodeName)
		{
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				return GetInnerNode(nodeNameList, innerNodeName, xmlDocument);
			}
			return null;
		}

		static XmlNodeList GetInnerNode(IEnumerable<ZString> nodeNameList, ZString innerNodeName, XmlDocument xmlDocument)
		{
			var nodeName = "";
			foreach (var item in nodeNameList)
			{
				nodeName += BuildNodeText(item);
			}
			if (!innerNodeName.IsEmpty)
			{
				nodeName += BuildNodeText(innerNodeName);
			}
			var node = xmlDocument.SelectNodes(nodeName);
			return node;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public static List<Tuple<string, string>> GetMultipleNodeValueList(ZString messageText, IEnumerable<ZString> parentNodeNameList, ZString codeName, ZString descriptionName)
		{
			var innernodeList = GetInnerNode(messageText, parentNodeNameList, "");
			XmlNode innernodes = null;
			if (innernodeList != null)
			{
				innernodes = innernodeList[0];
			}
			var valueList = new List<Tuple<string, string>>();
			if (innernodes != null)
			{
				foreach (XmlNode item in innernodes.ChildNodes)
				{
					if (item.LocalName != "#comment")
					{
						var code = codeName.IsEmpty ? ZString.Empty : GetNodeValue(messageText, parentNodeNameList, codeName);
						var description = descriptionName.IsEmpty ? ZString.Empty : GetNodeValue(messageText, parentNodeNameList, descriptionName);

						if (!code.IsEmpty || !description.IsEmpty)
						{
							valueList.Add(new Tuple<string, string>(code, description));
						}
					}
				}
			}
			return valueList;
		}

		public static List<string[]> GetChildNodesFromElement(ZString messageText, string element, params string[] nodeList)
		{
			var results = new List<string[]>();

			if (nodeList != null && nodeList.Any())
			{
				if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
				{
					var xnList = xmlDocument.SelectNodes(string.Format(CultureInfo.InvariantCulture, "//{0}", element));

					foreach (XmlNode xn in xnList)
					{
						var nodeData = new List<string>();
						foreach (var nodeName in nodeList)
						{
							var nodeText = xn.SelectSingleNode(nodeName)?.InnerText ?? string.Empty;
							nodeData.Add(nodeText);
						}
						results.Add(nodeData.ToArray());
					}
				}
			}
			return results;
		}

		public static List<Tuple<string, string>> GetMultipleNodeValueListForQueryRemainingBills(ZString messageText)
		{
			var valueList = new List<Tuple<string, string>>();
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var xnList = xmlDocument.SelectNodes("//AyrilanTasimaSenedi");
				foreach (XmlNode xn in xnList)
				{
					string blllno = xn.SelectSingleNode("tasimaSenediNumarasi").InnerText;
					string reason = xn.SelectSingleNode("ayrilmaSebebi").InnerText;
					valueList.Add(new Tuple<string, string>(blllno, reason));
				}
			}
			return valueList;
		}

		public static List<string> GetMultipleNodeValueList(ZString messageText, List<ZString> parentNodeNameList)
		{
			var innernodes = GetInnerNode(messageText, parentNodeNameList, "");
			var valueList = new List<string>();
			if (innernodes != null)
			{
				foreach (XmlNode item in innernodes)
				{
					ZString innerText = item.InnerText;
					if (!innerText.IsEmpty)
					{
						valueList.Add(innerText);
					}
				}
			}
			return valueList;
		}

		const string XmlNamespace = "http://www.gumruk.gov.tr/";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		static string BuildNodeText(string nodeName) => "/*[local-name() = '" + nodeName + "']";

		public static string CleanXML(string str) => str.Replace("\n", "").Replace("\r", "").Trim();

		public static XmlNode GetXmlNode(ZString messageText, IEnumerable<ZString> nodeNameSequence)
		{
			XmlNode result = null;
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var nodePath = nodeNameSequence.Aggregate(ZString.Empty, (accumulator, next) => accumulator + BuildNodeText(next));
				result = xmlDocument.SelectSingleNode(nodePath);
			}
			return result;
		}

		public static bool TryGetCredentialErrorMessage(ZString messageText, out ZString credentialMessageText)
		{
			credentialMessageText = TRMessageHelper.GetNodeValue(messageText, $"/*[local-name()='Root']/*[local-name()='Error']/*[local-name()='Message']").Trim();
			return !credentialMessageText.IsEmpty;
		}

		public static ZString SerializeSOAPMessage(Envelope envelope, XmlWriterSettings settings, XmlSerializerNamespaces ns)
		{
			ZString result;
			using (var stream = new MemoryStream())
			{
				using (var xmlWriter = XmlWriter.Create(stream, settings))
				{
					new EnvelopeSerializer().Serialize(xmlWriter, envelope, ns);
				}

				stream.Position = 0;

				using (var streamReader = new StreamReader(stream))
				{
					result = streamReader.ReadToEnd();
					streamReader.Close();
				}

				stream.Close();
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public static XmlNode GetXmlNodeFromPath(ZString messageText, ZString xpath)
		{
			if (XmlUtils.IsValidXml(messageText, out var xmlDocument))
			{
				var xnm = new XmlNamespaceManager(xmlDocument.NameTable);
				xnm.AddNamespace("x", XmlNamespace);
				return xmlDocument.SelectSingleNode(xpath, xnm);
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static List<Tuple<string, string>> GetMultipleNodeValueList(ZString messageText, IEnumerable<ZString> parentNodeNameList, (string codeElementName, string descriptionElementName) elementNames)
		{
			var result = new List<Tuple<string, string>>();

			var innerNodeList = GetInnerNode(messageText, parentNodeNameList, "");

			if (innerNodeList != null)
			{
				foreach (var innerNode in innerNodeList.Cast<XmlNode>().Where(node => node.LocalName != "#comment"))
				{
					result.Add(new Tuple<string, string>(
						innerNode[elementNames.codeElementName]?.InnerText ?? string.Empty,
						innerNode[elementNames.descriptionElementName]?.InnerText ?? string.Empty
					));
				}
			}

			return result;
		}

		public static ZString GenerateXml(ZString applicationReference, ZString userId, ZString userPassword, ZString xmlContent)
		{
			var result = ZString.Empty;
			StringBuilder sb = new StringBuilder();

			sb.Append((NoResString)"<Root>");
			sb.Append("<RefID>");
			sb.Append(applicationReference);
			sb.Append("</RefID>");
			sb.Append("<KullaniciAdi>");
			sb.Append(userId);
			sb.Append("</KullaniciAdi>");
			sb.Append((NoResString)"<Sifre>");
			sb.Append(userPassword);
			sb.Append((NoResString)"</Sifre>");
			sb.Append("<RequestMessage>");
			sb.Append(xmlContent);
			sb.Append("</RequestMessage>");
			sb.Append((NoResString)"</Root>");

			try
			{
				result = ToStringWithUTF8(XDocument.Parse(sb.ToString()));
			}
			catch (Exception)
			{
				result = sb.ToString();
			}

			return result;
		}

		static string ToStringWithUTF8(XDocument xmlDoc)
		{
			using (var writer = new StringWriterWithUtf8Encoding())
			{
				xmlDoc.Save(writer);
				return writer.ToString();
			}
		}

		class StringWriterWithUtf8Encoding : StringWriter
		{
			public StringWriterWithUtf8Encoding() : base(new StringBuilder(), CultureInfo.InvariantCulture) { }

			public override Encoding Encoding => Encoding.UTF8;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static List<Tuple<string, string, string, string>> GetMultipleNodeValueList(ZString messageText, IEnumerable<ZString> parentNodeNameList, (string field1, string field2, string field3, string field4) elementNames)
		{
			var result = new List<Tuple<string, string, string, string>>();

			var innerNodeList = GetInnerNode(messageText, parentNodeNameList, "");

			if (innerNodeList != null)
			{
				foreach (var innerNode in innerNodeList.Cast<XmlNode>().Where(node => node.LocalName != "#comment"))
				{
					result.Add(new Tuple<string, string, string, string>(
						innerNode[elementNames.field1]?.InnerText ?? string.Empty,
						innerNode[elementNames.field2]?.InnerText ?? string.Empty,
						innerNode[elementNames.field3]?.InnerText ?? string.Empty,
						innerNode[elementNames.field4]?.InnerText ?? string.Empty
					));
				}
			}

			return result;
		}

		public static ZString RemoveCountryCodePrefix(ZString customsOffice)
		{
			var result = customsOffice;

			if (!customsOffice.IsEmpty && customsOffice.StartsWith(Constants.CountryCodes.Turkey))
			{
				result = customsOffice.SubstringSafe(2);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static List<List<string>> GetMultipleNodeDoubleDimensionList(ZString messageText, IEnumerable<ZString> parentNodeNameList, IEnumerable<ZString> fieldList)
		{
			List<List<string>> lists = new List<List<string>>();
			var innerNodeList = GetInnerNode(messageText, parentNodeNameList, "");

			if (innerNodeList != null)
			{
				foreach (var innerNode in innerNodeList.Cast<XmlNode>().Where(node => node.LocalName != "#comment"))
				{
					var newList = new List<string>();
					foreach (var fieldName in fieldList)
					{
						newList.Add(innerNode[fieldName]?.InnerText ?? string.Empty);
					}
					lists.Add(newList);
				}
			}

			return lists;
		}

		public static ZString GetAlreadyRegisteredDischargeListNo(ZString xmlData)
		{
			var pattern = $"(?<={patternText1} )(.*)(?= {patternText2})";

			return Regex.Match(xmlData, pattern)?.Value ?? ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Pattern string")]
		const string patternText1 = "beyanname numarası için daha önceden";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Pattern string")]
		const string patternText2 = "numarası ile boşaltma listesi tescil edilmiştir!";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html encoding")]
		public static ZString DecodeHtmlIfNecessary(string innerXml)
		{
			string[] htmlEntities = { "&lt;", "&gt;", "&amp;", "&quot;", "&apos;" };
			return htmlEntities.Any(entity => innerXml.Contains(entity)) ? WebUtility.HtmlDecode(innerXml) : innerXml;
		}
	}
}
