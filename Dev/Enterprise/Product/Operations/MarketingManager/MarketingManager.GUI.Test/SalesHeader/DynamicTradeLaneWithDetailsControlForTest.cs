namespace Enterprise.MarketingManager.GUI.Testing
{
	class DynamicTradeLaneWithDetailsControlForTest : DynamicTradeLaneWithDetailsControl
	{
		public TradeLaneWithDetailsControl CurrentlyVisibleInnerControl_Exposed
		{
			get { return base.CurrentlyVisibleInnerControl; }
		}
	}
}
