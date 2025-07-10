using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class BulkUpdateFinishPageTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageInfo()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				var page = wizard.Pages.First((p) => p is BulkUpdateFinishPage) as BulkUpdateFinishPage;
				AssertNotNull("Prequisite", page);
				wizard.Show();
				page.Visible = true;

				var updater = page.CurrentDataItem as BulkRateUpdater;
				updater.ShowClientRates = true;

				page.NotifyActivated(wizard);
				AssertEquals("Update completed", page.Title);
				AssertStartsWith("Expected message to be shown", "You have successfully updated 0 rate(s).", page.Description);
			}
		}
	}
}
