using System.Collections.Generic;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class ExportAdditionalInformation : IUCCCodeListDetails
	{
		public string Domain => Constants.UccConstants.ExportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccAdditionalInformation;

		public string CodeListType => Constants.UccCodeListTypes.AdditionalInformation;

		public string DataSource => Constants.DataSources.BeAdditionalInformation;

		public List<(string, string)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
		};
	}
}
