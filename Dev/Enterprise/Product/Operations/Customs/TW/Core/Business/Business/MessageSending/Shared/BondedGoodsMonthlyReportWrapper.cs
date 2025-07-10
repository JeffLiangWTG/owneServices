using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class BondedGoodsMonthlyReportWrapper : IBondedGoodsMonthlyReport
	{
		readonly ZInt monthNumeric;
		readonly ZString traderReferenceId;

		public BondedGoodsMonthlyReportWrapper(ZInt monthNumeric, ZString traderReferenceId)
		{
			this.monthNumeric = monthNumeric;
			this.traderReferenceId = traderReferenceId;
		}

		ZInt IBondedGoodsMonthlyReport.MonthNumeric => monthNumeric;

		ZString IBondedGoodsMonthlyReport.TraderReferenceID => traderReferenceId;
	}
}
