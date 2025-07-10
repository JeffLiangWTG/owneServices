using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsDeclarationType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		public string Domain => Constants.UccConstants.NCTSDomain;

		public string CodeType => Constants.ZZRefCusCodeList.NctsDeclarationTypeCode;

		public string CodeListType => Constants.UccCodeListTypes.DeclarationTypeType;

		public string DataSource => Constants.UccDataSources.DeclarationTypes;

		public string XmlDataItemForCode => "DeclarationTypeCode";
	}
}
