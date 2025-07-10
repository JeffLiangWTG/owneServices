namespace Enterprise.MasterFiles.GUI
{
	public partial class ReceivablesUserControl
	{

		#region Component Designer generated code

		public Enterprise.ZArchitecture.GUI.ZTemplateTabControl ARTabControl;
		public ReceivablesConfigurationUserControl ReceivablesPageControl;
		internal ReceivablesInvoiceDetailsUserControl ReceivablesInvoiceDetailsControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage ARTabPage1;
		public Enterprise.ZArchitecture.GUI.ZTabPage InvoiceDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage CollectionCallsTabPage;
		private OrgCollectionNotesControl orgCollectionNotesControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage CreditControlAndSettlementTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage taxConfigurationTabPage;
		private System.ComponentModel.IContainer components;
		private CargoWise.Windows.UI.KSplitContainer taxConfigurationSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox accOrgTaxConfigurationGroup;
		private ZArchitecture.ZGrid accOrgTaxConfigurationGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox accOrgTaxRateGroup;
		private ZArchitecture.ZGrid accOrgTaxRateGrid;
		private Organisation.UserControls.Receivables.CreditControlAndSettlementControl creditControlAndSettlementControl;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox taxConfigurationTemplateGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton redefaultFromTemplateButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox taxConfigurationMainGroupBox;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ARTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ARTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CreditControlAndSettlementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CollectionCallsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.taxConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ARTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ARTabControl
			// 
			this.ARTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ARTabControl.Controls.Add(this.ARTabPage1);
			this.ARTabControl.Controls.Add(this.CreditControlAndSettlementTabPage);
			this.ARTabControl.Controls.Add(this.InvoiceDetailsTabPage);
			this.ARTabControl.Controls.Add(this.CollectionCallsTabPage);
			this.ARTabControl.Controls.Add(this.taxConfigurationTabPage);
			this.ARTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ARTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.ARTabControl.Name = "ARTabControl";
			this.ARTabControl.SelectedIndex = 0;
			this.ARTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 546, true);
			this.ARTabControl.TabIndex = 0;
			// 
			// ARTabPage1
			// 
			this.ARTabPage1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesUserControl|0d3350d0-25f7-49f1-b74a-5be44fa8990d", "Configuration");
			this.ARTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.ARTabPage1.Name = "ARTabPage1";
			this.ARTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.ARTabPage1.TabIndex = 0;
			this.ARTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.ARTabPage1_InitializeTab));
			// 
			// CreditControlAndSettlementTabPage
			// 
			this.CreditControlAndSettlementTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesUserControl|03b4500d-db6b-4165-ba97-e6618af8461f", "Credit Control and Settlement");
			this.CreditControlAndSettlementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.CreditControlAndSettlementTabPage.Name = "CreditControlAndSettlementTabPage";
			this.CreditControlAndSettlementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.CreditControlAndSettlementTabPage.TabIndex = 3;
			this.CreditControlAndSettlementTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CreditControlAndSettlementTabPage_InitializeTab));
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesUserControl|35c5eab1-6bf5-47cf-a06d-9ffec2e3f593", "Invoicing");
			this.InvoiceDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.InvoiceDetailsTabPage.Name = "InvoiceDetailsTabPage";
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.InvoiceDetailsTabPage.TabIndex = 1;
			this.InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.InvoiceDetailsTabPage_InitializeTab));
			// 
			// CollectionCallsTabPage
			// 
			this.CollectionCallsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ReceivablesUserControl|5367836e-856c-43a9-81a2-ee83dd6ca9d5", "Collection Calls");
			this.CollectionCallsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.CollectionCallsTabPage.Name = "CollectionCallsTabPage";
			this.CollectionCallsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CollectionCallsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.CollectionCallsTabPage.TabIndex = 2;
			this.CollectionCallsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CollectionCallsTabPage_InitializeTab));
			// 
			// taxConfigurationTabPage
			// 
			this.taxConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("447311a3-eab0-4209-bd0e-8f2969dc584b", "Tax Configuration");
			this.taxConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.taxConfigurationTabPage.Name = "taxConfigurationTabPage";
			this.taxConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.taxConfigurationTabPage.TabIndex = 4;
			this.taxConfigurationTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.taxConfigurationTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_RateNumerator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).OTR_RateDenominator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccOrgTaxRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).TaxRates)).SyncRoot)).Rate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).OTC_ETC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).OTC_ETC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).OTC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).OTC_RecoverTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.AROrgTaxConfigurations)).SyncRoot)).OTC_IsThresholdUsed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_OCT_ARTaxTemplate)));
			// 
			// ReceivablesUserControl
			// 
			this.Controls.Add(this.ARTabControl);
			this.IsModifyReceivables = true;
			this.IsModifyReceivablesConfig = true;
			this.IsModifyReceivablesInvoicing = true;
			this.Name = "ReceivablesUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 570, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.ARTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ARTabControl.ResumeLayout(false);
			this.ARTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void CollectionCallsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.orgCollectionNotesControl1 = new Enterprise.MasterFiles.GUI.OrgCollectionNotesControl();
			this.CollectionCallsTabPage.SuspendLayout();
			this.orgCollectionNotesControl1.SuspendLayout();
			this.CollectionCallsTabPage.Controls.Add(this.orgCollectionNotesControl1);
			// 
			// orgCollectionNotesControl1
			// 
			this.orgCollectionNotesControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orgCollectionNotesControl1, ".");
			this.orgCollectionNotesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orgCollectionNotesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.orgCollectionNotesControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 460, true);
			this.orgCollectionNotesControl1.Name = "orgCollectionNotesControl1";
			this.orgCollectionNotesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 518, true);
			this.orgCollectionNotesControl1.TabIndex = 0;
			this.CollectionCallsTabPage.PerformLayout();
			this.orgCollectionNotesControl1.ResumeLayout(true);
			this.orgCollectionNotesControl1.PerformLayout();
			this.CollectionCallsTabPage.ResumeLayout(true);
		}

		private void InvoiceDetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ReceivablesInvoiceDetailsControl = new Enterprise.MasterFiles.GUI.ReceivablesInvoiceDetailsUserControl();
			this.InvoiceDetailsTabPage.SuspendLayout();
			this.ReceivablesInvoiceDetailsControl.SuspendLayout();
			this.InvoiceDetailsTabPage.Controls.Add(this.ReceivablesInvoiceDetailsControl);
			// 
			// ReceivablesInvoiceDetailsControl
			// 
			this.ReceivablesInvoiceDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivablesInvoiceDetailsControl, ".");
			this.ReceivablesInvoiceDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceivablesInvoiceDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceivablesInvoiceDetailsControl.Name = "ReceivablesInvoiceDetailsControl";
			this.ReceivablesInvoiceDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.ReceivablesInvoiceDetailsControl.TabIndex = 0;
			this.InvoiceDetailsTabPage.PerformLayout();
			this.ReceivablesInvoiceDetailsControl.ResumeLayout(true);
			this.ReceivablesInvoiceDetailsControl.PerformLayout();
			this.InvoiceDetailsTabPage.ResumeLayout(true);
		}

		private void ARTabPage1_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ReceivablesPageControl = new Enterprise.MasterFiles.GUI.ReceivablesConfigurationUserControl();
			this.ARTabPage1.SuspendLayout();
			this.ReceivablesPageControl.SuspendLayout();
			this.ARTabPage1.Controls.Add(this.ReceivablesPageControl);
			// 
			// ReceivablesPageControl
			// 
			this.ReceivablesPageControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceivablesPageControl, ".");
			this.ReceivablesPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceivablesPageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceivablesPageControl.Name = "ReceivablesPageControl";
			this.ReceivablesPageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.ReceivablesPageControl.TabIndex = 0;
			this.ARTabPage1.PerformLayout();
			this.ReceivablesPageControl.ResumeLayout(true);
			this.ReceivablesPageControl.PerformLayout();
			this.ARTabPage1.ResumeLayout(true);
		}

		private void CreditControlAndSettlementTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.creditControlAndSettlementControl = new Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables.CreditControlAndSettlementControl();
			this.CreditControlAndSettlementTabPage.SuspendLayout();
			this.creditControlAndSettlementControl.SuspendLayout();
			this.CreditControlAndSettlementTabPage.Controls.Add(this.creditControlAndSettlementControl);
			// 
			// creditControlAndSettlementControl
			// 
			this.creditControlAndSettlementControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.creditControlAndSettlementControl, ".");
			this.creditControlAndSettlementControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.creditControlAndSettlementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.creditControlAndSettlementControl.Name = "creditControlAndSettlementControl";
			this.creditControlAndSettlementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 524, true);
			this.creditControlAndSettlementControl.TabIndex = 0;
			this.CreditControlAndSettlementTabPage.PerformLayout();
			this.creditControlAndSettlementControl.ResumeLayout(true);
			this.creditControlAndSettlementControl.PerformLayout();
			this.CreditControlAndSettlementTabPage.ResumeLayout(true);
		}

		private void taxConfigurationTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.taxConfigurationSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.accOrgTaxRateGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accOrgTaxRateGrid = new Enterprise.ZArchitecture.ZGrid();
			this.accOrgTaxConfigurationGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accOrgTaxConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.taxConfigurationMainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.taxConfigurationTemplateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.redefaultFromTemplateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.taxConfigurationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationSplitContainer)).BeginInit();
			this.taxConfigurationSplitContainer.Panel1.SuspendLayout();
			this.taxConfigurationSplitContainer.Panel2.SuspendLayout();
			this.taxConfigurationSplitContainer.SuspendLayout();
			this.accOrgTaxRateGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxRateGrid)).BeginInit();
			this.accOrgTaxRateGrid.SuspendLayout();
			this.accOrgTaxConfigurationGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxConfigurationGrid)).BeginInit();
			this.accOrgTaxConfigurationGrid.SuspendLayout();
			this.taxConfigurationMainGroupBox.SuspendLayout();
			this.taxConfigurationTemplateGuidFindBox.SuspendLayout();
			this.taxConfigurationTabPage.Controls.Add(this.taxConfigurationMainGroupBox);
			// 
			// taxConfigurationSplitContainer
			// 
			this.taxConfigurationSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.taxConfigurationSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.taxConfigurationSplitContainer.Name = "taxConfigurationSplitContainer";
			// 
			// taxConfigurationSplitContainer.Panel1
			// 
			this.taxConfigurationSplitContainer.Panel1.Controls.Add(this.accOrgTaxConfigurationGroup);
			// 
			// taxConfigurationSplitContainer.Panel2
			// 
			this.taxConfigurationSplitContainer.Panel2.Controls.Add(this.accOrgTaxRateGroup);
			this.taxConfigurationSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 461, true);
			this.taxConfigurationSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(288);
			this.taxConfigurationSplitContainer.TabIndex = 0;
			// 
			// accOrgTaxRateGroup
			// 
			this.accOrgTaxRateGroup.Controls.Add(this.accOrgTaxRateGrid);
			this.accOrgTaxRateGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxRateGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accOrgTaxRateGroup.Name = "accOrgTaxRateGroup";
			this.accOrgTaxRateGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 461, true);
			this.accOrgTaxRateGroup.TabIndex = 4;
			this.accOrgTaxRateGroup.TabStop = false;
			// 
			// accOrgTaxRateGrid
			// 
			this.accOrgTaxRateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.accOrgTaxRateGrid, "CompanyData.AROrgTaxConfigurations.TaxRates");
			this.accOrgTaxRateGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "OTR_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "OTR_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "OTR_Source";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OTR_RateNumerator";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OTR_RateDenominator";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Rate";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.accOrgTaxRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.accOrgTaxRateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxRateGrid.GridId = "3fb1e733-bd99-4c76-bb98-75f72508cfb6";
			this.accOrgTaxRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.accOrgTaxRateGrid.LayoutKey = "accOrgTaxConfigurationGrid";
			this.accOrgTaxRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.accOrgTaxRateGrid.Name = "accOrgTaxRateGrid";
			this.accOrgTaxRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 442, true);
			this.accOrgTaxRateGrid.TabIndex = 2;
			// 
			// accOrgTaxConfigurationGroup
			// 
			this.accOrgTaxConfigurationGroup.Controls.Add(this.accOrgTaxConfigurationGrid);
			this.accOrgTaxConfigurationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxConfigurationGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accOrgTaxConfigurationGroup.Name = "accOrgTaxConfigurationGroup";
			this.accOrgTaxConfigurationGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 461, true);
			this.accOrgTaxConfigurationGroup.TabIndex = 4;
			this.accOrgTaxConfigurationGroup.TabStop = false;
			// 
			// accOrgTaxConfigurationGrid
			// 
			this.accOrgTaxConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.accOrgTaxConfigurationGrid, "CompanyData.AROrgTaxConfigurations");
			this.accOrgTaxConfigurationGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "OTC_ETC";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo1.ColumnName = "OTC_ETC_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo1.ColumnName = "OTC_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.ColumnName = "OTC_RecoverTax";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo3.ColumnName = "OTC_IsThresholdUsed";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.accOrgTaxConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.accOrgTaxConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accOrgTaxConfigurationGrid.GridId = "3fb1e733-bd99-4c76-bb98-75f72508cfb6";
			this.accOrgTaxConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.accOrgTaxConfigurationGrid.LayoutKey = "accOrgTaxConfigurationGrid";
			this.accOrgTaxConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.accOrgTaxConfigurationGrid.Name = "accOrgTaxConfigurationGrid";
			this.accOrgTaxConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 442, true);
			this.accOrgTaxConfigurationGrid.TabIndex = 1;
			// 
			// taxConfigurationMainGroupBox
			// 
			this.taxConfigurationMainGroupBox.Controls.Add(this.redefaultFromTemplateButton);
			this.taxConfigurationMainGroupBox.Controls.Add(this.taxConfigurationTemplateGuidFindBox);
			this.taxConfigurationMainGroupBox.Controls.Add(this.taxConfigurationSplitContainer);
			this.taxConfigurationMainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taxConfigurationMainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.taxConfigurationMainGroupBox.Name = "taxConfigurationMainGroupBox";
			this.taxConfigurationMainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 519, true);
			this.taxConfigurationMainGroupBox.TabIndex = 3;
			this.taxConfigurationMainGroupBox.TabStop = false;
			// 
			// taxConfigurationTemplateGuidFindBox
			// 
			this.taxConfigurationTemplateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxConfigurationTemplateGuidFindBox, "CompanyData.OB_OCT_ARTaxTemplate");
			this.taxConfigurationTemplateGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f2f5bb5a-b492-465d-a2a9-4fbc27f169cc", "Tax Configuration Template");
			this.taxConfigurationTemplateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 23, true);
			this.taxConfigurationTemplateGuidFindBox.Name = "taxConfigurationTemplateGuidFindBox";
			this.taxConfigurationTemplateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.taxConfigurationTemplateGuidFindBox.ParentType = null;
			this.taxConfigurationTemplateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 20, true);
			this.taxConfigurationTemplateGuidFindBox.TabIndex = 1;
			// 
			// redefaultFromTemplateButton
			// 
			this.redefaultFromTemplateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0fe0bba9-228c-44b6-ad15-51036be6bbe8", "Re-default from Template");
			this.redefaultFromTemplateButton.IsCaptionOverridden = false;
			this.redefaultFromTemplateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 19, true);
			this.redefaultFromTemplateButton.Name = "redefaultFromTemplateButton";
			this.redefaultFromTemplateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.redefaultFromTemplateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 24, true);
			this.redefaultFromTemplateButton.TabIndex = 2;
			this.redefaultFromTemplateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.redefaultFromTemplateButton.ToolTipCaption = null;
			this.redefaultFromTemplateButton.UseVisualStyleBackColor = true;
			this.redefaultFromTemplateButton.Click += new System.EventHandler(this.RedefaultFromTemplateButton_Click);
			this.taxConfigurationTabPage.PerformLayout();
			this.taxConfigurationSplitContainer.Panel1.ResumeLayout(false);
			this.taxConfigurationSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.taxConfigurationSplitContainer)).EndInit();
			this.taxConfigurationSplitContainer.ResumeLayout(false);
			this.taxConfigurationSplitContainer.PerformLayout();
			this.accOrgTaxRateGroup.ResumeLayout(false);
			this.accOrgTaxRateGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxRateGrid)).EndInit();
			this.accOrgTaxRateGrid.ResumeLayout(false);
			this.accOrgTaxRateGrid.PerformLayout();
			this.accOrgTaxConfigurationGroup.ResumeLayout(false);
			this.accOrgTaxConfigurationGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.accOrgTaxConfigurationGrid)).EndInit();
			this.accOrgTaxConfigurationGrid.ResumeLayout(false);
			this.accOrgTaxConfigurationGrid.PerformLayout();
			this.taxConfigurationMainGroupBox.ResumeLayout(false);
			this.taxConfigurationMainGroupBox.PerformLayout();
			this.taxConfigurationTemplateGuidFindBox.ResumeLayout(true);
			this.taxConfigurationTemplateGuidFindBox.PerformLayout();
			this.taxConfigurationTabPage.ResumeLayout(true);

			InitTaxConfigurationTabPage();
		}

		#endregion
	}
}
