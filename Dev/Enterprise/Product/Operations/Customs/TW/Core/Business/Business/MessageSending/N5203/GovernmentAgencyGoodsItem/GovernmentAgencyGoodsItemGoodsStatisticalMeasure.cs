using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class GoodsStatisticalMeasure : IGoodsStatisticalMeasure
	{
		public GoodsStatisticalMeasure(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}

		public virtual ZString StatisticalUnitCode => entryLine.CL_CustomsSecondUnitQty;

		public virtual ZDecimal TariffQuantity => entryLine.CL_Calc_CustomsSecondQuantity;

		readonly CusEntryLine entryLine;
	}
}
