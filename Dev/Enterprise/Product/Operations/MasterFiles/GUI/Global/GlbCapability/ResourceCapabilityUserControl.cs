using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ResourceCapabilityUserControl : ZUserControl
	{
		public ResourceCapabilityUserControl()
		{
			InitializeComponent();

			CapabilityGrid.BeforeDetach += CapabilityGrid_BeforeDetaching;
			CapabilityGrid.Detached += CapabilityGrid_OnDetached;
			CapabilityGrid.InnerGrid.RowsDeleting += CapabilityGrid_InnerGrid_RowDeleteKeyDown;
		}

		GlbStaff Staff
		{
			get { return (GlbStaff)DataSource; }
		}

		#region Entity Selection

		void CapabilitiesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GridEntityFormOpener.OpenForm(CapabilityGrid.InnerGrid, e, GetSelectedCapability, ControllerIDs.GlbCapability);
		}

		BusinessObject GetSelectedCapability()
		{
			return CapabilityGrid.InnerGrid.GetCurrent();
		}

		#endregion

		#region Detaching Capabilities From Staff

		ResourceStringData GetMessageForLastStaff(IEnumerable<BusinessObject> toDetachBusinessObjects)
		{
			var message = string.Empty;
			var capabilityList = toDetachBusinessObjects.Where(c => (c as GlbCapability).G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope);

			if (!capabilityList.Any())
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			var capabilityGroups = StaffCapabilityGroupHelper.GetGroupsWithLastStaffPerGRPCapability(Staff.Factory, capabilityList.ToArray(), Staff.PK,
				disregardSecurityGroups: WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.Value)
				.OrderByDescending(c => c.Value.Count()).ThenBy(c => c.Key);

			if (!capabilityGroups.Any())
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			foreach (var pair in capabilityGroups)
			{
				message += Res.GetString("8DA92A38-C435-4341-9DBB-84945116F8F7", "- This staff member is the last member with capability {0} in group(s) {1}.{2}",
					pair.Key, string.Join(", ", pair.Value.OrderBy(s => s)), System.Environment.NewLine);
			}

			message += System.Environment.NewLine;
			var confirmationMessage = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed
				? Res.GetString("85C1A047-99C0-40FE-A7BC-093FC0CEE486", "Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. Do you still want to remove the capabilities from this staff member?")
				: Res.GetString("5E64EFE8-4272-4075-BFF4-E4212B536087", "Removing the capabilities from this staff member will mean that work relying on the combinations of group and capability above may get lost. You do not have the relevant permission to make this change.");

			MessageBoxButtons = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
			return Res.GetData("64DD4B0E-E019-4122-9186-C7CB19BD79B8", "{0}{1}").Format(message, confirmationMessage);
		}

		internal void CapabilityGrid_BeforeDetaching(object sender, ModuleButtonGridBeforeDetachEventArgs eventArgs)
		{
			var message = GetMessageForLastStaff(eventArgs.ToDetachBusinessObjects);

			if (message != DefaultDetachMessage)
			{
				CapabilityGrid.DetachMessage = message;
			}
		}

		void CapabilityGrid_InnerGrid_RowDeleteKeyDown(object sender, RowsDeletingEventArgs e)
		{
			var message = GetMessageForLastStaff(e.Objects);

			var dialogResult = Globals.Message.Show(Res.GetString("ab9c7811-3d5b-43d7-8117-06fd0587be44", "{0}", message.Caption),
				Res.GetString("a8ad6648-5071-4b99-8a85-59e4fb613106", "Confirm Detach..."), MessageBoxButtons, MessageBoxIcon.Information);

			if (dialogResult != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		void CapabilityGrid_OnDetached(object sender, ModuleButtonGridOnDetachedEventArgs eventArgs)
		{
			CapabilityGrid.DetachMessage = DefaultDetachMessage;
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
				CapabilityGrid.MessageBoxButtons = messageBoxButtons;
			}
		}

		MessageBoxButtons messageBoxButtons;

		#endregion
	}
}
