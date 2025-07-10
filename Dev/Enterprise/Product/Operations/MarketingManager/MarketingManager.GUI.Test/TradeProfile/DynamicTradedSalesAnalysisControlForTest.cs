namespace Enterprise.MarketingManager.GUI.Testing
{
	class DynamicTradedSalesAnalysisControlForTest : DynamicTradedSalesAnalysisControl
	{
		public TradedSalesAnalysisControl CurrentlyVisibleInnerControl_Exposed
		{
			get { return base.CurrentlyVisibleInnerControl; }
		}
	}
}
