using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailTimelineForm))]
	class TradeDetailTimelineFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var detail = Factory.New<OrgTradeDetail>();
			var collection = new TradeDetailTimelineCollection(detail);
			return new TradeDetailTimelineForm(collection);
		}
	}
}
