using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public static class Utils
	{
		public static XmlElement AddExchangeRate(this XmlDocument xmlDoc, string dataSourceUniqueString, DateTime publicationTime, IEnumerable<RefExchangeRateElement> refExchangeRates)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(refExchangeRates, nameof(refExchangeRates));
			Argument.NotNull(dataSourceUniqueString, nameof(dataSourceUniqueString));

			var root = xmlDoc.CreateElement("UniversalReferenceData");
			root.AppendChild(GeneratePublicationTimeElement(xmlDoc, publicationTime));
			root.AppendChild(GeneratePublicationTypeElement(xmlDoc));
			root.AppendChild(GenerateDataSourceElement(xmlDoc, dataSourceUniqueString));
			root.AppendChild(GenerateSchemaElement(xmlDoc, new string[] { "ZZN_ExRateType", "ZZN_RX_NKExCurrency", "ZZN_RN_NKCountry", "ZZN_StartDate" }));
			foreach (var exchangeRateNode in GenerateDataSetElements(xmlDoc, refExchangeRates))
			{
				root.AppendChild(exchangeRateNode);
			}

			xmlDoc.AppendChild(root);
			return root;
		}

		public static void ExportToXMLFile(string dataSourceUniqueString, DateTime publicationTime, IEnumerable<RefExchangeRateElement> refExchangeRates, string outputFile)
		{
			Argument.NotNull(dataSourceUniqueString, nameof(dataSourceUniqueString));
			Argument.NotNull(refExchangeRates, nameof(refExchangeRates));
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));

			var xmlDoc = new XmlDocument();
			xmlDoc.AddExchangeRate(dataSourceUniqueString, publicationTime, refExchangeRates);
			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);
			xmlDoc.Save(outputFile);
		}

		static XmlNode GeneratePublicationTimeElement(XmlDocument xmlDoc, DateTime publicationTime)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));

			var publicationTimeElement = xmlDoc.CreateElement("PublicationTime");
			var node = xmlDoc.CreateTextNode(string.Format(CultureInfo.InvariantCulture, "{0:s}", publicationTime));
			publicationTimeElement.AppendChild(node);
			return publicationTimeElement;
		}

		static XmlNode GeneratePublicationTypeElement(XmlDocument xmlDoc)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));

			var publicationTypeElement = xmlDoc.CreateElement("UpdateType");
			var node = xmlDoc.CreateTextNode("FULL");
			publicationTypeElement.AppendChild(node);
			return publicationTypeElement;
		}

		static XmlNode GenerateDataSourceElement(XmlDocument xmlDoc, string dataSourceUniqueString)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(dataSourceUniqueString, nameof(dataSourceUniqueString));

			var dataSourceElement = xmlDoc.CreateElement("DataSource");
			var node = xmlDoc.CreateTextNode(dataSourceUniqueString);
			dataSourceElement.AppendChild(node);
			return dataSourceElement;
		}

		static XmlNode GenerateSchemaElement(XmlDocument xmlDoc, IEnumerable<string> keys)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(keys, nameof(keys));

			var schemaElement = xmlDoc.CreateElement("Schema");
			var entityType = xmlDoc.CreateElement("EntityType");
			var entityTypeAttrName = xmlDoc.CreateAttribute("Name");
			entityTypeAttrName.Value = "RefExchangeRateZZ";
			var entityTypeAttrData = xmlDoc.CreateAttribute("Data");
			entityTypeAttrData.Value = "true";
			entityType.Attributes.Append(entityTypeAttrName);
			entityType.Attributes.Append(entityTypeAttrData);
			entityType.AppendChild(GenerateKeyElement(xmlDoc, keys));

			var properties = @"
