using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSManifestsToOpenProvider : INCTSManifestToOpen
	{
		public NCTSManifestsToOpenProvider(NctsManifestsToOpen nctsManifestsToOpen)
		{
			this.nctsManifestsToOpen = nctsManifestsToOpen;
		}
		readonly NctsManifestsToOpen nctsManifestsToOpen;

		public string OpeningStyle => nctsManifestsToOpen.IsPartial ? "2" : "3";
		public string SummaryDeclarationNo => nctsManifestsToOpen.CSI_ReferenceNumber2;
		public string AtWarehouse => nctsManifestsToOpen.AtWarehouse ? "1" : "0";
		public string BillOfLadingNo => nctsManifestsToOpen.CSI_ReferenceNumber;
		public string WarehouseCode => nctsManifestsToOpen.CSI_CustomsOffice;
		public int BillOfLadingLineNo => nctsManifestsToOpen.CSI_LineNo;
		public int PackageCountForOpening => ZInt.ParseSafe(nctsManifestsToOpen.CSI_Quantity3.ToString(), 0);
	}
}
