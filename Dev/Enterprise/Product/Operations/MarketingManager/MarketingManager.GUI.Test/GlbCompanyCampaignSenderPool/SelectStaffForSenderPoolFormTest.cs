using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SelectStaffForSenderPoolForm))]
	class SelectStaffForSenderPoolFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SelectStaffForSenderPoolForm(Factory.NewWithValidTestData<GlbCompanyCampaign>().SenderPool);
		}

		public void TestCloseWithErrorsInPool()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var frm = new SelectStaffForSenderPoolForm(campaign.SenderPool))
			{
				frm.Show();
				campaign.SenderPool.AddNew();
				campaign.SenderPool.AddNew();
				campaign.SenderPool.AddNew();
				frm.Close();

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertStartsWith("Message text", "There are errors in Sender Pool. Please correct them first.", message.Text);

				AssertEquals(true, frm.Visible);
			}
		}
	}
}
