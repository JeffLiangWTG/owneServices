namespace Enterprise.Customs.TW.GUI
{
	partial class ConsignorOrConsigneeUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.AddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.GovRegNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GovRegNumTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationFindBox.SuspendLayout();
			this.AddressDropEdit.SuspendLayout();
			this.GovRegNumTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.TWConsignorOrConsigneeAddress);
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.OrganisationFindBox.CaptionResourceString = null;
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganisationFindBox.ParentType = null;
			this.OrganisationFindBox.ShowDescriptionBox = false;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OrganisationFindBox.TabIndex = 1;
			// 
			// AddressDropEdit
			// 
			this.AddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressDropEdit, "E2_OA_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWConsignorOrConsigneeAddress)(null)).E2_OA_Address)));
			this.AddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AddressDropEdit.FilterAddressedByDefaultType = true;
			this.AddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
			this.AddressDropEdit.Name = "AddressDropEdit";
			this.AddressDropEdit.PreBoundMaxLength = 34;
			this.AddressDropEdit.ShowDescriptionBox = false;
			this.AddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.AddressDropEdit.TabIndex = 2;
			// 
			// GovRegNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.GovRegNumTextBox, "E2_GovRegNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWConsignorOrConsigneeAddress)(null)).E2_GovRegNum)));
			this.GovRegNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 0, true);
			this.GovRegNumTextBox.Name = "GovRegNumTextBox";
			this.GovRegNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.GovRegNumTextBox.TabIndex = 4;
			// 
			// GovRegNumTypeDropEdit
			// 
			this.GovRegNumTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GovRegNumTypeDropEdit, "E2_GovRegNumType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWConsignorOrConsigneeAddress)(null)).E2_GovRegNumType)));
			this.GovRegNumTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 0, true);
			this.GovRegNumTypeDropEdit.Name = "GovRegNumTypeDropEdit";
			this.GovRegNumTypeDropEdit.PreBoundMaxLength = 3;
			this.GovRegNumTypeDropEdit.ShouldResizeByMaxLength = false;
			this.GovRegNumTypeDropEdit.ShowDescriptionBox = false;
			this.GovRegNumTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.GovRegNumTypeDropEdit.TabIndex = 3;
			// 
			// ConsignorOrConsigneeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GovRegNumTextBox);
			this.Controls.Add(this.GovRegNumTypeDropEdit);
			this.Controls.Add(this.AddressDropEdit);
			this.Controls.Add(this.OrganisationFindBox);
			this.Name = "ConsignorOrConsigneeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.AddressDropEdit.ResumeLayout(true);
			this.AddressDropEdit.PerformLayout();
			this.GovRegNumTypeDropEdit.ResumeLayout(true);
			this.GovRegNumTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MasterFiles.GUI.ZOrganisationFindBox OrganisationFindBox;
		private ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare AddressDropEdit;
		private ZArchitecture.ZTextBox GovRegNumTextBox;
		private ZArchitecture.GUI.ZDropEdit GovRegNumTypeDropEdit;
	}
}
