using System.Collections.Generic;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class ExportSupportingDocumentType : IUCCCodeListDetails
	{
		public string Domain => Constants.UccConstants.ExportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccSupportingDocumentType;

		public string CodeListType => Constants.UccCodeListTypes.SupportingDocumentType;

		public string DataSource => Constants.DataSources.BeSupportingDocuments;

		public List<(string, string)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
		};
	}
}
