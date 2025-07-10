using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(BulkUpdateWizard))]
	public class BulkUpdateWizardTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestCreateInstance()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();
				AssertDataSource(wizard);
			}

			using (var wizard = new BulkUpdateWizard("AAAA"))
			{
				wizard.Show();
				AssertDataSource(wizard);
			}

			using (var wizard = new BulkUpdateWizard(null))
			{
				wizard.Show();
				AssertDataSource(wizard);
			}

			using (var wizard = new BulkUpdateWizard(RatingConstants.RatingHeaderTypes.Quote))
			{
				wizard.Show();
				AssertDataSource(wizard);
				Assert((wizard.DataSource as BulkRateUpdater).ShowActiveQuotes);
			}

			using (var wizard = new BulkUpdateWizard(RatingConstants.RatingHeaderTypes.ClientRate))
			{
				wizard.Show();
				AssertDataSource(wizard);
				Assert((wizard.DataSource as BulkRateUpdater).ShowClientRates);
			}

			using (var wizard = new BulkUpdateWizard(RatingConstants.RatingHeaderTypes.Costing))
			{
				wizard.Show();
				AssertDataSource(wizard);
				Assert((wizard.DataSource as BulkRateUpdater).ShowCostings);
			}

			using (var wizard = new BulkUpdateWizard(RatingConstants.RatingHeaderTypes.Tariff))
			{
				wizard.Show();
				AssertDataSource(wizard);
				Assert((wizard.DataSource as BulkRateUpdater).ShowCompanyTariffs);
			}

			using (var wizard = new BulkUpdateWizard(RatingConstants.RatingHeaderTypes.IntercompanyTariff))
			{
				wizard.Show();
				AssertDataSource(wizard);
				Assert((wizard.DataSource as BulkRateUpdater).ShowIntercompanyTariffs);
			}
		}

		void AssertDataSource(BulkUpdateWizard wizard)
		{
			AssertNotNull(wizard.DataSource);
			AssertNotNull(wizard.Updater);
			AssertEquals(wizard.DataSource.GetType(), typeof(BulkRateUpdater));
			AssertEquals(wizard.Updater.GetType(), typeof(BulkRateUpdater));
			AssertEquals(wizard.DataSource, wizard.Updater);
		}

		public void TestPages()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				AssertEquals("Expected 6 pages", wizard.Pages.Count, 6);
				AssertEquals(wizard.Pages[0].GetType(), typeof(WizardPageStart));
				AssertEquals(wizard.Pages[1].GetType(), typeof(BulkUpdateFilterPage));
				AssertEquals(wizard.Pages[2].GetType(), typeof(BulkUpdateEntriesPage));
				AssertEquals(wizard.Pages[3].GetType(), typeof(BulkUpdateActionsPage));
				AssertEquals(wizard.Pages[4].GetType(), typeof(BulkUpdatePreviewPage));
				AssertEquals(wizard.Pages[5].GetType(), typeof(BulkUpdateFinishPage));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			BulkUpdateWizard frm = new BulkUpdateWizard();
			frm.Show();
			return frm;
		}
		#endregion
	}
}
