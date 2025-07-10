using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsPreviousDocumentUnionGoods : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsPreviousDocumentUnionGoodsCode;

		public string CodeListType => Constants.UccCodeListTypes.PreviousDocumentUnionGoods;

		public string DataSource => Constants.UccDataSources.PreviousDocumentUnionGoods;

		public string XmlDataItemForCode => "PreviousDocumentTypeCode";
	}
}
