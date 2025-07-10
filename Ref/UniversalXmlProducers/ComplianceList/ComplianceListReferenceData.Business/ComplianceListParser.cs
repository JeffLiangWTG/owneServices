using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Business
{
	public class ComplianceListParser
	{
		readonly IEnumerable<RefComplianceListResponse> sourceList;
		readonly string exportFilePath;
		readonly DateTime publicationDate;

		public ComplianceListParser(IEnumerable<RefComplianceListResponse> sourceList, string exportFilePath, DateTime publicationDate)
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
				var complianceList = new RefComplianceList
				{
					RCL_IsActive = sourceData.RCL_IsActive,
					RCL_ListCode = sourceData.RCL_ListCode,
					RCL_ListName = sourceData.RCL_ListName,
					RCL_ListDescription = sourceData.RCL_ListDescription,
					RCL_ListPublisher = sourceData.RCL_ListPublisher,
					RCL_ListType = sourceData.RCL_ListType,
					RCL_PublisherJurisdiction = sourceData.RCL_PublisherJurisdiction,
					RCL_PublisherDescription = sourceData.RCL_PublisherDescription,
					RCL_MainSourceURL = sourceData.RCL_MainSourceURL,
					RCL_SecondarySourceURL = sourceData.RCL_SecondarySourceURL,
					RCL_IntegrationDate = sourceData.RCL_IntegrationDate,
					RCL_LastUpdatedDate = sourceData.RCL_LastUpdatedDate
				};

				complianceList.RCL_MainSourceURL = complianceList.RCL_MainSourceURL.Length > 254 ? complianceList.RCL_MainSourceURL.Substring(0, 254) : complianceList.RCL_MainSourceURL;
				complianceList.RCL_SecondarySourceURL = complianceList.RCL_SecondarySourceURL.Length > 254 ? complianceList.RCL_SecondarySourceURL.Substring(0, 254) : complianceList.RCL_SecondarySourceURL;

				writer.PopulateData(complianceList);
			}
			Helper.ExportToXMLFile(writer, exportFilePath);
		}
	}
}
