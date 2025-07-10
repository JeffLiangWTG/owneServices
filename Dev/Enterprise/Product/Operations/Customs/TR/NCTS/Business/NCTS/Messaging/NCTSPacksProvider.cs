using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSPacksProvider : INCTSPacks
	{
		public NCTSPacksProvider(CusInvPack packs)
		{
			this.packs = packs;
		}
		readonly CusInvPack packs;

		public string MarksAndNumbers => packs.B5_MarksAndNumbers;
		public string MarksAndNumbersLNG => TRMessageConstants.LanguageCode;
		public string UnitTypeCode => packs.B5_UnitType;
		public int PackagesUnitCount => ZInt.ParseSafe(packs.B5_UnitCount.ToString(), 0);
		public string PiecesUnitCount => string.Empty;
	}
}
