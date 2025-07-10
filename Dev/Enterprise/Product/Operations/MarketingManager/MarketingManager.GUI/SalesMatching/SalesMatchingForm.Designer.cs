namespace Enterprise.MarketingManager.GUI
{
	partial class SalesMatchingForm
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
		protected override void InitializeComponent()
		{
			this.UseExistingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewBuyerSupplierButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.textLabel = new Enterprise.ZArchitecture.ZLabel();
			this.salesAssociatedEntitiesGridControl = new Enterprise.MarketingManager.GUI.SalesAssociatedEntitiesGridControl();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MatchedSalesGrid = new Enterprise.MarketingManager.GUI.SalesMatchingGrid();
			this.associatedEntitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewModeTypeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.salesAssociatedEntitiesGridControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.MatchedSalesGrid.SuspendLayout();
			this.associatedEntitiesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 316, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesMatching);
			// 
			// UseExistingButton
			// 
			this.UseExistingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UseExistingButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("edfea1cc-b90b-46f9-8aa8-a64d43370f4a", "Use Existing Estimate");
			this.UseExistingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 283, true);
			this.UseExistingButton.Name = "UseExistingButton";
			this.UseExistingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UseExistingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
			this.UseExistingButton.TabIndex = 2;
			this.UseExistingButton.ToolTipCaption = null;
			this.UseExistingButton.UseVisualStyleBackColor = true;
			this.UseExistingButton.Click += new System.EventHandler(this.UseExistingButton_Click);
			// 
			// NewBuyerSupplierButton
			// 
			this.NewBuyerSupplierButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewBuyerSupplierButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c6ea4943-74ea-4eaa-930b-e05913121e13", "New Buyer / Supplier");
			this.NewBuyerSupplierButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 283, true);
			this.NewBuyerSupplierButton.Name = "NewBuyerSupplierButton";
			this.NewBuyerSupplierButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NewBuyerSupplierButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
			this.NewBuyerSupplierButton.TabIndex = 3;
			this.NewBuyerSupplierButton.ToolTipCaption = null;
			this.NewBuyerSupplierButton.UseVisualStyleBackColor = true;
			this.NewBuyerSupplierButton.Click += new System.EventHandler(this.NewBuyerSupplierButton_Click);
			// 
			// textLabel
			// 
			this.textLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c28cb81a-b894-499e-a6bb-ca9bce98473d", "Found existing values matched by Origin / Destination / Location. Select and update an existing value, or continue to create new value.");
			this.textLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.textLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.textLabel.Name = "textLabel";
			this.textLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 32, true);
			this.textLabel.TabIndex = 0;
			// 
			// salesAssociatedEntitiesGridControl
			// 
			this.salesAssociatedEntitiesGridControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.salesAssociatedEntitiesGridControl, "MatchedSalesCollection.LowestSalesValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISalesValue)(((Enterprise.MarketingManager.Business.SalesMatchingData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesMatching)(null)).MatchedSalesCollection)).SyncRoot)).LowestSalesValue)));
			this.salesAssociatedEntitiesGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesAssociatedEntitiesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.salesAssociatedEntitiesGridControl.Name = "salesAssociatedEntitiesGridControl";
			this.salesAssociatedEntitiesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 223, true);
			this.salesAssociatedEntitiesGridControl.TabIndex = 0;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 39, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.MatchedSalesGrid);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.associatedEntitiesGroupBox);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 240, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(455);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// MatchedSalesGrid
			// 
			this.MatchedSalesGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MatchedSalesGrid, "MatchedSalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.SalesMatchingDataCollection)(((Enterprise.MarketingManager.Business.SalesMatching)(null)).MatchedSalesCollection)));
			this.MatchedSalesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchedSalesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchedSalesGrid.Name = "MatchedSalesGrid";
			this.MatchedSalesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 240, true);
			this.MatchedSalesGrid.TabIndex = 2;
			// 
			// associatedEntitiesGroupBox
			// 
			this.associatedEntitiesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("16e3ada7-83a5-4fc4-a58b-dd7b4651dbb3", "Associations");
			this.associatedEntitiesGroupBox.Controls.Add(this.salesAssociatedEntitiesGridControl);
			this.associatedEntitiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.associatedEntitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.associatedEntitiesGroupBox.Name = "associatedEntitiesGroupBox";
			this.associatedEntitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 240, true);
			this.associatedEntitiesGroupBox.TabIndex = 0;
			this.associatedEntitiesGroupBox.TabStop = false;
			// 
			// NewModeTypeButton
			// 
			this.NewModeTypeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewModeTypeButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6be618d1-1a0d-4327-b0de-63a833efc702", "New Mode / Type");
			this.NewModeTypeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(593, 283, true);
			this.NewModeTypeButton.Name = "NewModeTypeButton";
			this.NewModeTypeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NewModeTypeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
			this.NewModeTypeButton.TabIndex = 4;
			this.NewModeTypeButton.ToolTipCaption = null;
			this.NewModeTypeButton.UseVisualStyleBackColor = true;
			this.NewModeTypeButton.Click += new System.EventHandler(this.NewModeTypeButton_Click);
			// 
			// SalesMatchingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("19c44836-9092-479b-b40e-109d596817f3", "Matched Existing Estimate Values");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 340, true);
			this.Controls.Add(this.NewModeTypeButton);
			this.Controls.Add(this.mainSplitContainer);
			this.Controls.Add(this.textLabel);
			this.Controls.Add(this.NewBuyerSupplierButton);
			this.Controls.Add(this.UseExistingButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesMatching);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			this.Name = "SalesMatchingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.UseExistingButton, 0);
			this.Controls.SetChildIndex(this.NewBuyerSupplierButton, 0);
			this.Controls.SetChildIndex(this.textLabel, 0);
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			this.Controls.SetChildIndex(this.NewModeTypeButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.salesAssociatedEntitiesGridControl.ResumeLayout(true);
			this.salesAssociatedEntitiesGridControl.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.MatchedSalesGrid.ResumeLayout(true);
			this.MatchedSalesGrid.PerformLayout();
			this.associatedEntitiesGroupBox.ResumeLayout(false);
			this.associatedEntitiesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.GUI.ZButton UseExistingButton;
		private ZArchitecture.GUI.ZButton NewBuyerSupplierButton;
		private ZArchitecture.ZLabel textLabel;
		private SalesAssociatedEntitiesGridControl salesAssociatedEntitiesGridControl;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox associatedEntitiesGroupBox;
		private SalesMatchingGrid MatchedSalesGrid;
		private ZArchitecture.GUI.ZButton NewModeTypeButton;
	}
}