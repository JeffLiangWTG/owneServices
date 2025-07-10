namespace Enterprise.MarketingManager.GUI
{
	partial class MultipleTradeDetailSelectionControl
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
		protected void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.tradeDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.reasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.selectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.deselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).BeginInit();
			this.tradeDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.TradeDetailSelectionItemCollection);
			// 
			// tradeDetailsGrid
			// 
			this.tradeDetailsGrid.AllowNavigation = false;
			this.tradeDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.tradeDetailsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.Parent.Product.MP_NameMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.Parent.OW_OriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.Parent.OW_DestinationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.Parent.OW_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.Parent.OW_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.PA_TradeMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.ProspectDetail.PAP_RecurrenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.ProspectDetail.PAP_RC_NKContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.PA_TradeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_RepeatsMnth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.ProspectDetail.PAP_OH_ServiceProvider)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.GUI.TradeDetailSelectionItem)(null)).TradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency)));
			this.tradeDetailsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c8bf5880-ed6d-45ef-a102-1d83b7bc6485", "Product");
			zTextBoxColumnStyleInfo1.ColumnName = "TradeDetail+Parent+Product+MP_NameMultilingual";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TradeDetail+Parent+OW_OriginID";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "TradeDetail+Parent+OW_DestinationID";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "TradeDetail+Parent+OW_OH_Buyer";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "TradeDetail+Parent+OW_OH_Supplier";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "TradeDetail+PA_TradeMode";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "TradeDetail+ProspectDetail+PAP_RecurrenceType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.ColumnName = "TradeDetail+ProspectDetail+PAP_RC_NKContainer";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "TradeDetail+PA_TradeType";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_Units";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_Weight";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_WeightUQ";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_Volume";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_VolumeUQ";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_RepeatsMnth";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "TradeDetail+ProspectDetail+PAP_OH_ServiceProvider";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_RX_NKCurrency";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.tradeDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.tradeDetailsGrid.CopySelectedRowsAllowed = true;
			this.tradeDetailsGrid.GridId = "18839db7-2572-4e42-830b-fff5b04f92a2";
			this.tradeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeDetailsGrid.LayoutKey = "tradeDetailsGrid";
			this.tradeDetailsGrid.LimitedColumns = null;
			this.tradeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 26, true);
			this.tradeDetailsGrid.Name = "tradeDetailsGrid";
			this.tradeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 135, true);
			this.tradeDetailsGrid.TabIndex = 1;
			// 
			// reasonLabel
			// 
			this.reasonLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("50929dcb-c782-4e9e-98a7-64ba23095e86", "Please select the trade lane details that you wish to create a quotation for.");
			this.reasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 5, true);
			this.reasonLabel.Name = "reasonLabel";
			this.reasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 18, true);
			this.reasonLabel.TabIndex = 0;
			// 
			// selectAllButton
			// 
			this.selectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectAllButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("42b0c77e-2c7c-4d16-8b67-616259b2040f", "Select All");
			this.selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 165, true);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.selectAllButton.TabIndex = 2;
			this.selectAllButton.ToolTipCaption = null;
			this.selectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// deselectAllButton
			// 
			this.deselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deselectAllButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("de049c21-ca55-4d14-91d0-3aa3e0f018a9", "Deselect All");
			this.deselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 165, true);
			this.deselectAllButton.Name = "deselectAllButton";
			this.deselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.deselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.deselectAllButton.TabIndex = 3;
			this.deselectAllButton.ToolTipCaption = null;
			this.deselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			// 
			// MultipleTradeDetailSelectionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.deselectAllButton);
			this.Controls.Add(this.reasonLabel);
			this.Controls.Add(this.selectAllButton);
			this.Controls.Add(this.tradeDetailsGrid);
			this.Name = "MultipleTradeDetailSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 190, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).EndInit();
			this.tradeDetailsGrid.ResumeLayout(false);
			this.tradeDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid tradeDetailsGrid;
		private ZArchitecture.ZLabel reasonLabel;
		private ZArchitecture.GUI.ZButton selectAllButton;
		private ZArchitecture.GUI.ZButton deselectAllButton;
	}
}