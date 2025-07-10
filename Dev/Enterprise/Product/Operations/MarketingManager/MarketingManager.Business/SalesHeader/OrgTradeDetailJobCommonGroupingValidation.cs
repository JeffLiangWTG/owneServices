using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class OrgTradeDetailJobCommonGroupingValidation : AutoOrgTradeDetailJobCommonGroupingValidation
	{
		public OrgTradeDetailJobCommonGroupingValidation(AutoOrgTradeDetailJobCommonGrouping grouping)
			: base(grouping)
		{
		}

		public new OrgTradeDetailJobCommonGrouping Parent
		{
			get { return (OrgTradeDetailJobCommonGrouping)base.Parent; }
		}

		protected override void CheckTradeMode()
		{
			base.CheckTradeMode();

			if (Parent.Product.ModeIsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.TradeModeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.TradeModeInfo);
		}

		protected override void CheckTradeType()
		{
			base.CheckTradeType();
			if (Parent.Product.TypeIsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.TradeTypeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.TradeTypeInfo);
		}
	}
}
