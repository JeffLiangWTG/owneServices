using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefComplianceCommodityAlertModule))]
	sealed class RefComplianceCommodityAlertModuleTest : ZModuleBasherTest
	{
		public void TestLicenseCheckpoint()
		{
			using var module = ZModuleFactory.Instance.Create(GetModuleID());
			AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using var module = new RefComplianceCommodityAlertModuleForTest();
			using var filterControl = module.GetNewFilterControlForTest();
			AssertEquals("Type of filter control should be RefComplianceCommodityAlertFilterControl", true, filterControl is RefComplianceCommodityAlertFilterControl);
		}
		public void TestGetNewGridCollection()
		{
			using var module = new RefComplianceCommodityAlertModuleForTest();
			var complianceCommodityAlertCollection = module.GetNewGridCollectionForTest();
			AssertEquals("Type of grid collection should be RefComplianceCommodityAlertCollection", true, complianceCommodityAlertCollection is RefComplianceCommodityAlertCollection);
		}

		public void TestGetNewFilterBusinessObject()
		{
			using var module = new RefComplianceCommodityAlertModuleForTest();
			var filterBusinessObject = module.GetNewFilterBusinessObjectForTest();
			AssertEquals("Type of filter bizO should be RefComplianceCommodityAlertFilterBusinessObject", true, filterBusinessObject is RefComplianceCommodityAlertFilterBusinessObject);
		}

		public void TestSecurityCheckpoint()
		{
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			security.RefComplianceCommodityAlert.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var module = new RefComplianceCommodityAlertModuleForTest())
			{
				AssertEquals(Env.Security.RefComplianceCommodityAlert, module.SecurityCheckpoint);
				AssertEquals(false, module.SecurityCheckpoint.IsAllowed);
			}
		}

		public void TestSetRiskStatus()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.RefComplianceCommodityAlert.IsAllowed = true;

			using (var moduleForTest = new RefComplianceCommodityAlertModuleForTest())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var alert1 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
				alert1.RCR_CountryRegion = "AU";
				alert1.RCR_CommodityRiskStatus = "PRS";
				var alert2 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
				alert2.RCR_CountryRegion = "US";
				alert2.RCR_CommodityRiskStatus = "PRS";
				var alert3 = Factory.NewWithValidTestData<RefComplianceCommodityAlert>();
				alert3.RCR_CountryRegion = "CA";
				alert3.RCR_CommodityRiskStatus = "PRS";

				Factory.Save();

				var filter = new ZQuery(RefComplianceCommodityAlertSchema.RCR_CountryRegion, "AU");
				filter.AddToFilter(JoinCondition.Or, RefComplianceCommodityAlertSchema.RCR_CountryRegion, "US");
				filter.AddToFilter(JoinCondition.Or, RefComplianceCommodityAlertSchema.RCR_CountryRegion, "CA");
				var orgHeaderCollection = new RefComplianceCommodityAlertCollection(Factory, filter);
				Factory.Save();

				moduleForTest.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");
				moduleForTest.Grid_Exposed.Select(0);
				var countryRegionColumn = moduleForTest.Grid_Exposed.Columns.Where(x => x.ColumnName.Equals("RCR_CountryRegion"));
				var setRiskStatusToHSKText = moduleForTest.SetRiskStatusToHighRiskMenuItem_Exposed.Text;
				var setRiskStatusToPRSText = moduleForTest.SetRiskStatusToPossibleRiskMenuItem_Exposed.Text;

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals("Set Risk Status to High Risk", setRiskStatusToHSKText);
					AssertEquals("Set Risk Status to Possible Risk", setRiskStatusToPRSText);
					AssertEquals(1, countryRegionColumn.Count());
					AssertEquals("Country/Region", countryRegionColumn.First().ColumnStyle.HeaderText);
					AssertEquals(3, moduleForTest.Grid_Exposed.ListManager.Count);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				moduleForTest.SetRiskStatusToHighRiskMenuItem_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("HSK", alert1.RCR_CommodityRiskStatus);
					AssertEquals("PRS", alert2.RCR_CommodityRiskStatus);
					AssertEquals("PRS", alert3.RCR_CommodityRiskStatus);
				});

				moduleForTest.Grid_Exposed.Select(0);
				moduleForTest.SetRiskStatusToPossibleRiskMenuItem_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("PRS", alert1.RCR_CommodityRiskStatus);
					AssertEquals("PRS", alert2.RCR_CommodityRiskStatus);
					AssertEquals("PRS", alert3.RCR_CommodityRiskStatus);
				});

				moduleForTest.Grid_Exposed.SelectAllElements();
				moduleForTest.SetRiskStatusToHighRiskMenuItem_Exposed.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("HSK", alert1.RCR_CommodityRiskStatus);
					AssertEquals("HSK", alert2.RCR_CommodityRiskStatus);
					AssertEquals("HSK", alert3.RCR_CommodityRiskStatus);
				});
			}
		}

		public void TestSetRiskStatusWhenNoItemSelected()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.RefComplianceCommodityAlert.IsAllowed = true;

			using (var moduleForTest = new RefComplianceCommodityAlertModuleForTest())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				moduleForTest.OnSetRiskStatus_Exposed("HSK");

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSetRiskStatusNotAllowedWhenNoSecurity()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.RefComplianceCommodityAlert.IsAllowed = false;

			using (var moduleForTest = new RefComplianceCommodityAlertModuleForTest())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var menuItems = moduleForTest.GetNewActionMenuItemsForTest();

				AssertEquals("Should not have action item due to security.", false, menuItems.Any(u => u.Text == moduleForTest.SetRiskStatusToHighRiskMenuItem_Exposed.Text));
				AssertEquals("Should not have action item due to security.", false, menuItems.Any(u => u.Text == moduleForTest.SetRiskStatusToPossibleRiskMenuItem_Exposed.Text));
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefComplianceCommodityAlert;
		}

		#endregion
	}
}
