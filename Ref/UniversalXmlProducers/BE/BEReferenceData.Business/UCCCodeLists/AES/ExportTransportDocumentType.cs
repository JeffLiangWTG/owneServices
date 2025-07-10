using System.Collections.Generic;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class ExportTransportDocumentType : IUCCCodeListDetails
	{
		public string Domain => Constants.UccConstants.ExportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccTransportDocumentType;

		public string CodeListType => Constants.UccCodeListTypes.TransportDocumentType;

		public string DataSource => Constants.DataSources.BeTransportDocuments;

		public List<(string, string)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
		};
	}
}
