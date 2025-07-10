using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class ManifestToOpenPackProvider : IOpeningTransportBillLines
	{
		public ManifestToOpenPackProvider(ManifestToOpenPack manifestToOpenPack)
		{
			ManifestToOpenPack = Argument.NotNull(manifestToOpenPack, nameof(manifestToOpenPack));
		}
		ManifestToOpenPack ManifestToOpenPack { get; }

		public string OpeningTransportBillLinesNo => ManifestToOpenPack.TPI_LineNumber.ToString();
		public string WareHouseCode => ManifestToOpenPack.TPI_WarehouseCode;
		public decimal OpeningQuantity => ManifestToOpenPack.TPI_Quantity.RoundAmount();
	}
}
