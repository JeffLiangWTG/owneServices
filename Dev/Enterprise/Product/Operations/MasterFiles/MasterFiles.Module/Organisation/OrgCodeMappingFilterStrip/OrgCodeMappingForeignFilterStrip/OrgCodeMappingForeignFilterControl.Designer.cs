namespace Enterprise.MasterFiles.Module
{ 
	partial class OrgCodeMappingForeignFilterControl
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
			this.foreignCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.relationshipTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.contextDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.foreignCodeTextBox.SuspendLayout();
			this.relationshipTypeDropEdit.SuspendLayout();
			this.contextDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgCodeMappingForeignModuleFilter);
			// 
			// foreignCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.foreignCodeTextBox, "ForeignCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Module.OrgCodeMappingForeignModuleFilter)(null)).ForeignCode)));
			this.foreignCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCodeMappingForeignFilterControl|a40eba76-ab24-4d7e-9feb-407106304c72", "Code");
			this.foreignCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 1, true);
			this.foreignCodeTextBox.Name = "ForeignCodeTextBox";
			this.contextDropEdit.MaxLength = 50;
			this.foreignCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.foreignCodeTextBox.TabIndex = 2;
			// 
			// relationshipTypeDropEdit
			// 
			this.relationshipTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relationshipTypeDropEdit, "RelationshipType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgCodeMappingForeignModuleFilter)(null)).RelationshipType)));
			this.relationshipTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCodeMappingForeignFilterControl|b434bee5-6ec0-4886-9c5e-8f9b3b47f962", "Relationship");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgCodeMappingForeignModuleFilter)(null)).Context)));
			this.contextDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCodeMappingForeignFilterControl|4a103d88-06a3-4e09-b8d8-0ed9c818164f", "Context");
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
			this.Controls.Add(this.foreignCodeTextBox);
			this.Name = "OrgCodeMappingForeignFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.foreignCodeTextBox.ResumeLayout(true);
			this.foreignCodeTextBox.PerformLayout();
			this.relationshipTypeDropEdit.ResumeLayout(true);
			this.relationshipTypeDropEdit.PerformLayout();
			this.contextDropEdit.ResumeLayout(true);
			this.contextDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox foreignCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit relationshipTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit contextDropEdit;
	}
}
