namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectCategoryMappingControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProjectCategoriesMappingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectCategoriesMappingGrid)).BeginInit();
			this.ProjectCategoriesMappingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.IssueTypeMap);
			// 
			// ProjectCategoriesMappingGrid
			// 
			this.ProjectCategoriesMappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProjectCategoriesMappingGrid, "JiraClassificationMap");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.IssueTypeMapItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)).SyncRoot)).JiraEntityName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.IssueTypeMapItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)).SyncRoot)).SelectionCriterionFieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.IssueTypeMapItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)).SyncRoot)).SelectionCriterionFieldValue)));
			this.ProjectCategoriesMappingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JiraEntityName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo1.ColumnName = "SelectionCriterionFieldName";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "SelectionCriterionFieldValue";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProjectCategoriesMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProjectCategoriesMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProjectCategoriesMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProjectCategoriesMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectCategoriesMappingGrid.GridId = "4c6fa956-f7b3-4f99-9319-cb09095f7d66";
			this.ProjectCategoriesMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectCategoriesMappingGrid.LayoutKey = "ProjectCategoriesMappingGrid";
			this.ProjectCategoriesMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProjectCategoriesMappingGrid.Name = "ProjectCategoriesMappingGrid";
			this.ProjectCategoriesMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			this.ProjectCategoriesMappingGrid.TabIndex = 0;
			// 
			// ProjectCategoryMappingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProjectCategoriesMappingGrid);
			this.Name = "ProjectCategoryMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProjectCategoriesMappingGrid)).EndInit();
			this.ProjectCategoriesMappingGrid.ResumeLayout(false);
			this.ProjectCategoriesMappingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ProjectCategoriesMappingGrid;
	}
}
