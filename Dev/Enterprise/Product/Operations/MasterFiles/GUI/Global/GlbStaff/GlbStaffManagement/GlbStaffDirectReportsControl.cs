using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbStaffDirectReportsControl : ZTreeViewControl
	{
		public GlbStaffDirectReportsControl()
		{
			InitializeComponent();

			roleColumn.Header = GlbStaffManagementTree.RoleColumnHeader;

			Tree.SetSortColumn(roleColumn, System.Windows.Forms.SortOrder.Descending);
			this.bottomToolStrip.Visible = true;
		}

		#region View/Model

		protected override ZTreeViewAdv CreateNewTreeViewAdv()
		{
			return new GlbStaffManagementTree();
		}

		public new GlbStaffManagementTreeModelView ModelView
		{
			get { return (GlbStaffManagementTreeModelView)base.ModelView; }
		}

		protected override IZTreeModelView GetNewTreeModelView()
		{
			return new GlbStaffManagementTreeModelView((GlbStaffManagementTreeModel)CurrentDataItem);
		}

		protected GlbStaffManagementTreeModel Model
		{
			get { return CurrentDataItem as GlbStaffManagementTreeModel; }
		}

		#endregion

		#region Tree

		protected override ResourceStringData DefaultNameOfATreeElement
		{
			get { return Res.GetData("62ffd29d-4458-4384-85df-0513db214b60", "Staff Direct Report Role"); }
		}

		protected override ResourceStringData DefaultNameOfTreeElementsPlural
		{
			get { return Res.GetData("0a7cea4a-fafe-4c65-abf5-4e4d40f98240", "Staff Direct Report Roles"); }
		}

		protected override void SetupTree()
		{
			base.SetupTree();

			roleTextBox.DrawText += RoleTextBox_DrawText;
		}

		static void RoleTextBox_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			var node = (GlbStaffManagementTreeNode)e.Node.Tag;
			if (node != null && node.BizObjForBinding.GetType() == typeof(GlbStaffManagementRoleWrapper))
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
			}
		}

		#endregion

		#region Buttons

		protected override void SetupButtons()
		{
			base.SetupButtons();

			bottomToolStrip.Items.Remove(NewToolStripButton);
			bottomToolStrip.Items.Remove(EditToolStripButton);
			AttachToolStripButton.CaptionResourceString = Res.GetData("decf092e-1eba-46ef-a3e6-6918f640c058", "Add");
			DetachToolStripButton.CaptionResourceString = Res.GetData("863b3a2b-3363-4102-9f46-e26bb712340c", "Detach/Transfer Role");

			if (!AttachIsAllowed)
			{
				AttachToolStripButton.Enabled = false;
			}

			if (!DetachIsAllowed)
			{
				DetachToolStripButton.Enabled = false;
			}
		}

		#region Security

		bool AttachIsAllowed => Env.Security.StaffReportingManagerRolesAdd.IsAllowed || IsCurrentUserLocalAdmin;

		bool DetachIsAllowed => Env.Security.StaffReportingManagerRolesDetach.IsAllowed || IsCurrentUserLocalAdmin;

		bool IsCurrentUserLocalAdmin => Model.Staff.IsCurrentUserLocalAdminForThisStaff || Model.Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Model.Staff.IsInDatabase;

		#endregion

		#region Detach

		protected override void DetachSelectedElements()
		{
			if (!DetachIsAllowed)
			{
				return;
			}

			if (Tree.SelectedNodes.Count == 0 || ((GlbStaffManagementTreeNode)Tree.SelectedNodes[0].Tag).BizObj is GlbStaffManagementRoleWrapper)
			{
				Globals.Message.ShowError(SelectManagersToDetachMessage);
				return;
			}

			if (Globals.Message.Show(Res.GetString("3e55df52-0a82-4628-99c2-b965d85aed07", "This will end this manager's responsibility for the selected Direct Report(s) today. Would you like to continue?"), Res.GetString("6d181092-5a3d-4ab5-b7a4-a93702f8f53f", "Detach Manager"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var wrapper = (GlbStaffManagementManagerWrapper)((GlbStaffManagementTreeNode)Tree.SelectedNodes[0].Tag).BizObj;
				DialogResult isTransfer;
				GlbStaff replacementManager = null;

				var role = wrapper.Manager.ReportingRole;
				if (role != null)
				{
					if (role.IsMandatory)
					{
						isTransfer = Globals.Message.Show(Res.GetString("4e204f19-95e1-4296-a3b0-ccaa8203a7cb", "Since this role is mandatory, this management responsibility must be transferred to another staff member. Their responsibility will commence tomorrow."), Res.GetString("4767dac6-0738-415f-b174-4bbf3a739587", "Transfer Responsibilities"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					}
					else
					{
						isTransfer = Globals.Message.Show(Res.GetString("bad17976-7a14-4b3a-8726-f454227e291b", "Would you like to transfer these direct reports to another manager? Their responsibility will commence tomorrow."), Res.GetString("4767dac6-0738-415f-b174-4bbf3a739587", "Transfer Responsibilities"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					}

					switch (isTransfer)
					{
						case DialogResult.Yes:
						case DialogResult.OK:
							{
								replacementManager = StaffPopupModuleHelper.GetStaffFromPopupModule(Model.Factory, ParentForm);
								break;
							}

						case DialogResult.Cancel:
							return;
					}
				}

				if (replacementManager != null && replacementManager == Model.Staff)
				{
					Globals.Message.ShowError(Res.GetString("dce60c75-3f88-4306-aedb-6de572330d23", "You cannot replace the current manager with themselves."));
					return;
				}

				foreach (var advNode in Tree.SelectedNodes)
				{
					var managerWrapper = (GlbStaffManagementManagerWrapper)((GlbStaffManagementTreeNode)advNode.Tag).BizObj;
					var managerRecord = managerWrapper.Manager;

					if (managerRecord.GSM_EffectiveDate > ZDateTime.Today)
					{
						managerRecord.Delete();
					}
					else
					{
						managerRecord.GSM_EndDate = ZDateTime.Today;

						if (replacementManager != null && !managerRecord.Staff.IsCurrentlyManagedBy(replacementManager, managerRecord.GSM_ManagerType))
						{
							var newManager = Model.Factory.New<GlbStaffManager>();
							newManager.GSM_GS_Staff = managerRecord.GSM_GS_Staff;
							newManager.GSM_GS_Manager = replacementManager.PK;
							newManager.GSM_ManagerType = managerRecord.GSM_ManagerType;

							newManager.GSM_EffectiveDate = ZDateTime.Today.AddDays(1);
						}
					}
				}

				ModelView.BuildTree();
			}
		}

		static string SelectManagersToDetachMessage => Res.GetString("91924acf-2f96-4e87-8bf3-e9de84bffd1a", "Please select a manager or managers to remove from the tree");

		protected virtual GlbStaffPopupModuleHelper StaffPopupModuleHelper
		{
			get
			{
				if (staffPopupModuleHelper == null)
				{
					staffPopupModuleHelper = new GlbStaffPopupModuleHelper();
				}

				return staffPopupModuleHelper;
			}
		}

		GlbStaffPopupModuleHelper staffPopupModuleHelper;

		#endregion

		#region Attach

		protected override void AttachToolStripButton_Click(object sender, EventArgs e)
		{
			var role = ZString.Empty;

			if (Tree.SelectedNodes.Count == 1)
			{
				var advNode = Tree.SelectedNodes[0];

				switch (((GlbStaffManagementTreeNode)advNode.Tag).BizObj)
				{
					case GlbStaffManagementManagerWrapper managerWrapper:
						{
							role = managerWrapper.Manager.GSM_ManagerType;
							break;
						}

					case GlbStaffManagementRoleWrapper roleWrapper:
						{
							role = SystemDataRegistry.Instance.StaffReportingRoles.Value.GetCodeFromDescription(roleWrapper.Role);
							break;
						}
				}
			}

			var addDirectReportsBizO = new AddDirectReportsBizO(Model.Staff, role);
			var form = new AddDirectReportsForm(addDirectReportsBizO);
			form.FormClosed += (o, args) =>
			{
				ModelView.BuildTree();
			};

			ZFormModaliser.Show(form, ParentForm);
		}

		#endregion

		#endregion

		#region Edit

		protected override void ShowEditForm(IBusiness bizObjToEdit)
		{
			switch (bizObjToEdit)
			{
				case GlbStaffManagementManagerWrapper managerWrapper:
					{
						ZControllerFactory.Create(ControllerIDs.GlbStaff).ShowEditForm(managerWrapper.Manager.Staff);
						break;
					}

				case GlbStaffManagementRoleWrapper _:
					{
						Globals.Message.ShowError(Res.GetString("77b5ca7b-cc9e-487e-b50f-9f1e48bfb959", "Please select a manager to edit from the tree"));
						break;
					}
			}
		}

		#endregion
	}
}
