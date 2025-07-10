namespace Enterprise.ProcessManagement.GUI
{
	partial class IssueTypeMappingControl
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
			this.IssueTypesMappingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IssueTypesMappingGrid)).BeginInit();
			this.IssueTypesMappingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.IssueTypeMap);
			// 
			// IssueTypesMappingGrid
			// 
			this.IssueTypesMappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IssueTypesMappingGrid, "JiraClassificationMap");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.IssueTypeMapItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)).SyncRoot)).JiraEntityName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.IssueTypeMapItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)).SyncRoot)).SelectionCriterionFieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.IssueTypeMapItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IssueTypeMap)(null)).JiraClassificationMap)).SyncRoot)).SelectionCriterionFieldValue)));
			this.IssueTypesMappingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JiraEntityName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo1.ColumnName = "SelectionCriterionFieldName";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "SelectionCriterionFieldValue";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.IssueTypesMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IssueTypesMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.IssueTypesMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IssueTypesMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IssueTypesMappingGrid.GridId = "2bf6bbb5-4ede-474f-be31-194c14faddee";
			this.IssueTypesMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IssueTypesMappingGrid.LayoutKey = "IssueTypesMappingGrid";
			this.IssueTypesMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IssueTypesMappingGrid.Name = "IssueTypesMappingGrid";
			this.IssueTypesMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			this.IssueTypesMappingGrid.TabIndex = 0;
			// 
			// IssueTypeMappingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IssueTypesMappingGrid);
			this.Name = "IssueTypeMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IssueTypesMappingGrid)).EndInit();
			this.IssueTypesMappingGrid.ResumeLayout(false);
			this.IssueTypesMappingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid IssueTypesMappingGrid;
	}
}
