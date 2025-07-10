using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105GovernmentAgencyGoodsItem_GoodsMeasure : IGoodsMeasure
	{
		public NX5105GovernmentAgencyGoodsItem_GoodsMeasure(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, "entryLine");
		}

		readonly CusEntryLine entryLine;

		public virtual ZDecimal NetWeightMeasure => entryLine.EffectiveNetWeight.InKilogramsSafe;

		public virtual ZDecimal TariffQuantity => entryLine.CL_EntryLineQty;

		public virtual ZString UnitCode => entryLine.CL_EntryLineUQ;

		ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;
	}
}
