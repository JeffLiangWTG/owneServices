using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsAdditionalReference : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsAdditionalReferenceCode;

		public string CodeListType => Constants.UccCodeListTypes.AdditionalReference;

		public string DataSource => Constants.UccDataSources.AdditionalReference;

		public override IReadOnlyList<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.N)
			};

		public string XmlDataItemForCode => "DocumentType";
	}
}
