using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SelectStaffPoolOpportunityAssignmentForm))]
	class SelectStaffPoolOpportunityAssignmentFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SelectStaffPoolOpportunityAssignmentForm(Factory.NewWithValidTestData<GlbCompanyCampaign>().SenderPool);
		}

		public void TestCloseWithErrorsInPool()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = new SelectStaffPoolOpportunityAssignmentForm(campaign.SenderPool))
			{
				form.Show();
				campaign.SenderPool.AddNew();
				campaign.SenderPool.AddNew();
				campaign.SenderPool.AddNew();
				form.Close();

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length);

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertContains("Message text", "There are errors in Staff Assignment Pool. Please correct them first.", message.Text);

				Assert(form.Visible);
			}
		}
	}
}
