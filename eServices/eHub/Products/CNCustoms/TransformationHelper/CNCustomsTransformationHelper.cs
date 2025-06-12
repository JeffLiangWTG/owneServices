using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.CNCustoms.Transforms.Helper
{
	public class CNCustomsTransformationHelper
	{
		public static XPathNavigator DeDuplicateValues(XPathNodeIterator sourceNodeSet, string nodeName)
		{
			var result = new XElement("Values");
			var uniqueValues = new List<string>();
			while (sourceNodeSet.MoveNext())
			{
				var curr = sourceNodeSet.Current;
				var nodeOfSelection = curr.SelectSingleNode(string.Format("./*[local-name()='{0}']", nodeName));
				var readValue = nodeOfSelection == null ? string.Empty : nodeOfSelection.Value;
				if (!string.IsNullOrEmpty(readValue) && uniqueValues.All(x => x != readValue))
				{
					uniqueValues.Add(readValue);
				}
			}
			foreach (var readValue in uniqueValues)
			{
				result.Add(new XElement("Value", readValue));
			}
			return result.CreateNavigator();
		}

		public static string DeDuplicateValuesAndConcatenate(XPathNodeIterator sourceNodeSet, string seperator, int maxLength, string suffix, bool shouldOrder)
		{
			var uniqueValues = new List<string>();
			if (sourceNodeSet != null)
			{
				while (sourceNodeSet.MoveNext())
				{
					var curr = sourceNodeSet.Current;
					var nodeOfSelection = curr.SelectSingleNode(".");
					var readValue = nodeOfSelection == null ? string.Empty : nodeOfSelection.Value;
					if (!string.IsNullOrEmpty(readValue) && uniqueValues.All(x => x != readValue))
					{
						uniqueValues.Add(readValue);
					}
				}
			}
			var result = string.Join(seperator, shouldOrder ? uniqueValues.OrderBy(x => x).ToArray() : uniqueValues.ToArray());
			if (result.Length > maxLength)
			{
				do
				{
					var lastIndexOfSeperator = result.LastIndexOf(seperator);
					if (lastIndexOfSeperator == -1)
					{
						break;
					}
					result = result.Remove(lastIndexOfSeperator);
				}
				while (result.Length + suffix.Length > maxLength);

				result = result + suffix;
				if (result.Length > maxLength)
				{
					result = result.Substring(0, maxLength);
				}
			}
			return result;
		}

		public static XPathNavigator DeDuplicateCustomsSupportingInformation(XPathNodeIterator sourceNodeSet, bool onlyByTypeAndNumber)
		{
			XNamespace ns = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var result = new XElement(ns + "CustomsSupportingInformationCollection", new XAttribute(XNamespace.Xmlns + "s0", ns.NamespaceName));

			var dictionary = new Dictionary<string, CustomsSupportingInformation>();
			if (sourceNodeSet != null)
			{
				while (sourceNodeSet.MoveNext())
				{
					var cusSuppportingInfo = new CustomsSupportingInformation(sourceNodeSet.Current);
					var key = string.Join("|", cusSuppportingInfo.EntryLineNumber,
																	cusSuppportingInfo.Type,
																	cusSuppportingInfo.SubType,
																	cusSuppportingInfo.ReferenceNumber,
																	onlyByTypeAndNumber ? string.Empty : cusSuppportingInfo.LineNo,
																	onlyByTypeAndNumber ? string.Empty : cusSuppportingInfo.UnitOfQuantity);

					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, cusSuppportingInfo);
					}
					else
					{
						dictionary[key].Quantity += cusSuppportingInfo.Quantity;
					}
				}

				foreach (var cusSuppportingInfo in dictionary.Values)
				{
					result.Add(cusSuppportingInfo.ToXElement(ns));
				}
			}
			return result.CreateNavigator();
		}

		public static XPathNavigator ReorderEcoRelationNodes(XPathNodeIterator sourceNodeSet)
		{
			XNamespace ns = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var result = new XElement(ns + "CustomsSupportingInformationCollection", new XAttribute(XNamespace.Xmlns + "s0", ns.NamespaceName));

			var ecoRelationNodes = new List<EcoRelation>();
			while (sourceNodeSet.MoveNext())
			{
				ecoRelationNodes.Add(new EcoRelation(sourceNodeSet.Current));
			}

			foreach (var ecoRelationNode in ecoRelationNodes.OrderBy(x => x.CertType).ThenBy(x => x.EcoCertNo).ThenBy(x => x.DecGNo))
			{
				result.Add(ecoRelationNode.ToXElement(ns));
			}

			return result.CreateNavigator();
		}

		public static string GetNodeValue(XPathNavigator current, string nodePath)
		{
			XPathNavigator navigator = null;
			var nodeNames = nodePath.Split('/');
			if (nodeNames.Any())
			{
				navigator = current;
				foreach (var nodeName in nodeNames)
				{
					if (navigator == null)
					{
						break;
					}
					navigator = navigator.SelectSingleNode(string.Format("./*[local-name()='{0}']", nodeName));
				}
			}
			return navigator == null ? string.Empty : navigator.Value;
		}

		public static decimal GetNodeValueAsDecimal(XPathNavigator current, string nodePath)
		{
			decimal result;
			var value = GetNodeValue(current, nodePath);
			if (!decimal.TryParse(value, out result))
			{
				result = 0m;
			}
			return result;
		}

		public static XElement CreateXElement(XNamespace ns, string nodePath, object content = null)
		{
			XElement result = null;
			var nodeNames = nodePath.Split('/');
			if (nodeNames.Any())
			{
				foreach (var nodeName in nodeNames.Reverse())
				{
					if (result == null)
					{
						result = new XElement(ns + nodeName, content);
					}
					else
					{
						var child = result;
						result = new XElement(ns + nodeName);
						result.Add(child);
					}
				}
			}
			return result;
		}

		public static string GetSmallestValue(XPathNodeIterator sourceNodeSet)
		{
			var smallestValue = "";
			if (sourceNodeSet != null)
			{
				while (sourceNodeSet.MoveNext())
				{
					var curr = sourceNodeSet.Current;
					var nodeOfSelection = curr.SelectSingleNode(".");
					var readValue = nodeOfSelection == null ? string.Empty : nodeOfSelection.Value;
					if (string.IsNullOrEmpty(smallestValue) || string.Compare(readValue, smallestValue) < 0)
					{
						smallestValue = readValue;
					}
				}
			}
			return smallestValue;
		}

		public static decimal Sum(XPathNodeIterator sourceNodeSet, int decimals, decimal minValue)
		{
			var sum = (decimal)sourceNodeSet.Cast<XPathNavigator>().Sum(x => x.ValueAsDouble);
			return Math.Max(minValue, decimal.Round(sum, decimals));
		}

		public static string CreateZippedMessage(XPathNodeIterator sourceNodeSet, string filename)
		{
			var result = string.Empty;

			if (sourceNodeSet.MoveNext())
			{
				var current = sourceNodeSet.Current;
				var root = XElement.Parse(current.OuterXml);

				XmlNamespaceManager nsmgr = new XmlNamespaceManager(current.NameTable);
				nsmgr.AddNamespace("gmi", "http://cargowise.com/ehub/core/genericmessagedelivery");
				nsmgr.AddNamespace("ns0", "http://www.chinaport.gov.cn/dec");

				var decElement = root.XPathSelectElement("./ns0:DecMessage", nsmgr);
				if (decElement != null)
				{
					var attachedDocuments = root.XPathSelectElements("./AttachedDocumentCollection/AttachedDocument", nsmgr);
					result = CreateZippedMessageText(decElement, attachedDocuments, filename);
				}
			}

			return result;
		}

		static string CreateZippedMessageText(XElement decMessage, IEnumerable<XElement> attachedDocuments, string filename)
		{
			string result;
			using (var zipStream = new MemoryStream())
			{
				using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
				{
					var decZipEntry = zipArchive.CreateEntry(filename + ".xml");
					using (var decWriter = new StreamWriter(decZipEntry.Open()))
					using (var xmlWriter = XmlWriter.Create(decWriter, new XmlWriterSettings() { Indent = true }))
					{
						var element = XElement.Parse(decMessage.ToString());
						element.Attributes().Where(e => e.IsNamespaceDeclaration).Remove();
						element.Save(xmlWriter);
					}

					foreach (var attachedDocument in attachedDocuments)
					{
						var fileName = attachedDocument.XPathSelectElement("FileName").Value;
						var imageData = attachedDocument.XPathSelectElement("ImageData").Value;
						var attachedDocumentZipEntry = zipArchive.CreateEntry(fileName);
						using (var attachedDocumentWriter = new BinaryWriter(attachedDocumentZipEntry.Open()))
						{
							attachedDocumentWriter.Write(Convert.FromBase64String(imageData));
						}
					}
				}

				result = Convert.ToBase64String(zipStream.ToArray());
			}
			return result;
		}

		#region Extracting from fileName

		string GetEventTimeFromFileName(string fileName)
		{
			var strs = fileName.Split('_');
			return strs.Length > 2 ? strs.Last() : string.Empty;
		}

		public string ExtractEventTime(string fileName)
		{
			var inputFormat = "yyyyMMddHHmmssfffffff";
			var inputFormatLength = inputFormat.Length;

			var eventTimeStr = GetEventTimeFromFileName(fileName).PadRight(inputFormatLength, '0').Substring(0, inputFormatLength);

			System.DateTime eventTime;

			if (!System.DateTime.TryParseExact(eventTimeStr, inputFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out eventTime))
			{
				eventTime = DateTime.UtcNow.AddHours(8);
			}

			return string.Format("{0:yyyy-MM-ddTHH:mm:ss.fffffff}", eventTime);
		}

		public string ExtractLocalReferenceNumber(string fileName)
		{
			var strs = fileName.Split('_');
			return strs.Length > 1 ? strs.ElementAt(1) : string.Empty;
		}

		#endregion
	}

	public class CustomsSupportingInformation
	{
		public string Type { get; set; }
		public string SubType { get; set; }
		public string ReferenceNumber { get; set; }
		public decimal Quantity { get; set; }
		public string LineNo { get; set; }
		public string UnitOfQuantity { get; set; }
		public string EntryLineNumber { get; set; }

		public CustomsSupportingInformation(XPathNavigator sourceNode)
		{
			Type = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "Type/Code");
			SubType = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "SubType/Code");
			ReferenceNumber = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "ReferenceNumber");
			LineNo = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "LineNo");
			UnitOfQuantity = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "UnitOfQuantity/Code");
			Quantity = CNCustomsTransformationHelper.GetNodeValueAsDecimal(sourceNode, "Quantity");
			EntryLineNumber = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "EntryLineNumber");
		}

		public XElement ToXElement(XNamespace ns)
		{
			var element = CNCustomsTransformationHelper.CreateXElement(ns, "CustomsSupportingInformation");
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "Type/Code", Type));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "SubType/Code", SubType));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "ReferenceNumber", ReferenceNumber));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "LineNo", LineNo));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "UnitOfQuantity/Code", UnitOfQuantity));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "Quantity", Quantity));

			if (!string.IsNullOrEmpty(EntryLineNumber))
			{
				element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "EntryLineNumber", EntryLineNumber));
			}

			return element;
		}
	}

	public class EcoRelation
	{
		public string CertType { get; set; }
		public string EcoCertNo { get; set; }
		public string DecGNo { get; set; }
		public string EcoGNo { get; set; }

		public EcoRelation(XPathNavigator sourceNode)
		{
			CertType = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "CertType");
			EcoCertNo = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "EcoCertNo");
			DecGNo = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "DecGNo");
			EcoGNo = CNCustomsTransformationHelper.GetNodeValue(sourceNode, "EcoGNo");
		}

		public XElement ToXElement(XNamespace ns)
		{
			var element = CNCustomsTransformationHelper.CreateXElement(ns, "EcoRelationNode");
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "CertType", CertType));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "EcoCertNo", EcoCertNo));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "DecGNo", DecGNo));
			element.Add(CNCustomsTransformationHelper.CreateXElement(ns, "EcoGNo", EcoGNo));

			return element;
		}
	}
}




