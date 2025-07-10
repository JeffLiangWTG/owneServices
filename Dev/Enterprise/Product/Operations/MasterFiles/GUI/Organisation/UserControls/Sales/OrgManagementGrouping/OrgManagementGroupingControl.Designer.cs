namespace Enterprise.MasterFiles.GUI
{
	partial class OrgManagementGroupingControl
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
			this.RelationColumn = new Aga.Controls.Tree.TreeColumn();
			this.ClientCodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.ClientNameTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Tree
			// 
			this.Tree.Columns.Add(this.RelationColumn);
			this.Tree.ElementType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.Tree.NodeControls.Add(this.ClientCodeTextBox);
			this.Tree.NodeControls.Add(this.ClientNameTextBox);
			// 
			// RelationColumn
			// 
			this.RelationColumn.Header = "Relation";
			this.RelationColumn.MinColumnWidth = 407;
			this.RelationColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.RelationColumn.TooltipText = null;
			this.RelationColumn.Width = 407;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgManagementGroupingModel);
			// 
			// ClientCodeTextBox
			// 
			this.ClientCodeTextBox.DataPropertyName = "ClientCode";
			this.ClientCodeTextBox.IncrementalSearchEnabled = true;
			this.ClientCodeTextBox.LeftMargin = 3;
			this.ClientCodeTextBox.ParentColumn = this.RelationColumn;
			this.ClientCodeTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// ClientNameTextBox
			// 
			this.ClientNameTextBox.DataPropertyName = "ClientName";
			this.ClientNameTextBox.IncrementalSearchEnabled = true;
			this.ClientNameTextBox.LeftMargin = 3;
			this.ClientNameTextBox.ParentColumn = this.RelationColumn;
			this.ClientNameTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// OrgManagementGroupingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "OrgManagementGroupingControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeColumn RelationColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox ClientCodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox ClientNameTextBox;
	}
}
