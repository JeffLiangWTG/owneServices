namespace Enterprise.MarketingManager.GUI.Testing
{
	class DynamicTradeLanesControlForTest : DynamicTradeLanesControl
	{
		public TradeLanesControl CurrentlyVisibleInnerControl_Exposed
		{
			get { return base.CurrentlyVisibleInnerControl; }
		}
	}
}
