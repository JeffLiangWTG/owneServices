using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AddDirectReportsForm : ZChildForm
	{
		public AddDirectReportsForm(AddDirectReportsBizO addDirectReportsBiZo, bool isReportingRoleReadOnly = false) : base(addDirectReportsBiZo)
		{
			AddDirectReportsBizO = addDirectReportsBiZo;
			InitializeComponent();
			reportingRoleDropEdit.ReadOnly = isReportingRoleReadOnly;
			AddDirectReportsButton = AddButton(Res.GetString("d28aed9d-3238-4e82-b0dc-45df027a0c4d", "&Add Selected as Direct Reports"), Icons.GetImage(IconTypes.AddButtonActive));
			AddDirectReportsButton.Click += new EventHandler(AddDirectReportsButton_Click);
			AddFilterGrid();
		}

		AddDirectReportsBizO AddDirectReportsBizO { get; }
		internal ZFilterGridModule FilterItemModule;
		internal ZFilterStripControl FilterStripControl;

		#region Add Filter Grid

		protected void AddFilterGrid()
		{
			FilterItemModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff);
			var staffModule = (IGlbStaffModule)FilterItemModule;
			staffModule?.HideRecentItems();

			FilterStripControl = (ZFilterStripControl)FilterItemModule.EmbeddedControl;
			FilterStripControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 30);
			FilterStripControl.Dock = DockStyle.Fill;
			FilterStripControl.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(staffGridPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(staffGridPanel.Height) - 30);
			staffGridGroupBox.Controls.Add(FilterStripControl);
		}

		#endregion

		#region Buttons

		public ZToolStripButton AddDirectReportsButton;
		public new ZToolStripButton CancelButton;

		ZToolStripButton AddButton(string text, Image image)
		{
			var button = new ZToolStripButton
			{
				Image = image,
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Text = text,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				AutoToolTip = false,
				Alignment = ToolStripItemAlignment.Right
			};
			toolStrip.Items.Add(button);

			return button;
		}

		protected void AddDirectReportsButton_Click(object sender, EventArgs e)
		{
			AddDirectReportsBizO.ValidateAll();

			if (AddDirectReportsBizO.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("e34d9058-1403-45db-bce8-673b1a57e8d9", "Fix validation errors before continuing"));
				return;
			}

			var shouldSupersede = DialogResult.Yes;

			if (AddDirectReportsBizO.ReportingRole.SharedRoleAllowed)
			{
				shouldSupersede = Globals.Message.Show(Res.GetString("e39d69b0-330f-4d7f-a6e9-f8fd7c17b124", "This role can be shared.\r\nClick Yes to supersede all existing managers for these direct reports.\r\nClick No to share the management role with the existing managers."), Res.GetString("fd11840a-976c-4a9a-9300-e7d9b4344e94", "Supersede/Share existing roles"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
			}

			if (shouldSupersede == DialogResult.Cancel)
			{
				return;
			}

			var nonSupersededStaff = new List<ZString>();

			if (FilterItemModule.DisplayGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("701bb1dc-d7aa-4cb8-86fa-0f117bb44948", "Please select one or more records from the grid"));
				return;
			}

			foreach (var directReportStaff in FilterItemModule.DisplayGrid.SelectedElements.Cast<GlbStaff>())
			{
				if (!AddDirectReportsBizO.AddNewManagerForStaff(directReportStaff, shouldSupersede == DialogResult.Yes))
				{
					nonSupersededStaff.Add(directReportStaff.GS_FullName);
				}
			}

			if (nonSupersededStaff.Count > 0)
			{
				if (AddDirectReportsBizO.ManagerType == DefaultStaffReportingRoles.Codes.DirectManager)
				{
					Globals.Message.ShowInformation(Res.GetString("2c599ec4-f8a7-456f-94d8-f6b0cf2b704d", "The following Direct Reports could not be set because either their existing {0}'s effective date is preceded by the new record or they have no Job Title for the new effective date. Update these staff members manually from their staff record:\r\n{1}", DefaultStaffReportingRoles.Descriptions.DirectManager, string.Join("\r\n", nonSupersededStaff)));
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("b86155bb-cf19-4624-a188-3ba0da60064e", "The following Direct Reports could not be set since their existing manager(s)' effective dates are preceded by the new record. Update these staff members manually from their staff record:\r\n{0}", string.Join("\r\n", nonSupersededStaff)));
				}
			}

			Close();
		}

		#endregion

		#region Overrides

		protected override void SaveToRecentItems()
		{
		}

		protected override bool AllowNew => false;

		#endregion
	}
}
