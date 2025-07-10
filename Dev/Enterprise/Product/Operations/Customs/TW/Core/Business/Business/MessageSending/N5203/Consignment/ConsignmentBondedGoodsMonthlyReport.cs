using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class BondedGoodsMonthlyReport : IBondedGoodsMonthlyReport
	{
		public BondedGoodsMonthlyReport(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = entryInstruction;
		}

		readonly CusEntryInstruction entryInstruction;

		public ZInt MonthNumeric => ZInt.ParseSafe(entryInstruction?.CEI_WHSMonth ?? ZString.Empty, ZInt.Zero);

		public ZString TraderReferenceID => entryInstruction?.CEI_WHSTradeReferenceNo ?? ZString.Empty;
	}
}
