using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI
{
	[TestedType(typeof(OpportunityProspectiveTradeProfileForm))]
	class OpportunityProspectiveTradeProfileFormTest : ZFormBasherTest
	{
		public void TestCheckCanSave_ValidatesLegacyOpportunityValue_ButNotOpportunity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_Status = "INV";
			opportunity.FillWithValidTestData();
			AssertHasErrors("Precondition: should have error", opportunity.P8_StatusInfo);
			Factory.Save();

			using (var form = new OpportunityProspectiveTradeProfileForm(opportunity))
			{
				form.Show();

				var valueItem = opportunity.ValueItems.AddNew();
				valueItem.PV_RevenueType = "XXX";
				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				valueItem.PV_RevenueType = valueItem.Lookups.ActiveValueTypes.Cast<ICodeDescription>().First().Code;
				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormShouldBeSavableAfterAddingNewProduct()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			Factory.Save();

			using (var form = new OpportunityProspectiveTradeProfileForm(opportunity))
			{
				form.Show();

				AssertEquals(ODisplayMode.Browse, form.DisplayMode);

				var newProduct = Factory.New<OrgSalesProduct>();
				form.Focus(newProduct, true);

				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
			}
		}

		public void TestLicenceCheckpointShouldTriggerOnInitialize()
		{
			Env.Licence.SalesValueAnalysis.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);

			var initialVALLicenceUsageCount = GetSalesValueAnalysisLicenceUsageCount();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			Factory.Save();

			AssertEquals("No new SalesValueAnalysis licence usage", initialVALLicenceUsageCount, GetSalesValueAnalysisLicenceUsageCount());

			try
			{
				using (var form = new OpportunityProspectiveTradeProfileForm(opportunity))
				{
					form.Show();

					AssertEquals("One new SalesValueAnalysis licence usage", initialVALLicenceUsageCount + 1, GetSalesValueAnalysisLicenceUsageCount());
				}
			}
			finally
			{
				Env.Licence.SalesValueAnalysis.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);
			}

			AssertEquals("No new SalesValueAnalysis licence usage", initialVALLicenceUsageCount + 1, GetSalesValueAnalysisLicenceUsageCount());
		}

		int GetSalesValueAnalysisLicenceUsageCount()
		{
			var filter = new ZQuery(StmActivityLogSchema.S7_FormCaption, SQLComparisonOperator.Equal, Env.Licence.SalesValueAnalysis.Name);
			return Factory.GetDatabaseCount(typeof(StmActivityLog), filter);
		}

		protected override Form GetFormToBashCore()
		{
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			return new OpportunityProspectiveTradeProfileForm(opportunity);
		}
	}
}
