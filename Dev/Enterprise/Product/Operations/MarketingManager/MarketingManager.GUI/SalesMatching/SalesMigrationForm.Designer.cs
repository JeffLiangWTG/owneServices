namespace Enterprise.MarketingManager.GUI
{
	partial class SalesMigrationForm
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
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AppendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.textLabel = new Enterprise.ZArchitecture.ZLabel();
			this.salesAssociatedEntitiesGridControl = new Enterprise.MarketingManager.GUI.SalesAssociatedEntitiesGridControl();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.matchingGrid = new Enterprise.MarketingManager.GUI.SalesMatchingGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.salesGrid = new Enterprise.MarketingManager.GUI.SalesMatchingGrid();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.UseMatchingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.salesAssociatedEntitiesGridControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.matchingGrid.SuspendLayout();
			this.salesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 479, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 24, true);
			this.MainStatusBar.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesMatching);
			// 
			// UpdateButton
			// 
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("38f162e6-e7e2-4140-99da-f6ceecd75999", "Override Organization Estimate");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 445, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 23, true);
			this.UpdateButton.TabIndex = 5;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.OverwriteButton_Click);
			// 
			// AppendButton
			// 
			this.AppendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AppendButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b1cc2d72-3cf2-436f-9fd8-3b613f33c383", "Add New Estimate");
			this.AppendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 445, true);
			this.AppendButton.Name = "AppendButton";
			this.AppendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AppendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 23, true);
			this.AppendButton.TabIndex = 6;
			this.AppendButton.UseVisualStyleBackColor = true;
			this.AppendButton.Click += new System.EventHandler(this.AppendButton_Click);
			// 
			// textLabel
			// 
			this.textLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("02e29e8b-6496-40c9-8218-203c4ddec300", "Select an existing value to use or override, or add the value to migrate.");
			this.textLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 99, true);
			this.textLabel.Name = "textLabel";
			this.textLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 32, true);
			this.textLabel.TabIndex = 2;
			// 
			// salesAssociatedEntitiesGridControl
			// 
			this.salesAssociatedEntitiesGridControl.AllowDrop = true;
			this.salesAssociatedEntitiesGridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.salesAssociatedEntitiesGridControl, "MatchedSalesCollection.LowestSalesValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISalesValue)(((Enterprise.MarketingManager.Business.SalesMatchingData)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.SalesMatching)(null)).MatchedSalesCollection)).SyncRoot)).LowestSalesValue)));
			this.salesAssociatedEntitiesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 19, true);
			this.salesAssociatedEntitiesGridControl.Name = "salesAssociatedEntitiesGridControl";
			this.salesAssociatedEntitiesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 284, true);
			this.salesAssociatedEntitiesGridControl.TabIndex = 1;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 133, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.matchingGrid);
			this.mainSplitContainer.Panel1.Controls.Add(this.zLabel1);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.zLabel2);
			this.mainSplitContainer.Panel2.Controls.Add(this.salesAssociatedEntitiesGridControl);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 306, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(454);
			this.mainSplitContainer.TabIndex = 3;
			// 
			// matchingGrid
			// 
			this.matchingGrid.AllowDrop = true;
			this.matchingGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.matchingGrid, "MatchedSalesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.SalesMatchingDataCollection)(((Enterprise.MarketingManager.Business.SalesMatching)(null)).MatchedSalesCollection)));
			this.matchingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.matchingGrid.Name = "matchingGrid";
			this.matchingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 284, true);
			this.matchingGrid.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0530081a-b327-46fd-8afe-9c7586f9122f", "Matching values");
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5e0b75a4-d95f-4289-a7bc-9f1393482b08", "Associations");
			this.zLabel2.IsFontBold = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.zLabel2.TabIndex = 0;
			// 
			// salesGrid
			// 
			this.salesGrid.AllowDrop = true;
			this.salesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.salesGrid, "SalesToMigrate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MarketingManager.Business.SalesMatchingDataCollection)(((Enterprise.MarketingManager.Business.SalesMatching)(null)).SalesToMigrate)));
			this.salesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 30, true);
			this.salesGrid.Name = "salesGrid";
			this.salesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 59, true);
			this.salesGrid.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("53fc7862-bb90-4e2e-b7a4-949636a6e774", "Value Analysis to migrate");
			this.label1.IsFontBold = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 12, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 13, true);
			this.label1.TabIndex = 0;
			// 
			// UseMatchingButton
			// 
			this.UseMatchingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UseMatchingButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("20eaa599-c731-4d08-b296-d386f1adae3d", "Use Existing Estimate");
			this.UseMatchingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 445, true);
			this.UseMatchingButton.Name = "UseMatchingButton";
			this.UseMatchingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UseMatchingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 23, true);
			this.UseMatchingButton.TabIndex = 4;
			this.UseMatchingButton.UseVisualStyleBackColor = true;
			this.UseMatchingButton.Click += new System.EventHandler(this.UseMatchingButton_Click);
			// 
			// SalesMigrationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a3e9af51-3d9e-460e-9352-fd3590cd4f9d", "Migrate Existing Estimate Values");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 503, true);
			this.Controls.Add(this.UseMatchingButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.salesGrid);
			this.Controls.Add(this.mainSplitContainer);
			this.Controls.Add(this.textLabel);
			this.Controls.Add(this.AppendButton);
			this.Controls.Add(this.UpdateButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesMatching);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			this.Name = "SalesMigrationForm";
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.AppendButton, 0);
			this.Controls.SetChildIndex(this.textLabel, 0);
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			this.Controls.SetChildIndex(this.salesGrid, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.UseMatchingButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.salesAssociatedEntitiesGridControl.ResumeLayout(true);
			this.salesAssociatedEntitiesGridControl.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel1.PerformLayout();
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			this.mainSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.matchingGrid.ResumeLayout(true);
			this.matchingGrid.PerformLayout();
			this.salesGrid.ResumeLayout(true);
			this.salesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton UpdateButton;
		private ZArchitecture.GUI.ZButton AppendButton;
		private ZArchitecture.ZLabel textLabel;
		private SalesAssociatedEntitiesGridControl salesAssociatedEntitiesGridControl;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private SalesMatchingGrid matchingGrid;
		private SalesMatchingGrid salesGrid;
		private ZArchitecture.ZLabel label1;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.GUI.ZButton UseMatchingButton;
	}
}