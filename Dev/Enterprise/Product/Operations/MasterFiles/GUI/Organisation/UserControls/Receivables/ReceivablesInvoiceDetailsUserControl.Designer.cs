namespace Enterprise.MasterFiles.GUI
{
	public partial class ReceivablesInvoiceDetailsUserControl
	{

		#region Component Designer generated code

		Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		Enterprise.ZArchitecture.GUI.ZTabControl InvoicingTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage InvoicingTab;
		Enterprise.ZArchitecture.GUI.ZPanel RightPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox WarehouseGroupBox;
		Enterprise.ZArchitecture.GUI.ZLinkLabel WarehouseOptionsLinkLabel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceBatchingGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PI_TypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit PI_StartDayDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit PI_IntervalDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit PI_ModuleDropEdit;
		internal Enterprise.ZArchitecture.ZGrid InvoiceBatchingGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox BuyersSellersConsolInvoiceStyleGroupbox;
		Enterprise.ZArchitecture.GUI.ZGroupBox ShippersConsolInvoiceStyleGroupbox;
		Enterprise.ZArchitecture.GUI.ZPanel LeftPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox GroupChargesBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PostingDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit StyleDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit DisplayDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit JobTypeDropEdit;
		Enterprise.ZArchitecture.ZGrid GroupChargesGrid;
		Enterprise.ZArchitecture.GUI.ZCheckBox OM_ARCombinedStatementInvoiceBoundCheckEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit;
		Enterprise.ZArchitecture.GUI.ZTabPage SequenceTab;
		Enterprise.ZArchitecture.ZGrid SequenceGrid;
		Enterprise.ZArchitecture.GUI.ZDropEdit BuyersConsolInvoiceStyleDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ShippersConsolInvoiceStyleDropEdit;
		Enterprise.ZArchitecture.ZGrid zGrid1;
		Enterprise.ZArchitecture.GUI.ZCheckBox CustomerSelfBillsCheckBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditTransportMode;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditSecondType;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditServiceDirection;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditServiceLevel;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditIsInclude;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox InvoiceCurrencyCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZTabPage ExchangeRatesTab;
		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		Enterprise.ZArchitecture.GUI.ZGroupBox JobBillingExchangeRatesGroupBox;
		AccExRateConfigs accExRateConfigs;
		Enterprise.ZArchitecture.GUI.ZGroupBox ClientOverrideExchageRatesGroupBox;
		internal Enterprise.ZArchitecture.ZGrid ExchangeRatesGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage ARInvoiceTemplateTabPage;
		TemplateConfigurationUserControl TemplateConfigurationUserControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage ARCashAdvanceTabPage;
		private CashAdvanceJobConfig ARCashAdvanceJobConfig;
		System.ComponentModel.IContainer components = null;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo19 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo20 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo21 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo22 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo23 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo24 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo25 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoicingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.InvoicingTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoiceBatchingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEditIsInclude = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.zDropEditSecondType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEditServiceDirection = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEditTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEditServiceLevel = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PI_TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PI_StartDayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PI_IntervalDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PI_ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceBatchingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BuyersSellersConsolInvoiceStyleGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShippersConsolInvoiceStyleGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BuyersConsolInvoiceStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShippersConsolInvoiceStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WarehouseOptionsLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.InvoiceDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GroupChargesBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PostingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisplayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JobTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GroupChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CustomerSelfBillsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SequenceTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SequenceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExchangeRatesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.JobBillingExchangeRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accExRateConfigs = new Enterprise.MasterFiles.GUI.AccExRateConfigs();
			this.ClientOverrideExchageRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExchangeRatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ARInvoiceTemplateTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TemplateConfigurationUserControl = new Enterprise.MasterFiles.GUI.TemplateConfigurationUserControl();
			this.ARCashAdvanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ARCashAdvanceJobConfig = new Enterprise.MasterFiles.GUI.CashAdvanceJobConfig();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.InvoicingTabControl.SuspendLayout();
			this.InvoicingTab.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.InvoiceBatchingGroupBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zDropEditIsInclude.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.zDropEditSecondType.SuspendLayout();
			this.zDropEditServiceDirection.SuspendLayout();
			this.zDropEditTransportMode.SuspendLayout();
			this.zDropEditServiceLevel.SuspendLayout();
			this.PI_TypeDropEdit.SuspendLayout();
			this.PI_StartDayDropEdit.SuspendLayout();
			this.PI_IntervalDropEdit.SuspendLayout();
			this.PI_ModuleDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceBatchingGrid)).BeginInit();
			this.InvoiceBatchingGrid.SuspendLayout();
			this.BuyersSellersConsolInvoiceStyleGroupbox.SuspendLayout();
			this.ShippersConsolInvoiceStyleGroupbox.SuspendLayout();
			this.BuyersConsolInvoiceStyleDropEdit.SuspendLayout();
			this.ShippersConsolInvoiceStyleDropEdit.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.WarehouseGroupBox.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.GroupChargesBox.SuspendLayout();
			this.PostingDropEdit.SuspendLayout();
			this.InvoiceDropEdit.SuspendLayout();
			this.StyleDropEdit.SuspendLayout();
			this.DisplayDropEdit.SuspendLayout();
			this.ModeDropEdit.SuspendLayout();
			this.DirectionDropEdit.SuspendLayout();
			this.JobTypeDropEdit.SuspendLayout();
			this.InvoiceCurrencyCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).BeginInit();
			this.GroupChargesGrid.SuspendLayout();
			this.SequenceTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SequenceGrid)).BeginInit();
			this.SequenceGrid.SuspendLayout();
			this.ExchangeRatesTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.JobBillingExchangeRatesGroupBox.SuspendLayout();
			this.accExRateConfigs.SuspendLayout();
			this.ClientOverrideExchageRatesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExchangeRatesGrid)).BeginInit();
			this.ExchangeRatesGrid.SuspendLayout();
			this.ARInvoiceTemplateTabPage.SuspendLayout();
			this.TemplateConfigurationUserControl.SuspendLayout();
			this.ARCashAdvanceTabPage.SuspendLayout();
			this.ARCashAdvanceJobConfig.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.InvoicingTabControl);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 463, true);
			this.MainPanel.TabIndex = 0;
			// 
			// InvoicingTabControl
			// 
			this.InvoicingTabControl.Controls.Add(this.InvoicingTab);
			this.InvoicingTabControl.Controls.Add(this.SequenceTab);
			this.InvoicingTabControl.Controls.Add(this.ExchangeRatesTab);
			this.InvoicingTabControl.Controls.Add(this.ARInvoiceTemplateTabPage);
			this.InvoicingTabControl.Controls.Add(this.ARCashAdvanceTabPage);
			this.InvoicingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvoicingTabControl.Name = "InvoicingTabControl";
			this.InvoicingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 460, true);
			this.InvoicingTabControl.TabIndex = 0;
			// 
			// InvoicingTab
			// 
			this.InvoicingTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|f091776c-56f2-475a-b40b-3bb033bb3312", "Invoicing");
			this.InvoicingTab.Controls.Add(this.RightPanel);
			this.InvoicingTab.Controls.Add(this.LeftPanel);
			this.InvoicingTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.InvoicingTab.Name = "InvoicingTab";
			this.InvoicingTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InvoicingTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 437, true);
			this.InvoicingTab.TabIndex = 0;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.InvoiceBatchingGroupBox);
			this.RightPanel.Controls.Add(this.BuyersSellersConsolInvoiceStyleGroupbox);
			this.RightPanel.Controls.Add(this.ShippersConsolInvoiceStyleGroupbox);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(441, 3, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 431, true);
			this.RightPanel.TabIndex = 1;
			// 
			// InvoiceBatchingGroupBox
			// 
			this.InvoiceBatchingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceBatchingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|7612b44c-2712-4ecc-8cf1-160bad6800b9", "Periodic Invoicing Configuration");
			this.InvoiceBatchingGroupBox.Controls.Add(this.zGroupBox1);
			this.InvoiceBatchingGroupBox.Controls.Add(this.zDropEditSecondType);
			this.InvoiceBatchingGroupBox.Controls.Add(this.zDropEditServiceDirection);
			this.InvoiceBatchingGroupBox.Controls.Add(this.zDropEditTransportMode);
			this.InvoiceBatchingGroupBox.Controls.Add(this.zDropEditServiceLevel);
			this.InvoiceBatchingGroupBox.Controls.Add(this.PI_TypeDropEdit);
			this.InvoiceBatchingGroupBox.Controls.Add(this.PI_StartDayDropEdit);
			this.InvoiceBatchingGroupBox.Controls.Add(this.PI_IntervalDropEdit);
			this.InvoiceBatchingGroupBox.Controls.Add(this.PI_ModuleDropEdit);
			this.InvoiceBatchingGroupBox.Controls.Add(this.InvoiceBatchingGrid);
			this.InvoiceBatchingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.InvoiceBatchingGroupBox.Name = "InvoiceBatchingGroupBox";
			this.InvoiceBatchingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 346, true);
			this.InvoiceBatchingGroupBox.TabIndex = 0;
			this.InvoiceBatchingGroupBox.TabStop = false;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|b3ac644e-f56e-4f00-bb25-63d0194b5a2b", "Specific Charge Code Configurations");
			this.zGroupBox1.Controls.Add(this.zDropEditIsInclude);
			this.zGroupBox1.Controls.Add(this.zGrid1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 237, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 103, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			// 
			// zDropEditIsInclude
			// 
			this.zDropEditIsInclude.AllowDrop = true;
			this.zDropEditIsInclude.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEditIsInclude, "CompanyData+InvoiceTypes.PI_Calc_IsInclude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Calc_IsInclude)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEditIsInclude, false);
			this.zDropEditIsInclude.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zDropEditIsInclude.Name = "zDropEditIsInclude";
			this.zDropEditIsInclude.PreBoundMaxLength = 3;
			this.zDropEditIsInclude.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 15, true);
			this.zDropEditIsInclude.TabIndex = 8;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "CompanyData+InvoiceTypes.DeferredCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).DeferredCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvTypeDeferredCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).DeferredCharges)).SyncRoot)).PO_ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgInvTypeDeferredCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).DeferredCharges)).SyncRoot)).PO_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvTypeDeferredCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).DeferredCharges)).SyncRoot)).PO_Description)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "PO_ChargeGroup";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "PO_AC";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d4bd80d3-8d38-4fae-a246-0e989236c9c4", "Job Description");
			zTextBoxColumnStyleInfo1.ColumnName = "PO_Description";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.GridId = "9526b0b6-694c-4fd3-8625-a3404b215e32";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 42, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 61, true);
			this.zGrid1.TabIndex = 5;
			// 
			// zDropEditSecondType
			// 
			this.zDropEditSecondType.AllowDrop = true;
			this.zDropEditSecondType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEditSecondType, "CompanyData+InvoiceTypes.PI_SecondaryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_SecondaryType)));
			this.zDropEditSecondType.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d4aae1ee-9199-4a57-8137-fba99f4b2fa6", "Sec. Layout", "Secondary Layout", "");
			this.zDropEditSecondType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 211, true);
			this.zDropEditSecondType.Name = "zDropEditSecondType";
			this.zDropEditSecondType.PreBoundMaxLength = 3;
			this.zDropEditSecondType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 15, true);
			this.zDropEditSecondType.TabIndex = 9;
			// 
			// zDropEditServiceDirection
			// 
			this.zDropEditServiceDirection.AllowDrop = true;
			this.zDropEditServiceDirection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEditServiceDirection, "CompanyData+InvoiceTypes.PI_ServiceDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_ServiceDirection)));
			this.zDropEditServiceDirection.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("15f11183-f243-47be-97bb-57620aa04f34", "Direction");
			this.zDropEditServiceDirection.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 185, true);
			this.zDropEditServiceDirection.Name = "zDropEditServiceDirection";
			this.zDropEditServiceDirection.PreBoundMaxLength = 3;
			this.zDropEditServiceDirection.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 15, true);
			this.zDropEditServiceDirection.TabIndex = 7;
			// 
			// zDropEditTransportMode
			// 
			this.zDropEditTransportMode.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditTransportMode, "CompanyData+InvoiceTypes.PI_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_TransportMode)));
			this.zDropEditTransportMode.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("90cb7e07-88f8-4496-af9e-6e052826b958", "Transport Mode");
			this.zDropEditTransportMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 185, true);
			this.zDropEditTransportMode.Name = "zDropEditTransportMode";
			this.zDropEditTransportMode.PreBoundMaxLength = 3;
			this.zDropEditTransportMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 15, true);
			this.zDropEditTransportMode.TabIndex = 6;
			// 
			// zDropEditServiceLevel
			// 
			this.zDropEditServiceLevel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditServiceLevel, "CompanyData+InvoiceTypes.PI_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_RS_NKServiceLevel)));
			this.zDropEditServiceLevel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DBFA8302-11B3-465D-AC83-35216CFB3C6B", "Service Level");
			this.zDropEditServiceLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 211, true);
			this.zDropEditServiceLevel.Name = "zDropEditServiceLevel";
			this.zDropEditServiceLevel.PreBoundMaxLength = 3;
			this.zDropEditServiceLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 15, true);
			this.zDropEditServiceLevel.TabIndex = 8;
			// 
			// PI_TypeDropEdit
			// 
			this.PI_TypeDropEdit.AllowDrop = true;
			this.PI_TypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PI_TypeDropEdit, "CompanyData+InvoiceTypes.PI_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Type)));
			this.PI_TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 156, true);
			this.PI_TypeDropEdit.Name = "PI_TypeDropEdit";
			this.PI_TypeDropEdit.PreBoundMaxLength = 3;
			this.PI_TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 15, true);
			this.PI_TypeDropEdit.TabIndex = 4;
			// 
			// PI_StartDayDropEdit
			// 
			this.PI_StartDayDropEdit.AllowDrop = true;
			this.PI_StartDayDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PI_StartDayDropEdit, "CompanyData+InvoiceTypes.PI_StartDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_StartDay)));
			this.PI_StartDayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 130, true);
			this.PI_StartDayDropEdit.Name = "PI_StartDayDropEdit";
			this.PI_StartDayDropEdit.PreBoundMaxLength = 3;
			this.PI_StartDayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 15, true);
			this.PI_StartDayDropEdit.TabIndex = 3;
			// 
			// PI_IntervalDropEdit
			// 
			this.PI_IntervalDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PI_IntervalDropEdit, "CompanyData+InvoiceTypes.PI_Interval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Interval)));
			this.PI_IntervalDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 156, true);
			this.PI_IntervalDropEdit.Name = "PI_IntervalDropEdit";
			this.PI_IntervalDropEdit.PreBoundMaxLength = 3;
			this.PI_IntervalDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 15, true);
			this.PI_IntervalDropEdit.TabIndex = 2;
			// 
			// PI_ModuleDropEdit
			// 
			this.PI_ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PI_ModuleDropEdit, "CompanyData+InvoiceTypes.PI_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Module)));
			this.PI_ModuleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6443b9d1-5322-4674-ae78-b862fde1bb55", "Job Type");
			this.PI_ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 130, true);
			this.PI_ModuleDropEdit.Name = "PI_ModuleDropEdit";
			this.PI_ModuleDropEdit.PreBoundMaxLength = 3;
			this.PI_ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 15, true);
			this.PI_ModuleDropEdit.TabIndex = 1;
			// 
			// InvoiceBatchingGrid
			// 
			this.InvoiceBatchingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceBatchingGrid, "CompanyData+InvoiceTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Interval)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).BillingIntervalDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_StartDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).CommenceOnDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).InvoiceLayoutDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).PI_SecondaryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceTypes)).SyncRoot)).SecondaryInvoiceLayoutDescription)));
			this.InvoiceBatchingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fe5a2c3e-9973-41ea-89bd-9d0efd20874b", "Job Type");
			zDropEditColumnStyleInfo4.ColumnName = "PI_Module";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo19.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("72db4283-3ea9-4023-8298-6fa4abdedab7", "Service Direction");
			zDropEditColumnStyleInfo19.ColumnName = "PI_ServiceDirection";
			zDropEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo20.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ed130eb2-cc8e-46c0-96ca-c60c038d3955", "Transport Mode");
			zDropEditColumnStyleInfo20.ColumnName = "PI_TransportMode";
			zDropEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo21.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("63EB971D-4A89-4808-9B70-180997A72936", "Service Level");
			zDropEditColumnStyleInfo21.ColumnName = "PI_RS_NKServiceLevel";
			zDropEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo22.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1fa761b6-13db-4a2f-a5c6-930e4981d4a5", "Billing Interval");
			zDropEditColumnStyleInfo22.ColumnName = "PI_Interval";
			zDropEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e64bae74-0c99-4271-8fa1-fdd9db1fae45", "Billing Interval Description");
			zTextBoxColumnStyleInfo2.ColumnName = "BillingIntervalDescription";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo23.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("670a74e6-4e6d-4026-92dc-61baa60ce55d", "Commence On");
			zDropEditColumnStyleInfo23.ColumnName = "PI_StartDay";
			zDropEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("056b1c75-6b1a-446e-abd4-2f0eb0db38d2", "Commence On Description");
			zTextBoxColumnStyleInfo5.ColumnName = "CommenceOnDescription";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo24.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e2fb5108-f2fe-48df-bfe1-87113e916b31", "Layout");
			zDropEditColumnStyleInfo24.ColumnName = "PI_Type";
			zDropEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0496f809-67af-44fb-a67b-8249835c8fb2", "Layout Des.");
			zTextBoxColumnStyleInfo6.ColumnName = "InvoiceLayoutDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo25.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("45173103-ce13-4122-8a39-c560b9fa729b", "Sec. Layout");
			zDropEditColumnStyleInfo25.ColumnName = "PI_SecondaryType";
			zDropEditColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("23d225fd-24d4-426c-b472-d8b0e36a4e81", "Sec. Layout Des.");
			zTextBoxColumnStyleInfo7.ColumnName = "SecondaryInvoiceLayoutDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo19);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo20);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo21);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo22);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo23);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo24);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo25);
			this.InvoiceBatchingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InvoiceBatchingGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceBatchingGrid.GridId = "25437d9c-dcdd-468d-be7e-24079fdd9284";
			this.InvoiceBatchingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceBatchingGrid.LayoutKey = "zGrid1";
			this.InvoiceBatchingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.InvoiceBatchingGrid.Name = "InvoiceBatchingGrid";
			this.InvoiceBatchingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 104, true);
			this.InvoiceBatchingGrid.TabIndex = 0;
			// 
			// BuyersSellersConsolInvoiceStyleGroupbox
			// 
			this.BuyersSellersConsolInvoiceStyleGroupbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BuyersSellersConsolInvoiceStyleGroupbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|5c9ea518-873f-4a4d-a77b-a765798eb44f", "Buyers Consol Invoicing", "Settings for Buyers Consol Invoicing and Auto-Rating.");
			this.BuyersSellersConsolInvoiceStyleGroupbox.Controls.Add(this.BuyersConsolInvoiceStyleDropEdit);
			this.BuyersSellersConsolInvoiceStyleGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 355, true);
			this.BuyersSellersConsolInvoiceStyleGroupbox.Name = "BuyersSellersConsolInvoiceStyleGroupbox";
			this.BuyersSellersConsolInvoiceStyleGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 66, true);
			this.BuyersSellersConsolInvoiceStyleGroupbox.TabIndex = 1;
			this.BuyersSellersConsolInvoiceStyleGroupbox.TabStop = false;
			// 
			// BuyersConsolInvoiceStyleDropEdit
			// 
			this.BuyersConsolInvoiceStyleDropEdit.AllowDrop = true;
			this.BuyersConsolInvoiceStyleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BuyersConsolInvoiceStyleDropEdit, "CompanyData+OB_ARBuyersConsolInvoicingStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARBuyersConsolInvoicingStyle)));
			this.BuyersConsolInvoiceStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 28, true);
			this.BuyersConsolInvoiceStyleDropEdit.Name = "BuyersConsolInvoiceStyleDropEdit";
			this.BuyersConsolInvoiceStyleDropEdit.PreBoundMaxLength = 3;
			this.BuyersConsolInvoiceStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 15, true);
			this.BuyersConsolInvoiceStyleDropEdit.TabIndex = 0;
			// 
			// ShippersConsolInvoiceStyleGroupbox
			// 
			this.ShippersConsolInvoiceStyleGroupbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.ShippersConsolInvoiceStyleGroupbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|acda073f-9127-4953-9749-749d979bba9d", "Shippers Consol Invoicing", "Settings for Shippers Consol Invoicing and Auto-Rating.");
			this.ShippersConsolInvoiceStyleGroupbox.Controls.Add(this.ShippersConsolInvoiceStyleDropEdit);
			this.ShippersConsolInvoiceStyleGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 427, true);
			this.ShippersConsolInvoiceStyleGroupbox.Name = "ShippersConsolInvoiceStyleGroupbox";
			this.ShippersConsolInvoiceStyleGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 66, true);
			this.ShippersConsolInvoiceStyleGroupbox.TabIndex = 1;
			this.ShippersConsolInvoiceStyleGroupbox.TabStop = false;
			// 
			// ShippersConsolInvoiceStyleDropEdit
			// 
			this.ShippersConsolInvoiceStyleDropEdit.AllowDrop = true;
			this.ShippersConsolInvoiceStyleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShippersConsolInvoiceStyleDropEdit, "CompanyData+OB_ARShippersConsolInvoicingStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARShippersConsolInvoicingStyle)));
			this.ShippersConsolInvoiceStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 28, true);
			this.ShippersConsolInvoiceStyleDropEdit.Name = "ShippersConsolInvoiceStyleDropEdit";
			this.ShippersConsolInvoiceStyleDropEdit.PreBoundMaxLength = 3;
			this.ShippersConsolInvoiceStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 15, true);
			this.ShippersConsolInvoiceStyleDropEdit.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.WarehouseGroupBox);
			this.LeftPanel.Controls.Add(this.InvoiceDetailsGroupBox);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 431, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// WarehouseGroupBox
			// 
			this.WarehouseGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|cd2c128c-7146-409b-a76e-42224cf7ada3", "Warehouse Invoicing Options");
			this.WarehouseGroupBox.Controls.Add(this.WarehouseOptionsLinkLabel);
			this.WarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 373, true);
			this.WarehouseGroupBox.Name = "WarehouseGroupBox";
			this.WarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 48, true);
			this.WarehouseGroupBox.TabIndex = 1;
			this.WarehouseGroupBox.TabStop = false;
			// 
			// WarehouseOptionsLinkLabel
			// 
			this.WarehouseOptionsLinkLabel.AutoSize = true;
			this.WarehouseOptionsLinkLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|368c6c72-5db0-4bc1-84cf-e3bbcced01d6", "Click here to navigate to the Warehouse Invoicing Tab");
			this.WarehouseOptionsLinkLabel.IsFontBold = false;
			this.WarehouseOptionsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.WarehouseOptionsLinkLabel.Name = "WarehouseOptionsLinkLabel";
			this.WarehouseOptionsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 13, true);
			this.WarehouseOptionsLinkLabel.TabIndex = 0;
			this.WarehouseOptionsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WarehouseOptionsLinkLabel_LinkClicked);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|dc72a814-538b-4e3e-93fa-d95681ae8cb8", "Invoice Details");
			this.InvoiceDetailsGroupBox.Controls.Add(this.GroupChargesBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.CustomerSelfBillsCheckBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.OM_ARCombinedStatementInvoiceBoundCheckEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvoiceDetailsGroupBox.Name = "InvoiceDetailsGroupBox";
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 364, true);
			this.InvoiceDetailsGroupBox.TabIndex = 0;
			this.InvoiceDetailsGroupBox.TabStop = false;
			// 
			// GroupChargesBox
			// 
			this.GroupChargesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.GroupChargesBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8aa62abd-e06c-46b1-86f7-60072f20ed79", "Charge Grouping / Roll Up");
			this.GroupChargesBox.Controls.Add(this.PostingDropEdit);
			this.GroupChargesBox.Controls.Add(this.InvoiceDropEdit);
			this.GroupChargesBox.Controls.Add(this.StyleDropEdit);
			this.GroupChargesBox.Controls.Add(this.DisplayDropEdit);
			this.GroupChargesBox.Controls.Add(this.ModeDropEdit);
			this.GroupChargesBox.Controls.Add(this.DirectionDropEdit);
			this.GroupChargesBox.Controls.Add(this.JobTypeDropEdit);
			this.GroupChargesBox.Controls.Add(this.InvoiceCurrencyCodeFindBox);
			this.GroupChargesBox.Controls.Add(this.GroupChargesGrid);
			this.GroupChargesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 60, true);
			this.GroupChargesBox.Name = "GroupChargesBox";
			this.GroupChargesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 298, true);
			this.GroupChargesBox.TabIndex = 2;
			this.GroupChargesBox.TabStop = false;
			// 
			// PostingDropEdit
			// 
			this.PostingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PostingDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_InvoicePostingStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_InvoicePostingStyle)));
			this.PostingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 249, true);
			this.PostingDropEdit.Name = "PostingDropEdit";
			this.PostingDropEdit.PreBoundMaxLength = 3;
			this.PostingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 15, true);
			this.PostingDropEdit.TabIndex = 7;
			// 
			// InvoiceDropEdit
			// 
			this.InvoiceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_InvoiceLineDisplayOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_InvoiceLineDisplayOption)));
			this.InvoiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 223, true);
			this.InvoiceDropEdit.Name = "InvoiceDropEdit";
			this.InvoiceDropEdit.PreBoundMaxLength = 3;
			this.InvoiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 15, true);
			this.InvoiceDropEdit.TabIndex = 6;
			// 
			// StyleDropEdit
			// 
			this.StyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_GroupOrSubtotalStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_GroupOrSubtotalStyle)));
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 199, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.PreBoundMaxLength = 3;
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 15, true);
			this.StyleDropEdit.TabIndex = 5;
			// 
			// DisplayDropEdit
			// 
			this.DisplayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisplayDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_GroupOrSubTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_GroupOrSubTotal)));
			this.DisplayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 173, true);
			this.DisplayDropEdit.Name = "DisplayDropEdit";
			this.DisplayDropEdit.PreBoundMaxLength = 3;
			this.DisplayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 15, true);
			this.DisplayDropEdit.TabIndex = 4;
			// 
			// ModeDropEdit
			// 
			this.ModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModeDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_TransportMode)));
			this.ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 147, true);
			this.ModeDropEdit.Name = "ModeDropEdit";
			this.ModeDropEdit.PreBoundMaxLength = 3;
			this.ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 15, true);
			this.ModeDropEdit.TabIndex = 3;
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_ServiceDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_ServiceDirection)));
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 173, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.PreBoundMaxLength = 3;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 15, true);
			this.DirectionDropEdit.TabIndex = 2;
			// 
			// JobTypeDropEdit
			// 
			this.JobTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobTypeDropEdit, "CompanyData+InvoiceRollupOrGroups.PG_JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_JobType)));
			this.JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 147, true);
			this.JobTypeDropEdit.Name = "JobTypeDropEdit";
			this.JobTypeDropEdit.PreBoundMaxLength = 3;
			this.JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 15, true);
			this.JobTypeDropEdit.TabIndex = 1;
			// 
			// InvoiceCurrencyCodeFindBox
			// 
			this.InvoiceCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceCurrencyCodeFindBox, "CompanyData+InvoiceRollupOrGroups.PG_RX_NKInvoicePostingCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_RX_NKInvoicePostingCurrency)));
			this.InvoiceCurrencyCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2504b634-79a1-41eb-b16b-ccbbf172a85a", "Currency");
			this.InvoiceCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 275, true);
			this.InvoiceCurrencyCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.InvoiceCurrencyCodeFindBox.Name = "InvoiceCurrencyCodeFindBox";
			this.InvoiceCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceCurrencyCodeFindBox.ParentType = null;
			this.InvoiceCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.InvoiceCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.InvoiceCurrencyCodeFindBox.TabIndex = 8;
			// 
			// GroupChargesGrid
			// 
			this.GroupChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GroupChargesGrid, "CompanyData+InvoiceRollupOrGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_GroupOrSubTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_GroupOrSubtotalStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_InvoiceLineDisplayOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_InvoicePostingStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgInvoiceRollupOrGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.InvoiceRollupOrGroups)).SyncRoot)).PG_RX_NKInvoicePostingCurrency)));
			this.GroupChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|baf8d615-447c-4386-b720-df37d0535417", "Job", "Job Invoice Type.");
			zDropEditColumnStyleInfo5.ColumnName = "PG_JobType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo6.ColumnName = "PG_ServiceDirection";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo7.ColumnName = "PG_TransportMode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo8.ColumnName = "PG_GroupOrSubTotal";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo9.ColumnName = "PG_GroupOrSubtotalStyle";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo10.ColumnName = "PG_InvoiceLineDisplayOption";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo11.ColumnName = "PG_InvoicePostingStyle";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0fb53e27-2ec5-4658-ac03-cad58bae9804", "Currency", "Invoice Posting Currency");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "PG_RX_NKInvoicePostingCurrency";
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.GroupChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.GroupChargesGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupChargesGrid.GridId = "63aa1c83-9969-49fa-8c66-830af71133aa";
			this.GroupChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupChargesGrid.LayoutKey = "GroupChargesGrid";
			this.GroupChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.GroupChargesGrid.Name = "GroupChargesGrid";
			this.GroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 125, true);
			this.GroupChargesGrid.TabIndex = 0;
			// 
			// CustomerSelfBillsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerSelfBillsCheckBox, "CompanyData+OB_ARCustomerSelfBillsRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_ARCustomerSelfBillsRevenue)));
			this.CustomerSelfBillsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0aaff7da-35aa-440d-b13e-ad0705909df1", "Customer Self Bills", "Customer Self Bills", "Customer Issues Self-Billing Invoices to your business");
			this.CustomerSelfBillsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CustomerSelfBillsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 15, true);
			this.CustomerSelfBillsCheckBox.Name = "CustomerSelfBillsCheckBox";
			this.CustomerSelfBillsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.CustomerSelfBillsCheckBox.TabIndex = 1;
			// 
			// OM_ARCombinedStatementInvoiceBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_ARCombinedStatementInvoiceBoundCheckEdit, "MiscServ.OM_ARCombinedStatementInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARCombinedStatementInvoice)));
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|42739187-cdb1-4214-bc95-afdac2ca0fe7", "Issue Statement Pack");
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 40, true);
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit.Name = "OM_ARCombinedStatementInvoiceBoundCheckEdit";
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.OM_ARCombinedStatementInvoiceBoundCheckEdit.TabIndex = 1;
			// 
			// OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit, "MiscServ.OM_ARReceiptInvoiceAfterPostingDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARReceiptInvoiceAfterPostingDefault)));
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|c380199b-5286-4fe5-a086-aa868f62b80d", "Enter Receipt After Posting Invoice", "Receipt After Posting Invoice.");
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.Checked = true;
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.CheckState = System.Windows.Forms.CheckState.Checked;
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 15, true);
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.Name = "OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit";
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.OM_ARReceiptInvoiceAfterPostingDefaultBoundCheckEdit.TabIndex = 0;
			// 
			// SequenceTab
			// 
			this.SequenceTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|72cc929a-c8d2-4301-a955-b7bee5c4e476", "Charge Code Print Sequence");
			this.SequenceTab.Controls.Add(this.SequenceGrid);
			this.SequenceTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.SequenceTab.Name = "SequenceTab";
			this.SequenceTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SequenceTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 437, true);
			this.SequenceTab.TabIndex = 1;
			// 
			// SequenceGrid
			// 
			this.SequenceGrid.AllowNavigation = false;
			this.SequenceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequenceGrid, "InvoiceOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).InvoiceOrders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccClientInvoiceOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).InvoiceOrders)).SyncRoot)).AI_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccClientInvoiceOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).InvoiceOrders)).SyncRoot)).AC_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccClientInvoiceOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).InvoiceOrders)).SyncRoot)).AI_InvoiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccClientInvoiceOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).InvoiceOrders)).SyncRoot)).AI_PrintOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccClientInvoiceOrder)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).InvoiceOrders)).SyncRoot)).AC_PrintSequence)));
			this.SequenceGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AI_AC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|ad88a310-2bf3-4570-ba0b-c8c74b55a928", "Desc.", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "AC_Desc";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.ColumnName = "AI_InvoiceType";
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AI_PrintOrder";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|b52d65ad-8648-4635-bd84-ec5cd7c448b0", "Base Sequence");
			zCalcEditColumnStyleInfo4.ColumnName = "AC_PrintSequence";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SequenceGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SequenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SequenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.SequenceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SequenceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.SequenceGrid.GridId = "599c8e42-2834-4607-8d79-2a415c40705e";
			this.SequenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SequenceGrid.LayoutKey = "zGrid1";
			this.SequenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SequenceGrid.Name = "SequenceGrid";
			this.SequenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 419, true);
			this.SequenceGrid.TabIndex = 0;
			// 
			// ExchangeRatesTab
			// 
			this.ExchangeRatesTab.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesInvoiceDetailsUserControl|49c488df-e813-40b8-a1d3-76b5585ad979", "Job Billing Exchange Rates");
			this.ExchangeRatesTab.Controls.Add(this.SplitContainer);
			this.ExchangeRatesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ExchangeRatesTab.Name = "ExchangeRatesTab";
			this.ExchangeRatesTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ExchangeRatesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 437, true);
			this.ExchangeRatesTab.TabIndex = 2;
			this.ExchangeRatesTab.UseVisualStyleBackColor = true;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.JobBillingExchangeRatesGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.ClientOverrideExchageRatesGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 431, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(190);
			this.SplitContainer.SplitterWidth = 62;
			this.SplitContainer.TabIndex = 4;
			// 
			// JobBillingExchangeRatesGroupBox
			// 
			this.JobBillingExchangeRatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a01f46ee-b808-4252-a3c8-51dcc2809ef3", "Job Billing Exchange Rates");
			this.JobBillingExchangeRatesGroupBox.Controls.Add(this.accExRateConfigs);
			this.JobBillingExchangeRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobBillingExchangeRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobBillingExchangeRatesGroupBox.Name = "JobBillingExchangeRatesGroupBox";
			this.JobBillingExchangeRatesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JobBillingExchangeRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 190, true);
			this.JobBillingExchangeRatesGroupBox.TabIndex = 6;
			this.JobBillingExchangeRatesGroupBox.TabStop = false;
			// 
			// accExRateConfigs
			// 
			this.accExRateConfigs.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accExRateConfigs, "CompanyData.AccARExchangeRateConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AccARExchangeRateConfigurations)));
			this.accExRateConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accExRateConfigs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.accExRateConfigs.Name = "accExRateConfigs";
			this.accExRateConfigs.ReadOnly = false;
			this.accExRateConfigs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 172, true);
			this.accExRateConfigs.TabIndex = 5;
			// 
			// ClientOverrideExchageRatesGroupBox
			// 
			this.ClientOverrideExchageRatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c7a10961-74b2-476f-818c-6afd13f53aa0", "Client Override Exchange Rates");
			this.ClientOverrideExchageRatesGroupBox.Controls.Add(this.ExchangeRatesGrid);
			this.ClientOverrideExchageRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientOverrideExchageRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientOverrideExchageRatesGroupBox.Name = "ClientOverrideExchageRatesGroupBox";
			this.ClientOverrideExchageRatesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ClientOverrideExchageRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 216, true);
			this.ClientOverrideExchageRatesGroupBox.TabIndex = 5;
			this.ClientOverrideExchageRatesGroupBox.TabStop = false;
			// 
			// ExchangeRatesGrid
			// 
			this.ExchangeRatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExchangeRatesGrid, "ExchangeRateCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExchangeRateCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefExchangeRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExchangeRateCollection)).SyncRoot)).RE_RX_NKExCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefExchangeRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExchangeRateCollection)).SyncRoot)).RE_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefExchangeRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExchangeRateCollection)).SyncRoot)).RE_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefExchangeRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExchangeRateCollection)).SyncRoot)).RE_ExRateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefExchangeRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExchangeRateCollection)).SyncRoot)).RE_SellRate)));
			this.ExchangeRatesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RE_RX_NKExCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "RE_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "RE_ExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "RE_ExRateType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RE_SellRate";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ExchangeRatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ExchangeRatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ExchangeRatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExchangeRatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ExchangeRatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ExchangeRatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExchangeRatesGrid.GridId = "96c5cdd9-61b8-43fc-ae34-10f4a513f724";
			this.ExchangeRatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExchangeRatesGrid.LayoutKey = "TermsGrid";
			this.ExchangeRatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.ExchangeRatesGrid.Name = "ExchangeRatesGrid";
			this.ExchangeRatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 198, true);
			this.ExchangeRatesGrid.TabIndex = 3;
			// 
			// ARInvoiceTemplateTabPage
			// 
			this.ARInvoiceTemplateTabPage.Controls.Add(this.TemplateConfigurationUserControl);
			this.ARInvoiceTemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ARInvoiceTemplateTabPage.Name = "ARInvTemplateTabPage";
			this.ARInvoiceTemplateTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ARInvoiceTemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 437, true);
			this.ARInvoiceTemplateTabPage.TabIndex = 3;
			this.ARInvoiceTemplateTabPage.Text = "Tax Invoice Template Config";
			this.ARInvoiceTemplateTabPage.UseVisualStyleBackColor = true;
			// 
			// TemplateConfigurationUserControl
			// 
			this.TemplateConfigurationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateConfigurationUserControl, "CompanyData.EInvoicingTemplateFileConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileViewCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.EInvoicingTemplateFileConfigurations)));
			this.TemplateConfigurationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateConfigurationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TemplateConfigurationUserControl.Name = "TemplateConfigurationUserControl";
			this.TemplateConfigurationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 431, true);
			this.TemplateConfigurationUserControl.TabIndex = 0;
			// 
			// ARCashAdvanceTabPage
			// 
			this.ARCashAdvanceTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a54614fd-4ff8-4333-acdf-667dfbb76f68", "Advance Payment Charges");
			this.ARCashAdvanceTabPage.Controls.Add(this.ARCashAdvanceJobConfig);
			this.ARCashAdvanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ARCashAdvanceTabPage.Name = "ARCashAdvanceTabPage";
			this.ARCashAdvanceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ARCashAdvanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 437, true);
			this.ARCashAdvanceTabPage.TabIndex = 4;
			this.ARCashAdvanceTabPage.UseVisualStyleBackColor = true;
			// 
			// ARCashAdvanceJobConfig
			// 
			this.ARCashAdvanceJobConfig.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ARCashAdvanceJobConfig, "CompanyData.AccARCashAdvanceConfigurations");
			this.ARCashAdvanceJobConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ARCashAdvanceJobConfig.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.ARCashAdvanceJobConfig.Name = "ARCashAdvanceJobConfig";
			this.ARCashAdvanceJobConfig.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2183, 1078, true);
			this.ARCashAdvanceJobConfig.TabIndex = 0;
			// 
			// ReceivablesInvoiceDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "ReceivablesInvoiceDetailsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 463, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.InvoicingTabControl.ResumeLayout(false);
			this.InvoicingTabControl.PerformLayout();
			this.InvoicingTab.ResumeLayout(false);
			this.InvoicingTab.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.InvoiceBatchingGroupBox.ResumeLayout(false);
			this.InvoiceBatchingGroupBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zDropEditIsInclude.ResumeLayout(true);
			this.zDropEditIsInclude.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.zDropEditSecondType.ResumeLayout(true);
			this.zDropEditSecondType.PerformLayout();
			this.zDropEditServiceDirection.ResumeLayout(true);
			this.zDropEditServiceDirection.PerformLayout();
			this.zDropEditTransportMode.ResumeLayout(true);
			this.zDropEditTransportMode.PerformLayout();
			this.zDropEditServiceLevel.ResumeLayout(true);
			this.zDropEditServiceLevel.PerformLayout();
			this.PI_TypeDropEdit.ResumeLayout(true);
			this.PI_TypeDropEdit.PerformLayout();
			this.PI_StartDayDropEdit.ResumeLayout(true);
			this.PI_StartDayDropEdit.PerformLayout();
			this.PI_IntervalDropEdit.ResumeLayout(true);
			this.PI_IntervalDropEdit.PerformLayout();
			this.PI_ModuleDropEdit.ResumeLayout(true);
			this.PI_ModuleDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceBatchingGrid)).EndInit();
			this.InvoiceBatchingGrid.ResumeLayout(false);
			this.InvoiceBatchingGrid.PerformLayout();
			this.BuyersSellersConsolInvoiceStyleGroupbox.ResumeLayout(false);
			this.BuyersSellersConsolInvoiceStyleGroupbox.PerformLayout();
			this.BuyersConsolInvoiceStyleDropEdit.ResumeLayout(true);
			this.BuyersConsolInvoiceStyleDropEdit.PerformLayout();
			this.ShippersConsolInvoiceStyleGroupbox.ResumeLayout(false);
			this.ShippersConsolInvoiceStyleGroupbox.PerformLayout();
			this.ShippersConsolInvoiceStyleDropEdit.ResumeLayout(true);
			this.ShippersConsolInvoiceStyleDropEdit.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.WarehouseGroupBox.ResumeLayout(false);
			this.WarehouseGroupBox.PerformLayout();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.GroupChargesBox.ResumeLayout(false);
			this.GroupChargesBox.PerformLayout();
			this.PostingDropEdit.ResumeLayout(true);
			this.PostingDropEdit.PerformLayout();
			this.InvoiceDropEdit.ResumeLayout(true);
			this.InvoiceDropEdit.PerformLayout();
			this.StyleDropEdit.ResumeLayout(true);
			this.StyleDropEdit.PerformLayout();
			this.DisplayDropEdit.ResumeLayout(true);
			this.DisplayDropEdit.PerformLayout();
			this.ModeDropEdit.ResumeLayout(true);
			this.ModeDropEdit.PerformLayout();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			this.JobTypeDropEdit.ResumeLayout(true);
			this.JobTypeDropEdit.PerformLayout();
			this.InvoiceCurrencyCodeFindBox.ResumeLayout(true);
			this.InvoiceCurrencyCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).EndInit();
			this.GroupChargesGrid.ResumeLayout(false);
			this.GroupChargesGrid.PerformLayout();
			this.SequenceTab.ResumeLayout(false);
			this.SequenceTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SequenceGrid)).EndInit();
			this.SequenceGrid.ResumeLayout(false);
			this.SequenceGrid.PerformLayout();
			this.ExchangeRatesTab.ResumeLayout(false);
			this.ExchangeRatesTab.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.JobBillingExchangeRatesGroupBox.ResumeLayout(false);
			this.JobBillingExchangeRatesGroupBox.PerformLayout();
			this.accExRateConfigs.ResumeLayout(true);
			this.accExRateConfigs.PerformLayout();
			this.ClientOverrideExchageRatesGroupBox.ResumeLayout(false);
			this.ClientOverrideExchageRatesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExchangeRatesGrid)).EndInit();
			this.ExchangeRatesGrid.ResumeLayout(false);
			this.ExchangeRatesGrid.PerformLayout();
			this.ARInvoiceTemplateTabPage.ResumeLayout(false);
			this.ARInvoiceTemplateTabPage.PerformLayout();
			this.TemplateConfigurationUserControl.ResumeLayout(true);
			this.TemplateConfigurationUserControl.PerformLayout();
			this.ARCashAdvanceTabPage.ResumeLayout(false);
			this.ARCashAdvanceTabPage.PerformLayout();
			this.ARCashAdvanceJobConfig.ResumeLayout(true);
			this.ARCashAdvanceJobConfig.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
