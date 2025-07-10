using System;
using System.Drawing;
using System.Linq;
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
	public partial class GlbStaffManagementControl : ZTreeViewControl
	{
		public GlbStaffManagementControl()
		{
			InitializeComponent();

			roleColumn.Header = GlbStaffManagementTree.RoleColumnHeader;
			jobTitleColumn.Header = GlbStaffManagementTree.JobTitleColumnHeader;
			effectiveDateColumn.Header = GlbStaffManagementTree.EffectiveDateColumnHeader;
			branchColumn.Header = GlbStaffManagementTree.BranchColumnHeader;

			Tree.SetSortColumn(roleColumn, System.Windows.Forms.SortOrder.Descending);
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
			get { return Res.GetData("8773a774-7e75-465b-81d2-a430ab0f24e0", "Staff Management Role"); }
		}

		protected override ResourceStringData DefaultNameOfTreeElementsPlural
		{
			get { return Res.GetData("8c65bbbd-b92f-4fc8-b2b5-f7c1df153f95", "Staff Management Roles"); }
		}

		protected override void SetupTree()
		{
			base.SetupTree();

			roleTextBox.DrawText += RoleTextBox_DrawText;
			jobTitleTextBox.DrawText += Control_DrawText;
			effectiveDateTextBox.DrawText += Control_DrawText;
			branchTextBox.DrawText += Control_DrawText;
		}

		static void RoleTextBox_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			var node = (GlbStaffManagementTreeNode)e.Node.Tag;
			if (node != null && node.BizObjForBinding.GetType() == typeof(GlbStaffManagementRoleWrapper))
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
			}
		}

		static void Control_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			var node = (GlbStaffManagementTreeNode)e.Node.Tag;
			if (node != null)
			{
				e.Font = new Font(e.Font, FontStyle.Regular);
			}
		}

		#endregion

		#region Buttons

		public ZToolStripButton HistoryToolStripButton { get; private set; }

		protected override void SetupButtons()
		{
			base.SetupButtons();

			bottomToolStrip.Items.Remove(AttachToolStripButton);
			DetachToolStripButton.CaptionResourceString = Res.GetData("39f817e5-0b1f-4951-a02e-fb689b239add", "Mark as Ended");

			HistoryToolStripButton = new ZToolStripButton();
			HistoryToolStripButton.CaptionResourceString = Res.GetData("397f815b-d453-4aa5-9187-d38e38250ce7", "View/Edit History");
			HistoryToolStripButton.Image = Icons.GetImage(IconTypes.FindButtonActive);
			HistoryToolStripButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			HistoryToolStripButton.Click += HistoryToolStripButton_Click;
			bottomToolStrip.Items.Add(HistoryToolStripButton);

			if (!NewIsAllowed)
			{
				NewToolStripButton.Enabled = false;
			}

			if (!DetachIsAllowed)
			{
				DetachToolStripButton.Enabled = false;
			}

			if (!EditIsAllowed)
			{
				EditToolStripButton.Enabled = false;
			}

			if (!ViewHistoryIsAllowed)
			{
				HistoryToolStripButton.Enabled = false;
			}
		}

		#region Security

		bool NewIsAllowed => Env.Security.StaffReportingManagerRolesAdd.IsAllowed || IsCurrentUserLocalAdmin;

		bool DetachIsAllowed => Env.Security.StaffReportingManagerRolesDetach.IsAllowed || IsCurrentUserLocalAdmin;

		bool EditIsAllowed => Env.Security.StaffReportingManagerRolesEdit.IsAllowed || IsCurrentUserLocalAdmin;

		bool ViewHistoryIsAllowed
		{
			get
			{
				return Model.Staff.IsCurrentUser && Env.Security.StaffViewOwnReportingManagerRolesHistory.IsAllowed ||
				Env.Security.StaffViewOtherReportingManagerRolesHistory.IsAllowed ||
				IsCurrentUserLocalAdmin;
			}
		}

		bool IsCurrentUserLocalAdmin => Model.Staff.IsCurrentUserLocalAdminForThisStaff || Model.Staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !Model.Staff.IsInDatabase;

		#endregion

		protected override void DetachSelectedElements()
		{
			if (DetachIsAllowed)
			{
				if (Tree.SelectedNodes.Count == 0)
				{
					Globals.Message.ShowError(SelectManagersToDetachMessage);
				}
				else if (Globals.Message.Show(Res.GetString("378c2ec3-180d-47d1-8491-9e7f12e4d47f", "This will set the End Date of responsibility for the selected Reporting Manager(s) to today. Would you like to continue?"), Res.GetString("6d181092-5a3d-4ab5-b7a4-a93702f8f53f", "Detach Manager"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					foreach (var advNode in Tree.SelectedNodes)
					{
						switch (((GlbStaffManagementTreeNode)advNode.Tag).BizObj)
						{
							case GlbStaffManagementManagerWrapper managerWrapper:
								{
									var managerRecord = managerWrapper.Manager;
									var role = managerRecord.ReportingRole;
									var staff = Model.Staff;

									if (staff.GS_IsActive && role != null && role.IsMandatory && managerRecord.IsCurrentManager && (!role.SharedRoleAllowed || staff.GetCurrentManagers().Count(x => x.GSM_ManagerType == managerRecord.GSM_ManagerType) == 1))
									{
										Globals.Message.ShowError(Res.GetString("4888f8c2-d9d9-435e-889c-00a6f174fe36", "You cannot detach the only manager in a mandatory role. You can create a new record to supersede this one using the \"New\" button.", managerRecord.Manager.GS_FullName));
									}
									else if (managerRecord.IsFutureManager || (managerRecord.HasCycle() && Globals.Message.Show(Res.GetString("a8662a13-dd17-41d0-ba97-8b5e062ed0b1", "You selected a cycle manager to be detached, it will be removed. Would you like to continue?"), Res.GetString("9a4efcfc-76aa-4929-a2ed-2f03340a924e", "Detach cycle Manager"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
									{
										managerRecord.Delete();
									}
									else
									{
										managerRecord.GSM_EndDate = ZDateTime.Today;
									}
									break;
								}

							case GlbStaffManagementRoleWrapper _:
								{
									Globals.Message.ShowError(SelectManagersToDetachMessage);
									break;
								}
						}
					}

					ModelView.BuildTree();
				}
			}
		}

		string SelectManagersToDetachMessage => Res.GetString("5bc11964-5c5c-47e8-927e-8647b806dd2d", "Please select a manager to remove from the tree");

		#region New

		protected override void NewToolStripButton_Click(object sender, EventArgs e)
		{
			var manager = Model.Staff.Factory.New<GlbStaffManager>();
			manager.GSM_GS_Staff = Model.Staff.PK;
			if (Tree.SelectedNodes.Count == 1)
			{
				var advNode = Tree.SelectedNodes[0];

				switch (((GlbStaffManagementTreeNode)advNode.Tag).BizObj)
				{
					case GlbStaffManagementManagerWrapper managerWrapper:
						{
							manager.GSM_ManagerType = managerWrapper.Manager.GSM_ManagerType;
							break;
						}

					case GlbStaffManagementRoleWrapper roleWrapper:
						{
							manager.GSM_ManagerType = SystemDataRegistry.Instance.StaffReportingRoles.Value.GetCodeFromDescription(roleWrapper.Role);
							break;
						}
				}
			}

			manager.GSM_EffectiveDate = ZDateTime.Today;

			var form = new GlbStaffManagerForm(manager);
			form.FormClosed += (o, args) =>
			{
				ModelView.BuildTree();
			};

			ZFormModaliser.Show(form, ParentForm);
		}

		#endregion

		#region History

		protected void HistoryToolStripButton_Click(object sender, EventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var reloadedStaff = newFactory.Load<GlbStaff>(Model.Staff.PK);
			var managerCollection = new GlbStaffManagerCollection(reloadedStaff, new ZQuery());
			var form = new GlbStaffManagerHistoryForm(managerCollection);

			form.Saved += (obj, args) =>
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
			if (EditIsAllowed)
			{
				switch (bizObjToEdit)
				{
					case GlbStaffManagementManagerWrapper managerWrapper:
						{
							var manager = Model.Staff.Factory.Load<GlbStaffManager>(managerWrapper.Manager.PK);
							var form = new GlbStaffManagerForm(manager);
							ZFormModaliser.Show(form, ParentForm);

							break;
						}

					case GlbStaffManagementRoleWrapper _:
						{
							Globals.Message.ShowError(Res.GetString("77b5ca7b-cc9e-487e-b50f-9f1e48bfb959", "Please select a manager to edit from the tree"));
							break;
						}
				}
			}
		}

		#endregion

		#region ReadOnly

		protected override void SetControlsReadOnly()
		{
			base.SetControlsReadOnly();

			HistoryToolStripButton.Enabled = false;
		}

		#endregion
	}
}
