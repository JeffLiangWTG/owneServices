using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public static class PuescDictionaryStrings
	{
		// xml element names are dependant on xml document, but attributes are always the same
		// we do not need the names of the xml element, only its attributes
		// attributes to read :
		public const string Code = "code";
		public const string CountryCode = "countryCode"; // data for CL008AES has country code in the "countryCode" attribute
		public const string Description = "description";
		public const string DescriptionEng = "descriptionEng";
		public const string ValidTo = "validTo";
		public const string ValidFrom = "validFrom";
	}

	public static class PuescDictionaryXmlParser
	{
		static bool isRoot = true;
		static string dictionaryIndex = string.Empty;

		public static List<PuescBasedDictionaryElement> ParseXml(XmlTextReader xmlDoc)
		{
			var retv = new List<PuescBasedDictionaryElement>();
			isRoot = true;

			while (xmlDoc.Read())
			{
				switch (xmlDoc.NodeType)
				{
					case XmlNodeType.Element:
						OnXmlElement(xmlDoc, retv);
						break;

					case XmlNodeType.XmlDeclaration:
					case XmlNodeType.EndElement:
					case XmlNodeType.Text:
					case XmlNodeType.Comment:
					default:
						// do nothing
						break;
				}
			}

			xmlDoc.Close();
			return retv;
		}

		static void OnXmlElement(XmlTextReader xmlDoc, List<PuescBasedDictionaryElement> data)
		{
			var element = new PuescBasedDictionaryElement();
			element.DictionaryIndex = dictionaryIndex;

			while (xmlDoc.MoveToNextAttribute()) // Read the attributes.
			{
				OnXmlElementAttribute(xmlDoc.Name, xmlDoc.Value, element);
			}

			if (false == isRoot)
			{
				data.Add(element);
			}
			else // root element handled
			{
				isRoot = false;
			}
		}

		static void OnXmlElementAttribute(string name, string value, PuescBasedDictionaryElement element)
		{
			switch (name)
			{
				case PuescDictionaryStrings.Code:
				case PuescDictionaryStrings.CountryCode:
					OnCodeAttribute(value, element);
					break;
				case PuescDictionaryStrings.Description:
					OnDescriptionAttribute(value, element);
					break;
				case PuescDictionaryStrings.DescriptionEng:
					OnDescriptionEngAttribute(value, element);
					break;
				case PuescDictionaryStrings.ValidTo:
					OnValidToAttribute(value, element);
					break;
				case PuescDictionaryStrings.ValidFrom:
					OnValidFromAttribute(value, element);
					break;
			}
		}

		static void OnCodeAttribute(string value, PuescBasedDictionaryElement element)
		{
			if (!isRoot)
			{
				element.Code = value;
			}
			else
			{
				dictionaryIndex = value;
			}
		}

		static void OnDescriptionAttribute(string value, PuescBasedDictionaryElement element)
		{
			element.Description = value;
		}

		static void OnDescriptionEngAttribute(string value, PuescBasedDictionaryElement element)
		{
			element.DescriptionEng = value;
		}

		static void OnValidToAttribute(string value, PuescBasedDictionaryElement element)
		{
			element.ValidTo = ConvertPuescStringDateTimeToDateTime(value);
		}

		static void OnValidFromAttribute(string value, PuescBasedDictionaryElement element)
		{
			element.ValidFrom = ConvertPuescStringDateTimeToDateTime(value);
		}

		static DateTime ConvertPuescStringDateTimeToDateTime(string value)
		{
			var retv = DateTime.Parse(value, CultureInfo.InvariantCulture);

			return DictionariesConstants.DefaultEndDateDateTime <= retv ? DictionariesConstants.DefaultEndDateDateTime : retv;
		}
	}
}
