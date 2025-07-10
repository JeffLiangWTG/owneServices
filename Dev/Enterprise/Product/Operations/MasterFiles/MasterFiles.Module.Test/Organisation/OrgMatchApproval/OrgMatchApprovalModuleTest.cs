using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgMatchApprovalModule))]
	sealed class OrgMatchApprovalModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgMatchApproval;
		}

		public void TestCheckpoints()
		{
			using (OrgMatchApprovalModule module = new OrgMatchApprovalModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.OrgMatchApproval, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestEditActionCaptionOverridden()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				AssertNotNull("Edit menu item should be renamed to 'Review'", module.FormActionMenu.FindByText("R&eview"));
			}
		}

		public void TestDontAllowNew()
		{
			using (OrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				AssertEquals("AllowNew false", false, module.AllowNew);
			}
		}

		public void TestDontAllowDelete()
		{
			using (OrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				AssertEquals("AllowDelete false", false, module.AllowDelete);
			}
		}

		public void TestDontDefaultSortOrder()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				AssertEquals("AllowDelete false", null, module.DefaultSortOrder);
			}
		}

		public void TestGetNewGridCollection()
		{
			OrgMatchApprovalCollection matchApprovals = new OrgMatchApprovalCollection(Factory);
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);

			DummyBusinessObjectAutoLogged dummyParent1 = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval1 = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent1.PK, OrgMatchApprovalType.DummyType);
			DummyBusinessObjectAutoLogged dummyParent3 = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval3 = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent3.PK, OrgMatchApprovalType.DummyType);
			DummyBusinessObjectAutoLogged dummyParent2 = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval2 = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent2.PK, OrgMatchApprovalType.DummyType2);

			Factory.Save();

			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				OrgMatchApprovalCollection loadedMatchApprovals = (OrgMatchApprovalCollection)module.GridCollection;
				loadedMatchApprovals.Load();
				AssertEquals(3, loadedMatchApprovals.Count);
				AssertNotNull(loadedMatchApprovals.FindByPK(matchApproval1.PK));
				AssertNotNull(loadedMatchApprovals.FindByPK(matchApproval2.PK));
				AssertNotNull(loadedMatchApprovals.FindByPK(matchApproval3.PK));
			}
		}

		public void TestDontAllowCollectionSort()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				PropertyInfo allowSortProperty = typeof(BusinessObjectCollection).GetProperty("AllowSort", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				bool allowSort = (bool)allowSortProperty.GetValue(module.GridCollection, null);
				AssertEquals("Collection AllowSort should be false so that we use the our custom sort by date/group by match type comparer always", false, allowSort);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgMatchApprovalFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollectionType()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				Assert("Invalid type", module.NewGridCollection is OrgMatchApprovalModule.ModuleOrgMatchApprovalCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is OrgMatchApprovalFilterBusinessObject);
			}
		}

		public void TestIsUnmatchForCurrentUserOnly()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				OrgMatchApprovalFilterBusinessObject filterBusinessObject = (OrgMatchApprovalFilterBusinessObject)module.FilterBusinessObject;
				((ModuleTextFilter)filterBusinessObject["Tracking Number"]).Property = "splaty";
				((ModuleTextFilter)filterBusinessObject["Tracking Number"]).IsActive = true;

				AssertEquals(false, module.IsUnmatchForCurrentUserOnly);

				filterBusinessObject.RevertFilterToUnmatchedByCurrentUser();

				AssertEquals(true, module.IsUnmatchForCurrentUserOnly);
			}
		}

		[RequiresSTA]
		public void TestRevertFilterToUnmatchedByCurrentUser()
		{
			OrgMatchApproval dummyMatchApproval = NewDummyMatchApproval();

			Factory.Save();

			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				OrgMatchApprovalFilterBusinessObject filterBusinessObject = (OrgMatchApprovalFilterBusinessObject)module.FilterBusinessObject;
				((ModuleTextFilter)filterBusinessObject["Tracking Number"]).Property = "SomeReferenceNumber111";
				((ModuleTextFilter)filterBusinessObject["Tracking Number"]).IsActive = true;
				((ModuleFlagsFilter)filterBusinessObject["Status"]).Property0 = false;
				((ModuleFlagsFilter)filterBusinessObject["Status"]).IsActive = true;

				module.PerformSearch();

				AssertEquals("Grid collection should return no records initially for the test", 0, module.GridCollection.Count);

				module.RevertFilterToUnmatchedByCurrentUser();

				AssertEquals("Filter business object should be reverted back to unmatched by current user", "", ((ModuleTextFilter)filterBusinessObject["Tracking Number"]).Property);
				AssertEquals("Filter business object should be reverted back to unmatched by current user", true, ((ModuleFlagsFilter)filterBusinessObject["Status"]).Property0);
				AssertEquals("Grid collection should be refreshed with the newly assigned filter", true, module.GridCollection.Count >= 1);
			}
		}

		[RequiresSTA]
		public void TestRevertFilterToUnmatchedByCurrentUserAfterWarningUser()
		{
			OrgMatchApproval dummyMatchApproval = NewDummyMatchApproval();
			Factory.Save();

			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				OrgMatchApprovalFilterBusinessObject filterBusinessObject = (OrgMatchApprovalFilterBusinessObject)module.FilterBusinessObject;
				((ModuleTextFilter)filterBusinessObject["Tracking Number"]).Property = "original value";
				((ModuleTextFilter)filterBusinessObject["Tracking Number"]).IsActive = true;
				((ModuleFlagsFilter)filterBusinessObject["Status"]).Property0 = false;
				((ModuleFlagsFilter)filterBusinessObject["Status"]).IsActive = true;

				module.PerformSearch();

				AssertEquals("Grid collection should return no records initially for the test", 0, module.GridCollection.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("User selects no, return false", false, module.RevertFilterToUnmatchedByCurrentUserAfterWarningUser());
				AssertEquals("There should be no change as the user selected NO", "original value", ((ModuleTextFilter)filterBusinessObject["Tracking Number"]).Property);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("User selects yes, return true", true, module.RevertFilterToUnmatchedByCurrentUserAfterWarningUser());
				AssertEquals("Filter business object should be reverted back to unmatched by current user", "", ((ModuleTextFilter)filterBusinessObject["Tracking Number"]).Property);
				AssertEquals("Filter business object should be reverted back to unmatched by current user", true, ((ModuleFlagsFilter)filterBusinessObject["Status"]).Property0);
				AssertEquals("Grid collection should be refreshed with the newly assigned filter", true, module.GridCollection.Count >= 1);
			}
		}

		public void TestRevertFilterToUnmatchedByCurrentUserAfterWarningUser_DontAskTheUserIfTheFilterIsAlreadyUnmatchedByCurrentUser()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				OrgMatchApprovalFilterBusinessObject filterBusinessObject = (OrgMatchApprovalFilterBusinessObject)module.FilterBusinessObject;
				filterBusinessObject.RevertFilterToUnmatchedByCurrentUser();

				AssertEquals("Filter is already set, result should be true", true, module.RevertFilterToUnmatchedByCurrentUserAfterWarningUser());
				AssertEquals("No message need to be shown to the user as the filter is already set", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestPerformSearch()
		{
			using (TestOrgMatchApprovalModule module = new TestOrgMatchApprovalModule())
			{
				OrgMatchApprovalFilterBusinessObject filterBusinessObject = (OrgMatchApprovalFilterBusinessObject)module.FilterBusinessObject;
				((ModuleFlagsFilter)filterBusinessObject["Status"]).Property0 = true;
				((ModuleFlagsFilter)filterBusinessObject["Status"]).IsActive = true;

				OrgMatchApproval dummyMatchApproval = NewDummyMatchApproval();
				Factory.Save();
				module.PerformSearch();

				OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
				dummyMatchApproval.ApproveMatchBySupervisor(organisation); // take the approval out of the grid filter
				Factory.Save();

				AssertEquals("Grid collection should have 1 record initially for the test", 1, module.GridCollection.Count);
				module.PerformSearch();
				AssertEquals("Grid collection should return no records after the refresh", 0, module.GridCollection.Count);
			}
		}

		#region Test Classes

		class TestOrgMatchApprovalModule : OrgMatchApprovalModule
		{
			public new BusinessObjectCollection GridCollection
			{
				get { return base.GridCollection; }
			}

			public new FilterBusinessObject FilterBusinessObject
			{
				get { return base.FilterBusinessObject; }
			}

			public new SortInfo DefaultSortOrder
			{
				get { return base.DefaultSortOrder; }
			}

			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}
		}

		class DummyBusinessObjectAutoLogged : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObjectAutoLogged(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;
		}

		#endregion

		#region Implementation

		OrgMatchApproval NewDummyMatchApproval()
		{
			DummyBusinessObjectAutoLogged dummyParent = Factory.New<DummyBusinessObjectAutoLogged>();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyOrgMatchApproval result = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(OrgMatchApproval.Schema.TableName);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return NewDummyMatchApproval();
		}

		#endregion
	}
}
