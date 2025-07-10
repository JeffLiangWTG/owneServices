using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class AllocationAdjustmentDialogTest : BaseAgencyTest
	{
		public void TestConfirmAdjustAllocations_OK()
		{
			AllocationUsage usage = new AllocationUsage(55, 25, 75, 20, 200, 0, 0);
			AllocationUsage required = new AllocationUsage(2, 0, 0, 6, 0, 0);
			AllocationUsageSet set = new AllocationUsageSet(usage, Allocation);
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			bool result = AllocationAdjustmentDialog.ConfirmAdjustAllocations(set, required);
			AssertNotNull("Should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
			AssertType("Should have shown the correct form", typeof(AllocationAdjustmentDialog), ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have returned true", true, result);
			AllocationAdjustmentDialog dialog = (AllocationAdjustmentDialog)ZFormModaliser.LastFormShownDialogForTest;
			AllocationAdjustmentDetails details = (AllocationAdjustmentDetails)dialog.LastDataSourceForTest;
			AssertEquals("TotalRequired_TEU", 77m, details.TotalRequired_TEU);
			AssertEquals("TotalRequired_Tonnes", 206m, details.TotalRequired_Tonnes);
			AssertEquals("Allocated_TEU", 70m, details.Allocated_TEU);
			AssertEquals("Allocated_Tonnes", 200m, details.Allocated_Tonnes);
			AssertEquals("Should now be using the new over allocation percent", 10m, Allocation.E0_OverAllocationPercent);
		}

		public void TestConfirmAdjustAllocations_Cancel()
		{
			AllocationUsage usage = new AllocationUsage(55, 25, 75, 20, 200, 0, 0);
			AllocationUsage required = new AllocationUsage(2, 0, 0, 6, 0, 0);
			AllocationUsageSet set = new AllocationUsageSet(usage, Allocation);
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			bool result = AllocationAdjustmentDialog.ConfirmAdjustAllocations(set, required);
			AssertNotNull("Should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
			AssertType("Should have shown the correct form", typeof(AllocationAdjustmentDialog), ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have returned true", false, result);
			AllocationAdjustmentDialog dialog = (AllocationAdjustmentDialog)ZFormModaliser.LastFormShownDialogForTest;
			AllocationAdjustmentDetails details = (AllocationAdjustmentDetails)dialog.LastDataSourceForTest;
			AssertEquals("TotalRequired_TEU", 77m, details.TotalRequired_TEU);
			AssertEquals("TotalRequired_Tonnes", 206m, details.TotalRequired_Tonnes);
			AssertEquals("Allocated_TEU", 70m, details.Allocated_TEU);
			AssertEquals("Allocated_Tonnes", 200m, details.Allocated_Tonnes);
			AssertEquals("Should still be using default over allocation percent", true, Allocation.E0_UseDefaultOverAllocation);
		}

		public void TestUpdatePath_Success()
		{
			AssertEquals("precondition:", true, Allocation.E0_UseDefaultOverAllocation);
			AssertEquals("precondition:", 0m, Allocation.E0_OverAllocationPercent);
			AllocationUsage usage = new AllocationUsage(51, 20, 15, 70, 200, 0);
			AllocationAdjustmentDetails details = new AllocationAdjustmentDetails(Allocation, usage);
			using (AllocationAdjustmentDialog dialog = new AllocationAdjustmentDialog(details))
			{
				dialog.Show();
				details.Login = EnvProxy.Instance.CurrentUser.LoginName;
				details.Password = User.MasterPassword;
				dialog.ClickOkForTesting();
				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
				AssertEquals("Should have shown no further dialogs", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestUpdatePath_NoPermission()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			TwoWayEncoder encoder = new TwoWayEncoder(staff.PK.ToGuid());
			staff.GS_LoginName = "joe.random";
			staff.GS_Code = "ZAC";
			staff.StaffPlainTextPassword = "random";
			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();
			AssertEquals("precondition:", true, Allocation.E0_UseDefaultOverAllocation);
			AssertEquals("precondition:", 0m, Allocation.E0_OverAllocationPercent);
			AllocationUsage usage = new AllocationUsage(51, 20, 15, 70, 200, 0);
			AllocationAdjustmentDetails details = new AllocationAdjustmentDetails(Allocation, usage);
			using (AllocationAdjustmentDialog dialog = new AllocationAdjustmentDialog(details))
			{
				dialog.Show();
				details.Login = "joe.random";
				details.Password = "random";
				dialog.ClickOkForTesting();
				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error joe.random is not authorized to adjust the allocations.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestUpdatePath_InvalidLogin()
		{
			AssertEquals("precondition:", true, Allocation.E0_UseDefaultOverAllocation);
			AssertEquals("precondition:", 0m, Allocation.E0_OverAllocationPercent);
			AllocationUsage usage = new AllocationUsage(51, 20, 15, 70, 200, 0);
			AllocationAdjustmentDetails details = new AllocationAdjustmentDetails(Allocation, usage);
			using (AllocationAdjustmentDialog dialog = new AllocationAdjustmentDialog(details))
			{
				dialog.Show();
				details.Login = "BOB";
				details.Password = "BLAH";
				dialog.ClickOkForTesting();
				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error Invalid username / password.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestUpdatePath_Errors()
		{
			AssertEquals("precondition:", true, Allocation.E0_UseDefaultOverAllocation);
			AssertEquals("precondition:", 0m, Allocation.E0_OverAllocationPercent);
			AllocationUsage usage = new AllocationUsage(51, 20, 15, 70, 200, 0);
			AllocationAdjustmentDetails details = new AllocationAdjustmentDetails(Allocation, usage);
			using (AllocationAdjustmentDialog dialog = new AllocationAdjustmentDialog(details))
			{
				dialog.Show();
				details.Login = "";
				details.Password = "";
				dialog.ClickOkForTesting();
				AssertEquals("DialogResult", DialogResult.None, dialog.DialogResult);
				AssertEquals("Should have shown an error", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#region Implementation
		SlotAllocation Allocation
		{
			get
			{
				if (allocation == null)
				{
					allocation = Factory.New<JobVoyage>().Countries.GetCountry("AU", true).SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 70);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 200);
				}

				return allocation;
			}
		}

		SlotAllocation allocation;
		#endregion
	}
}
