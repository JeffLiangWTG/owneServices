using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DivideAllLegacyEstimatedValuesByTwelveControllerTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestShow()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var legacyValue1A = opp1.ValueItems.AddNew();
			legacyValue1A.PV_Value = 12;
			var legacyValue1B = opp1.ValueItems.AddNew();
			legacyValue1B.PV_Value = 120;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var legacyValue2A = opp2.ValueItems.AddNew();
			legacyValue2A.PV_Value = 18;
			var legacyValue2B = opp2.ValueItems.AddNew();
			legacyValue2B.PV_Value = 180;

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var controller = new DivideAllLegacyEstimatedValuesByTwelveController();
			controller.Show();

			AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Divide all Legacy Estimated Values by 12", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(@"Are you sure you want to divide the legacy estimated values for all opportunities by 12?

Note: This does not recalculate the total estimated values.", UnitTestUserNotification.Instance.LastMessage.Text);

			var newFactory = new BusinessObjectFactory();
			var legacyValue1AInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue1A.PK);
			var legacyValue1BInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue1B.PK);
			var legacyValue2AInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue2A.PK);
			var legacyValue2BInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue2B.PK);
			CombineAssertions("Should not have updated values if user selects 'No'", () =>
			{
				AssertEquals("legacyValue1A", 12m, legacyValue1AInNewFactory.PV_Value);
				AssertEquals("legacyValue1B", 120m, legacyValue1BInNewFactory.PV_Value);
				AssertEquals("legacyValue2A", 18m, legacyValue2AInNewFactory.PV_Value);
				AssertEquals("legacyValue2B", 180m, legacyValue2BInNewFactory.PV_Value);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.Show();

			newFactory = new BusinessObjectFactory();
			legacyValue1AInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue1A.PK);
			legacyValue1BInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue1B.PK);
			legacyValue2AInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue2A.PK);
			legacyValue2BInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValue2B.PK);
			CombineAssertions("Should have updated values if user selects 'Yes'", () =>
			{
				AssertEquals("legacyValue1A", 1m, legacyValue1AInNewFactory.PV_Value);
				AssertEquals("legacyValue1B", 10m, legacyValue1BInNewFactory.PV_Value);
				AssertEquals("legacyValue2A", 1.5m, legacyValue2AInNewFactory.PV_Value);
				AssertEquals("legacyValue2B", 15m, legacyValue2BInNewFactory.PV_Value);
			});
			AssertEquals("Divide all Legacy Estimated Values by 12 Completed.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			controller.Show();
			AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Cannot divide all Legacy Estimated Values by 12", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(@"This has already been run previously. This is a one-off function.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShow_Concurrency()
		{
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			var legacyValueA = opp.ValueItems.AddNew();
			legacyValueA.PV_Value = 12;
			var legacyValueB = opp.ValueItems.AddNew();
			legacyValueB.PV_Value = 120;

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var controller = new DivideAllLegacyEstimatedValuesByTwelveController_WithAConcurrentDivideAllLegacyEstimatedValuesBy12();
			controller.Show();

			AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Cannot divide all Legacy Estimated Values by 12", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals(@"Another user has run this function at the same time.", UnitTestUserNotification.Instance.LastMessage.Text);

			var newFactory = new BusinessObjectFactory();
			var legacyValueAInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValueA.PK);
			var legacyValueBInNewFactory = newFactory.Load<OrgOpportunityValue>(legacyValueB.PK);
			CombineAssertions("Should not have updated values", () =>
			{
				AssertEquals("legacyValueA", 12m, legacyValueAInNewFactory.PV_Value);
				AssertEquals("legacyValueB", 120m, legacyValueBInNewFactory.PV_Value);
			});
		}
	}
}
