using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsSupportingDocument : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsSupportingDocumentCode;

		public string CodeListType => Constants.UccCodeListTypes.SupportingDocumentType;

		public string DataSource => Constants.UccDataSources.SupportingDocuments;

		public override IReadOnlyList<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			(Constants.AttributeNames.Reference, Constants.AttributeValues.Y),
			(Constants.AttributeNames.ItemNumber, Constants.AttributeValues.N),
			(Constants.AttributeNames.Complement, Constants.AttributeValues.N),
		};

		public string XmlDataItemForCode => "SupportingDocumentCode";
	}
}
