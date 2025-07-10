using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffUOM : AutoRefCusTariffUOM
	{
		public RefCusTariffUOM(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZZ8_ZZ1_Tariff
		{
			get { return base.ZZ8_ZZ1_Tariff; }
			set { base.ZZ8_ZZ1_Tariff = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZZ8_ZZ1_Tariff);

		[RelatedBusinessObject("CusTradeGroup")]
		public override ZGuid ZZ8_ZZA_TradeGroup
		{
			get { return base.ZZ8_ZZA_TradeGroup; }
			set { base.ZZ8_ZZA_TradeGroup = value; }
		}

		public CusRefTradeGroupView CusTradeGroup => Factory.Load<CusRefTradeGroupView>(ZZ8_ZZA_TradeGroup);
	}
}
