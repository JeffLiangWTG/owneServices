
namespace Enterprise.MarketingManager.GUI
{
	internal interface ISalesHeaderStripControl
	{
		bool IsDeleteButtonFocused { get; }
		bool ReadOnly { get; set; }
		bool Collapsed { get; set; }
	}
}
