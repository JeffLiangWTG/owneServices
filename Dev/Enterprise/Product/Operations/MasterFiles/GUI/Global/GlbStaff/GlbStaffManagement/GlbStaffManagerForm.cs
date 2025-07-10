using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbStaffManagerForm : ZChildForm
	{
		public GlbStaffManagerForm(GlbStaffManager manager, bool isReportingRoleReadOnly = false)
			: base(manager)
		{
			Manager = manager;
			InitializeComponent();
			reportingRoleDropEdit.ReadOnly = isReportingRoleReadOnly;
			CancelButton = AddButton(Res.GetString("3d76ab03-f776-47e0-b8ea-709fc49381df", "&Cancel"), Icons.GetImage(IconTypes.BlackWhite_Cancel));
			CancelButton.Click += new EventHandler(CancelButton_Click);
			OkButton = AddButton(Res.GetString("f1b72ee3-f2fc-4bcb-8c79-e527f2cae50b", "&OK"), Icons.GetImage(IconTypes.BlackWhite_Tick));
			OkButton.Click += new EventHandler(OkButton_Click);
			FormClosed += new FormClosedEventHandler(RevertUncommittedChanges);
		}

		GlbStaffManager Manager { get; }

		bool ShouldCommitChanges { get; set; }

		#region Buttons

		public ZToolStripButton OkButton;
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

		protected void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected void OkButton_Click(object sender, EventArgs e)
		{
			Manager.Validation.ValidateAll();
			if (!Manager.HasErrors && RunAdditionalValidation())
			{
				ShouldCommitChanges = true;
				Close();
			}
		}

		protected void RevertUncommittedChanges(object sender, EventArgs e)
		{
			if (!ShouldCommitChanges)
			{
				if (!Manager.IsInDatabase)
				{
					Manager.Delete();
				}
				else
				{
					Manager.Reload();
				}
			}
		}

		#endregion

		#region Overrides

		protected bool RunAdditionalValidation()
		{
			var role = ((StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode(Manager.GSM_ManagerType));

			if (role == null)
			{
				return false;
			}

			var existingManagers = Manager.GetOtherManagersWithOverlappingPeriods();
			if (existingManagers.Length > 0)
			{
				if (role.SharedRoleAllowed)
				{
					var result = Globals.Message.Show(Res.GetString("1914bf35-b333-4ea2-90fe-dfe84d03f876", "One or more existing Reporting Managers exist for this role. Do you wish to supersede the existing Manager(s)?"), Res.GetString("a6b8b348-5ab9-4965-9060-705165a1f8ef", "Supersede Manager"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						if (existingManagers.Any(x => x.GSM_EffectiveDate > Manager.GSM_EffectiveDate))
						{
							Globals.Message.ShowError(InvalidEffectiveDateMessage, InvalidEffectiveDateCaption);
							return false;
						}

						if (!SetExistingManagerEndDates(existingManagers))
						{
							return false;
						}
					}
					else if (result == DialogResult.No)
					{
						if (Manager.GSM_GS_Manager == Manager.GSM_GS_Staff || existingManagers.Any(x => x.GSM_GS_Manager == Manager.GSM_GS_Staff))
						{
							Globals.Message.ShowError(Res.GetString("506a9acd-44f6-4a61-a9a1-d24ad066486c", "Self-managed roles cannot be shared."));
							return false;
						}
					}
					else if (result == DialogResult.Cancel)
					{
						return false;
					}
				}
				else
				{
					if (existingManagers.Any(x => x.GSM_EffectiveDate > Manager.GSM_EffectiveDate))
					{
						Globals.Message.ShowError(InvalidEffectiveDateMessage, InvalidEffectiveDateCaption);
						return false;
					}

					if (!SetExistingManagerEndDates(existingManagers))
					{
						return false;
					}
				}
			}

			return true;
		}

		bool SetExistingManagerEndDates(GlbStaffManager[] managers)
		{
			foreach (var existingManager in managers)
			{
				if (existingManager.GSM_EffectiveDate.Date == Manager.GSM_EffectiveDate.Date)
				{
					if (Globals.Message.Show(Res.GetString("4a27c62e-9833-4bc8-9818-cfca1e52a61b", "Existing manager {0} is being superseded despite their effective date being the same as the new manager. The pre-existing record will be deleted.", existingManager.Manager.GS_FullName), Res.GetString("2feca91e-b79a-4e76-b3d5-2f8f9e8b16ae", "Delete Pre-Existing Manager"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
					{
						existingManager.Delete();
					}
					else
					{
						return false;
					}
				}
				else
				{
					existingManager.GSM_EndDate = Manager.GSM_EffectiveDate.AddDays(-1);
				}
			}

			return true;
		}

		static string InvalidEffectiveDateMessage => Res.GetString("4586506b-7dc8-4488-a1ad-ffa0915f6f11", "The new Reporting Manager's Effective Date cannot precede any existing managers of the same role. To add historical records, click the History button from the Staff Form's Reporting Manager Grid.");
		static string InvalidEffectiveDateCaption => Res.GetString("1e69b56f-72bc-46f0-b39f-91e72f332679", "Invalid Effective Date");

		protected override void SaveToRecentItems()
		{
		}

		protected override bool AllowNew => false;

		#endregion
	}
}
