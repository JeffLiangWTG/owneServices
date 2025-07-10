using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryModelMark
{
	static public class ModelAndMarkDictionaryStrings
	{
		// example :
		// <C3041 id="28822" idmodelu="9543" model="BR400" marka="Access Motor" validFrom="2013-01-01"/>

		// element attributes
		public const string ValidFrom = "validFrom";
		public const string ValidTo = "validTo";
		public const string Model = "model";
		public const string Mark = "marka";
		public const string Code = "idmodelu";
	}

	public static class ModelAndMarkBasedDictionaryXmlParser
	{
		static bool isRoot = true;
		static public List<ModelAndMarkBasedElement> ParseXml(XmlTextReader xmlDoc)
		{
			//xmlDoc.MoveToContent(); // ignore xmlDeclaration
			var retv = new List<ModelAndMarkBasedElement>();
			isRoot = true;

			while (xmlDoc.Read())
			{
				switch (xmlDoc.NodeType)
				{
					case XmlNodeType.Element:
						OnXmlElement(xmlDoc, retv);
						break;

					case XmlNodeType.EndElement:
					case XmlNodeType.XmlDeclaration:
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

		static void OnXmlElement(XmlTextReader xmlDoc, List<ModelAndMarkBasedElement> data)
		{
			ModelAndMarkBasedElement element = new ModelAndMarkBasedElement();

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

		static void OnXmlElementAttribute(string name, string value, ModelAndMarkBasedElement element)
		{
			switch (name)
			{
				case ModelAndMarkDictionaryStrings.Code:
					OnCodeAttribute(value, element);
					break;
				case ModelAndMarkDictionaryStrings.Mark:
					OnMarkAttribute(value, element);
					break;
				case ModelAndMarkDictionaryStrings.Model:
					OnModelAttribute(value, element);
					break;
				case ModelAndMarkDictionaryStrings.ValidFrom:
					OnValidFromAttribute(value, element);
					break;
				case ModelAndMarkDictionaryStrings.ValidTo:
					OnValidToAttribute(value, element);
					break;

				default:
					// do nothing
					break;
			}
		}

		static void OnCodeAttribute(string value, ModelAndMarkBasedElement element)
		{
			element.Code = value;
		}
		static void OnModelAttribute(string value, ModelAndMarkBasedElement element)
		{
			element.Model = value;
		}

		static void OnMarkAttribute(string value, ModelAndMarkBasedElement element)
		{
			element.Mark = value;
		}

		static void OnValidToAttribute(string value, ModelAndMarkBasedElement element)
		{
			element.ValidTo = ConvertPuescStringDateTimeToDateTime(value);
		}

		static void OnValidFromAttribute(string value, ModelAndMarkBasedElement element)
		{
			element.ValidFrom = ConvertPuescStringDateTimeToDateTime(value);
		}

		static DateTime ConvertPuescStringDateTimeToDateTime(string value)
		{
			var retv = DateTime.ParseExact(value, DictionariesConstants.XmlDateTimeFormat, CultureInfo.InvariantCulture);

			if (DictionariesConstants.DefaultEndDateDateTime <= retv)
			{
				return DictionariesConstants.DefaultEndDateDateTime;
			}

			return retv;
		}
	}
}
