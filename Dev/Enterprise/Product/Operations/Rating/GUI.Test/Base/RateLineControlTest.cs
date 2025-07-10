using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateLineControlTest : TestCaseWithFactory
	{
		public void TestOverrideDescCheckBoxChecked()
		{
			BulkRateUpdater updater = new BulkRateUpdater();

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
			RateEntry entry1 = rate.AddRateEntry("ORG");
			RateLine rateLine1 = entry1.RateLines.AddNew();

			Costing cost = Helper.NewCosting(Helper.NewOrgHeader());
			RateEntry entry2 = cost.AddRateEntry("ORG");
			RateLine rateLine2 = entry2.RateLines.AddNew();

			CompanyTariff tariff = Helper.NewCompanyTariff();
			RateEntry entry3 = tariff.AddRateEntry("ORG");
			RateLine rateLine3 = entry3.RateLines.AddNew();

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());
			RateEntry entry4 = quote.AddRateEntry("ORG");
			RateLine rateLine4 = entry4.RateLines.AddNew();

			IntercompanyTariff intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			RateEntry entry5 = intercompanyTariff.AddRateEntry("ORG");
			RateLine rateLine5 = entry5.RateLines.AddNew();

			updater.Entries.Add(entry1);
			updater.Entries.Add(entry2);
			updater.Entries.Add(entry3);
			updater.Entries.Add(entry4);
			updater.Entries.Add(entry5);

			using (ZForm form = new ZForm(updater))
			{
				RateLineControlForTest control = new RateLineControlForTest();
				control.SetDataBinding(updater, "ActionsLine");
				form.Controls.Add(control);
				form.Show();

				entry1.IncludeInUpdate = true;
				Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed = false;
				control.OverrideDescCheckBox.Checked = true;
				AssertEquals("Unchecked since no security rights", false, control.OverrideDescCheckBox.Checked);
				string expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Client Rates -> Charge Description Override Allowed";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed = true;

				entry2.IncludeInUpdate = true;
				Env.Security.CostingRatesChargeDescriptionOverride.IsAllowed = false;
				control.OverrideDescCheckBox.Checked = true;
				AssertEquals("Unchecked since no security rights", false, control.OverrideDescCheckBox.Checked);
				expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Costing -> Charge Description Override Allowed";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.CostingRatesChargeDescriptionOverride.IsAllowed = true;

				entry3.IncludeInUpdate = true;
				Env.Security.CompanyTariffRatesChargeDescriptionOverride.IsAllowed = false;
				control.OverrideDescCheckBox.Checked = true;
				AssertEquals("Unchecked since no security rights", false, control.OverrideDescCheckBox.Checked);
				expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Company Tariffs -> Charge Description Override Allowed";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.CompanyTariffRatesChargeDescriptionOverride.IsAllowed = true;

				entry4.IncludeInUpdate = true;
				Env.Security.QuotationChargeDescriptionOverride.IsAllowed = false;
				control.OverrideDescCheckBox.Checked = true;
				AssertEquals("Unchecked since no security rights", false, control.OverrideDescCheckBox.Checked);
				expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Quotations -> Charge Description Override Allowed";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.QuotationChargeDescriptionOverride.IsAllowed = true;

				entry5.IncludeInUpdate = true;
				Env.Security.IntercompanyTariffsChargeDescriptionOverride.IsAllowed = false;
				control.OverrideDescCheckBox.Checked = true;
				AssertEquals("Unchecked since no security rights", false, control.OverrideDescCheckBox.Checked);
				expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Tariffs & Rates -> Intercompany Tariffs -> Charge Description Override Allowed";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.IntercompanyTariffsChargeDescriptionOverride.IsAllowed = true;
			}
		}

		public void TestRateControlBinding_UnitFactor()
		{
			using (var wizard = new BulkUpdateWizard(RatingConstants.RatingHeaderTypes.IntercompanyTariff))
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateActionsPage);
				var updater = wizard.Updater;

				AssertNotNull("Prequisite:page", page);
				AssertNotNull("Prequisite:updater", updater);

				page.Visible = true;
				updater.Module = "FWD";
				updater.Type = "AIR";
				updater.Mode = "LSE";

				if (page.Controls.Find("rateLineControl", true).FirstOrDefault() is RateLineControl rateLineControl)
				{
					AssertUnitFactor(rateLineControl, expectIntercompanyTariffsTypeHeader: true, expectReadOnly: false);

					updater.ShowIntercompanyTariffs = false;
					updater.ShowClientRates = true;
					AssertUnitFactor(rateLineControl, expectIntercompanyTariffsTypeHeader: false, expectReadOnly: true);
				}
				else
				{
					Assert("rateLineControl is missing.", false);
				}
			}

			void AssertUnitFactor(RateLineControl rateLineControl, bool expectIntercompanyTariffsTypeHeader, bool expectReadOnly)
			{
				var unitFactorCtrl = (ZDropEdit)(rateLineControl.Find(c => c.Name == "UnitFactorDropEdit").FirstOrDefault());

				AssertNotNull("Prequisite:rateLineControl.BindingDataSource", rateLineControl.BindingDataSource);
				AssertNotNull("Prequisite:unitFactorCtrl", unitFactorCtrl);

				AssertEquals(expectIntercompanyTariffsTypeHeader, ((RateLine)rateLineControl.BindingDataSource).IsIntercompanyTariff());
				AssertEquals(expectReadOnly, unitFactorCtrl.ReadOnly);
			}
		}

		class RateLineControlForTest : RateLineControl
		{
			public RateLineControlForTest()
				: base() { }

			public new ZCheckBox OverrideDescCheckBox
			{
				get { return base.OverrideDescCheckBox; }
			}
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;
	}
}
