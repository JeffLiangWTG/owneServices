using System;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeDetailTimelineForm : ZChildForm
	{
		[Obsolete("This just for designer")]
		public TradeDetailTimelineForm()
		{
			InitializeComponent();
		}

		public TradeDetailTimelineForm(TradeDetailTimelineCollection timelineCollection)
			: base(timelineCollection)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;
	}
}
