using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AccAllowedBranchDepartmentComboModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestDetachButton()
		{
			var branch = GetValidatedBranch();
			var allowedDepartment = branch.AllowedDepartments.AddNew();
			allowedDepartment.AAB_GE_Department = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			AssertEquals(1, branch.AllowedDepartments.Count);
			var mostRecentLog = branch.Logs.MostRecentLog;

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
				var grid = branchForm.Controls.Find("DepartmentsModuleButtonGrid", true)[0] as AccAllowedBranchDepartmentComboModuleButtonGrid;
				tabControl.SelectedIndex = 1;

				grid.SelectFirstRowIfOnlyRowInGrid();
				Assert(!branch.HasChanges);
				grid.DetachSelectedElement();
				branchForm.FireSaveButton();
				Assert(!branch.HasChanges);
			}

			AssertEquals("EDT", branch.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertNotEquals("new EDT log made", mostRecentLog.PK, branch.Logs.MostRecentLog.PK);
			AssertNotNull("Attached log remains because it was made in a previous save", branch.Logs.EarliestLogByEventTime(Events.EditedARecord, l => l.SL_Reference == $"Attached - ({GlbDepartment.CurrentDepartment.GE_Code}) {GlbDepartment.CurrentDepartment.GE_Desc}"));
			AssertNotNull(branch.Logs.EarliestLogByEventTime(Events.EditedARecord, l => l.SL_Reference == $"Detached - ({GlbDepartment.CurrentDepartment.GE_Code}) {GlbDepartment.CurrentDepartment.GE_Desc}"));
			AssertEquals("Detached", 0, branch.AllowedDepartments.Count);
		}

		public void TestAttachButton()
		{
			var branch = GetValidatedBranch();
			AssertEquals(0, branch.AllowedDepartments.Count);
			var attachedDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			var mostRecentLog = branch.Logs.MostRecentLog;

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				Assert(!branch.HasChanges);
				var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
				var grid = branchForm.Controls.Find("DepartmentsModuleButtonGrid", true)[0] as AccAllowedBranchDepartmentComboModuleButtonGrid;
				tabControl.SelectedIndex = 1;

				var attacher = new AccAllowedBranchDepartmentComboModuleAttacher(branch.AllowedDepartments, new GlbDepartmentCollection(Factory), ModuleIDs.GlbDepartment);
				attacher.Show(branchForm);
				attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { attachedDepartment });
				attacher.LastShownAttachPopupForTesting.Dispose();
				Assert(branch.HasChanges);
				branchForm.FireSaveButton();
				Assert(!branch.HasChanges);
			}

			AssertEquals("EDT", branch.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertNotEquals("new EDT log made", mostRecentLog.PK, branch.Logs.MostRecentLog.PK);
			AssertNotNull(branch.Logs.EarliestLogByEventTime(Events.EditedARecord, l => l.SL_Reference == $"Attached - ({GlbDepartment.CurrentDepartment.GE_Code}) {GlbDepartment.CurrentDepartment.GE_Desc}"));
			AssertNull(branch.Logs.EarliestLogByEventTime(Events.EditedARecord, l => l.SL_Reference == $"Detached - ({GlbDepartment.CurrentDepartment.GE_Code}) {GlbDepartment.CurrentDepartment.GE_Desc}"));
			AssertEquals("Attached", 1, branch.AllowedDepartments.Count);
			AssertEquals(branch.AllowedDepartments[0].AAB_GE_Department, attachedDepartment.PK);
		}

		public void TestOpenAttachedDepartment()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var attachedDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);

			using (var branchForm = new GlbBranchForm(branch))
			{
				var phone = branch.GB_Phone;
				var postCode = branch.GB_PostCode;
				var state = branch.GB_State;
				try
				{
					branch.GB_Phone = "+61 7 3268 2903";
					branch.GB_PostCode = "4011";
					branch.GB_State = "QLD";

					branchForm.Show();
					var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
					var grid = branchForm.Controls.Find("DepartmentsModuleButtonGrid", true)[0] as AccAllowedBranchDepartmentComboModuleButtonGrid;
					tabControl.SelectedIndex = 1;

					var attacher = new AccAllowedBranchDepartmentComboModuleAttacher(branch.AllowedDepartments, new GlbDepartmentCollection(Factory), ModuleIDs.GlbDepartment);
					attacher.Show(branchForm);
					attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { attachedDepartment });
					attacher.LastShownAttachPopupForTesting.Dispose();
					branchForm.FireSaveButton();

					grid.InnerGrid.PerformMouseDownForTest(0, 2);
					AssertNotEquals(ZControllerFactory.Create(ControllerIDs.GlbDepartment).AlreadyDeletedOrIrreversiblyChangedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					((ZForm)grid.LastShownZForm).Close();
				}
				finally
				{
					branch.GB_Phone = phone;
					branch.GB_PostCode = postCode;
					branch.GB_State = state;
				}
			}
		}

		GlbBranch GetValidatedBranch()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			branch.GB_Phone_Formatted = "0455555555";
			branch.GB_Address1 = "72 O'RIORDAN STREET";
			branch.GB_Address2 = "WISETECH GLOBAL";
			branch.GB_PostCode = "2015";
			branch.GB_City = "ALEXANDRIA";
			branch.GB_State = "NSW";
			branch.GB_RN_NKCountryCode = "AU";
			branch.Validation.ValidateAll();
			Factory.Save();
			return branch;
		}
	}
}
