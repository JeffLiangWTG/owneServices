namespace Enterprise.MarketingManager.GUI
{
	partial class CampaignSalesRelationControl
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
			this.organizationNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.organizationCodeColumn = new Aga.Controls.Tree.TreeColumn();
			this.contactNameColumn = new Aga.Controls.Tree.TreeColumn();
			this.organizationNameNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.organizationCodeNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.contactNameNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.informationLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// mainPanel.Panel1
			//
			this.mainPanel.Panel1.Controls.Add(this.informationLabel);
			// 
			// Tree
			// 
			this.Tree.Columns.Add(this.organizationNameColumn);
			this.Tree.Columns.Add(this.organizationCodeColumn);
			this.Tree.Columns.Add(this.contactNameColumn);
			this.Tree.NodeControls.Add(this.organizationNameNodeTextBox);
			this.Tree.NodeControls.Add(this.organizationCodeNodeTextBox);
			this.Tree.NodeControls.Add(this.contactNameNodeTextBox);
			// 
			// ShowCommunicationsCheckBox
			// 
			this.ShowCommunicationsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 3, true);
			this.ShowCommunicationsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			// 
			// organizationNameColumn
			// 
			this.organizationNameColumn.Header = "Organization Name";
			this.organizationNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.organizationNameColumn.TooltipText = null;
			this.organizationNameColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			// 
			// organizationCodeColumn
			// 
			this.organizationCodeColumn.Header = "Org. Code";
			this.organizationCodeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.organizationCodeColumn.TooltipText = null;
			this.organizationCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			// 
			// contactNameColumn
			// 
			this.contactNameColumn.Header = "Contact Name";
			this.contactNameColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.contactNameColumn.TooltipText = null;
			this.contactNameColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			// 
			// organizationNameNodeTextBox
			// 
			this.organizationNameNodeTextBox.DataPropertyName = "ClientName";
			this.organizationNameNodeTextBox.IncrementalSearchEnabled = true;
			this.organizationNameNodeTextBox.LeftMargin = 3;
			this.organizationNameNodeTextBox.ParentColumn = this.organizationNameColumn;
			// 
			// organizationCodeNodeTextBox
			// 
			this.organizationCodeNodeTextBox.DataPropertyName = "ClientCode";
			this.organizationCodeNodeTextBox.IncrementalSearchEnabled = true;
			this.organizationCodeNodeTextBox.LeftMargin = 3;
			this.organizationCodeNodeTextBox.ParentColumn = this.organizationCodeColumn;
			// 
			// contactNameNodeTextBox
			// 
			this.contactNameNodeTextBox.DataPropertyName = "ContactName";
			this.contactNameNodeTextBox.IncrementalSearchEnabled = true;
			this.contactNameNodeTextBox.LeftMargin = 3;
			this.contactNameNodeTextBox.ParentColumn = this.contactNameColumn;
			// 
			// informationLabel
			// 
			this.informationLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("09230dcf-25c6-4803-8034-33986b58b213", "This view only displays activity records post campaign delivery.");
			this.informationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			this.informationLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 1, true);
			this.informationLabel.Name = "informationLabel";
			this.informationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 19, true);
			this.informationLabel.TabIndex = 9;
			// 
			// CampaignSalesRelationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CampaignSalesRelationControl";
			this.ShowNewButton = false;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeColumn organizationNameColumn;
		private Aga.Controls.Tree.TreeColumn organizationCodeColumn;
		private Aga.Controls.Tree.TreeColumn contactNameColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox organizationNameNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox organizationCodeNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox contactNameNodeTextBox;
		private ZArchitecture.ZLabel informationLabel;
	}
}
