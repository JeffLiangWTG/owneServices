using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefComplianceListModule))]
	sealed class RefComplianceListModuleTest : ZModuleBasherTest
	{
		public void TestLicenseCheckpoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new RefComplianceListModule())
			{
				AssertEquals("SecurityCheckpoint should be RefComplianceList:", Env.Security.RefComplianceList, module.SecurityCheckpoint);
				AssertEquals(true, module.SecurityCheckpoint.ChildCheckPoints.Contains(Env.Security.RefComplianceListEdit));
				AssertEquals(true, module.SecurityCheckpoint.ChildCheckPoints.Contains(Env.Security.RefComplianceListView));
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new RefComplianceListModuleForTest())
			using (var filterControl = module.GetNewFilterControlForTest())
			{
				AssertEquals("Type of filter control should be RefComplianceListFilterControl", true, filterControl is RefComplianceListFilterControl);
			}
		}
		public void TestGetNewGridCollection()
		{
			using (var module = new RefComplianceListModuleForTest())
			{
				var complianceListCollection = module.GetNewGridCollectionForTest();
				AssertEquals("Type of grid collection should be RefComplianceListCollection", true, complianceListCollection is RefComplianceListCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new RefComplianceListModuleForTest())
			{
				var filterBusinessObject = module.GetNewFilterBusinessObjectForTest();
				AssertEquals("Type of filter bizo should be RefComplianceListFilterBusinessObject", true, filterBusinessObject is RefComplianceListFilterBusinessObject);
			}
		}

		[RequiresSTA]
		public void TestIncludeExcludeSelectedComplianceList()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.RefComplianceListEdit.IsAllowed = true;

			using (var moduleForTest = new RefComplianceListModuleForTest())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var complianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
				complianceList1.RCL_ListName = "THIS SHOULD BE A TEST ONLY RECORD1";
				var complianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
				complianceList2.RCL_ListName = "THIS SHOULD BE A TEST ONLY RECORD2";
				var complianceList3 = Factory.NewWithValidTestData<RefComplianceList>();
				complianceList3.RCL_ListName = "THIS SHOULD BE A TEST ONLY RECORD3";

				complianceList1.RCL_IsExcluded = true;
				complianceList2.RCL_IsExcluded = true;
				complianceList3.RCL_IsExcluded = true;

				Factory.Save();

				var filter = new ZQuery(RefComplianceListSchema.RCL_ListName, "THIS SHOULD BE A TEST ONLY RECORD1");
				filter.AddToFilter(JoinCondition.Or, RefComplianceListSchema.RCL_ListName, "THIS SHOULD BE A TEST ONLY RECORD2");
				filter.AddToFilter(JoinCondition.Or, RefComplianceListSchema.RCL_ListName, "THIS SHOULD BE A TEST ONLY RECORD3");
				var orgHeaderCollection = new RefComplianceListCollection(Factory, filter);
				Factory.Save();

				moduleForTest.Grid_Exposed.SetDataBinding(orgHeaderCollection, "");
				moduleForTest.Grid_Exposed.Select(0);
				var excludedIncludedColumn = moduleForTest.Grid_Exposed.Columns.Where(x => x.ColumnName.Equals("RCL_IsExcluded"));
				var excludeMenuItemText = moduleForTest.ExcludeComplianceListMenuItem_Exposed.Text;
				var includeMenuItemText = moduleForTest.IncludeComplianceListMenuItem_Exposed.Text;

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals("&Exclude Selected Lists", excludeMenuItemText);
					AssertEquals("&Include Selected Lists", includeMenuItemText);
					AssertEquals(1, excludedIncludedColumn.Count());
					AssertEquals("Is Excluded", excludedIncludedColumn.First().ColumnStyle.HeaderText);
					AssertEquals(3, moduleForTest.Grid_Exposed.ListManager.Count);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				moduleForTest.OnIncludeExclude_Exposed(false);

				CombineAssertions(() =>
				{
					AssertEquals(false, complianceList1.RCL_IsExcluded);
					AssertEquals(true, complianceList2.RCL_IsExcluded);
					AssertEquals(true, complianceList3.RCL_IsExcluded);
				});

				moduleForTest.Grid_Exposed.Select(0);
				moduleForTest.OnIncludeExclude_Exposed(true);

				CombineAssertions(() =>
				{
					AssertEquals(true, complianceList1.RCL_IsExcluded);
					AssertEquals(true, complianceList2.RCL_IsExcluded);
					AssertEquals(true, complianceList3.RCL_IsExcluded);
				});

				moduleForTest.Grid_Exposed.SelectAllElements();
				moduleForTest.OnIncludeExclude_Exposed(false);

				CombineAssertions(() =>
				{
					AssertEquals(false, complianceList1.RCL_IsExcluded);
					AssertEquals(false, complianceList2.RCL_IsExcluded);
					AssertEquals(false, complianceList3.RCL_IsExcluded);
				});
			}
		}

		[RequiresSTA]
		public void TestIncludeExcludeSelectedComplianceListWhenNoItemSelected()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.RefComplianceListEdit.IsAllowed = true;

			using (var moduleForTest = new RefComplianceListModuleForTest())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				moduleForTest.OnIncludeExclude_Exposed(true);

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
		public void TestIncludeExcludeSelectedComplianceListNotAllowedWhenNoSecurity()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.RefComplianceListEdit.IsAllowed = true;

			using (var moduleForTest = new RefComplianceListModuleForTest())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var menuItems = moduleForTest.GetNewActionMenuItemsForTest();

				AssertEquals("Should not have action item due to security.", false, menuItems.Contains(moduleForTest.IncludeComplianceListMenuItem_Exposed));
				AssertEquals("Should not have action item due to security.", false, menuItems.Contains(moduleForTest.ExcludeComplianceListMenuItem_Exposed));
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefComplianceList;
		}

		#endregion
	}
}
