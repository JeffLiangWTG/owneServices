using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsPreviousDocumentType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsPreviousDocumentCode;

		public string CodeListType => Constants.UccCodeListTypes.PreviousDocumentType;

		public string DataSource => Constants.UccDataSources.PreviousDocumentsNcts;

		public override IReadOnlyList<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.Y),
			(Constants.AttributeNames.ItemNumber, Constants.AttributeValues.N),
			(Constants.AttributeNames.Complement, Constants.AttributeValues.N),
		};

		public string XmlDataItemForCode => "PreviousDocumentTypeCode";
	}
}
