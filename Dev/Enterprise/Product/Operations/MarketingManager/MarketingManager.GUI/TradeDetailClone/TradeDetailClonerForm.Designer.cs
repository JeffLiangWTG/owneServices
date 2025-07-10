namespace Enterprise.MarketingManager.GUI
{
	partial class TradeDetailClonerForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.confirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.tradeDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UnselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectUnsuccessfulButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).BeginInit();
			this.tradeDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 340, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1231, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TradeDetailCloner);
			// 
			// confirmButton
			// 
			this.confirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.confirmButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e34d1152-98a4-4cfc-89b6-61338f0f394c", "Confirm");
			this.confirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1125, 309, true);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.confirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 23, true);
			this.confirmButton.TabIndex = 6;
			this.confirmButton.ToolTipCaption = null;
			this.confirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// tradeDetailsGrid
			// 
			this.tradeDetailsGrid.AllowNavigation = false;
			this.tradeDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.tradeDetailsGrid, "SourceTradeDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.Sales.Product.MP_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.PA_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.Sales.OriginCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.Sales.DestinationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.Sales.OW_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.Sales.OW_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.PA_TradeMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TradeDetail.PA_TradeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).RecurrenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TEUQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).RateOffered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).JobCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).EstimatedValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCloneItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCloner)(null)).SourceTradeDetails)).SyncRoot)).TotalEstimatedValue)));
			this.tradeDetailsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0e155c87-8289-4b45-8b33-8c7ff2d759ed", "Product");
			zTextBoxColumnStyleInfo1.ColumnName = "TradeDetail+Sales+Product+MP_Name";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6dc314a4-8fd1-4c02-b931-f88491018ce9", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "TradeDetail+PA_StatusDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("fc505c99-8082-4d83-b46b-43f5fcac0e3d", "Origin");
			zTextBoxColumnStyleInfo3.ColumnName = "TradeDetail+Sales+OriginCode";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("10c2afdb-0bab-41d3-98e1-304c5eb14671", "Destination");
			zTextBoxColumnStyleInfo4.ColumnName = "TradeDetail+Sales+DestinationCode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TradeDetail+Sales+OW_OH_Buyer";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "TradeDetail+Sales+OW_OH_Supplier";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "TradeDetail+PA_TradeMode";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "TradeDetail+PA_TradeType";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "RecurrenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ContainerType";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ContainerCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TEUQuantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Weight";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "WeightUQ";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "Volume";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "VolumeUQ";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "RateOffered";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JobCount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "EstimatedValue";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Currency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "TotalEstimatedValue";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.tradeDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.tradeDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.tradeDetailsGrid.GridId = "20179ca5-7508-45b3-9a64-41540349dbb2";
			this.tradeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeDetailsGrid.LayoutKey = "tradeDetailsGrid";
			this.tradeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.tradeDetailsGrid.Name = "tradeDetailsGrid";
			this.tradeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1215, 293, true);
			this.tradeDetailsGrid.TabIndex = 2;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ce4158fb-f0a9-480e-b773-fab7b1c43a00", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 309, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 23, true);
			this.SelectAllButton.TabIndex = 3;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// UnselectAllButton
			// 
			this.UnselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnselectAllButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("49014e82-0684-4ce5-8ae0-b8251a159440", "Un-select All");
			this.UnselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 309, true);
			this.UnselectAllButton.Name = "UnselectAllButton";
			this.UnselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UnselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 23, true);
			this.UnselectAllButton.TabIndex = 5;
			this.UnselectAllButton.ToolTipCaption = null;
			this.UnselectAllButton.Click += new System.EventHandler(this.UnselectAllButton_Click);
			// 
			// SelectUnsuccessfulButton
			// 
			this.SelectUnsuccessfulButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectUnsuccessfulButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("db9947c5-a43c-4453-9947-c94ef2374708", "Select Unsuccessful");
			this.SelectUnsuccessfulButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 309, true);
			this.SelectUnsuccessfulButton.Name = "SelectUnsuccessfulButton";
			this.SelectUnsuccessfulButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectUnsuccessfulButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 23, true);
			this.SelectUnsuccessfulButton.TabIndex = 4;
			this.SelectUnsuccessfulButton.ToolTipCaption = null;
			this.SelectUnsuccessfulButton.Click += new System.EventHandler(this.SelectUnsuccessfulButton_Click);
			// 
			// TradeDetailClonerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("1e08d2e3-2449-47f7-a4dc-2f07f3569667", "Copy Trade Details");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1231, 364, true);
			this.Controls.Add(this.SelectUnsuccessfulButton);
			this.Controls.Add(this.UnselectAllButton);
			this.Controls.Add(this.SelectAllButton);
			this.Controls.Add(this.tradeDetailsGrid);
			this.Controls.Add(this.confirmButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TradeDetailCloner);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 267, true);
			this.Name = "TradeDetailClonerForm";
			this.Controls.SetChildIndex(this.confirmButton, 0);
			this.Controls.SetChildIndex(this.tradeDetailsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SelectAllButton, 0);
			this.Controls.SetChildIndex(this.UnselectAllButton, 0);
			this.Controls.SetChildIndex(this.SelectUnsuccessfulButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).EndInit();
			this.tradeDetailsGrid.ResumeLayout(false);
			this.tradeDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid tradeDetailsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton confirmButton;
		private ZArchitecture.GUI.ZButton SelectAllButton;
		private ZArchitecture.GUI.ZButton UnselectAllButton;
		private ZArchitecture.GUI.ZButton SelectUnsuccessfulButton;
	}
}