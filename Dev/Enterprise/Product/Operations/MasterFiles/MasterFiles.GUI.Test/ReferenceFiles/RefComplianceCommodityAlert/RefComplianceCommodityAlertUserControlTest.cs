using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefComplianceCommodityAlertUserControlTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			var alertItem = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();

			alertItem.RCR_SourceURL = "/?tariff=040490&c=CA&impexp=E&tab=exportControl2022";

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new RefComplianceCommodityAlertUserControlForTest())
			{
				form.SetDataBinding(alertItem, ".");
				form.Controls.Add(control);
				form.Show();

				const string expectedUrl = "https://app.borderwise.com/?tariff=040490&c=CA&impexp=E&tab=exportControl2022";

				AssertEquals("Type is RefComplianceCommodityAlert", typeof(RefComplianceCommodityAlert), control.DataSourceType);
				AssertEquals("Control elements display correct information", expectedUrl, control.SourceURLLinkLabel_Exposed.Text);

				control.SourceURLLinkLabel_Exposed.OnLinkClicked_Exposed(null);
				AssertEquals(expectedUrl, WebUrlLauncher.LastUrlLaunched);
			}
		}
	}
}
