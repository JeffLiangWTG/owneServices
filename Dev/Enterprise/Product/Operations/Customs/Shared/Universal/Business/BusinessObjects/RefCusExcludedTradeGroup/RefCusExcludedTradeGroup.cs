using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusExcludedTradeGroup : AutoRefCusExcludedTradeGroup
	{
		public RefCusExcludedTradeGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString TradeGroupDescription => TradeGroup?.ZZA_Description ?? ZString.Empty;

		public CusRefTradeGroupView TradeGroup => Factory.Load<CusRefTradeGroupView>(ZZC_ZZA_TradeGroup);

		[RelatedBusinessObject("TradeGroup")]
		public override ZGuid ZZC_ZZA_TradeGroup { get => base.ZZC_ZZA_TradeGroup; set => base.ZZC_ZZA_TradeGroup = value; }
	}
}