<Property Name=""ZZN_ExRateType"" Type=""String"" MaxLength=""3"" FixedLength=""false"" Unicode=""false"" />
<Property Name=""ZZN_StartDate"" Type=""DateTime"" Precision=""0"" />
<Property Name=""ZZN_EndDate"" Type=""DateTime"" Precision=""0"" />
<Property Name=""ZZN_Rate"" Type=""Decimal"" Precision=""18"" Scale=""9"" />
<Property Name=""ZZN_RX_NKExCurrency"" Type=""String"" MaxLength=""3"" FixedLength=""true"" Unicode=""false"" />
<Property Name=""ZZN_RN_NKCountry"" Type=""String"" MaxLength=""2"" FixedLength=""true"" Unicode=""false"" />";
			entityType.InnerXml = entityType.InnerXml + properties.Replace("\r\n", "");
			schemaElement.AppendChild(entityType);
			return schemaElement;
		}

		static XmlNode GenerateKeyElement(XmlDocument xmlDoc, IEnumerable<string> keys)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(keys, nameof(keys));

			var keyElement = xmlDoc.CreateElement("Key");
			foreach (var key in keys)
			{
				var ele = xmlDoc.CreateElement("PropertyRef");
				var attr = xmlDoc.CreateAttribute("Name");
				attr.Value = key;
				ele.Attributes.Append(attr);
				keyElement.AppendChild(ele);
			}
			return keyElement;
		}

		static IEnumerable<XmlElement> GenerateDataSetElements(XmlDocument xmlDoc, IEnumerable<RefExchangeRateElement> exchangeRates)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(exchangeRates, nameof(exchangeRates));

			var exchangeRateNodes = new List<XmlElement>();
			foreach (var exchangeRate in exchangeRates)
			{
				var refExchangeRateZZElement = xmlDoc.CreateElement("RefExchangeRateZZ");
				refExchangeRateZZElement.AppendChild(GenerateRefExchangeRateElement(xmlDoc, "ZZN_ExRateType", exchangeRate.ZZN_ExRateType));
				refExchangeRateZZElement.AppendChild(GenerateRefExchangeRateElement(xmlDoc, "ZZN_StartDate", string.Format(CultureInfo.InvariantCulture, "{0:s}", exchangeRate.ZZN_StartDate)));
				refExchangeRateZZElement.AppendChild(GenerateRefExchangeRateElement(xmlDoc, "ZZN_EndDate", string.Format(CultureInfo.InvariantCulture, "{0:s}", exchangeRate.ZZN_EndDate)));
				refExchangeRateZZElement.AppendChild(GenerateRefExchangeRateElement(xmlDoc, "ZZN_Rate", string.Format(CultureInfo.InvariantCulture, "{0:0.0000}", exchangeRate.ZZN_Rate)));
				refExchangeRateZZElement.AppendChild(GenerateRefExchangeRateElement(xmlDoc, "ZZN_RX_NKExCurrency", exchangeRate.ZZN_RX_NKExCurrency));
				refExchangeRateZZElement.AppendChild(GenerateRefExchangeRateElement(xmlDoc, "ZZN_RN_NKCountry", exchangeRate.ZZN_RN_NKCountry));
				exchangeRateNodes.Add(refExchangeRateZZElement);
			}
			return exchangeRateNodes;
		}

		static XmlNode GenerateRefExchangeRateElement(XmlDocument xmlDoc, string key, string value)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));

			var ele = xmlDoc.CreateElement(key);
			var node = xmlDoc.CreateTextNode(value);
			ele.AppendChild(node);
			return ele;
		}

		public static decimal FormatCurrency(decimal value, int decimals = 4)
		{
			Argument.InRangeWithBoundIncluded(decimals, 0, 28, nameof(decimals));
			return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
		}

		public static string RemoveTrailingZeros(string tariffCode)
		{
			Argument.NotNullOrEmpty(tariffCode, nameof(tariffCode));

			while (true)
			{
				if (!tariffCode.EndsWith("00", StringComparison.OrdinalIgnoreCase) || tariffCode.Length == 2)
				{
					return tariffCode;
				}

				tariffCode = tariffCode.Substring(0, tariffCode.Length - 2);
			}
		}

		public static string GetLastDayInMonthDateString(int year, int month)
		{
			Argument.InRangeWithBoundIncluded(month, 1, 12, nameof(month));
			int days = DateTime.DaysInMonth(year, month);
			return string.Join("", year, month.ToString("00", CultureInfo.InvariantCulture), days);
		}

		public static int ConvertRomanNumeralToInt(string romanNumeral)
		{
			var romanMap = new Dictionary<char, int>() {
										{ 'I', 1 },
										{ 'V', 5 },
										{ 'X', 10 },
										{ 'L', 50 },
										{ 'C', 100 },
										{ 'D', 500 },
										{ 'M', 1000 }
			};

			if (string.IsNullOrEmpty(romanNumeral))
			{
				return int.MinValue;
			}

			int number = 0;
			for (int i = 0; i < romanNumeral.Length; i++)
			{
				if (i + 1 < romanNumeral.Length && romanMap[romanNumeral[i]] < romanMap[romanNumeral[i + 1]])
				{
					number -= romanMap[romanNumeral[i]];
				}
				else
				{
					number += romanMap[romanNumeral[i]];
				}
			}
			return number;
		}
	}
}
