using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbStaffDirectReportsControl
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
			this.roleTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Tree
			// 
			this.Tree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.Tree.Columns.Add(this.roleColumn);
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.Tree.NodeControls.Add(this.roleTextBox);
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 256, true);
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
			this.roleColumn.Width = 194;
			// 
			// roleTextBox
			// 
			this.roleTextBox.DataPropertyName = "Role";
			this.roleTextBox.IncrementalSearchEnabled = true;
			this.roleTextBox.LeftMargin = 3;
			this.roleTextBox.ParentColumn = this.roleColumn;
			this.roleTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// GlbStaffManagementControl
			// 
			this.Name = "GlbStaffManagementControl";
			this.NameOfATreeElement = Enterprise.MasterFiles.GUI.Res.GetData("62ffd29d-4458-4384-85df-0513db214b60", "Staff Direct Report Role");
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

		private Aga.Controls.Tree.NodeControls.NodeTextBox roleTextBox;
	}
}
