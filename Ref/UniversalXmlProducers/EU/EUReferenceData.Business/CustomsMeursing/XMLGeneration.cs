using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business
{
	public static class XMLGeneration
	{
		public static void ExportToXMLFile(DateTime? publicationTime, IList<CustomsMeursingData> meursingDatas, IList<GeographicalAreaCompositionData> geoData, string outputFile, Exception exception = null)
		{
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));

			var xmlDoc = new XmlDocument();
			var root = xmlDoc.CreateElement("UniversalReferenceData");
			root.AppendChild(GenerateTextNodeElement(xmlDoc, "DataSource", "EUN Meursing Data"));
			if (publicationTime != null)
			{
				root.AppendChild(GenerateTextNodeElement(xmlDoc, "PublicationTime", $"{publicationTime:s}"));
			}
			root.AppendChild(GenerateTextNodeElement(xmlDoc, "UpdateType", "FULL"));
			root.AppendChild(GenerateSchemaElement(xmlDoc));
			if (meursingDatas != null && geoData != null)
			{
				foreach (var exportPortNode in GenerateDataSetElements(xmlDoc, meursingDatas, geoData))
				{
					root.AppendChild(exportPortNode);
				}
			}
			if (exception != null)
			{
				root.AppendChild(GenerateExceptionNodeElement(xmlDoc, exception));
			}

			xmlDoc.AppendChild(root);
			xmlDoc.Save(outputFile);
		}

		static XmlNode GenerateTextNodeElement(XmlDocument xmlDoc, string elementName, string value)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));

			var element = xmlDoc.CreateElement(elementName);
			var node = xmlDoc.CreateTextNode(value);
			element.AppendChild(node);
			return element;
		}

		static XmlNode GenerateSchemaElement(XmlDocument xmlDoc)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));

			var schemaElement = xmlDoc.CreateElement("Schema");
			schemaElement.InnerXml = @"
		<EntityType Name=""RefCusTariff"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
				<PropertyRef Name=""ZZ1_TariffCode"" />
				<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
			</Key>
			<Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
			<Property Name=""ZZ1_Description"" Type=""varchar"" />
			<Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
			<Property Name=""ZZ1_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ1_EndDate"" Type=""smalldatetime"" />
			<Property Name=""RefCusRate"" Type=""RefCusRate"" />
		</EntityType>
		<EntityType Name=""RefCusRate"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
				<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
				<PropertyRef Name=""RefCusApplicability"" />
			</Key>
			<Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" MaxLength=""5"" />
			<Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
			<Property Name=""ZZ2_RateFormula"" Type=""varchar"" MaxLength=""500"" />
			<Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
			<Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusApplicability"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
				<PropertyRef Name=""ZZT_AdditionalCode"" />
			</Key>
			<Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
			<Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
			<Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
			<Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3""/>
			<Property Name=""ZZT_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
			<Key>
				<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
				<PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
			</Key>
			<Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
			<Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		</EntityType>
		";
			return schemaElement;
		}

		static XmlNode GenerateExceptionNodeElement(XmlDocument xmlDoc, Exception exception)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(exception, nameof(exception));

			var element = xmlDoc.CreateElement("Error");
			element.AppendChild(GenerateTextNodeElement(xmlDoc, "Message", exception.Message));
			element.AppendChild(GenerateTextNodeElement(xmlDoc, "Description", exception.Message));
			element.AppendChild(GenerateTextNodeElement(xmlDoc, "StackTrace", exception.ToString()));
			return element;
		}

		static IEnumerable<XmlElement> GenerateDataSetElements(XmlDocument xmlDoc, IList<CustomsMeursingData> meursingDatas, IList<GeographicalAreaCompositionData> geoData)
		{
			Argument.NotNull(xmlDoc, nameof(xmlDoc));
			Argument.NotNull(meursingDatas, nameof(meursingDatas));
			Argument.NotNull(geoData, nameof(geoData));

			var meursingDataNodes = new List<XmlElement>();

			var groupedMeursingDatas = meursingDatas.GroupBy(x => x.TariffCode).Select(x => new { TariffCode = x.Key, List = x.ToList() });
			var groupedGeoDataByGeogrAreaID = geoData.GroupBy(x => x.CountryGroup).ToDictionary(x => x.Key, x => x.Select(y => y.MemberCountry).ToList());
			var groupedGeoDataByGroupAbbreviation = geoData.GroupBy(x => x.GroupAbbreviation).ToDictionary(x => x.Key, x => x.Select(y => y.MemberCountry).ToList());

			foreach (var groupedMeursingData in groupedMeursingDatas)
			{
				var tariffElement = xmlDoc.CreateElement("RefCusTariff");
				tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_ZZI_NKTariffType", "MEU"));
				tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_TariffCode", groupedMeursingData.TariffCode));
				tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_Description", groupedMeursingData.TariffCode + " Meursing Additional Code"));
				var startDate = groupedMeursingData.List.OrderBy(x => x.DatStart).First().DatStart;
				tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_StartDate", Utils.GetDateTime(startDate, Utils.DefaultDateTime.Min)));
				var existEndDateIsEmpty = groupedMeursingData.List.Exists(data => string.IsNullOrEmpty(data.DatEnd));
				if (existEndDateIsEmpty)
				{
					tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_EndDate", Utils.GetDateTime(string.Empty, Utils.DefaultDateTime.Max)));
				}
				else
				{
					var endDate = groupedMeursingData.List.OrderByDescending(x => x.DatEnd).First().DatEnd;
					tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_EndDate", Utils.GetDateTime(endDate, Utils.DefaultDateTime.Max)));
				}
				tariffElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ1_ZZZ_NKDataGrouping", "EUN"));

				foreach (var excelData in groupedMeursingData.List)
				{
					var rateElement = xmlDoc.CreateElement("RefCusRate");
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_StartDate", Utils.GetDateTime(excelData.DatStart, Utils.DefaultDateTime.Min)));
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_EndDate", Utils.GetDateTime(excelData.DatEnd, Utils.DefaultDateTime.Max)));
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_ZY1_NKRateCode", RateCode(excelData.MeasTypID, excelData.GeogrAreaID)));
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_ZY1_ZZR_NKRateType", "DUT"));
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping", "EUN"));
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_RateFormula", RateFormula(excelData.DutyCondFull)));
					rateElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZ2_ZZZ_NKDataGrouping", "EUN"));

					var applicabilityElement = xmlDoc.CreateElement("RefCusApplicability");
					applicabilityElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZT_StartDate", Utils.GetDateTime(excelData.DatStart, Utils.DefaultDateTime.Min)));
					applicabilityElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZT_EndDate", Utils.GetDateTime(excelData.DatEnd, Utils.DefaultDateTime.Max)));
					applicabilityElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZT_ZZA_NKTradeGroup", excelData.GeogrAreaID));
					applicabilityElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZT_ZZA_ZZZ_NKDataGrouping", "EUN"));
					applicabilityElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZT_AdditionalCode", excelData.RedInd));

					if (excelData.GeogrAreaID != "1011")
					{
						var excludedTradeGroups = GetExcludedTradeGroups(excelData, groupedMeursingData.List, groupedGeoDataByGeogrAreaID, groupedGeoDataByGroupAbbreviation);
						foreach (var excluded in excludedTradeGroups)
						{
							var excludedTradeGroupElement = xmlDoc.CreateElement("RefCusExcludedTradeGroup");
							excludedTradeGroupElement.AppendChild(GenerateTextNodeElement(xmlDoc, "ZZC_ZZA_NKTradeGroup", excluded));
							applicabilityElement.AppendChild(excludedTradeGroupElement);
						}
					}

					rateElement.AppendChild(applicabilityElement);
					tariffElement.AppendChild(rateElement);
				}
				meursingDataNodes.Add(tariffElement);
			}
			return meursingDataNodes;
		}

		static List<string> GetExcludedTradeGroups(CustomsMeursingData groupedMeursingRecord, List<CustomsMeursingData> groupedMeursingData
			, Dictionary<string, List<string>> groupedGeoDataByGeogrAreaID, Dictionary<string, List<string>> groupedGeoDataByGroupAbbreviation)
		{
			var geoGroup = new List<string>();
			if (groupedGeoDataByGeogrAreaID.ContainsKey(groupedMeursingRecord.GeogrAreaID))
			{
				geoGroup = groupedGeoDataByGeogrAreaID[groupedMeursingRecord.GeogrAreaID];
				geoGroup.AddRange(groupedGeoDataByGroupAbbreviation.Where(x => geoGroup.Contains(x.Key)).SelectMany(x => x.Value));
			}
			var applicableMearsingRecords = groupedMeursingData.Where(x => x.RedInd == groupedMeursingRecord.RedInd && x.GeogrAreaID != groupedMeursingRecord.GeogrAreaID).Select(x => x.GeogrAreaID);
			return applicableMearsingRecords.Intersect(geoGroup).ToList() ?? new List<string>();
		}

		static string RateFormula(object value)
		{
			var num = value.ToString().Split()[0];
			return num == "0.000" ? "0" : $"{num} * [DTN]";
		}

		static string RateCode(string meas, string geographyCode)
		{
			const string ergaOmnesCode = "1011";
			switch (geographyCode)
			{
				case ergaOmnesCode:
					switch (meas)
					{
						case "672":
							return "ADSZ";
						case "674":
							return "EA";
						case "673":
							return "ADFM";
						default:
							return string.Empty;
					}
				default:
					switch (meas)
					{
						case "672":
							return "ADSZR";
						case "674":
							return "EAR";
						case "673":
							return "ADFMR";
						default:
							return string.Empty;
					}
			}
		}
	}
}
