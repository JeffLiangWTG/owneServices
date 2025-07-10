namespace Enterprise.MasterFiles.GUI
{
	partial class EDICodeMappingUserControl
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
			if (disposing && (components != null))
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrganisationCodeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ForeignCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LocalCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocalGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ContextTextBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.OrganisationCodeGuidFindBox.SuspendLayout();
			this.RelationshipDropEdit.SuspendLayout();
			this.LocalCodeFindBox.SuspendLayout();
			this.LocalCodeDropEdit.SuspendLayout();
			this.LocalGuidFindBox.SuspendLayout();
			this.ContextTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgPatternMatchOverride);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dadb6d91-a6fa-4f99-8d6c-b28d9728108c", "Details");
			this.DetailsGroupBox.Controls.Add(this.OrganisationCodeGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.ForeignCodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.RelationshipDropEdit);
			this.DetailsGroupBox.Controls.Add(this.LocalCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.LocalCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.LocalGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.ContextTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 130, true);
			this.DetailsGroupBox.TabIndex = 2;
			this.DetailsGroupBox.TabStop = false;
			// 
			// OrganisationCodeGuidFindBox
			// 
			this.OrganisationCodeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationCodeGuidFindBox, "OO_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_OH)));
			this.OrganisationCodeGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("56c192d1-1e0e-43e4-a58c-4877893cabbc", "Organization Code");
			this.OrganisationCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 20, true);
			this.OrganisationCodeGuidFindBox.Name = "OrganisationCodeGuidFindBox";
			this.OrganisationCodeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganisationCodeGuidFindBox.ParentType = null;
			this.OrganisationCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 25, true);
			this.OrganisationCodeGuidFindBox.TabIndex = 7;
			// 
			// ForeignCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForeignCodeTextBox, "OO_ForeignCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_ForeignCode)));
			this.ForeignCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("62388ade-f0ed-416d-a5ff-647f0a3727ca", "Foreign Code");
			this.ForeignCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 59, true);
			this.ForeignCodeTextBox.Name = "ForeignCodeTextBox";
			this.ForeignCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 25, true);
			this.ForeignCodeTextBox.TabIndex = 8;
			// 
			// RelationshipDropEdit
			// 
			this.RelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelationshipDropEdit, "OO_Relationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_Relationship)));
			this.RelationshipDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("60892a1a-ebe8-4d0f-a11c-3e71423d7562", "Relationship");
			this.RelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 96, true);
			this.RelationshipDropEdit.Name = "RelationshipDropEdit";
			this.RelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 25, true);
			this.RelationshipDropEdit.TabIndex = 9;
			this.RelationshipDropEdit.TextChanged += new System.EventHandler(this.RelationshipTextBox_TextChanged);
			// 
			// LocalCodeFindBox
			// 
			this.LocalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalCodeFindBox, "OO_LocalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_LocalCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).Lookups.OrgCoNames)));
			this.LocalCodeFindBox.BindToList = "Lookups+OrgCoNames";
			this.LocalCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84b84e3a-e330-43fa-a985-fd7f934f0e1f", "Local Code");
			this.LocalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 20, true);
			this.LocalCodeFindBox.Name = "LocalCodeFindBox";
			this.LocalCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocalCodeFindBox.ParentType = null;
			this.LocalCodeFindBox.PreBoundMaxLength = 3;
			this.LocalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 25, true);
			this.LocalCodeFindBox.TabIndex = 10;
			// 
			// LocalCodeDropEdit
			// 
			this.LocalCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalCodeDropEdit, "OO_LocalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_LocalCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).Lookups.OrgCoNames)));
			this.LocalCodeDropEdit.BindToList = "Lookups+OrgCoNames";
			this.LocalCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84b84e3a-e330-43fa-a985-fd7f934f0e1f", "Local Code");
			this.LocalCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 20, true);
			this.LocalCodeDropEdit.Name = "LocalCodeDropEdit";
			this.LocalCodeDropEdit.PreBoundMaxLength = 3;
			this.LocalCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 25, true);
			this.LocalCodeDropEdit.TabIndex = 10;
			// 
			// LocalGuidFindBox
			// 
			this.LocalGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalGuidFindBox, "OO_LocalGuid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_LocalGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).Lookups.OrgCoNames)));
			this.LocalGuidFindBox.BindToList = "Lookups+OrgCoNames";
			this.LocalGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84b84e3a-e330-43fa-a985-fd7f934f0e1f", "Local Code");
			this.LocalGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 20, true);
			this.LocalGuidFindBox.Name = "LocalGuidFindBox";
			this.LocalGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocalGuidFindBox.ParentType = null;
			this.LocalGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 25, true);
			this.LocalGuidFindBox.TabIndex = 11;
			// 
			// ContextTextBox
			// 
			this.ContextTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContextTextBox, "OO_Context");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_Context)));
			this.ContextTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9e4f93d3-3094-40dc-b4d1-5784d8314dca", "Context");
			this.ContextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 59, true);
			this.ContextTextBox.Name = "ContextTextBox";
			this.ContextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 25, true);
			this.ContextTextBox.TabIndex = 12;
			// 
			// EDICodeMappingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "EDICodeMappingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 130, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.OrganisationCodeGuidFindBox.ResumeLayout(true);
			this.OrganisationCodeGuidFindBox.PerformLayout();
			this.RelationshipDropEdit.ResumeLayout(true);
			this.RelationshipDropEdit.PerformLayout();
			this.LocalCodeFindBox.ResumeLayout(true);
			this.LocalCodeFindBox.PerformLayout();
			this.LocalCodeDropEdit.ResumeLayout(true);
			this.LocalCodeDropEdit.PerformLayout();
			this.LocalGuidFindBox.ResumeLayout(true);
			this.LocalGuidFindBox.PerformLayout();
			this.ContextTextBox.ResumeLayout(true);
			this.ContextTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox OrganisationCodeGuidFindBox;
		protected ZArchitecture.ZTextBox ForeignCodeTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox LocalCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit RelationshipDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit LocalCodeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox LocalGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ContextTextBox;
	}
}
