namespace Enterprise.MarketingManager.GUI
{
	partial class TradeDetailSelectionForm
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.tradeDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.selectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.reasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.bottomFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.continueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).BeginInit();
			this.tradeDetailsGrid.SuspendLayout();
			this.bottomFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 339, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.EntityTradeDetailWrapperCollection);
			// 
			// tradeDetailsGrid
			// 
			this.tradeDetailsGrid.AllowNavigation = false;
			this.tradeDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.tradeDetailsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).Parent.OW_OriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).Parent.OW_DestinationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).Parent.OW_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).Parent.OW_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).PA_TradeMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).ProspectDetail.PAP_RecurrenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).ProspectDetail.PAP_RC_NKContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).TradeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).CurrentProspectPeriod.PAS_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).CurrentProspectPeriod.PAS_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).CurrentProspectPeriod.PAS_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).CurrentProspectPeriod.PAS_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).CurrentProspectPeriod.PAS_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).JobCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).ProspectDetail.PAP_OH_ServiceProvider)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.EntityTradeDetailWrapper)(null)).CurrencyCode)));
			this.tradeDetailsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Parent+OW_OriginID";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Parent+OW_DestinationID";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Parent+OW_OH_Buyer";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "Parent+OW_OH_Supplier";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "PA_TradeMode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "ProspectDetail+PAP_RecurrenceType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "ProspectDetail+PAP_RC_NKContainer";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.ColumnName = "TradeType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CurrentProspectPeriod+PAS_Units";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CurrentProspectPeriod+PAS_Weight";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.ColumnName = "CurrentProspectPeriod+PAS_WeightUQ";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CurrentProspectPeriod+PAS_Volume";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.ColumnName = "CurrentProspectPeriod+PAS_VolumeUQ";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JobCount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ProspectDetail+PAP_OH_ServiceProvider";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.tradeDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.tradeDetailsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.tradeDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.tradeDetailsGrid.CopySelectedRowsAllowed = true;
			this.tradeDetailsGrid.GridId = "18839db7-2572-4e42-830b-fff5b04f92a2";
			this.tradeDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeDetailsGrid.LayoutKey = "tradeDetailsGrid";
			this.tradeDetailsGrid.LimitedColumns = null;
			this.tradeDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 30, true);
			this.tradeDetailsGrid.Name = "tradeDetailsGrid";
			this.tradeDetailsGrid.ReadOnly = true;
			this.tradeDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 275, true);
			this.tradeDetailsGrid.TabIndex = 1;
			this.tradeDetailsGrid.DoubleClick += new System.EventHandler(this.TradeDetailsGrid_DoubleClick);
			// 
			// selectButton
			// 
			this.selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.selectButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("439907f4-d059-4e8c-a842-fefd6689e236", "Select");
			this.selectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 2, true);
			this.selectButton.Name = "selectButton";
			this.selectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.selectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.selectButton.TabIndex = 0;
			this.selectButton.ToolTipCaption = null;
			this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("00c8b01b-c598-4680-9c9f-c3b72c85f224", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(687, 2, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// reasonLabel
			// 
			this.reasonLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0203b905-ef8b-420e-aa52-ce7d46701f78", "Please select a trade lane detail that you wish to create an one off quote for.");
			this.reasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.reasonLabel.Name = "reasonLabel";
			this.reasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 22, true);
			this.reasonLabel.TabIndex = 0;
			this.reasonLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// bottomFlowLayoutPanel
			// 
			this.bottomFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bottomFlowLayoutPanel.Controls.Add(this.cancelButton);
			this.bottomFlowLayoutPanel.Controls.Add(this.continueButton);
			this.bottomFlowLayoutPanel.Controls.Add(this.selectButton);
			this.bottomFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.bottomFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 309, true);
			this.bottomFlowLayoutPanel.Name = "bottomFlowLayoutPanel";
			this.bottomFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 26, true);
			this.bottomFlowLayoutPanel.TabIndex = 2;
			// 
			// continueButton
			// 
			this.continueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.continueButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b14953a7-5464-45bf-84bf-e8a2ccfdcb0d", "Continue Without");
			this.continueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 2, true);
			this.continueButton.Name = "continueButton";
			this.continueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.continueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.continueButton.TabIndex = 1;
			this.continueButton.ToolTipCaption = null;
			this.continueButton.Visible = false;
			this.continueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// TradeDetailSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("fb7216e7-e3fd-40a5-aefb-d83aaf212d6c", "Trade Detail Selection");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 363, true);
			this.Controls.Add(this.bottomFlowLayoutPanel);
			this.Controls.Add(this.reasonLabel);
			this.Controls.Add(this.tradeDetailsGrid);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.EntityTradeDetailWrapperCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 400, true);
			this.Name = "TradeDetailSelectionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.tradeDetailsGrid, 0);
			this.Controls.SetChildIndex(this.reasonLabel, 0);
			this.Controls.SetChildIndex(this.bottomFlowLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tradeDetailsGrid)).EndInit();
			this.tradeDetailsGrid.ResumeLayout(false);
			this.tradeDetailsGrid.PerformLayout();
			this.bottomFlowLayoutPanel.ResumeLayout(false);
			this.bottomFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid tradeDetailsGrid;
		private ZArchitecture.GUI.ZButton selectButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.ZLabel reasonLabel;
		private CargoWise.Windows.UI.KFlowLayoutPanel bottomFlowLayoutPanel;
		private ZArchitecture.GUI.ZButton continueButton;
	}
}