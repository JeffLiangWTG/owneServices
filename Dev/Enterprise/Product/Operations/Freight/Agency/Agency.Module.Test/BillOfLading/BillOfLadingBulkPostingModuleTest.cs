using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class BillOfLadingBulkPostingModuleTest : BulkPostingModuleTest
	{
		public void TestBulkPostingSecurityWithoutRights()
		{
			var parentCheckPoint = Env.Security.Operations;
			var rightsCheckpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.Bulk);
			var expected = "You do not have the appropriate security rights to run this function." + "\r\n\r\n" + "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:" + "\r\n\r\n" + "Operate -> Liner & Agency -> Bills of Lading -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost";
			var staff = GetStaff(parentCheckPoint, rightsCheckpoint, false);
			parentCheckPoint.IsAllowedForAllBranches = false;
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: Current user has changed", staff.PK, Env.CurrentUser.PK);
				using (var module = GetNewModuleForTest())
				using (ZForm form = new ZForm())
				{
					var bizo = Factory.NewWithValidTestData<BillOfLading>();
					Factory.Save();
					var billOfLadingModule = (BillOfLadingModule)module;
					var filterControl = (BillOfLadingFilterControl)billOfLadingModule.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();
					filterControl.Find();
					var grid = (ZDisplayGrid)billOfLadingModule.DisplayGrid;
					grid.SelectAllElements();
					AssertEquals("Some business object should be selected.", 1, billOfLadingModule.GetSelectedBusinessObjects().Length);
					var postMenuItem = MenuAssertion.AssertHasMenu(module.PostMenuItem as KMenuItem, "Post All Charges and Costs");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					postMenuItem.PerformClick();
					AssertEquals("Should be Access Denied", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestBulkPostingSecurityWithRights()
		{
			var parentCheckPoint = Env.Security.Operations;
			var rightsCheckpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.Bulk);
			var staff = GetStaff(parentCheckPoint, rightsCheckpoint, true);
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: Current user has changed", staff.PK, Env.CurrentUser.PK);
				using (var module = GetNewModuleForTest())
				using (ZForm form = new ZForm())
				{
					var bizo = Factory.NewWithValidTestData<BillOfLading>();
					Factory.Save();
					var billOfLadingModule = (BillOfLadingModule)module;
					var filterControl = (BillOfLadingFilterControl)billOfLadingModule.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();
					filterControl.Find();
					var grid = (ZDisplayGrid)billOfLadingModule.DisplayGrid;
					grid.SelectAllElements();
					AssertEquals("Some business object should be selected.", 1, billOfLadingModule.GetSelectedBusinessObjects().Length);
					var postMenuItem = MenuAssertion.AssertHasMenu(module.PostMenuItem as KMenuItem, "Post All Charges and Costs");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					postMenuItem.PerformClick();
					AssertContains("Should ask about posting and not about security rights.", "Are you sure you want to Post All Charges and Costs for this Bills of Lading?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (BillOfLadingModule)ZModuleFactory.Instance.Create(ModuleIDs.AgencyBillOfLading);
		}

		GlbStaff GetStaff(SecurityCheckpoint parentCheckpoint, SecurityCheckpoint rightsCheckpoint, bool isAllowed)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TestStaff";
			if (parentCheckpoint != null)
			{
				var security = Factory.NewWithValidTestData<GlbSecurity>();
				security.GU_SecurityItemIsAllowed = true;
				security.GU_SecurityRight = parentCheckpoint.Code;
				security.GU_GC = GlbCompany.CurrentCompany.PK;
				security.GU_GB = GlbBranch.CurrentBranch.PK;
				security.GU_GE = GlbDepartment.CurrentDepartment.PK;
				security.GU_GS = staff.PK;
			}

			if (rightsCheckpoint != null)
			{
				var security = Factory.NewWithValidTestData<GlbSecurity>();
				security.GU_SecurityItemIsAllowed = isAllowed;
				security.GU_SecurityRight = rightsCheckpoint.Code;
				security.GU_GC = GlbCompany.CurrentCompany.PK;
				security.GU_GB = GlbBranch.CurrentBranch.PK;
				security.GU_GE = GlbDepartment.CurrentDepartment.PK;
				security.GU_GS = staff.PK;
			}

			Factory.Save();
			return staff;
		}
		#endregion
	}
}
