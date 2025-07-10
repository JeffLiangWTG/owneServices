using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	partial class EDICodeMappingRelationshipLocalCodeFilterControl
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
			this.RelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ComparisonOperatorForOrgCoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocalCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocalGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LocalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelationshipDropEdit.SuspendLayout();
			this.ComparisonOperatorForOrgCoDropEdit.SuspendLayout();
			this.LocalCodeDropEdit.SuspendLayout();
			this.LocalGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.EDICodeMappingRelationshipLocalCodeModuleFilter);
			// 
			// RelationshipDropEdit
			// 
			this.RelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelationshipDropEdit, "Relationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.EDICodeMappingRelationshipLocalCodeModuleFilter)(null)).Relationship)));
			this.RelationshipDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingRelationshipLocalCodeFilterControl|921d7bdf-7b94-4257-b883-3ee01162f03a", "Relationship");
			this.RelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 1, true);
			this.RelationshipDropEdit.Name = "RelationshipDropEdit";
			this.RelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 18, true);
			this.RelationshipDropEdit.TabIndex = 3;
			this.RelationshipDropEdit.TextChanged += new System.EventHandler(this.RelationshipDropEdit_TextChanged);
			// 
			// ComparisonOperatorForOrgCoDropEdit
			// 
			this.ComparisonOperatorForOrgCoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComparisonOperatorForOrgCoDropEdit, "ComparisonOperatorForOrgCo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.EDICodeMappingRelationshipLocalCodeModuleFilter)(null)).ComparisonOperatorForOrgCo)));
			this.ComparisonOperatorForOrgCoDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingRelationshipLocalCodeFilterControl|f4ca8643-522a-4f87-bc35-c4156dac7e34", "Local Code Comparison ");
			this.ComparisonOperatorForOrgCoDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.ComparisonOperatorForOrgCoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 23, true);
			this.ComparisonOperatorForOrgCoDropEdit.Name = "ComparisonOperatorForOrgCoDropEdit";
			this.ComparisonOperatorForOrgCoDropEdit.PreBoundMaxLength = 7;
			this.ComparisonOperatorForOrgCoDropEdit.ShowDescriptionBox = false;
			this.ComparisonOperatorForOrgCoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 18, true);
			this.ComparisonOperatorForOrgCoDropEdit.TabIndex = 4;
			this.ComparisonOperatorForOrgCoDropEdit.CodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.ComparisonOperatorForOrgCoDropEdit.CodeBox.Font = new Font(OFont.NormalFontName, 8.0f, FontStyle.Italic);
			this.ComparisonOperatorForOrgCoDropEdit.CaptionRenderingEnabled = false;
			// 
			// LocalCodeDropEdit
			// 
			this.LocalCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalCodeDropEdit, "OrgCoName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.EDICodeMappingRelationshipLocalCodeModuleFilter)(null)).OrgCoName)));
			this.LocalCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingRelationshipLocalCodeFilterControl|3c768ff6-ef5a-4a71-8a25-e27b139c575d", "Code Value");
			this.LocalCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 23, true);
			this.LocalCodeDropEdit.Name = "LocalCodeDropEdit";
			this.LocalCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 18, true);
			this.LocalCodeDropEdit.TabIndex = 5;
			// 
			// LocalGuidFindBox
			// 
			this.LocalGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalGuidFindBox, "OrgCoGuid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.EDICodeMappingRelationshipLocalCodeModuleFilter)(null)).OrgCoGuid)));
			this.LocalGuidFindBox.CaptionResourceString = this.LocalCodeDropEdit.CaptionResourceString;
			this.LocalGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 23, true);
			this.LocalGuidFindBox.Name = "LocalGuidFindBox";
			this.LocalGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocalGuidFindBox.ParentType = null;
			this.LocalGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 18, true);
			this.LocalGuidFindBox.TabIndex = 5;
			// 
			// LocalCodeFindBox
			// 
			this.LocalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalCodeFindBox, "OrgCoName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.EDICodeMappingRelationshipLocalCodeModuleFilter)(null)).OrgCoName)));
			this.LocalCodeFindBox.CaptionResourceString = this.LocalCodeDropEdit.CaptionResourceString;
			this.LocalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 23, true);
			this.LocalCodeFindBox.Name = "LocalCodeFindBox";
			this.LocalCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocalCodeFindBox.ParentType = null;
			this.LocalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 18, true);
			this.LocalCodeFindBox.TabIndex = 5;
			// 
			// EDICodeMappingRelationshipLocalCodeFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RelationshipDropEdit);
			this.Controls.Add(this.ComparisonOperatorForOrgCoDropEdit);
			this.Controls.Add(this.LocalCodeDropEdit);
			this.Controls.Add(this.LocalGuidFindBox);
			this.Controls.Add(this.LocalCodeFindBox);
			this.Name = "EDICodeMappingRelationshipLocalCodeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelationshipDropEdit.ResumeLayout(true);
			this.RelationshipDropEdit.PerformLayout();
			this.ComparisonOperatorForOrgCoDropEdit.ResumeLayout(true);
			this.ComparisonOperatorForOrgCoDropEdit.PerformLayout();
			this.LocalCodeDropEdit.ResumeLayout(true);
			this.LocalCodeDropEdit.PerformLayout();
			this.LocalGuidFindBox.ResumeLayout(true);
			this.LocalGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit RelationshipDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ComparisonOperatorForOrgCoDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit LocalCodeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox LocalGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox LocalCodeFindBox;
	}
}
