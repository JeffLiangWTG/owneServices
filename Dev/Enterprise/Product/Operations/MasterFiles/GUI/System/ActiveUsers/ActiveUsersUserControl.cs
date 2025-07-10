using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ActiveUsersUserControl : ZUserControl
	{
		public ActiveUsersUserControl()
		{
			InitializeComponent();
			buttonDetails.Visible = GlbStaff.CurrentUser != null &&
				(GlbStaff.CurrentUser.GS_IsDeveloper || GlbStaff.CurrentUser.IsSupportUser);
		}

		public ActiveUsersUserControl(ActiveUsersModuleBusinessObject activeUsersModuleBO)
			: this()
		{
			this.ActiveUsersModuleBO = activeUsersModuleBO;
		}

		public ActiveUser SelectedElement
		{
			get { return ActiveUsersGrid.SelectedRowCount == 0 ? null : ActiveUsersGrid.SelectedElements[0] as ActiveUser; }
		}

		readonly ActiveUsersModuleBusinessObject ActiveUsersModuleBO;

		void ActiveUsersUserControl_Load(object sender, EventArgs e)
		{
			SetDataBinding(ActiveUsersModuleBO, "");
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			ActiveUsersModuleBO.Refresh();
		}

		void ActiveUsersGrid_DoubleClick(object sender, EventArgs e)
		{
			if (buttonDetails.Visible && ActiveUsersGrid.SelectedRowCount > 0)
			{
				ZControllerFactory.Create(ControllerIDs.ActiveUsers).ShowViewForm(ActiveUsersGrid.SelectedElements[0]);
			}
		}

		void buttonDetails_Click(object sender, EventArgs e)
		{
			if (buttonDetails.Visible)
			{
				if (ActiveUsersGrid.SelectedRowCount > 0)
				{
					ZControllerFactory.Create(ControllerIDs.ActiveUsers).ShowViewForm(ActiveUsersGrid.SelectedElements[0]);
				}
				else
				{
					Globals.Message.Show(Res.GetString("8eb54024-bf4a-4236-90eb-8b07b4af1608",
						"Please select a user to view details for."));
				}
			}
		}
	}
}
