using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCapabilityForm : ZTemplateForm
	{
		public GlbCapabilityForm(GlbCapability businessEntity)
			: base(businessEntity)
		{
			this.capability = businessEntity;

			CapabilityMembersGrid.BeforeDetach += CapabilityMembersGrid_BeforeDetaching;
			CapabilityMembersGrid.Detached += CapabilityMembersGrid_OnDetached;
			CapabilityMembersGrid.InnerGrid.RowsDeleting += CapabilityMembersGrid_InnerGrid_RowDeleteKeyDown;

			if (!DesignModeFinder.IsDesigning)
			{
				if (!ObjectFactory.Get<IBMSRegistry>().DisplayResponsiveReleaseGateUiSettings)
				{
					var releaseGroupColumnInfos = ReleaseGroupsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					ReleaseGroupsGrid.ColumnStyles.Remove(releaseGroupColumnInfos.Single(i => i.ColumnName == "GGC_CapabilityStartableWorkflowLimit"));
				}
			}
		}

		readonly GlbCapability capability;

		void MainTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			var isGroupScope = capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope;
			ReleaseGroupsGroupBox.Visible = isGroupScope;
			ReleaseGroupsInaccessibleLabel.Visible = !isGroupScope;
		}

		#region Entity Selection

		void StaffGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GridEntityFormOpener.OpenForm(CapabilityMembersGrid.InnerGrid, e, GetSelectedStaff, ControllerIDs.GlbStaff);
		}

		BusinessObject GetSelectedStaff()
		{
			return CapabilityMembersGrid.InnerGrid.GetCurrent();
		}

		#endregion

		#region Detaching Staff From Capability

		ResourceStringData GetMessageForLastStaff(IEnumerable<BusinessObject> toDetachBusinessObjects)
		{
			var message = string.Empty;
			var toDetachstaffList = toDetachBusinessObjects;

			if (capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope)
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			var lastStaff = StaffCapabilityGroupHelper.GetLastStaffWithGRPCapabilityPerGroup(capability.Factory, toDetachstaffList.ToArray(), capability,
				disregardSecurityGroups: WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.Value)
				.OrderByDescending(g => g.Value.Count()).ThenBy(g => g.Key);

			if (!lastStaff.Any())
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			foreach (var pair in lastStaff)
			{
				message += Res.GetString("03C0EE82-FDFC-4626-8EF2-0B0643F10AB4", "- Last member(s) with this capability in {0} group: {1}.{2}",
					pair.Key, string.Join(", ", pair.Value.OrderBy(s => s)), System.Environment.NewLine);
			}

			message += System.Environment.NewLine;
			var confirmationMessage = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed
				? Res.GetString("AE42982B-3290-4C62-AC31-0152527AA99B", "Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this capability?")
				: Res.GetString("2CE94743-8401-4713-A520-E6715EBC9C55", "Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. You do not have the relevant permission to make this change.");

			MessageBoxButtons = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
			return Res.GetData("BAC127CC-64C0-4A47-A5D4-3A057A0B116B", "{0}{1}").Format(message, confirmationMessage);
		}

		internal void CapabilityMembersGrid_BeforeDetaching(object sender, ModuleButtonGridBeforeDetachEventArgs eventArgs)
		{
			var message = GetMessageForLastStaff(eventArgs.ToDetachBusinessObjects);

			if (message != DefaultDetachMessage)
			{
				CapabilityMembersGrid.DetachMessage = message;
			}
		}

		internal void CapabilityMembersGrid_InnerGrid_RowDeleteKeyDown(object sender, RowsDeletingEventArgs e)
		{
			if (!e.Cancel)
			{
				var message = GetMessageForLastStaff(e.Objects);
				var dialogResult = Globals.Message.Show(Res.GetString("ab9c7811-3d5b-43d7-8117-06fd0587be44", "{0}", message.Caption),
					Res.GetString("a8ad6648-5071-4b99-8a85-59e4fb613106", "Confirm Detach..."), MessageBoxButtons, MessageBoxIcon.Information);

				if (dialogResult != DialogResult.Yes)
				{
					e.Cancel = true;
				}
			}
		}

		void CapabilityMembersGrid_OnDetached(object sender, ModuleButtonGridOnDetachedEventArgs eventArgs)
		{
			CapabilityMembersGrid.DetachMessage = DefaultDetachMessage;
		}

		internal MessageBoxButtons MessageBoxButtons
		{
			get
			{
				return messageBoxButtons;
			}
			set
			{
				messageBoxButtons = value;
				CapabilityMembersGrid.MessageBoxButtons = messageBoxButtons;
			}
		}

		MessageBoxButtons messageBoxButtons;

		#endregion
	}
}
