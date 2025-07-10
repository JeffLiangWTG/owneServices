using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Nomenclature
{
	public static class NomenclatureParser
	{
		public static string ConvertToXMLFile(string dataSource, string generalTariffStructureXml, string tariffStructureXmlForNorway, DateTime lastModified, string outputFileWithPath)
		{
			errorBuilder = errorBuilder ?? new StringBuilder();
			errorBuilder.Clear();

			nomenclatureList = new List<RefCusNomenclatureGroup>();

			if (string.IsNullOrEmpty(generalTariffStructureXml))
			{
				errorBuilder.AppendLine("Unable to parse Nomenclature data, empty or NULL file.");
				return errorBuilder.ToString();
			}

			var tariff = new XmlDocument();
			tariff.LoadXml(generalTariffStructureXml);

			if (string.IsNullOrEmpty(tariffStructureXmlForNorway))
			{
				errorBuilder.AppendLine("Unable to parse Nomenclature data (norwegian language), empty or NULL file.");
				return errorBuilder.ToString();
			}
			nomenclatureWithNorwegianStrings = NomenclatureParserNO.CreateNomenclature(tariffStructureXmlForNorway);

			var xmlNodeList = tariff.SelectNodes("/customstariffstructure");
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				TraverseXml(xmlNode);
			}

			if (nomenclatureList.Any())
			{
				var refCusNomenclatureGroupConfiguration = GetRefCusNomenclatureGroupConfiguration();
				FileHelper.ExportToXMLFile(dataSource, outputFileWithPath, refCusNomenclatureGroupConfiguration, lastModified, nomenclatureList);
			}

			return errorBuilder.ToString();
		}

		public static List<RefCusNomenclatureGroup> CreateNomenclature(string generalTariffStructureXml)
		{
			nomenclatureList = new List<RefCusNomenclatureGroup>();

			var tariff = new XmlDocument();
			tariff.LoadXml(generalTariffStructureXml);

			var xmlNodeList = tariff.SelectNodes("/customstariffstructure");
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				TraverseXml(xmlNode);
			}

			return nomenclatureList;
		}

		static string TraverseXml(XmlNode xmlNodeList, string nomenclatur = "")
		{
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				var idNode = xmlNode["id"]?.InnerText ?? string.Empty;
				var itemNode = xmlNode["item"]?.InnerText ?? string.Empty;
				var hsNumNode = xmlNode["hsNumber"]?.InnerText ?? string.Empty;
				var descNode = xmlNode["description"]?.InnerText ?? string.Empty;

				var haveId = !string.IsNullOrWhiteSpace(idNode);
				var havehsNum = !string.IsNullOrWhiteSpace(hsNumNode);

				var parentNodeName = xmlNode.ParentNode.Name;
				var nodeName = xmlNode.Name;

				var norwegianTranslatedString = string.Empty;

				if (haveId || havehsNum)
				{
					var nomString = nomenclatur;

					if (parentNodeName == "divisions" && !nodeName.StartsWith("sub", StringComparison.InvariantCultureIgnoreCase) || !string.IsNullOrWhiteSpace(nomString))
					{
						nomString += ".";
					}

					var newValue = ExtractNomenclatureData(xmlNode, idNode, hsNumNode, havehsNum);

					nomString += newValue;
					if (nomenclatureWithNorwegianStrings != null)
					{
						norwegianTranslatedString = Tariff.Nomenclature.GetTranslatedString(nomenclatureWithNorwegianStrings, nomString);
					}

					XmlAddRecord(nomString, hsNumNode, !string.IsNullOrWhiteSpace(descNode) ? descNode : itemNode, norwegianTranslatedString);

					TraverseXml(xmlNode, nomString);
				}
				else
				{
					if (parentNodeName == "divisions" && nodeName == "commodity")
					{
						nomenclatur += "..";
					}
					nomenclatur = TraverseXml(xmlNode, nomenclatur);
				}
			}
			return nomenclatur;
		}

		static string ExtractNomenclatureData(XmlNode xmlNode, string idNode, string hsNumNode, bool havehsNum)
		{
			string newValue;

			if (havehsNum && xmlNode.Name == "commodity")
			{
				var parentHsNum = GetOneParentValue(xmlNode.ParentNode, "hsNumber");
				var parentIdNum = GetOneParentValue(xmlNode.ParentNode, "id");

				if (parentHsNum == hsNumNode && idNode.Contains(hsNumNode))
				{
					newValue = idNode.Removestart(hsNumNode);
				}
				else if (!string.IsNullOrWhiteSpace(parentHsNum) && hsNumNode.Contains(parentHsNum))
				{
					newValue = hsNumNode.Removestart(parentHsNum);
				}
				else if (idNode.Contains(hsNumNode))
				{
					newValue = idNode.Removestart(hsNumNode);
					if (newValue.Replace("0", "").Length == 0)
					{
						newValue = hsNumNode.Removestart(parentIdNum);
					}
				}
				else if (!string.IsNullOrWhiteSpace(parentIdNum) && hsNumNode.Contains(parentIdNum))
				{
					newValue = hsNumNode.Removestart(parentIdNum);
				}
				else
				{
					newValue = GetAllParentValues(xmlNode.ParentNode, hsNumNode, "id");
				}
			}
			else
			{
				var remove = GetOneParentValue(xmlNode.ParentNode, "id");
				var identity = !string.IsNullOrWhiteSpace(hsNumNode) ? hsNumNode : idNode;
				newValue = identity.Removestart(remove);

				if (newValue != identity && !xmlNode.Name.StartsWith("sub", StringComparison.InvariantCultureIgnoreCase))
				{
					newValue = "." + newValue;
				}
			}

			return newValue;
		}

		static string GetAllParentValues(XmlNode xmlNode, string fullString, string idType)
		{
			var parent = xmlNode?.ParentNode?.Name;
			if (parent != "sections" && parent != null)
			{
				var idNode = xmlNode[idType]?.InnerText ?? string.Empty;
				if (!string.IsNullOrWhiteSpace(idNode))
				{
					return fullString.Removestart(idNode);
				}
			}
			return GetAllParentValues(xmlNode.ParentNode, fullString, idType);
		}

		static string GetOneParentValue(XmlNode xmlNode, string idType)
		{
			var removeThis = string.Empty;

			if (xmlNode?.Name != "sections" && !string.IsNullOrWhiteSpace(xmlNode?.Name))
			{
				var result = xmlNode[idType]?.InnerText ?? string.Empty;

				if (!string.IsNullOrWhiteSpace(result))
				{
					return result;
				}
				removeThis += GetOneParentValue(xmlNode?.ParentNode, idType);
			}
			return removeThis;
		}

		static string Removestart(this string input, string removeThis)
		{
			var index = input.IndexOf(removeThis, StringComparison.InvariantCultureIgnoreCase);
			return index < 0 ? input : input.Remove(index, removeThis.Length);
		}

		static void XmlAddRecord(string compositeKey, string value, string description, string descriptionNO)
		{
			if (!string.IsNullOrWhiteSpace(compositeKey) && !string.IsNullOrWhiteSpace(description))
			{
				var group = new RefCusNomenclatureGroup
				{
					ZZ5_CompositeKey = compositeKey,
					ZZ5_Value = value,
					ZZ5_Description = DataHelpers.CleanHtmlStringIfApplicable(description)
				};

				if (!string.IsNullOrEmpty(descriptionNO))
				{
					group.RefCusNomenclatureLanguages = new RefCusNomenclatureLanguage[]
					{
						new RefCusNomenclatureLanguage
						{
							ZX8_Description = DataHelpers.CleanHtmlStringIfApplicable(descriptionNO)
						}
					};
				}

				nomenclatureList.Add(group);
			}
			else
			{
				errorBuilder.AppendLine("Unable to parse Nomenclature due to empty key, or empty description.");
			}
		}

		static XmlWriterConfiguration GetRefCusNomenclatureGroupConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var nomenclatureConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, Constants.CountryCodes.Norway);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.CountryCodes.Norway);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_StartDate, false, Constants.MinimumDateTime);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_EndDate, false, Constants.MaximumDateTime);
			nomenclatureConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			nomenclatureConfiguration.IncludeColumn(x => x.ZZ5_Value, false);
			nomenclatureConfiguration.IncludeColumn(x => x.ZZ5_Description, false);
			nomenclatureConfiguration.IncludeColumn(x => x.RefCusNomenclatureLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(nomenclatureConfiguration);

			var languagesConfiguration = new EntityTypeConfiguration<RefCusNomenclatureLanguage>(true);
			languagesConfiguration.IncludeColumnWithConstantValue(x => x.ZX8_ZX6_NKLanguage, true, Constants.CountryCodes.Norway);
			languagesConfiguration.IncludeColumn(x => x.ZX8_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(languagesConfiguration);

			return writerConfiguration;
		}

		static StringBuilder errorBuilder;
		static List<RefCusNomenclatureGroup> nomenclatureList = null;
		static List<RefCusNomenclatureGroup> nomenclatureWithNorwegianStrings = null;
	}
}
