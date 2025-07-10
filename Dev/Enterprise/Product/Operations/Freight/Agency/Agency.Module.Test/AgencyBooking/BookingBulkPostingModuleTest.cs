using System.Windows.Forms;
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
	internal class BookingBulkPostingModuleTest : BulkPostingModuleTest
	{
		public void TestBulkPostingSecurityWithoutRights()
		{
			SecurityCheckpoint parentCheckPoint = Env.Security.Operations;
			SecurityCheckpoint rightsCheckpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBookingJobInvoicing, SecurityCore.Bulk);
			string expected = "You do not have the appropriate security rights to run this function." + "\r\n\r\n" + "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:" + "\r\n\r\n" + "Operate -> Liner & Agency -> Bookings -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost";
			GlbStaff staff = GetStaff(parentCheckPoint, rightsCheckpoint, false);
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: Current user has changed", staff.PK, Env.CurrentUser.PK);
				using (IBulkPostingModuleInternalsForTesting module = GetNewModuleForTest())
				using (ZForm form = new ZForm())
				{
					var bizo = Factory.NewWithValidTestData<AgencyBooking>();
					Factory.Save();
					var bookingModule = (BookingModule)module;
					var filterControl = (AgencyBookingFilterControl)bookingModule.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();
					filterControl.Find();
					var grid = (ZDisplayGrid)bookingModule.DisplayGrid;
					grid.SelectAllElements();
					AssertEquals("Some business object should be selected.", 1, bookingModule.GetSelectedBusinessObjects().Length);
					MenuItem postMenuItem = MenuAssertion.AssertHasMenu(module.PostMenuItem as MenuItem, "Post All Charges and Costs");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					postMenuItem.PerformClick();
					AssertEquals("Should be Access Denied", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestBulkPostingSecurityWithRights()
		{
			SecurityCheckpoint parentCheckPoint = Env.Security.Operations;
			SecurityCheckpoint rightsCheckpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBookingJobInvoicing, SecurityCore.Bulk);
			GlbStaff staff = GetStaff(parentCheckPoint, rightsCheckpoint, true);
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Precondition: Current user has changed", staff.PK, Env.CurrentUser.PK);
				using (IBulkPostingModuleInternalsForTesting module = GetNewModuleForTest())
				using (ZForm form = new ZForm())
				{
					var bizo = Factory.NewWithValidTestData<AgencyBooking>();
					Factory.Save();
					var bookingModule = (BookingModule)module;
					var filterControl = (AgencyBookingFilterControl)bookingModule.EmbeddedControl;
					form.Controls.Add(filterControl);
					form.Show();
					filterControl.Find();
					var grid = (ZDisplayGrid)bookingModule.DisplayGrid;
					grid.SelectAllElements();
					AssertEquals("Some business object should be selected.", 1, bookingModule.GetSelectedBusinessObjects().Length);
					MenuItem postMenuItem = MenuAssertion.AssertHasMenu(module.PostMenuItem as MenuItem, "Post All Charges and Costs");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					postMenuItem.PerformClick();
					AssertContains("Should ask about posting and not about security rights.", "Are you sure you want to Post All Charges and Costs for this Booking?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (BookingModule)ZModuleFactory.Instance.Create(ModuleIDs.AgencyBooking);
		}

		GlbStaff GetStaff(SecurityCheckpoint parentCheckpoint, SecurityCheckpoint rightsCheckpoint, bool isAllowed)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TestStaff";
			if (parentCheckpoint != null)
			{
				GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
				security.GU_SecurityItemIsAllowed = true;
				security.GU_SecurityRight = parentCheckpoint.Code;
				security.GU_GC = GlbCompany.CurrentCompany.PK;
				security.GU_GB = GlbBranch.CurrentBranch.PK;
				security.GU_GE = GlbDepartment.CurrentDepartment.PK;
				security.GU_GS = staff.PK;
			}

			if (rightsCheckpoint != null)
			{
				GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
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
