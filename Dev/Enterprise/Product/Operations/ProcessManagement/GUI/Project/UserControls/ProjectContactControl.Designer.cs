using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectContactControl : ZUserControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.ClientInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactPhoneDiallerUserControl = new Enterprise.ProcessManagement.GUI.ProjectContactPhoneDiallerUserControl();
			this.ContactEmailBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zAddressControl1 = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.TechInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TechContactPhoneDiallerUserControl = new Enterprise.ProcessManagement.GUI.ProjectContactPhoneDiallerUserControl();
			this.ClientOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.TechContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClientInfoGroupBox.SuspendLayout();
			this.ContactPhoneDiallerUserControl.SuspendLayout();
			this.ContactFindBox.SuspendLayout();
			this.zAddressControl1.SuspendLayout();
			this.TechInfoGroupBox.SuspendLayout();
			this.TechContactPhoneDiallerUserControl.SuspendLayout();
			this.ClientOrganisationFindBox.SuspendLayout();
			this.TechContactGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.Project);
			// 
			// ClientInfoGroupBox
			// 
			this.ClientInfoGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("ProjectForm|757d4652-cda9-4236-a803-54f22924e6de", "Client Information");
			this.ClientInfoGroupBox.Controls.Add(this.ContactPhoneDiallerUserControl);
			this.ClientInfoGroupBox.Controls.Add(this.ContactEmailBox);
			this.ClientInfoGroupBox.Controls.Add(this.ContactFindBox);
			this.ClientInfoGroupBox.Controls.Add(this.zAddressControl1);
			this.ClientInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientInfoGroupBox.Name = "ClientInfoGroupBox";
			this.ClientInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 173, true);
			this.ClientInfoGroupBox.TabIndex = 0;
			this.ClientInfoGroupBox.TabStop = false;
			// 
			// ContactPhoneDiallerUserControl
			// 
			this.ContactPhoneDiallerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactPhoneDiallerUserControl, "WKP_OC_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_OC_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).ClientOrganisationPK)));
			this.ContactPhoneDiallerUserControl.BindToOrg = "ClientOrganisationPK";
			this.ContactPhoneDiallerUserControl.CurrentOrg = null;
			this.ContactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 108, true);
			this.ContactPhoneDiallerUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.ContactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.Name = "ContactPhoneDiallerUserControl";
			this.ContactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.TabIndex = 2;
			// 
			// ContactEmailBox
			// 
			this.ContactEmailBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ContactEmailBox, "ContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).ContactEmail)));
			this.ContactEmailBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ContactEmailBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactEmailBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ContactEmailBox.ForeColor = System.Drawing.Color.Blue;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContactEmailBox, false);
			this.ContactEmailBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 111, true);
			this.ContactEmailBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContactEmailBox.Name = "ContactEmailBox";
			this.ContactEmailBox.ReadOnly = true;
			this.ContactEmailBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 12, true);
			this.ContactEmailBox.TabIndex = 4;
			this.ContactEmailBox.Text = "<ContactEmail>";
			// 
			// ContactFindBox
			// 
			this.ContactFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactFindBox, "WKP_OC_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_OC_Contact)));
			this.ContactFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("df5d9495-0b74-4a84-9dab-c618ffa39f96", "Contact");
			this.ContactFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 83, true);
			this.ContactFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContactFindBox.Name = "ContactFindBox";
			this.ContactFindBox.PreBoundMaxLength = 45;
			this.ContactFindBox.ShowDescriptionBox = false;
			this.ContactFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.ContactFindBox.TabIndex = 1;
			// 
			// zAddressControl1
			// 
			this.zAddressControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zAddressControl1, "WKP_OA_ClientAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_OA_ClientAddress)));
			this.zAddressControl1.BindToOrgList = "Lookups+Clients";
			this.zAddressControl1.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("ProjectForm|cd9fe03b-fca8-45d7-971a-5ba5bc4f4792", "Client");
			this.LabelCaptionRenderProvider.SetLabelTop(this.zAddressControl1, 2);
			this.zAddressControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 18, true);
			this.zAddressControl1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zAddressControl1.Name = "zAddressControl1";
			this.zAddressControl1.PopupCaption = "";
			this.zAddressControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 59, true);
			this.zAddressControl1.TabIndex = 0;
			// 
			// TechInfoGroupBox
			// 
			this.TechInfoGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("ProjectTechnicianControl|6f25b9e9-72cd-47fa-a0d0-e4e27014b6c1", "Technician Information");
			this.TechInfoGroupBox.Controls.Add(this.TechContactPhoneDiallerUserControl);
			this.TechInfoGroupBox.Controls.Add(this.ClientOrganisationFindBox);
			this.TechInfoGroupBox.Controls.Add(this.TechContactGuidFindBox);
			this.TechInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TechInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TechInfoGroupBox.Name = "TechInfoGroupBox";
			this.TechInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 131, true);
			this.TechInfoGroupBox.TabIndex = 3;
			this.TechInfoGroupBox.TabStop = false;
			// 
			// TechContactPhoneDiallerUserControl
			// 
			this.TechContactPhoneDiallerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TechContactPhoneDiallerUserControl, "WKP_OC_TechnicalContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_OC_TechnicalContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).TechnicianOrganisationPK)));
			this.TechContactPhoneDiallerUserControl.BindToOrg = "TechnicianOrganisationPK";
			this.TechContactPhoneDiallerUserControl.CurrentOrg = null;
			this.TechContactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 74, true);
			this.TechContactPhoneDiallerUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.TechContactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.TechContactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.TechContactPhoneDiallerUserControl.Name = "TechContactPhoneDiallerUserControl";
			this.TechContactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.TechContactPhoneDiallerUserControl.TabIndex = 2;
			// 
			// ClientOrganisationFindBox
			// 
			this.ClientOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientOrganisationFindBox, "TechnicianOrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).TechnicianOrganisationPK)));
			this.ClientOrganisationFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("ProjectTechnicianControl|6bd5ff73-85c1-4710-9bb5-444fed695482", "Client");
			this.ClientOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 24, true);
			this.ClientOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ClientOrganisationFindBox.Name = "ClientOrganisationFindBox";
			this.ClientOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.ClientOrganisationFindBox.TabIndex = 0;
			// 
			// TechContactGuidFindBox
			// 
			this.TechContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TechContactGuidFindBox, "WKP_OC_TechnicalContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_OC_TechnicalContact)));
			this.TechContactGuidFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("4cd67f4c-635e-492a-b74b-8bc4eb099d21", "Contact");
			this.TechContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 50, true);
			this.TechContactGuidFindBox.Name = "TechContactGuidFindBox";
			this.TechContactGuidFindBox.PreBoundMaxLength = 45;
			this.TechContactGuidFindBox.ShowDescriptionBox = false;
			this.TechContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.TechContactGuidFindBox.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ClientInfoGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.TechInfoGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 307, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			this.splitContainer1.TabIndex = 4;
			// 
			// ProjectContactControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "ProjectContactControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 307, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClientInfoGroupBox.ResumeLayout(false);
			this.ClientInfoGroupBox.PerformLayout();
			this.ContactPhoneDiallerUserControl.ResumeLayout(true);
			this.ContactPhoneDiallerUserControl.PerformLayout();
			this.ContactFindBox.ResumeLayout(true);
			this.ContactFindBox.PerformLayout();
			this.zAddressControl1.ResumeLayout(true);
			this.zAddressControl1.PerformLayout();
			this.TechInfoGroupBox.ResumeLayout(false);
			this.TechInfoGroupBox.PerformLayout();
			this.TechContactPhoneDiallerUserControl.ResumeLayout(true);
			this.TechContactPhoneDiallerUserControl.PerformLayout();
			this.ClientOrganisationFindBox.ResumeLayout(true);
			this.ClientOrganisationFindBox.PerformLayout();
			this.TechContactGuidFindBox.ResumeLayout(true);
			this.TechContactGuidFindBox.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZGroupBox ClientInfoGroupBox;
		protected ZAddressControl zAddressControl1;
		protected ZGuidFindBox ContactFindBox;
		protected ZTextBox ContactEmailBox;
		protected ProjectContactPhoneDiallerUserControl ContactPhoneDiallerUserControl;
		private ZGroupBox TechInfoGroupBox;
		protected ProjectContactPhoneDiallerUserControl TechContactPhoneDiallerUserControl;
		protected MasterFiles.GUI.ZOrganisationFindBox ClientOrganisationFindBox;
		protected ZGuidFindBox TechContactGuidFindBox;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
	}
}