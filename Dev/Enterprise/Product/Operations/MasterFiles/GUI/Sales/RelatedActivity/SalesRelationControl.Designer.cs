namespace Enterprise.MasterFiles.GUI
{
	partial class SalesRelationControl
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
			this.salesRelationColumn = new Aga.Controls.Tree.TreeColumn();
			this.summaryColumn = new Aga.Controls.Tree.TreeColumn();
			this.createdTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.lastEditTimeColumn = new Aga.Controls.Tree.TreeColumn();
			this.typeNodeComboBox = new Aga.Controls.Tree.NodeControls.NodeComboBox();
			this.uniqueIDNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.summaryNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.createdTimeNodeDateBox = new Aga.Controls.Tree.NodeControls.NodeDateBox();
			this.lastEditTimeDateBox = new Aga.Controls.Tree.NodeControls.NodeDateBox();
			this.ShowCommunicationsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.popupToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.bottomLeftToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.communicationCheckBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.communicationCheckBoxPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// Tree
			// 
			this.Tree.Columns.Add(this.salesRelationColumn);
			this.Tree.Columns.Add(this.summaryColumn);
			this.Tree.Columns.Add(this.createdTimeColumn);
			this.Tree.Columns.Add(this.lastEditTimeColumn);
			this.Tree.ElementType = typeof(Enterprise.MasterFiles.Business.IRelatableActivity);
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.Tree.NodeControls.Add(this.typeNodeComboBox);
			this.Tree.NodeControls.Add(this.uniqueIDNodeTextBox);
			this.Tree.NodeControls.Add(this.summaryNodeTextBox);
			this.Tree.NodeControls.Add(this.createdTimeNodeDateBox);
			this.Tree.NodeControls.Add(this.lastEditTimeDateBox);
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 342, true);
			// 
			// mainPanel
			// 
			// 
			// mainPanel.Panel1
			// 
			this.mainPanel.Panel1.Controls.Add(this.communicationCheckBoxPanel);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.SalesRelationModel);
			// 
			// salesRelationColumn
			// 
			this.salesRelationColumn.Header = "Relation";
			this.salesRelationColumn.MinColumnWidth = 200;
			this.salesRelationColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.salesRelationColumn.TooltipText = null;
			this.salesRelationColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			// 
			// summaryColumn
			// 
			this.summaryColumn.Header = "Summary";
			this.summaryColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.summaryColumn.TooltipText = null;
			this.summaryColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			// 
			// createdTimeColumn
			// 
			this.createdTimeColumn.Header = "Created Time";
			this.createdTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.createdTimeColumn.TooltipText = null;
			this.createdTimeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			// 
			// lastEditTimeColumn
			// 
			this.lastEditTimeColumn.Header = "Last Edit Time";
			this.lastEditTimeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.lastEditTimeColumn.TooltipText = null;
			this.lastEditTimeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			// 
			// typeNodeComboBox
			// 
			this.typeNodeComboBox.DataPropertyName = "ActivityType";
			this.typeNodeComboBox.EditOnClick = true;
			this.typeNodeComboBox.EditorWidth = 40;
			this.typeNodeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.typeNodeComboBox.IncrementalSearchEnabled = true;
			this.typeNodeComboBox.LeftMargin = 3;
			this.typeNodeComboBox.ParentColumn = this.salesRelationColumn;
			this.typeNodeComboBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// uniqueIDNodeTextBox
			// 
			this.uniqueIDNodeTextBox.DataPropertyName = "UniqueID";
			this.uniqueIDNodeTextBox.EditOnClick = true;
			this.uniqueIDNodeTextBox.IncrementalSearchEnabled = true;
			this.uniqueIDNodeTextBox.LeftMargin = 3;
			this.uniqueIDNodeTextBox.ParentColumn = this.salesRelationColumn;
			this.uniqueIDNodeTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// summaryNodeTextBox
			// 
			this.summaryNodeTextBox.DataPropertyName = "Summary";
			this.summaryNodeTextBox.IncrementalSearchEnabled = true;
			this.summaryNodeTextBox.LeftMargin = 3;
			this.summaryNodeTextBox.ParentColumn = this.summaryColumn;
			this.summaryNodeTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// createdTimeNodeDateBox
			// 
			this.createdTimeNodeDateBox.DataPropertyName = "SystemCreateTime";
			this.createdTimeNodeDateBox.DateTimeFormat = null;
			this.createdTimeNodeDateBox.IncrementalSearchEnabled = true;
			this.createdTimeNodeDateBox.LeftMargin = 3;
			this.createdTimeNodeDateBox.ParentColumn = this.createdTimeColumn;
			this.createdTimeNodeDateBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// lastEditTimeDateBox
			// 
			this.lastEditTimeDateBox.DataPropertyName = "SystemLastEditTime";
			this.lastEditTimeDateBox.DateTimeFormat = null;
			this.lastEditTimeDateBox.IncrementalSearchEnabled = true;
			this.lastEditTimeDateBox.LeftMargin = 3;
			this.lastEditTimeDateBox.ParentColumn = this.lastEditTimeColumn;
			this.lastEditTimeDateBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// ShowCommunicationsCheckBox
			// 
			this.ShowCommunicationsCheckBox.AutoSize = true;
			this.ShowCommunicationsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0e3ffea2-bb50-48dd-9a8a-e5c86974c94a", "Show Communications");
			this.ShowCommunicationsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShowCommunicationsCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ShowCommunicationsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowCommunicationsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 2, true);
			this.ShowCommunicationsCheckBox.Name = "ShowCommunicationsCheckBox";
			this.ShowCommunicationsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 15, true);
			this.ShowCommunicationsCheckBox.TabIndex = 0;
			this.ShowCommunicationsCheckBox.CheckedChanged += new System.EventHandler(this.ShowCommunicationsCheckBox_CheckedChanged);
			// 
			// popupToolStripButton
			// 
			this.popupToolStripButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("29fc79ac-038d-4eb8-a1d3-09e005e1909b", "Popup");
			this.popupToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.popupToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.popupToolStripButton.Name = "popupToolStripButton";
			this.popupToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 21, true);
			this.popupToolStripButton.Click += new System.EventHandler(this.popupToolStripButton_Click);
			// 
			// bottomLeftToolStrip
			// 
			this.bottomLeftToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bottomLeftToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.bottomLeftToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.bottomLeftToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.bottomLeftToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.bottomLeftToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.popupToolStripButton});
			this.bottomLeftToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 378, true);
			this.bottomLeftToolStrip.Name = "bottomLeftToolStrip";
			this.bottomLeftToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 21, true);
			this.bottomLeftToolStrip.TabIndex = 8;
			this.bottomLeftToolStrip.Text = "zToolStrip1";
			// 
			// communicationCheckBoxPanel
			// 
			this.communicationCheckBoxPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.communicationCheckBoxPanel.Controls.Add(this.ShowCommunicationsCheckBox);
			this.communicationCheckBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 0, true);
			this.communicationCheckBoxPanel.Name = "communicationCheckBoxPanel";
			this.communicationCheckBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 16, true);
			this.communicationCheckBoxPanel.TabIndex = 4;
			// 
			// SalesRelationControl
			// 
			this.Controls.Add(this.bottomLeftToolStrip);
			this.Name = "SalesRelationControl";
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.bottomLeftToolStrip, 0);
			this.Tree.ResumeLayout(false);
			this.Tree.PerformLayout();
			this.mainPanel.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.communicationCheckBoxPanel.ResumeLayout(false);
			this.communicationCheckBoxPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.GUI.ZCheckBox ShowCommunicationsCheckBox;
		private Aga.Controls.Tree.TreeColumn salesRelationColumn;
		private Aga.Controls.Tree.TreeColumn summaryColumn;
		private Aga.Controls.Tree.TreeColumn createdTimeColumn;
		private Aga.Controls.Tree.NodeControls.NodeComboBox typeNodeComboBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox uniqueIDNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox summaryNodeTextBox;
		private ZArchitecture.GUI.ZToolStripButton popupToolStripButton;
		private Aga.Controls.Tree.NodeControls.NodeDateBox createdTimeNodeDateBox;
		private Aga.Controls.Tree.TreeColumn lastEditTimeColumn;
		private ZArchitecture.GUI.ZToolStrip bottomLeftToolStrip;
		private Aga.Controls.Tree.NodeControls.NodeDateBox lastEditTimeDateBox;
		protected ZArchitecture.GUI.ZPanel communicationCheckBoxPanel;
	}
}
