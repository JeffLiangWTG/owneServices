namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class ForwarderAdditionalDetailsControlTest : QuotedBookingCustomizableControlTest<ForwarderAdditionalDetailsControl>
	{
		protected override string TabName
		{
			get
			{
				return "AdditionalDetailsTabPage";
			}
		}

		protected override string[] ExpectedPanelNames
		{
			get
			{
				return new string[] { CustomizablePanelNames.LeftBottomPanel, CustomizablePanelNames.MiddleTopPanel, CustomizablePanelNames.MiddleBottomPanel, CustomizablePanelNames.RightTopPanel, CustomizablePanelNames.RightBottomPanel };
			}
		}
	}
}
