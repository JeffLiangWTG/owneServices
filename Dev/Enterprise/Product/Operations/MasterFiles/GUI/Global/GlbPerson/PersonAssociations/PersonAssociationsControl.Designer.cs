namespace Enterprise.MasterFiles.GUI
{
	partial class PersonAssociationsControl
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
			this.groupColumn = new Aga.Controls.Tree.TreeColumn();
			this.descriptionColumn = new Aga.Controls.Tree.TreeColumn();
			this.activeColumn = new Aga.Controls.Tree.TreeColumn();
			this.primaryColumn = new Aga.Controls.Tree.TreeColumn();
			this.workingAddressUNLOCOColumn = new Aga.Controls.Tree.TreeColumn();
			this.cityColumn = new Aga.Controls.Tree.TreeColumn();
			this.stateColumn = new Aga.Controls.Tree.TreeColumn();
			this.createdTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.countryColumn = new Aga.Controls.Tree.TreeColumn();
			this.emailColumn = new Aga.Controls.Tree.TreeColumn(); 
			this.groupTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.descriptionTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.activeCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this.primaryCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this.workingAddressUNLOCOTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.cityTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.stateTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.createdTimeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.countryTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.emailTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.showInactiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Tree
			// 
			this.Tree.Columns.Add(this.groupColumn);
			this.Tree.Columns.Add(this.descriptionColumn);
			this.Tree.Columns.Add(this.primaryColumn);
			this.Tree.Columns.Add(this.workingAddressUNLOCOColumn);
			this.Tree.Columns.Add(this.cityColumn);
			this.Tree.Columns.Add(this.stateColumn);
			this.Tree.Columns.Add(this.countryColumn);
			this.Tree.Columns.Add(this.createdTimeColumn);
			this.Tree.Columns.Add(this.activeColumn);
			this.Tree.Columns.Add(this.emailColumn);			
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.Tree.NodeControls.Add(this.groupTextBox);
			this.Tree.NodeControls.Add(this.descriptionTextBox);
			this.Tree.NodeControls.Add(this.primaryCheckBox);
			this.Tree.NodeControls.Add(this.workingAddressUNLOCOTextBox);
			this.Tree.NodeControls.Add(this.countryTextBox);
			this.Tree.NodeControls.Add(this.cityTextBox);
			this.Tree.NodeControls.Add(this.stateTextBox);
			this.Tree.NodeControls.Add(this.createdTimeTextBox);
			this.Tree.NodeControls.Add(this.activeCheckBox);
			this.Tree.NodeControls.Add(this.emailTextBox);
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 365, true);
			// 
			// mainPanel
			// 
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.PersonAssociationsTreeModel);
			// 
			// groupColumn
			// 
			this.groupColumn.Header = "";
			this.groupColumn.MinColumnWidth = 10;
			this.groupColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.groupColumn.TooltipText = null;
			this.groupColumn.Width = 180;
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Header = "Description";
			this.descriptionColumn.MinColumnWidth = 10;
			this.descriptionColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.descriptionColumn.TooltipText = null;
			this.descriptionColumn.Width = 250;
			// 
			// activeColumn
			// 
			this.activeColumn.Header = "Active";
			this.activeColumn.MinColumnWidth = 10;
			this.activeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.activeColumn.TooltipText = null;
			this.activeColumn.Width = 60;
			// 
			// primaryColumn
			// 
			this.primaryColumn.Header = "Primary Workplace";
			this.primaryColumn.MinColumnWidth = 10;
			this.primaryColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.primaryColumn.TooltipText = null;
			this.primaryColumn.Width = 120;
			// 
			// workingAddressUNLOCOColumn
			// 
			this.workingAddressUNLOCOColumn.Header = "UNLOCO";
			this.workingAddressUNLOCOColumn.MinColumnWidth = 10;
			this.workingAddressUNLOCOColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.workingAddressUNLOCOColumn.TooltipText = null;
			this.workingAddressUNLOCOColumn.Width = 80;
			// 
			// cityColumn
			// 
			this.cityColumn.Header = "City";
			this.cityColumn.MinColumnWidth = 10;
			this.cityColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.cityColumn.TooltipText = null;
			this.cityColumn.Width = 150;
			// 
			// stateColumn
			// 
			this.stateColumn.Header = "State";
			this.stateColumn.MinColumnWidth = 10;
			this.stateColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.stateColumn.TooltipText = null;
			this.stateColumn.Width = 60;
			// 
			// createdTimeColumn
			// 
			this.createdTimeColumn.Header = "Created Time";
			this.createdTimeColumn.MinColumnWidth = 10;
			this.createdTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.createdTimeColumn.TooltipText = null;
			this.createdTimeColumn.Width = 170;
			// 
			// countryColumn
			// 
			this.countryColumn.Header = "Country";
			this.countryColumn.MinColumnWidth = 10;
			this.countryColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.countryColumn.TooltipText = null;
			this.countryColumn.Width = 150;
			// 
			// emailColumn
			// 
			this.emailColumn.Header = "Email";
			this.emailColumn.MinColumnWidth = 10;
			this.emailColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.emailColumn.TooltipText = null;
			this.emailColumn.Width = 150;
			// 
			// groupTextBox
			// 
			this.groupTextBox.DataPropertyName = "Grouping";
			this.groupTextBox.IncrementalSearchEnabled = true;
			this.groupTextBox.LeftMargin = 3;
			this.groupTextBox.ParentColumn = this.groupColumn;
			this.groupTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.DataPropertyName = "Description";
			this.descriptionTextBox.IncrementalSearchEnabled = true;
			this.descriptionTextBox.LeftMargin = 3;
			this.descriptionTextBox.ParentColumn = this.descriptionColumn;
			this.descriptionTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// activeCheckBox
			// 
			this.activeCheckBox.DataPropertyName = "Active";
			this.activeCheckBox.LeftMargin = 25;
			this.activeCheckBox.ParentColumn = this.activeColumn;
			// 
			// primaryCheckBox
			// 
			this.primaryCheckBox.DataPropertyName = "IsPrimary";
			this.primaryCheckBox.LeftMargin = 55;
			this.primaryCheckBox.ParentColumn = this.primaryColumn;
			this.primaryCheckBox.EditEnabled = true;
			// 
			// workingAddressUNLOCOTextBox
			// 
			this.workingAddressUNLOCOTextBox.DataPropertyName = "WorkingAddressUNLOCO";
			this.workingAddressUNLOCOTextBox.LeftMargin = 3;
			this.workingAddressUNLOCOTextBox.ParentColumn = this.workingAddressUNLOCOColumn;
			// 
			// cityTextBox
			// 
			this.cityTextBox.DataPropertyName = "City";
			this.cityTextBox.IncrementalSearchEnabled = true;
			this.cityTextBox.LeftMargin = 3;
			this.cityTextBox.ParentColumn = this.cityColumn;
			this.cityTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// stateTextBox
			// 
			this.stateTextBox.DataPropertyName = "State";
			this.stateTextBox.IncrementalSearchEnabled = true;
			this.stateTextBox.LeftMargin = 3;
			this.stateTextBox.ParentColumn = this.stateColumn;
			this.stateTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// createdTimeTextBox
			// 
			this.createdTimeTextBox.DataPropertyName = "CreatedTime";
			this.createdTimeTextBox.IncrementalSearchEnabled = true;
			this.createdTimeTextBox.LeftMargin = 3;
			this.createdTimeTextBox.ParentColumn = this.createdTimeColumn;
			this.createdTimeTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// countryTextBox
			// 
			this.countryTextBox.DataPropertyName = "Country";
			this.countryTextBox.IncrementalSearchEnabled = true;
			this.countryTextBox.LeftMargin = 3;
			this.countryTextBox.ParentColumn = this.countryColumn;
			this.countryTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// emailTextBox
			// 
			this.emailTextBox.DataPropertyName = "Email";
			this.emailTextBox.IncrementalSearchEnabled = true;
			this.emailTextBox.LeftMargin = 3;
			this.emailTextBox.ParentColumn = this.emailColumn;
			this.emailTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// showInactiveCheckBox
			// 
			this.showInactiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showInactiveCheckBox.AutoSize = true;
			this.showInactiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showInactiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 378, true);
			this.showInactiveCheckBox.Name = "showInactiveCheckBox";
			this.showInactiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 16, true);
			this.showInactiveCheckBox.TabIndex = 3;
			this.showInactiveCheckBox.Text = "Show Inactive";
			this.showInactiveCheckBox.CheckedChanged += new System.EventHandler(this.ShowInactiveCheckBox_CheckedChanged);
			// 
			// PersonAssociationsControl
			// 
			this.Controls.Add(this.showInactiveCheckBox);
			this.Name = "PersonAssociationsControl";
			this.NameOfATreeElement = Enterprise.MasterFiles.GUI.Res.GetData("D4FF790E-383D-4854-89AC-C9A3B4D234FF", "Person Association");
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.showInactiveCheckBox, 0);
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

		private Aga.Controls.Tree.TreeColumn activeColumn;
		private Aga.Controls.Tree.TreeColumn primaryColumn;
		private Aga.Controls.Tree.TreeColumn workingAddressUNLOCOColumn;
		private Aga.Controls.Tree.TreeColumn cityColumn;
		private Aga.Controls.Tree.TreeColumn descriptionColumn;
		private Aga.Controls.Tree.TreeColumn groupColumn;
		private Aga.Controls.Tree.TreeColumn stateColumn;
		private Aga.Controls.Tree.TreeColumn createdTimeColumn;
		private Aga.Controls.Tree.TreeColumn countryColumn;
		private Aga.Controls.Tree.TreeColumn emailColumn;

		private Aga.Controls.Tree.NodeControls.NodeCheckBox activeCheckBox;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox primaryCheckBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox workingAddressUNLOCOTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox cityTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox descriptionTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox groupTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox stateTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox createdTimeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox countryTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox emailTextBox;

		protected internal ZArchitecture.GUI.ZCheckBox showInactiveCheckBox;
	}
}
