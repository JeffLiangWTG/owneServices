namespace Enterprise.MarketingManager.GUI
{
	partial class TradeDetailCommitmentUpdaterForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.confirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.tradeDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
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
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater);
			// 
			// confirmButton
			// 
			this.confirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.confirmButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b89c6c38-68d6-4340-9924-969536721490", "Confirm");
			this.confirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1009, 311, true);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.confirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.confirmButton.TabIndex = 3;
			this.confirmButton.ToolTipCaption = null;
			this.confirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2abde29c-a099-4ab4-84c0-4e0cea5e012a", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1119, 311, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// tradeDetailsGrid
			// 
			this.tradeDetailsGrid.AllowNavigation = false;
			this.tradeDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.tradeDetailsGrid, "TradeDetailItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.Parent.Product.MP_NameMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.Parent.OW_OriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.Parent.OW_DestinationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.Parent.OW_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.Parent.OW_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.PA_TradeMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.PA_TradeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.PA_Calc_EstimatedAnnualValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.PA_Calc_EstimatedMonthlyValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetailStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).TradeDetail.ProspectDetail.PAP_RecurrenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).ExpectedTradeStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).ProspectPeriodEndType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).ProspectPeriodStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).ProspectPeriodEnd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentItem)(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater)(null)).TradeDetailItems)).SyncRoot)).ForecastTypeDescription)));
			this.tradeDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d27232fb-ed98-40c7-8b85-5411ecabfb24", "Product");
			zTextBoxColumnStyleInfo1.ColumnName = "TradeDetail+Parent+Product+MP_NameMultilingual";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TradeDetail+Parent+OW_OriginID";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "TradeDetail+Parent+OW_DestinationID";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "TradeDetail+Parent+OW_OH_Buyer";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "TradeDetail+Parent+OW_OH_Supplier";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "TradeDetail+PA_TradeMode";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.ColumnName = "TradeDetail+PA_TradeType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_RX_NKCurrency";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TradeDetail+PA_Calc_EstimatedAnnualValue";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TradeDetail+PA_Calc_EstimatedMonthlyValue";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.ColumnName = "TradeDetailStatusDescription";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "TradeDetail+ProspectDetail+PAP_RecurrenceType";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "ExpectedTradeStartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "ProspectPeriodEndType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "ProspectPeriodStart";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "ProspectPeriodEnd";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "ForecastTypeDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.tradeDetailsGrid.GridId = "20179ca5-7508-45b3-9a64-41540349dbb2";
			this.tradeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeDetailsGrid.LayoutKey = "tradeDetailsGrid";
			this.tradeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.tradeDetailsGrid.Name = "tradeDetailsGrid";
			this.tradeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1215, 293, true);
			this.tradeDetailsGrid.TabIndex = 2;
			// 
			// OpportunityTradeStatusConversionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ce015382-d878-464a-ab5c-f2b64dca0536", "Update Trade Lane Status");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1231, 364, true);
			this.Controls.Add(this.tradeDetailsGrid);
			this.Controls.Add(this.confirmButton);
			this.Controls.Add(this.cancelButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TradeDetailCommitmentUpdater);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 267, true);
			this.Name = "OpportunityTradeStatusConversionForm";
			this.Controls.SetChildIndex(this.confirmButton, 0);
			this.Controls.SetChildIndex(this.tradeDetailsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
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
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}