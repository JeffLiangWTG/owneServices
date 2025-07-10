using System.Collections.Generic;
using CargoWise.RefDbRepo.BEReferenceData.Services;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class ExportAdditionalReference : IUCCCodeListDetails
	{
		public string Domain => Constants.UccConstants.ExportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccAdditionalReference;

		public string CodeListType => Constants.UccCodeListTypes.AdditionalReference;

		public string DataSource => Constants.DataSources.BeAdditionalReference;

		public List<(string, string)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
		};
	}
}
