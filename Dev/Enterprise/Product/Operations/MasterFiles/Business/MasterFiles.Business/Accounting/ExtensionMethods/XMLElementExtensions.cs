using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class XMLElementExtensions
	{
		public static XmlNode GetSingleNode(this TextReader reader, string xpath, params (string prefix, string uri)[] namespaces)
		{
			XmlNode result = null;
			if (XmlUtils.IsValidXml(reader, out var xmlDocument))
			{
				var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
				namespaces.ForEach(d => nsmgr.AddNamespace(d.prefix, d.uri));
				result = xmlDocument.SelectSingleNode(xpath, nsmgr);
			}
			return result;
		}

		public static string GetNodeValue(this XElement xElement, string nodeName, bool includeChildrenOfChildrenInSearch = false)
		{
			if (xElement != null)
			{
				var node = includeChildrenOfChildrenInSearch ? xElement.Descendants(nodeName).FirstOrDefault() : xElement.Element(nodeName);
				if (node != null)
				{
					return node.Value;
				}
			}

			return "";
		}

		public static string GetAttributeValue(this XElement xElement, string attributeName)
		{
			if (xElement != null)
			{
				var attribute = xElement.Attribute(attributeName);
				if (attribute != null)
				{
					return attribute.Value;
				}
			}

			return "";
		}

		public static bool TryToSetValueFromXElelment<T>(this XElement xElement, string nodeName, Action<T> setValue)
			where T : IZType
		{
			return SetValueFromXML(GetNodeValue(xElement, nodeName), setValue);
		}

		public static bool TryToSetValueFromXAttribute<T>(this XElement xElement, string attributeName, Action<T> setValue)
			where T : IZType
		{
			return SetValueFromXML(GetAttributeValue(xElement, attributeName), setValue);
		}

		static bool SetValueFromXML<T>(string xmlValue, Action<T> setValue)
		{
			IZType castValue = null;
			var ableToParse = false;
			if (typeof(T) == typeof(ZDateTime) && ZDateTime.TryParseISO8601Date(xmlValue, out ZDateTime dateValue))
			{
				castValue = dateValue;
				ableToParse = true;
			}
			else if (typeof(T) == typeof(ZString) && !string.IsNullOrEmpty(xmlValue))
			{
				castValue = (ZString)xmlValue;
				ableToParse = true;
			}
			else if (typeof(T) == typeof(ZGuid) && ZGuid.TryParse(xmlValue, out ZGuid guidValue))
			{
				castValue = guidValue;
				ableToParse = true;
			}
			else if (typeof(T) == typeof(ZDecimal) && ZDecimal.TryParse(xmlValue, out ZDecimal decimalValue))
			{
				castValue = decimalValue;
				ableToParse = true;
			}
			else if (typeof(T) == typeof(ZByte) && ZByte.TryParse(xmlValue, out ZByte byteValue))
			{
				castValue = byteValue;
				ableToParse = true;
			}
			else if (typeof(T) == typeof(ZBool) && ZBool.TryParse(xmlValue, out ZBool boolValue))
			{
				castValue = boolValue;
				ableToParse = true;
			}

			if (ableToParse)
			{
				setValue((T)castValue);
			}

			return ableToParse;
		}
	}
}
