namespace Enterprise.Customs.US.AMS.GUI
{
	partial class ImportFromSailingForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/// 
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.billsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.importSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.selectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.deselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LegendSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LegendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReplaceDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReplaceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.billsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.importSplitContainer)).BeginInit();
			this.importSplitContainer.Panel1.SuspendLayout();
			this.importSplitContainer.Panel2.SuspendLayout();
			this.importSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LegendSplitContainer)).BeginInit();
			this.LegendSplitContainer.Panel1.SuspendLayout();
			this.LegendSplitContainer.Panel2.SuspendLayout();
			this.LegendSplitContainer.SuspendLayout();
			this.LegendGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 485, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.BillImportActionCollection);
			// 
			// NotificationProvider
			// 
			this.NotificationProvider.NotificationRenderer = null;
			// 
			// billsGrid
			// 
			this.billsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.billsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.BillImportAction)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.BillImportAction)(null)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.BillImportAction)(null)).ActionDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.BillImportAction)(null)).IssuerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.BillImportAction)(null)).BillNumber)));
			this.billsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("8af1283b-432f-4ad8-af9b-26c0736ce726", "Selected?");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("4688b034-6bfb-4e7f-8972-147a517418da", "Action");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "ActionDesc";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("9f1c4331-5e71-4925-acc0-b5b12513d61c", "Issuer Code");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "IssuerCode";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("7ff438ff-9dbc-40a1-8920-c287b3439518", "Bill Of Lading");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "BillNumber";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			this.billsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.billsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.billsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.billsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.billsGrid.CopySelectedRowsAllowed = true;
			this.billsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billsGrid.GridId = "b3281d42-b4fd-45be-8733-52cc0c4af731";
			this.billsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.billsGrid.LayoutKey = "billsGrid";
			this.billsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.billsGrid.Name = "billsGrid";
			this.billsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 346, true);
			this.billsGrid.TabIndex = 1;
			// 
			// importSplitContainer
			// 
			this.importSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.importSplitContainer.IsSplitterFixed = true;
			this.importSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importSplitContainer.Name = "importSplitContainer";
			this.importSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// importSplitContainer.Panel1
			// 
			this.importSplitContainer.Panel1.Controls.Add(this.LegendGroupBox);
			// 
			// importSplitContainer.Panel2
			// 
			this.importSplitContainer.Panel2.Controls.Add(this.selectAllButton);
			this.importSplitContainer.Panel2.Controls.Add(this.deselectAllButton);
			this.importSplitContainer.Panel2.Controls.Add(this.OKButton);
			this.importSplitContainer.Panel2.Controls.Add(this.cancelButton);
			this.importSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 135, true);
			this.importSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			this.importSplitContainer.TabIndex = 2;
			// 
			// selectAllButton
			// 
			this.selectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectAllButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("19d98802-71c3-43e0-8e67-1fa9bed41448", "Select All");
			this.selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 3, true);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.selectAllButton.TabIndex = 0;
			this.selectAllButton.UseVisualStyleBackColor = true;
			this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// deselectAllButton
			// 
			this.deselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deselectAllButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("13913cb7-a553-44b1-b035-bf55a0a91335", "Deselect All");
			this.deselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 3, true);
			this.deselectAllButton.Name = "deselectAllButton";
			this.deselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.deselectAllButton.TabIndex = 1;
			this.deselectAllButton.UseVisualStyleBackColor = true;
			this.deselectAllButton.Click += new System.EventHandler(this.deselectAllButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("453c96ff-33b6-4713-b8ab-9e4cf5356334", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("7758ba3c-8e4f-42db-a283-104929add418", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 3, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// LegendSplitContainer
			// 
			this.LegendSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LegendSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.LegendSplitContainer.IsSplitterFixed = true;
			this.LegendSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegendSplitContainer.Name = "LegendSplitContainer";
			this.LegendSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// LegendSplitContainer.Panel1
			// 
			this.LegendSplitContainer.Panel1.Controls.Add(this.billsGrid);
			// 
			// LegendSplitContainer.Panel2
			// 
			this.LegendSplitContainer.Panel2.Controls.Add(this.importSplitContainer);
			this.LegendSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 485, true);
			this.LegendSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(346);
			this.LegendSplitContainer.TabIndex = 3;
			// 
			// LegendGroupBox
			// 
			this.LegendGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("6bb760c6-c6bf-4394-9010-734ca7b0f4c6", "Legend");
			this.LegendGroupBox.Controls.Add(this.ReplaceDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.ReplaceLabel);
			this.LegendGroupBox.Controls.Add(this.DeleteDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.DeleteLabel);
			this.LegendGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LegendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegendGroupBox.Name = "LegendGroupBox";
			this.LegendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 102, true);
			this.LegendGroupBox.TabIndex = 0;
			this.LegendGroupBox.TabStop = false;
			// 
			// ReplaceDescriptionLabel
			// 
			this.ReplaceDescriptionLabel.IsFontBold = true;
			this.ReplaceDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
			this.ReplaceDescriptionLabel.Name = "ReplaceDescriptionLabel";
			this.ReplaceDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 34, true);
			this.ReplaceDescriptionLabel.TabIndex = 4;
			this.ReplaceDescriptionLabel.Text = "If selected, these bills will be deleted then re-added.";
			// 
			// ReplaceLabel
			// 
			this.ReplaceLabel.AutoSize = true;
			this.ReplaceLabel.BackColor = System.Drawing.Color.Transparent;
			this.ReplaceLabel.IsFontBold = true;
			this.ReplaceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 76, true);
			this.ReplaceLabel.Name = "ReplaceLabel";
			this.ReplaceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.ReplaceLabel.TabIndex = 3;
			this.ReplaceLabel.Text = "REPLACE";
			// 
			// DeleteDescriptionLabel
			// 
			this.DeleteDescriptionLabel.IsFontBold = true;
			this.DeleteDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 9, true);
			this.DeleteDescriptionLabel.Name = "DeleteDescriptionLabel";
			this.DeleteDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 45, true);
			this.DeleteDescriptionLabel.TabIndex = 2;
			this.DeleteDescriptionLabel.Text = "These bills do not existing on the sailing. Select Delete to remove them permanen" +
    "tly from the AMS job.";
			// 
			// DeleteLabel
			// 
			this.DeleteLabel.AutoSize = true;
			this.DeleteLabel.BackColor = System.Drawing.Color.Transparent;
			this.DeleteLabel.IsFontBold = true;
			this.DeleteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 25, true);
			this.DeleteLabel.Name = "DeleteLabel";
			this.DeleteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.DeleteLabel.TabIndex = 1;
			this.DeleteLabel.Text = "DELETE";
			// 
			// ImportFromSailingForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("50a3abdd-1ae6-4d3c-a99b-9a7a5b5df6a9", "Existing Bills Actions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 509, true);
			this.Controls.Add(this.LegendSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.BillImportActionCollection);
			this.MinimizeBox = false;
			this.Name = "ImportFromSailingForm";
			this.RememberFormSize = false;
			this.Text = "Existing Bills Actions";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LegendSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.billsGrid)).EndInit();
			this.importSplitContainer.Panel1.ResumeLayout(false);
			this.importSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.importSplitContainer)).EndInit();
			this.importSplitContainer.ResumeLayout(false);
			this.LegendSplitContainer.Panel1.ResumeLayout(false);
			this.LegendSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LegendSplitContainer)).EndInit();
			this.LegendSplitContainer.ResumeLayout(false);
			this.LegendGroupBox.ResumeLayout(false);
			this.LegendGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid billsGrid;
		private CargoWise.Windows.UI.KSplitContainer importSplitContainer;
		private ZArchitecture.GUI.ZButton selectAllButton;
		private ZArchitecture.GUI.ZButton deselectAllButton;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private CargoWise.Windows.UI.KSplitContainer LegendSplitContainer;
		private ZArchitecture.GUI.ZGroupBox LegendGroupBox;
		private ZArchitecture.ZLabel DeleteDescriptionLabel;
		private ZArchitecture.ZLabel DeleteLabel;
		private ZArchitecture.ZLabel ReplaceDescriptionLabel;
		private ZArchitecture.ZLabel ReplaceLabel;
	}
}