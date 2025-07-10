using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class GoodsMeasure : IGoodsMeasure
	{
		public GoodsMeasure(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}

		public virtual ZDecimal NetWeightMeasure => entryLine.EffectiveNetWeight.InKilogramsSafe;

		public virtual ZDecimal TariffQuantity => entryLine.CL_EntryLineQty;

		public virtual ZString UnitCode => entryLine.CL_EntryLineUQ;

		ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;

		readonly CusEntryLine entryLine;
	}
}
