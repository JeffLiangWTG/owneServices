namespace Enterprise.MasterFiles.GUI
{
	partial class GlbStaffManagementControl
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
			this.roleColumn = new Aga.Controls.Tree.TreeColumn();
			this.jobTitleColumn = new Aga.Controls.Tree.TreeColumn();
			this.effectiveDateColumn = new Aga.Controls.Tree.TreeColumn();
			this.branchColumn = new Aga.Controls.Tree.TreeColumn();
			this.roleTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.jobTitleTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.effectiveDateTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.branchTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Tree
			// 
			this.Tree.Columns.Add(this.roleColumn);
			this.Tree.Columns.Add(this.jobTitleColumn);
			this.Tree.Columns.Add(this.effectiveDateColumn);
			this.Tree.Columns.Add(this.branchColumn);
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.Tree.NodeControls.Add(this.roleTextBox);
			this.Tree.NodeControls.Add(this.jobTitleTextBox);
			this.Tree.NodeControls.Add(this.effectiveDateTextBox);
			this.Tree.NodeControls.Add(this.branchTextBox);
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 365, true);
			// 
			// mainPanel
			// 
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffManagementTreeModel);
			// 
			// roleColumn
			// 
			this.roleColumn.Header = "Role";
			this.roleColumn.MinColumnWidth = 130;
			this.roleColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.roleColumn.TooltipText = null;
			this.roleColumn.Width = 199;
			// 
			// jobTitleColumn
			// 
			this.jobTitleColumn.Header = "Job Title";
			this.jobTitleColumn.MinColumnWidth = 120;
			this.jobTitleColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.jobTitleColumn.TooltipText = null;
			this.jobTitleColumn.Width = 120;
			// 
			// effectiveDateColumn
			// 
			this.effectiveDateColumn.Header = "Effective";
			this.effectiveDateColumn.MinColumnWidth = 60;
			this.effectiveDateColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.effectiveDateColumn.TooltipText = null;
			this.effectiveDateColumn.Width = 60;
			// 
			// branchColumn
			// 
			this.branchColumn.Header = "Branch";
			this.branchColumn.MinColumnWidth = 70;
			this.branchColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.branchColumn.TooltipText = null;
			this.branchColumn.Width = 70;
			// 
			// roleTextBox
			// 
			this.roleTextBox.DataPropertyName = "Role";
			this.roleTextBox.IncrementalSearchEnabled = true;
			this.roleTextBox.LeftMargin = 3;
			this.roleTextBox.ParentColumn = this.roleColumn;
			this.roleTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// jobTitleTextBox
			// 
			this.jobTitleTextBox.DataPropertyName = "JobTitle";
			this.jobTitleTextBox.IncrementalSearchEnabled = true;
			this.jobTitleTextBox.LeftMargin = 3;
			this.jobTitleTextBox.ParentColumn = this.jobTitleColumn;
			this.jobTitleTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// effectiveDateTextBox
			// 
			this.effectiveDateTextBox.DataPropertyName = "EffectiveDate";
			this.effectiveDateTextBox.IncrementalSearchEnabled = true;
			this.effectiveDateTextBox.LeftMargin = 3;
			this.effectiveDateTextBox.ParentColumn = this.effectiveDateColumn;
			this.effectiveDateTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// locationTextBox
			// 
			this.branchTextBox.DataPropertyName = "Branch";
			this.branchTextBox.IncrementalSearchEnabled = true;
			this.branchTextBox.LeftMargin = 3;
			this.branchTextBox.ParentColumn = this.branchColumn;
			this.branchTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// GlbStaffManagementControl
			// 
			this.Name = "GlbStaffManagementControl";
			this.NameOfATreeElement = Enterprise.MasterFiles.GUI.Res.GetData("c2dadd34-ee53-4e49-98f5-e250b6383bcd", "Staff Management Role");
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Tree.ResumeLayout(false);
			this.Tree.PerformLayout();
			this.mainPanel.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeColumn roleColumn;
		private Aga.Controls.Tree.TreeColumn jobTitleColumn;
		private Aga.Controls.Tree.TreeColumn effectiveDateColumn;
		private Aga.Controls.Tree.TreeColumn branchColumn;

		private Aga.Controls.Tree.NodeControls.NodeTextBox roleTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox jobTitleTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox effectiveDateTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox branchTextBox;
	}
}
