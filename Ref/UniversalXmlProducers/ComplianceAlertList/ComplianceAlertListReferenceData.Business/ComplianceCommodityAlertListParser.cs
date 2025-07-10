using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Business
{
	public class ComplianceCommodityAlertListParser
	{
		readonly IEnumerable<ComplianceCountryAlertDetailsResponseModel> sourceList;
		readonly string exportFilePath;
		readonly DateTime publicationDate;

		public ComplianceCommodityAlertListParser(IEnumerable<ComplianceCountryAlertDetailsResponseModel> sourceList, string exportFilePath, DateTime publicationDate)
		{
			this.sourceList = sourceList;
			this.exportFilePath = exportFilePath;
			this.publicationDate = publicationDate;
		}

		public void ExportXml()
		{
			var writer = Helper.GenerateXmlWriter(publicationDate);
			foreach (var sourceData in sourceList)
			{
				if (sourceData.ExportAlerts != null)
				{
					foreach (var alert in sourceData.ExportAlerts)
					{
						writer.PopulateData(GetAlertList(alert, sourceData, Constants.Export));
					}
				}

				if (sourceData.ImportAlerts != null)
				{
					foreach (var alert in sourceData.ImportAlerts)
					{
						writer.PopulateData(GetAlertList(alert, sourceData, Constants.Import));
					}
				}
			}

			Helper.ExportToXMLFile(writer, exportFilePath);
		}

		static RefComplianceCommodityAlert GetAlertList(ComplianceAlertDetailsModel alert, ComplianceCountryAlertDetailsResponseModel sourceData, string direction)
		{
			return new RefComplianceCommodityAlert
			{
				RCR_IsActive = alert.IsActive,
				RCR_AlertCode = SubstringSafe(alert.Code, 200),
				RCR_AlertName = SubstringSafe(alert.Name, 200),
				RCR_AlertDescription = alert.Description ?? string.Empty,
				RCR_AlertType = alert.NomenclatureWide ? Constants.NomenclatureAlert : alert.CommoditySpecific ? Constants.CommodityAlert : Constants.LocationAlert,
				RCR_CountryRegion = sourceData.CountryCode,
				RCR_PublishYear = (short)2022,
				RCR_SourceURL = SubstringSafe(alert.ContentUrl, 2048) ?? string.Empty,
				RCR_TradeDirection = direction,
			};
		}

		static string SubstringSafe(string value, int length)
		{
			if (value != null && value.Length > length)
			{
				return value.Substring(0, length);
			}

			return value;
		}

		class Constants
		{
			public const string Export = "EXP";
			public const string Import = "IMP";

			public const string NomenclatureAlert = "NOM";
			public const string CommodityAlert = "COM";
			public const string LocationAlert = "LOC";
		}
	}
}
