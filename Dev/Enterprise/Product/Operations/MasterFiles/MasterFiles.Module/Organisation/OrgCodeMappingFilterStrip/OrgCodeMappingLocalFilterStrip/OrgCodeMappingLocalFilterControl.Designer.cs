namespace Enterprise.MasterFiles.Module
{ 
	partial class OrgCodeMappingLocalFilterControl
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
			this.localCodeGuidFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.relationshipTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.contextDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.localCodeGuidFindBox.SuspendLayout();
			this.relationshipTypeDropEdit.SuspendLayout();
			this.contextDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgCodeMappingLocalModuleFilter);
			// 
			// localCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.localCodeGuidFindBox, "LocalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.OrgCodeMappingLocalModuleFilter)(null)).LocalCode)));
			this.localCodeGuidFindBox.ShowDescriptionBox = false;
			this.localCodeGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCodeMappingLocalFilterControl|5b490e6a-99eb-4b77-9ce3-e24f00ca76dc", "Code");
			this.localCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 1, true);
			this.localCodeGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.localCodeGuidFindBox.Name = "LocalCodeTextBox";
			this.localCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.localCodeGuidFindBox.TabIndex = 2;
			// 
			// relationshipTypeDropEdit
			// 
			this.relationshipTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relationshipTypeDropEdit, "RelationshipType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgCodeMappingLocalModuleFilter)(null)).RelationshipType)));
			this.relationshipTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCodeMappingLocalFilterControl|09a54bf2-00d5-4d7f-a998-7c25e290618e", "Relationship");
			this.relationshipTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 1, true);
			this.relationshipTypeDropEdit.Name = "RelationshipTypeDropEdit";
			this.relationshipTypeDropEdit.ShowDescriptionBox = false;
			this.relationshipTypeDropEdit.MaxLength = 3;
			this.relationshipTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.relationshipTypeDropEdit.TabIndex = 3;
			// 
			// contextDropEdit
			// 
			this.contextDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contextDropEdit, "Context");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgCodeMappingLocalModuleFilter)(null)).Context)));
			this.contextDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCodeMappingLocalFilterControl|7c27c0c9-c57a-43dc-8aaf-2451117ef68a", "Context");
			this.contextDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 1, true);
			this.contextDropEdit.Name = "ContextDropEdit";
			this.contextDropEdit.ShowDescriptionBox = false;
			this.contextDropEdit.MaxLength = 3;
			this.contextDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.contextDropEdit.TabIndex = 4;
			// 
			// OrgCodeMappingForeignFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.relationshipTypeDropEdit);
			this.Controls.Add(this.contextDropEdit);
			this.Controls.Add(this.localCodeGuidFindBox);
			this.Name = "OrgCodeMappingForeignFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.localCodeGuidFindBox.ResumeLayout(true);
			this.localCodeGuidFindBox.PerformLayout();
			this.relationshipTypeDropEdit.ResumeLayout(true);
			this.relationshipTypeDropEdit.PerformLayout();
			this.contextDropEdit.ResumeLayout(true);
			this.contextDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox localCodeGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit relationshipTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit contextDropEdit;
	}
}
