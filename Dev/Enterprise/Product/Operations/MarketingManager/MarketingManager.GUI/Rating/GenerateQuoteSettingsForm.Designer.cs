namespace Enterprise.MarketingManager.GUI
{
	partial class GenerateQuoteSettingsForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.actionTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.actionTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.createNewQuoteRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.createAmendmentRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.amendmentQuoteGrid = new Enterprise.ZArchitecture.ZGrid();
			this.multipleTradeDetailSelectionControl = new Enterprise.MarketingManager.GUI.MultipleTradeDetailSelectionControl();
			this.navigationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.createButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.tradeDetailSelectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.actionTypeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.amendmentQuoteGrid)).BeginInit();
			this.amendmentQuoteGrid.SuspendLayout();
			this.multipleTradeDetailSelectionControl.SuspendLayout();
			this.navigationPanel.SuspendLayout();
			this.tradeDetailSelectionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 469, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.GenerateQuoteSettings);
			// 
			// actionTypeGroupBox
			// 
			this.actionTypeGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("bba380aa-5a43-4130-8512-b623ace1aad6", "Quotation Type");
			this.actionTypeGroupBox.Controls.Add(this.actionTypeLabel);
			this.actionTypeGroupBox.Controls.Add(this.createNewQuoteRadioButton);
			this.actionTypeGroupBox.Controls.Add(this.createAmendmentRadioButton);
			this.actionTypeGroupBox.Controls.Add(this.amendmentQuoteGrid);
			this.actionTypeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.actionTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.actionTypeGroupBox.Name = "actionTypeGroupBox";
			this.actionTypeGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.actionTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 203, true);
			this.actionTypeGroupBox.TabIndex = 4;
			this.actionTypeGroupBox.TabStop = false;
			// 
			// actionTypeLabel
			// 
			this.actionTypeLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("862a5173-0183-4b7d-956d-da1e14c9724a", "Existing quotations exist. Please select what type of quotation you wish to create.");
			this.actionTypeLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.actionTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.actionTypeLabel.Name = "actionTypeLabel";
			this.actionTypeLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.actionTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 17, true);
			this.actionTypeLabel.TabIndex = 0;
			// 
			// createNewQuoteRadioButton
			// 
			this.createNewQuoteRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.createNewQuoteRadioButton, "ShouldCreateNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).ShouldCreateNew)));
			this.createNewQuoteRadioButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e3278fe7-eb4f-417c-89bf-0bfef93dc5fc", "Create New Quotation");
			this.createNewQuoteRadioButton.Checked = true;
			this.createNewQuoteRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.createNewQuoteRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 39, true);
			this.createNewQuoteRadioButton.Name = "createNewQuoteRadioButton";
			this.createNewQuoteRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.createNewQuoteRadioButton.TabIndex = 1;
			this.createNewQuoteRadioButton.TabStop = true;
			// 
			// createAmendmentRadioButton
			// 
			this.createAmendmentRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.createAmendmentRadioButton, "ShouldCreateAmendmentForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).ShouldCreateAmendmentForBinding)));
			this.createAmendmentRadioButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("28d8b968-3651-45cd-8f66-3dc271a08c1d", "Create Amendment for Existing");
			this.createAmendmentRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.createAmendmentRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 63, true);
			this.createAmendmentRadioButton.Name = "createAmendmentRadioButton";
			this.createAmendmentRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.createAmendmentRadioButton.TabIndex = 2;
			this.createAmendmentRadioButton.TabStop = true;
			// 
			// amendmentQuoteGrid
			// 
			this.amendmentQuoteGrid.AllowNavigation = false;
			this.amendmentQuoteGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.amendmentQuoteGrid, "QuoteSelectionItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).QuoteSelectionItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.GUI.QuoteSelectionItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).QuoteSelectionItems)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.QuoteSelectionItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).QuoteSelectionItems)).SyncRoot)).Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.QuoteSelectionItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).QuoteSelectionItems)).SyncRoot)).Quote.Summary)));
			this.amendmentQuoteGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "Number";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("bf23833b-4119-4c9f-9a21-2699dc45fce7", "Summary");
			zTextBoxColumnStyleInfo2.ColumnName = "Quote+Summary";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.amendmentQuoteGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.amendmentQuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.amendmentQuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.amendmentQuoteGrid.CopySelectedRowsAllowed = true;
			this.amendmentQuoteGrid.GridId = "34f23df7-e407-4c09-b27d-9349cb3606a5";
			this.amendmentQuoteGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.amendmentQuoteGrid.LayoutKey = "amendmentQuoteGrid";
			this.amendmentQuoteGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 87, true);
			this.amendmentQuoteGrid.Name = "amendmentQuoteGrid";
			this.amendmentQuoteGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 109, true);
			this.amendmentQuoteGrid.TabIndex = 3;
			// 
			// multipleTradeDetailSelectionControl
			// 
			this.multipleTradeDetailSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.multipleTradeDetailSelectionControl, "TradeDetailSelectionItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItemCollection)(((Enterprise.MarketingManager.GUI.GenerateQuoteSettings)(null)).TradeDetailSelectionItems)));
			this.multipleTradeDetailSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.multipleTradeDetailSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.multipleTradeDetailSelectionControl.Name = "multipleTradeDetailSelectionControl";
			this.multipleTradeDetailSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 191, true);
			this.multipleTradeDetailSelectionControl.TabIndex = 0;
			// 
			// navigationPanel
			// 
			this.navigationPanel.Controls.Add(this.createButton);
			this.navigationPanel.Controls.Add(this.cancelButton);
			this.navigationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.navigationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 438, true);
			this.navigationPanel.Name = "navigationPanel";
			this.navigationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 31, true);
			this.navigationPanel.TabIndex = 0;
			// 
			// createButton
			// 
			this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.createButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d101f475-a5b8-4dbb-ae3b-4ae10680ac62", "Create");
			this.createButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(643, 5, true);
			this.createButton.Name = "createButton";
			this.createButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.createButton.TabIndex = 1;
			this.createButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("1a5aa231-6e20-4c76-b5c1-b052de4ea07b", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(727, 5, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// tradeDetailSelectionGroupBox
			// 
			this.tradeDetailSelectionGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("388210b3-ee61-4835-8164-fce92a1a280d", "Trade Lane Detail Selection");
			this.tradeDetailSelectionGroupBox.Controls.Add(this.multipleTradeDetailSelectionControl);
			this.tradeDetailSelectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tradeDetailSelectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.tradeDetailSelectionGroupBox.Name = "tradeDetailSelectionGroupBox";
			this.tradeDetailSelectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 208, true);
			this.tradeDetailSelectionGroupBox.TabIndex = 5;
			this.tradeDetailSelectionGroupBox.TabStop = false;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.tradeDetailSelectionGroupBox);
			this.mainSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.actionTypeGroupBox);
			this.mainSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 438, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// GenerateQuoteSettingsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("dcfceb5a-299a-40d4-9bc4-882d9b2ebbee", "New Quotation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 493, true);
			this.Controls.Add(this.mainSplitContainer);
			this.Controls.Add(this.navigationPanel);
			this.DataSourceType = typeof(Enterprise.MarketingManager.GUI.GenerateQuoteSettings);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.Name = "GenerateQuoteSettingsForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.navigationPanel, 0);
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.actionTypeGroupBox.ResumeLayout(false);
			this.actionTypeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.amendmentQuoteGrid)).EndInit();
			this.amendmentQuoteGrid.ResumeLayout(false);
			this.amendmentQuoteGrid.PerformLayout();
			this.multipleTradeDetailSelectionControl.ResumeLayout(true);
			this.multipleTradeDetailSelectionControl.PerformLayout();
			this.navigationPanel.ResumeLayout(false);
			this.navigationPanel.PerformLayout();
			this.tradeDetailSelectionGroupBox.ResumeLayout(false);
			this.tradeDetailSelectionGroupBox.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel navigationPanel;
		private ZArchitecture.GUI.ZButton createButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private MultipleTradeDetailSelectionControl multipleTradeDetailSelectionControl;
		private ZArchitecture.ZLabel actionTypeLabel;
		private ZArchitecture.GUI.ZRadioButton createAmendmentRadioButton;
		private ZArchitecture.GUI.ZRadioButton createNewQuoteRadioButton;
		private ZArchitecture.ZGrid amendmentQuoteGrid;
		private ZArchitecture.GUI.ZGroupBox actionTypeGroupBox;
		private ZArchitecture.GUI.ZGroupBox tradeDetailSelectionGroupBox;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
	}
}