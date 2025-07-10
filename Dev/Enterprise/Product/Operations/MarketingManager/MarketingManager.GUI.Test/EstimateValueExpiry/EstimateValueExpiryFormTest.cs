using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(EstimateValueExpiryForm))]
	class EstimateValueExpiryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_IsTraded = false;
			var detail = sales.TradeDetails.AddNew();
			detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			var action = new EstimateValueExpiryAction(new OrgTradeDetail[] { detail }, EstimateValueExpiryAction.ActionType.SetExpiry);
			return new EstimateValueExpiryForm(action);
		}
	}
}
