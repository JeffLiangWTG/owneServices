using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsPreviousDocumentExportType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsPreviousDocumentCode;

		public string CodeListType => Constants.UccCodeListTypes.PreviousDocumentExportType;

		public string DataSource => Constants.UccDataSources.PreviousDocumentsNcts;

		public override string ExtraType => Constants.UccConstants.ExportExtraType;

		public override IReadOnlyList<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.House),
		};

		public string XmlDataItemForCode => "PreviousDocumentTypeCode";
	}
}
