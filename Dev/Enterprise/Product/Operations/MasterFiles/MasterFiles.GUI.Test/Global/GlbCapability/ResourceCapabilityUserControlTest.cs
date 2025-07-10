using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ResourceCapabilityUserControlTest : TestCaseWithFactory
	{
		#region Detaching Capabilities From Staff Tests

		public void TestDetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			DetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast(allowed: true, "Do you still want to remove the capabilities from this staff member?", MessageBoxButtons.YesNo);
		}

		public void TestDetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast_AndOperationIsNotAllowed()
		{
			DetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast(allowed: false, "You do not have the relevant permission to make this change.", MessageBoxButtons.OK);
		}

		public void DetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast(bool allowed, string message, MessageBoxButtons buttons)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			// see corresponding test in MasterFiles (GetGroupsWithLastStaffPerGRPCapabilityTest)
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = false;
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);
			staff1.Groups.Add(group3);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability2);
			staff3.Groups.Add(group2);

			Factory.Save();

			using (var control = new ResourceCapabilityUserControl())
			{
				control.SetDataBinding(staff1, "");
				control.Show();
				control.CapabilityGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbCapability[] { capability1, capability2 }));

				AssertEquals(@"- This staff member is the last member with capability C1 in group(s) G2, G3.
- This staff member is the last member with capability C2 in group(s) G1, G3.

Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. " + message, control.CapabilityGrid.DetachMessage.Caption);

				AssertEquals(control.MessageBoxButtons, buttons);
			}
		}

		public void TestDetachingCapabilities_ShouldShowDefaultMessage_WhenStaffIsNotLast()
		{
			DetachingCapabilities_ShouldShowDefaultMessage_WhenStaffIsNotLast(allowed: true);
		}

		public void TestDetachingCapabilities_ShouldShowDefaultMessage_WhenStaffIsNotLast_AndOperationIsNotAllowed()
		{
			DetachingCapabilities_ShouldShowDefaultMessage_WhenStaffIsNotLast(allowed: false);
		}

		public void DetachingCapabilities_ShouldShowDefaultMessage_WhenStaffIsNotLast(bool allowed)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.G4_Code = "C1";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var control = new ResourceCapabilityUserControl())
			{
				control.SetDataBinding(staff1, "");
				control.Show();
				control.CapabilityGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbCapability[] { capability1 }));

				AssertEquals("Are you sure you want to detach the selected capabilities?", control.CapabilityGrid.DetachMessage.Caption);
				AssertEquals(control.MessageBoxButtons, MessageBoxButtons.YesNo);
			}
		}

		public void TestDetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast_OnlyForNonSecurityGroups()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = true;
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);
			staff1.Groups.Add(group3);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability2);
			staff3.Groups.Add(group2);

			Factory.Save();

			using (var control = new ResourceCapabilityUserControl())
			{
				control.SetDataBinding(staff1, "");
				control.Show();
				control.CapabilityGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbCapability[] { capability1, capability2 }));

				AssertEquals(@"- This staff member is the last member with capability C1 in group(s) G3.
- This staff member is the last member with capability C2 in group(s) G3.

Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the capabilities from this staff member?", control.CapabilityGrid.DetachMessage.Caption);
			}
		}

		public void TestDetachingCapabilities_ShouldShowWarningMessage_WhenStaffIsLast_ForAllGroups()
		{
			WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = true;
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group2);
			staff1.Groups.Add(group3);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability2);
			staff3.Groups.Add(group2);

			Factory.Save();

			using (var control = new ResourceCapabilityUserControl())
			{
				control.SetDataBinding(staff1, "");
				control.Show();
				control.CapabilityGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new GlbCapability[] { capability1, capability2 }));

				AssertEquals(@"- This staff member is the last member with capability C1 in group(s) G2, G3.
- This staff member is the last member with capability C2 in group(s) G1, G3.

Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the capabilities from this staff member?", control.CapabilityGrid.DetachMessage.Caption);
			}
		}

		public void TestDeleteKey_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ResourceCapabilityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(staff1, "");
				control.CapabilityGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(control.CapabilityGrid.InnerGrid, Keys.Delete);
				Application.DoEvents();

				AssertEquals(@"- This staff member is the last member with capability C1 in group(s) G1.
- This staff member is the last member with capability C2 in group(s) G1.

Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the capabilities from this staff member?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestDeleteKey_ShouldNotRemoveGroup_WhenResponseIsNo()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ResourceCapabilityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(staff1, "");
				control.CapabilityGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(control.CapabilityGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Application.DoEvents();

				AssertEquals("Capabilities should not be detached", 2, staff1.Capabilities.Count);
			}
		}

		public void TestDeleteKey_ShouldRemoveGroup_WhenResponseIsYes()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability2);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ResourceCapabilityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(staff1, "");
				control.CapabilityGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(control.CapabilityGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();

				AssertEquals("Capabilities should be detached", 0, staff1.Capabilities.Count);
			}
		}

		public void TestDeleteMenuItemClick_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new ResourceCapabilityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(staff1, "");
				control.CapabilityGrid.InnerGrid.SelectAllElements();
				control.CapabilityGrid.InnerGrid.DeleteMenuItem.PerformClick();

				AssertEquals(@"- This staff member is the last member with capability C1 in group(s) G1.

Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the capabilities from this staff member?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}
}
