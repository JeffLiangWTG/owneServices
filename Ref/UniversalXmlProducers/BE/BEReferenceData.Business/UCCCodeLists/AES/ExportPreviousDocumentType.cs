using System.Collections.Generic;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class ExportPreviousDocumentType : IUCCCodeListDetails
	{
		public string Domain => Constants.UccConstants.ExportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccPreviousDocumentType;

		public string CodeListType => Constants.UccCodeListTypes.PreviousDocumentType;

		public string DataSource => Constants.DataSources.BePreviousDocuments;

		public List<(string, string)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
		};
	}
}
