using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsigneeDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IMCartageDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RelatedPartiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CurrencyUpliftGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.cfxUpliftEditLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SCRConsigneeDefaultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACRConsigneeDefaultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMBalanceInvoicePackageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_IMOwnsProductsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.exportBillAgentChargesDirectCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_IMSendSeaImportDocsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMSendImportDocsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwnersRefInitialNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OM_IMDocumentAddressPreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductImportAuditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_ConsigneeAuthorityToLeaveDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutoImporterJobRefCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_IMPaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMDefaultINCOTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_IMAirDepotFreeDaysBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_IMCopySeaBillsBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OM_IMImporterCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMOriginalSeaBillsBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ASNDefaultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImporterOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ASNDefaultFieldTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OM_IMLastOrderReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrderManagementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_IMDisallowOrdersCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetentionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerPenaltiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CTOStoragesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerFillingRatioTolerancesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PaymentDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AUPaymentDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomsDefaultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsDefaultsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.OB_CusPaidByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMEnablePromptToCreateProductsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_IMEnableVisibilityEventDeliveryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IMCartageDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedPartiesGrid)).BeginInit();
			this.RelatedPartiesGrid.SuspendLayout();
			this.CurrencyUpliftGroupBox.SuspendLayout();
			this.ConfigurationGroupBox.SuspendLayout();
			this.SCRConsigneeDefaultDropEdit.SuspendLayout();
			this.ACRConsigneeDefaultDropEdit.SuspendLayout();
			this.OM_IMSendSeaImportDocsDropEdit.SuspendLayout();
			this.OM_IMSendImportDocsDropEdit.SuspendLayout();
			this.OM_IMDocumentAddressPreferenceDropEdit.SuspendLayout();
			this.ProductImportAuditDropEdit.SuspendLayout();
			this.OM_ConsigneeAuthorityToLeaveDropEdit.SuspendLayout();
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.SuspendLayout();
			this.OM_IMPaymentMethodDropEdit.SuspendLayout();
			this.OM_IMDefaultINCOTermDropEdit.SuspendLayout();
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.SuspendLayout();
			this.OM_IMImporterCategoryBoundDropEdit.SuspendLayout();
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.SuspendLayout();
			this.ASNDefaultGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ASNDefaultFieldTypeGrid)).BeginInit();
			this.ASNDefaultFieldTypeGrid.SuspendLayout();
			this.OrderManagementGroupBox.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.DetentionTabControl.SuspendLayout();
			this.PaymentDetailsTabControl.SuspendLayout();
			this.CustomsDefaultsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.OB_CusPaidByDropEdit.SuspendLayout();
			this.OM_IMEnablePromptToCreateProductsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// IMCartageDetailsGroupBox
			// 
			this.IMCartageDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|deef80cf-8598-426e-8833-48a909e584f0", "Related Parties");
			this.IMCartageDetailsGroupBox.Controls.Add(this.RelatedPartiesGrid);
			this.IMCartageDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IMCartageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IMCartageDetailsGroupBox.Name = "IMCartageDetailsGroupBox";
			this.IMCartageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 110, true);
			this.IMCartageDetailsGroupBox.TabIndex = 0;
			this.IMCartageDetailsGroupBox.TabStop = false;
			// 
			// RelatedPartiesGrid
			// 
			this.RelatedPartiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedPartiesGrid, "ConsigneeRelatedParties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PR_PartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).Lookups.ConsigneePartyTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PartyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PR_OA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PR_FreightTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PR_FreightContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PR_OH_RelatedParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).RelatedPartyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).CompanyLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeRelatedParties)).SyncRoot)).PR_Location)));
			this.RelatedPartiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.BindToList = "Lookups.ConsigneePartyTypeList";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "PR_PartyType";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|30b5e855-0819-4565-9058-412ead63db3b", "Party Type Description", "The type of this related party/organization.");
			zDropEditColumnStyleInfo5.ColumnName = "PartyTypeDescription";
			zDropEditColumnStyleInfo5.IsReadOnly = true;
			zDropEditColumnStyleInfo5.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidDropEditColumnStyleInfo2.ColumnName = "PR_OA";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo13.ColumnName = "PR_FreightTransportMode";
			zDropEditColumnStyleInfo13.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo14.ColumnName = "PR_FreightContainerMode";
			zDropEditColumnStyleInfo14.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "PR_OH_RelatedParty";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|1949ee8e-7dc0-4b71-8ef5-526ab4a84da8", "Related Party Name");
			zTextBoxColumnStyleInfo2.ColumnName = "RelatedPartyName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|50e28bbb-d25e-4ff7-b456-6f1e6410546a", "Company", "Company/System", "Whether this related party applies only to the logged in company or to the entire system.");
			zDropEditColumnStyleInfo15.ColumnName = "CompanyLevel";
			zDropEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("102bcb3e-b7a3-43e7-8f56-0694173a7480", "UNLOCO");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "PR_Location";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RelatedPartiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.RelatedPartiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.RelatedPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.RelatedPartiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.RelatedPartiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedPartiesGrid.GridId = "f1dae12c-b261-4e1b-ac9a-c4e982dfe9ad";
			this.RelatedPartiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedPartiesGrid.LayoutKey = "zGrid1";
			this.RelatedPartiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.RelatedPartiesGrid.Name = "RelatedPartiesGrid";
			this.RelatedPartiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1055, 94, true);
			this.RelatedPartiesGrid.TabIndex = 0;
			// 
			// CurrencyUpliftGroupBox
			// 
			this.CurrencyUpliftGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|6114bd15-288e-405d-b04a-700bd885e31d", "Currency Uplift");
			this.CurrencyUpliftGroupBox.Controls.Add(this.cfxUpliftEditLink);
			this.CurrencyUpliftGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 215, true);
			this.CurrencyUpliftGroupBox.Name = "CurrencyUpliftGroupBox";
			this.CurrencyUpliftGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 66, true);
			this.CurrencyUpliftGroupBox.TabIndex = 2;
			this.CurrencyUpliftGroupBox.TabStop = false;
			// 
			// cfxUpliftEditLink
			// 
			this.cfxUpliftEditLink.AutoSize = true;
			this.cfxUpliftEditLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e3c1808-017b-4338-8ac0-547edc983886", "Click here to navigate to the \'A/R\' tab");
			this.cfxUpliftEditLink.IsFontBold = false;
			this.cfxUpliftEditLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 23, true);
			this.cfxUpliftEditLink.Name = "cfxUpliftEditLink";
			this.cfxUpliftEditLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 13, true);
			this.cfxUpliftEditLink.TabIndex = 1;
			this.cfxUpliftEditLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cfxUpliftEditLink_LinkClicked);
			// 
			// ConfigurationGroupBox
			// 
			this.ConfigurationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|1a4bb858-b1c2-49df-8bd2-710e853528e1", "Importer / Consignee Defaults");
			this.ConfigurationGroupBox.Controls.Add(this.SCRConsigneeDefaultDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.ACRConsigneeDefaultDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OB_CusPaidByDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMBalanceInvoicePackageCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMOwnsProductsCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.exportBillAgentChargesDirectCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMSendSeaImportDocsDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMSendImportDocsDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OwnersRefInitialNumberButton);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMDocumentAddressPreferenceDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.ProductImportAuditDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_ConsigneeAuthorityToLeaveDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.AutoImporterJobRefCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMPaymentMethodDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMDefaultINCOTermDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMSeaDepotFreeDaysBoundCalcEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMAirDepotFreeDaysBoundCalcEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMCopySeaBillsBoundCalcEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMImporterCategoryBoundDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMOriginalSeaBillsBoundCalcEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMEnablePromptToCreateProductsDropEdit);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMAdvanceCargoReportingSelfFilerCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.OM_IMEnableVisibilityEventDeliveryCheckBox);
			this.ConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
			this.ConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 207, true);
			this.ConfigurationGroupBox.TabIndex = 0;
			this.ConfigurationGroupBox.TabStop = false;
			// 
			// OM_IMAdvanceCargoReportingSelfFilerCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMAdvanceCargoReportingSelfFilerCheckBox, "MiscServ.OM_IMAdvanceCargoReportingSelfFiler");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAdvanceCargoReportingSelfFiler)));
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e10c147-f539-47fe-9f69-397bd199b398", "Advance Cargo Reporting Self-Filer");
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 160, true);
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox.Name = "OM_IMAdvanceCargoReportingSelfFilerCheckBox";
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 19, true);
			this.OM_IMAdvanceCargoReportingSelfFilerCheckBox.TabIndex = 26;
			// 
			// OM_IMEnableVisibilityEventDeliveryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMEnableVisibilityEventDeliveryCheckBox, "MiscServ.OM_IsVisibilityEventEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IsVisibilityEventEnabled)));
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.Visible = FreightDataRegistry.Instance.EnableVisibilityEventDelivery.Value;
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9D9AE707-FAE4-4280-9B01-2DAD78033D67", "Enable Visibility Event Delivery");
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 184, true);
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.Name = "OM_IMEnableVisibilityEventDeliveryCheckBox";
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 19, true);
			this.OM_IMEnableVisibilityEventDeliveryCheckBox.TabIndex = 27;
			// 
			// SCRConsigneeDefaultDropEdit
			// 
			this.SCRConsigneeDefaultDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SCRConsigneeDefaultDropEdit, "MiscServ.OM_IMSeaCargoReportDefaultConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMSeaCargoReportDefaultConsignee)));
			this.SCRConsigneeDefaultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 136, true);
			this.SCRConsigneeDefaultDropEdit.Name = "SCRConsigneeDefaultDropEdit";
			this.SCRConsigneeDefaultDropEdit.PreBoundMaxLength = 3;
			this.SCRConsigneeDefaultDropEdit.ShowDescriptionBox = false;
			this.SCRConsigneeDefaultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 15, true);
			this.SCRConsigneeDefaultDropEdit.TabIndex = 25;
			// 
			// ACRConsigneeDefaultDropEdit
			// 
			this.ACRConsigneeDefaultDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACRConsigneeDefaultDropEdit, "MiscServ.OM_IMAirCargoReportDefaultConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAirCargoReportDefaultConsignee)));
			this.ACRConsigneeDefaultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 136, true);
			this.ACRConsigneeDefaultDropEdit.Name = "ACRConsigneeDefaultDropEdit";
			this.ACRConsigneeDefaultDropEdit.PreBoundMaxLength = 3;
			this.ACRConsigneeDefaultDropEdit.ShowDescriptionBox = false;
			this.ACRConsigneeDefaultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 15, true);
			this.ACRConsigneeDefaultDropEdit.TabIndex = 24;
			// 
			// OM_IMBalanceInvoicePackageCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMBalanceInvoicePackageCheckBox, "MiscServ.OM_IMBalanceInvoicePackage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMBalanceInvoicePackage)));
			this.OM_IMBalanceInvoicePackageCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("47411b5f-5036-498c-be7f-d0ed622fff83", "Balance Invoice Package");
			this.OM_IMBalanceInvoicePackageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMBalanceInvoicePackageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 113, true);
			this.OM_IMBalanceInvoicePackageCheckBox.Name = "OM_IMBalanceInvoicePackageCheckBox";
			this.OM_IMBalanceInvoicePackageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 19, true);
			this.OM_IMBalanceInvoicePackageCheckBox.TabIndex = 14;
			// 
			// OM_IMOwnsProductsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMOwnsProductsCheckBox, "MiscServ.OM_IMOwnsProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMOwnsProducts)));
			this.OM_IMOwnsProductsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B44EE758-573E-4205-BD6E-90F385B1B354", "Always Owns Product");
			this.OM_IMOwnsProductsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMOwnsProductsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 65, true);
			this.OM_IMOwnsProductsCheckBox.Name = "OM_IMOwnsProductsCheckBox";
			this.OM_IMOwnsProductsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 19, true);
			this.OM_IMOwnsProductsCheckBox.TabIndex = 13;
			// 
			// exportBillAgentChargesDirectCheckBox
			// 
			this.BindingSource.SetBindingMember(this.exportBillAgentChargesDirectCheckBox, "CompanyData.OB_EXBillAgentChargesDirect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_EXBillAgentChargesDirect)));
			this.exportBillAgentChargesDirectCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.exportBillAgentChargesDirectCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 89, true);
			this.exportBillAgentChargesDirectCheckBox.Name = "exportBillAgentChargesDirectCheckBox";
			this.exportBillAgentChargesDirectCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 19, true);
			this.exportBillAgentChargesDirectCheckBox.TabIndex = 13;
			// 
			// OM_IMSendSeaImportDocsDropEdit
			// 
			this.OM_IMSendSeaImportDocsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMSendSeaImportDocsDropEdit, "MiscServ+OM_IMSendSeaImportDocsTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMSendSeaImportDocsTo)));
			this.OM_IMSendSeaImportDocsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 16, true);
			this.OM_IMSendSeaImportDocsDropEdit.Name = "OM_IMSendSeaImportDocsDropEdit";
			this.OM_IMSendSeaImportDocsDropEdit.PreBoundMaxLength = 3;
			this.OM_IMSendSeaImportDocsDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMSendSeaImportDocsDropEdit.ShowDescriptionBox = false;
			this.OM_IMSendSeaImportDocsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMSendSeaImportDocsDropEdit.TabIndex = 18;
			// 
			// OM_IMSendImportDocsDropEdit
			// 
			this.OM_IMSendImportDocsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMSendImportDocsDropEdit, "MiscServ+OM_IMSendImportDocsTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMSendImportDocsTo)));
			this.OM_IMSendImportDocsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 40, true);
			this.OM_IMSendImportDocsDropEdit.Name = "OM_IMSendImportDocsDropEdit";
			this.OM_IMSendImportDocsDropEdit.PreBoundMaxLength = 3;
			this.OM_IMSendImportDocsDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMSendImportDocsDropEdit.ShowDescriptionBox = false;
			this.OM_IMSendImportDocsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMSendImportDocsDropEdit.TabIndex = 19;
			// 
			// OwnersRefInitialNumberButton
			// 
			this.OwnersRefInitialNumberButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|2487a052-ad99-489a-81ba-33b46208c26e", "Reset");
			this.OwnersRefInitialNumberButton.IsCaptionOverridden = false;
			this.OwnersRefInitialNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(639, 134, true);
			this.OwnersRefInitialNumberButton.Name = "OwnersRefInitialNumberButton";
			this.OwnersRefInitialNumberButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OwnersRefInitialNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OwnersRefInitialNumberButton.TabIndex = 17;
			this.OwnersRefInitialNumberButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OwnersRefInitialNumberButton.ToolTipCaption = null;
			this.OwnersRefInitialNumberButton.Click += new System.EventHandler(this.OwnersRefInitialNumberButton_Click);
			// 
			// OM_IMDocumentAddressPreferenceDropEdit
			// 
			this.OM_IMDocumentAddressPreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMDocumentAddressPreferenceDropEdit, "MiscServ+OM_IMDocumentAddressPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMDocumentAddressPreference)));
			this.OM_IMDocumentAddressPreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 64, true);
			this.OM_IMDocumentAddressPreferenceDropEdit.Name = "OM_IMDocumentAddressPreferenceDropEdit";
			this.OM_IMDocumentAddressPreferenceDropEdit.PreBoundMaxLength = 3;
			this.OM_IMDocumentAddressPreferenceDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMDocumentAddressPreferenceDropEdit.ShowDescriptionBox = false;
			this.OM_IMDocumentAddressPreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMDocumentAddressPreferenceDropEdit.TabIndex = 20;
			// 
			// ProductImportAuditDropEdit
			// 
			this.ProductImportAuditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductImportAuditDropEdit, "MiscServ+OM_IMValidationForUnauditedClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMValidationForUnauditedClassification)));
			this.ProductImportAuditDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a90858df-94ce-4804-9ccb-075989e2a589", "Unaudited Import Product");
			this.ProductImportAuditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 112, true);
			this.ProductImportAuditDropEdit.Name = "ProductImportAuditDropEdit";
			this.ProductImportAuditDropEdit.PreBoundMaxLength = 3;
			this.ProductImportAuditDropEdit.ShouldResizeByMaxLength = true;
			this.ProductImportAuditDropEdit.ShowDescriptionBox = false;
			this.ProductImportAuditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.ProductImportAuditDropEdit.TabIndex = 22;
			// 
			// OM_ConsigneeAuthorityToLeaveDropEdit
			// 
			this.OM_ConsigneeAuthorityToLeaveDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_ConsigneeAuthorityToLeaveDropEdit, "MiscServ.OM_ConsigneeAuthorityToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ConsigneeAuthorityToLeave)));
			this.OM_ConsigneeAuthorityToLeaveDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 88, true);
			this.OM_ConsigneeAuthorityToLeaveDropEdit.Name = "OM_ConsigneeAuthorityToLeaveDropEdit";
			this.OM_ConsigneeAuthorityToLeaveDropEdit.PreBoundMaxLength = 3;
			this.OM_ConsigneeAuthorityToLeaveDropEdit.ShouldResizeByMaxLength = true;
			this.OM_ConsigneeAuthorityToLeaveDropEdit.ShowDescriptionBox = false;
			this.OM_ConsigneeAuthorityToLeaveDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_ConsigneeAuthorityToLeaveDropEdit.TabIndex = 21;
			// 
			// OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit
			// 
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit, "MiscServ+OM_IMAutoPopulateOwnerRefWithOrderNums");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums)));
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 88, true);
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.Name = "OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit";
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.PreBoundMaxLength = 3;
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.ShowDescriptionBox = false;
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.TabIndex = 3;
			// 
			// AutoImporterJobRefCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AutoImporterJobRefCheckBox, "MiscServ+OM_IMAutoImpJobRefered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAutoImpJobRefered)));
			this.AutoImporterJobRefCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoImporterJobRefCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 136, true);
			this.AutoImporterJobRefCheckBox.Name = "AutoImporterJobRefCheckBox";
			this.AutoImporterJobRefCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 19, true);
			this.AutoImporterJobRefCheckBox.TabIndex = 16;
			// 
			// OM_IMPaymentMethodDropEdit
			// 
			this.OM_IMPaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMPaymentMethodDropEdit, "MiscServ.OM_IMPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPaymentMethod)));
			this.OM_IMPaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 136, true);
			this.OM_IMPaymentMethodDropEdit.Name = "OM_IMPaymentMethodDropEdit";
			this.OM_IMPaymentMethodDropEdit.PreBoundMaxLength = 3;
			this.OM_IMPaymentMethodDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMPaymentMethodDropEdit.ShowDescriptionBox = false;
			this.OM_IMPaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMPaymentMethodDropEdit.TabIndex = 5;
			// 
			// OM_IMEnablePromptToCreateProductsDropEdit
			// 
			this.OM_IMEnablePromptToCreateProductsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMEnablePromptToCreateProductsDropEdit, "MiscServ+OM_IMEnablePromptToCreateProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMEnablePromptToCreateProducts)));
			this.OM_IMEnablePromptToCreateProductsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 112, true);
			this.OM_IMEnablePromptToCreateProductsDropEdit.Name = "OM_IMEnablePromptToCreateProductsDropEdit";
			this.OM_IMEnablePromptToCreateProductsDropEdit.PreBoundMaxLength = 3;
			this.OM_IMEnablePromptToCreateProductsDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMEnablePromptToCreateProductsDropEdit.ShowDescriptionBox = false;
			this.OM_IMEnablePromptToCreateProductsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMEnablePromptToCreateProductsDropEdit.TabIndex = 4;
			// 
			// OM_IMDefaultINCOTermDropEdit
			// 
			this.OM_IMDefaultINCOTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMDefaultINCOTermDropEdit, "MiscServ.OM_IMDefaultINCOTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMDefaultINCOTerm)));
			this.OM_IMDefaultINCOTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 40, true);
			this.OM_IMDefaultINCOTermDropEdit.Name = "OM_IMDefaultINCOTermDropEdit";
			this.OM_IMDefaultINCOTermDropEdit.PreBoundMaxLength = 3;
			this.OM_IMDefaultINCOTermDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMDefaultINCOTermDropEdit.ShowDescriptionBox = false;
			this.OM_IMDefaultINCOTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMDefaultINCOTermDropEdit.TabIndex = 1;
			// 
			// OM_IMImporterRequiresOrderNumbersOnDocsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox, "MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs)));
			this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 18, true);
			this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.Name = "OM_IMImporterRequiresOrderNumbersOnDocsCheckBox";
			this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 19, true);
			this.OM_IMImporterRequiresOrderNumbersOnDocsCheckBox.TabIndex = 11;
			// 
			// OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox
			// 
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox, "MiscServ.OM_RS_NKIMDefaultServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_RS_NKIMDefaultServiceLevel)));
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 16, true);
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.Name = "OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox";
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.ParentType = null;
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.PopupCaption = "Select Service Level";
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.PreBoundMaxLength = 3;
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.ShowDescriptionBox = false;
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.TabIndex = 6;
			// 
			// OM_IMSeaDepotFreeDaysBoundCalcEdit
			// 
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OM_IMSeaDepotFreeDaysBoundCalcEdit, "MiscServ.OM_IMSeaDepotFreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMSeaDepotFreeDays)));
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.CaptionResourceString = null;
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.DecimalPlaces = 0;
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.Decimals = 0;
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 112, true);
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.Name = "OM_IMSeaDepotFreeDaysBoundCalcEdit";
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.TabIndex = 10;
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.Text = "0";
			this.OM_IMSeaDepotFreeDaysBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_IMAirDepotFreeDaysBoundCalcEdit
			// 
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OM_IMAirDepotFreeDaysBoundCalcEdit, "MiscServ.OM_IMAirDepotFreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAirDepotFreeDays)));
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.CaptionResourceString = null;
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.DecimalPlaces = 0;
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.Decimals = 0;
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 88, true);
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.Name = "OM_IMAirDepotFreeDaysBoundCalcEdit";
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.TabIndex = 9;
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.Text = "0";
			this.OM_IMAirDepotFreeDaysBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_IMCopySeaBillsBoundCalcEdit
			// 
			this.OM_IMCopySeaBillsBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OM_IMCopySeaBillsBoundCalcEdit, "MiscServ.OM_IMCopySeaBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMCopySeaBills)));
			this.OM_IMCopySeaBillsBoundCalcEdit.CaptionResourceString = null;
			this.OM_IMCopySeaBillsBoundCalcEdit.DecimalPlaces = 2;
			this.OM_IMCopySeaBillsBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 64, true);
			this.OM_IMCopySeaBillsBoundCalcEdit.Name = "OM_IMCopySeaBillsBoundCalcEdit";
			this.OM_IMCopySeaBillsBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMCopySeaBillsBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.OM_IMCopySeaBillsBoundCalcEdit.TabIndex = 8;
			this.OM_IMCopySeaBillsBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OM_IMImporterCategoryBoundDropEdit
			// 
			this.OM_IMImporterCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMImporterCategoryBoundDropEdit, "MiscServ.OM_IMImporterCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMImporterCategory)));
			this.OM_IMImporterCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 16, true);
			this.OM_IMImporterCategoryBoundDropEdit.Name = "OM_IMImporterCategoryBoundDropEdit";
			this.OM_IMImporterCategoryBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_IMImporterCategoryBoundDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMImporterCategoryBoundDropEdit.ShowDescriptionBox = false;
			this.OM_IMImporterCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMImporterCategoryBoundDropEdit.TabIndex = 0;
			// 
			// OM_IMJobRequireOrderTrackLinkBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit, "MiscServ.OM_IMJobRequireOrderTrackLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMJobRequireOrderTrackLink)));
			this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 41, true);
			this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.Name = "OM_IMJobRequireOrderTrackLinkBoundCheckEdit";
			this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 19, true);
			this.OM_IMJobRequireOrderTrackLinkBoundCheckEdit.TabIndex = 12;
			// 
			// OM_IMMergeCustomsInvoiceLinesByBoundDropEdit
			// 
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit, "MiscServ.OM_IMMergeCustomsInvoiceLinesBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMMergeCustomsInvoiceLinesBy)));
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 64, true);
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.Name = "OM_IMMergeCustomsInvoiceLinesByBoundDropEdit";
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.ShouldResizeByMaxLength = true;
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.ShowDescriptionBox = false;
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.TabIndex = 2;
			// 
			// OM_IMOriginalSeaBillsBoundCalcEdit
			// 
			this.OM_IMOriginalSeaBillsBoundCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OM_IMOriginalSeaBillsBoundCalcEdit, "MiscServ.OM_IMOriginalSeaBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMOriginalSeaBills)));
			this.OM_IMOriginalSeaBillsBoundCalcEdit.CaptionResourceString = null;
			this.OM_IMOriginalSeaBillsBoundCalcEdit.DecimalPlaces = 2;
			this.OM_IMOriginalSeaBillsBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 40, true);
			this.OM_IMOriginalSeaBillsBoundCalcEdit.Name = "OM_IMOriginalSeaBillsBoundCalcEdit";
			this.OM_IMOriginalSeaBillsBoundCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMOriginalSeaBillsBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.OM_IMOriginalSeaBillsBoundCalcEdit.TabIndex = 7;
			this.OM_IMOriginalSeaBillsBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ASNDefaultGroupBox
			// 
			this.ASNDefaultGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|6a8fa588-97d7-4909-b1e5-adbafa8255f0", "ASN Refresh Defaults");
			this.ASNDefaultGroupBox.Controls.Add(this.ImporterOverrideCheckBox);
			this.ASNDefaultGroupBox.Controls.Add(this.ASNDefaultFieldTypeGrid);
			this.ASNDefaultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(890, 3, true);
			this.ASNDefaultGroupBox.Name = "ASNDefaultGroupBox";
			this.ASNDefaultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 230, true);
			this.ASNDefaultGroupBox.TabIndex = 1;
			this.ASNDefaultGroupBox.TabStop = false;
			// 
			// ImporterOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterOverrideCheckBox, "CompanyData.ImporterOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.ImporterOverride)));
			this.ImporterOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImporterOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 16, true);
			this.ImporterOverrideCheckBox.Name = "ImporterOverrideCheckBox";
			this.ImporterOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 19, true);
			this.ImporterOverrideCheckBox.TabIndex = 0;
			// 
			// ASNDefaultFieldTypeGrid
			// 
			this.ASNDefaultFieldTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ASNDefaultFieldTypeGrid, "CompanyData.DefaultOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.DefaultOptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.IMProductValueDefaultOption)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.DefaultOptions)).SyncRoot)).FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IMProductValueDefaultOption)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.DefaultOptions)).SyncRoot)).FieldTypes)));
			this.ASNDefaultFieldTypeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.BindToList = "FieldTypes";
			zDropEditColumnStyleInfo6.ColumnName = "FieldType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ASNDefaultFieldTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ASNDefaultFieldTypeGrid.GridId = "2ad2bee6-8222-47d0-a455-4b008b13d279";
			this.ASNDefaultFieldTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ASNDefaultFieldTypeGrid.LayoutKey = "zGrid2";
			this.ASNDefaultFieldTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 40, true);
			this.ASNDefaultFieldTypeGrid.Name = "ASNDefaultFieldTypeGrid";
			this.ASNDefaultFieldTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 183, true);
			this.ASNDefaultFieldTypeGrid.TabIndex = 1;
			// 
			// OM_IMLastOrderReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMLastOrderReferenceTextBox, "MiscServ+OM_IMLastOrderReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMLastOrderReference)));
			this.OM_IMLastOrderReferenceTextBox.CaptionResourceString = null;
			this.OM_IMLastOrderReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 39, true);
			this.OM_IMLastOrderReferenceTextBox.Name = "OM_IMLastOrderReferenceTextBox";
			this.OM_IMLastOrderReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.OM_IMLastOrderReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 17, true);
			this.OM_IMLastOrderReferenceTextBox.TabIndex = 3;
			// 
			// OM_IMAllowAttachedOrderXMLUpdateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMAllowAttachedOrderXMLUpdateCheckBox, "MiscServ+OM_IMAllowAttachedOrderXMLUpdate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMAllowAttachedOrderXMLUpdate)));
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 19, true);
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox.Name = "OM_IMAllowAttachedOrderXMLUpdateCheckBox";
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 17, true);
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox.TabIndex = 2;
			this.OM_IMAllowAttachedOrderXMLUpdateCheckBox.UseVisualStyleBackColor = true;
			// 
			// OM_IMDefaultToNewOrdersToNextOrderNumCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox, "MiscServ+OM_IMDefaultToNewOrdersToNextOrderNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMDefaultToNewOrdersToNextOrderNum)));
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 41, true);
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.Name = "OM_IMDefaultToNewOrdersToNextOrderNumCheckBox";
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 19, true);
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.TabIndex = 1;
			this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrderManagementGroupBox
			// 
			this.OrderManagementGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|8dbacaa7-0e81-4ae1-8650-77ca1fd5779e", "Order Manager Settings");
			this.OrderManagementGroupBox.Controls.Add(this.OM_IMDisallowOrdersCheckBox);
			this.OrderManagementGroupBox.Controls.Add(this.OM_IMDefaultToNewOrdersToNextOrderNumCheckBox);
			this.OrderManagementGroupBox.Controls.Add(this.OM_IMLastOrderReferenceTextBox);
			this.OrderManagementGroupBox.Controls.Add(this.OM_IMAllowAttachedOrderXMLUpdateCheckBox);
			this.OrderManagementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 215, true);
			this.OrderManagementGroupBox.Name = "OrderManagementGroupBox";
			this.OrderManagementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 66, true);
			this.OrderManagementGroupBox.TabIndex = 3;
			this.OrderManagementGroupBox.TabStop = false;
			// 
			// OM_IMDisallowOrdersCheckBox
			// 
			this.OM_IMDisallowOrdersCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_IMDisallowOrdersCheckBox, "MiscServ.OM_IMDisallowOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMDisallowOrders)));
			this.OM_IMDisallowOrdersCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_IMDisallowOrdersCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.OM_IMDisallowOrdersCheckBox.Name = "OM_IMDisallowOrdersCheckBox";
			this.OM_IMDisallowOrdersCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
			this.OM_IMDisallowOrdersCheckBox.TabIndex = 0;
			this.OM_IMDisallowOrdersCheckBox.UseVisualStyleBackColor = true;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.OrderManagementGroupBox);
			this.zPanel1.Controls.Add(this.CurrencyUpliftGroupBox);
			this.zPanel1.Controls.Add(this.ConfigurationGroupBox);
			this.zPanel1.Controls.Add(this.ASNDefaultGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 288, true);
			this.zPanel1.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.DetentionTabControl);
			this.BottomPanel.Controls.Add(this.PaymentDetailsTabControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 179, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// DetentionTabControl
			// 
			this.DetentionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetentionTabControl.Controls.Add(this.ContainerPenaltiesTabPage);
			this.DetentionTabControl.Controls.Add(this.CTOStoragesTabPage);
			this.DetentionTabControl.Controls.Add(this.ContainerFillingRatioTolerancesTabPage);
			this.DetentionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetentionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 0, true);
			this.DetentionTabControl.Name = "DetentionTabControl";
			this.DetentionTabControl.SelectedIndex = 0;
			this.DetentionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 179, true);
			this.DetentionTabControl.TabIndex = 8;
			this.DetentionTabControl.TabStop = false;
			// 
			// ContainerDetentionTabPage
			// 
			this.ContainerPenaltiesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|dc8c1b50-7abb-40f7-9f45-9eada8d3c5dc", "Container Penalties");
			this.ContainerPenaltiesTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerPenaltiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.ContainerPenaltiesTabPage.Name = "ContainerDetentionTabPage";
			this.ContainerPenaltiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 206, true);
			this.ContainerPenaltiesTabPage.TabIndex = 8;
			this.ContainerPenaltiesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ContainerPenaltiesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_PenaltyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PenaltyDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_OriginPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_FreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeContainerPenalties)).SyncRoot)).PD_FreeDayType)));

			// 
			// CTOStoragesTabPage
			// 
			this.CTOStoragesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|4b3c45e7-2cbe-434e-ae13-21680440506b", "CTO Storage");
			this.CTOStoragesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.CTOStoragesTabPage.Name = "CTOStoragesTabPage";
			this.CTOStoragesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 206, true);
			this.CTOStoragesTabPage.TabIndex = 9;
			this.CTOStoragesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CTOStoragesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_OriginPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_OH_CTO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_CreditorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_FreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsigneeCTOStorages)).SyncRoot)).PD_FreeDayType)));
			//
			// ContainerFillingRatioTolerancesTabPage
			//
			this.ContainerFillingRatioTolerancesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|EDDF61C2-AACC-4A14-B6E5-C24B73577D1A", "Container Filling Ratio Tolerances");
			this.ContainerFillingRatioTolerancesTabPage.Name = "ContainerFillingRatioTolerancesTabPage";
			this.ContainerFillingRatioTolerancesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 206, true);
			this.ContainerFillingRatioTolerancesTabPage.TabIndex = 10;
			this.ContainerFillingRatioTolerancesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ContainerFillingRatioTolerancesTabPage_InitializeTab));
			// 
			// PaymentDetailsTabControl
			// 
			this.PaymentDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PaymentDetailsTabControl.Controls.Add(this.AUPaymentDetailsTabPage);
			this.PaymentDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.PaymentDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PaymentDetailsTabControl.Name = "PaymentDetailsTabControl";
			this.PaymentDetailsTabControl.SelectedIndex = 0;
			this.PaymentDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 179, true);
			this.PaymentDetailsTabControl.TabIndex = 0;
			// 
			// AUPaymentDetailsTabPage
			// 
			this.AUPaymentDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeDetailsUserControl|7e959820-7566-4c07-8412-b18ddebd7e18", "AU EFT");
			this.AUPaymentDetailsTabPage.IsAccessible = false;
			this.AUPaymentDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.AUPaymentDetailsTabPage.Name = "AUPaymentDetailsTabPage";
			this.AUPaymentDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AUPaymentDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 158, true);
			this.AUPaymentDetailsTabPage.TabIndex = 0;
			this.AUPaymentDetailsTabPage.TabVisible = false;
			this.AUPaymentDetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AUPaymentDetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMShowDutyOnWarehouseEntries)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMEftQuarantineFromImport)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMEFTBankAccount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMEFTBankBSB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMMinEFTAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMMaxEFTAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMEftCustomsFromImport)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMEftHoldUntilPayAuthorised)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMIsGSTDeferred)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AUIsDutyDeferred)));
			// 
			// CustomsDefaultsGroupBox
			// 
			this.CustomsDefaultsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CustomsDefaultsGroupBox|5E882EEE-66E5-4523-81C6-2D827CF980A5", "Customs Defaults");
			this.CustomsDefaultsGroupBox.Controls.Add(this.CustomsDefaultsTabControl);
			this.CustomsDefaultsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.CustomsDefaultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsDefaultsGroupBox.Name = "CustomsDefaultsGroupBox";
			this.CustomsDefaultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 243, true);
			this.CustomsDefaultsGroupBox.TabIndex = 0;
			this.CustomsDefaultsGroupBox.TabStop = false;
			// 
			// CustomsDefaultsTabControl
			// 
			this.CustomsDefaultsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CustomsDefaultsTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.CustomsDefaultsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CustomsDefaultsTabControl.Name = "CustomsDefaultsTabControl";
			this.CustomsDefaultsTabControl.SelectedIndex = 0;
			this.CustomsDefaultsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 227, true);
			this.CustomsDefaultsTabControl.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 528, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.IMCartageDetailsGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 582, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(109);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(415);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(110);
			this.splitContainer1.TabIndex = 3;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer2.IsSplitterFixed = true;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 415, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.zPanel1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.BottomPanel);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 470, true);
			this.splitContainer2.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(170);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(286);
			this.splitContainer2.TabIndex = 0;
			// 
			// CusPaidByDropEdit
			// 
			this.OB_CusPaidByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OB_CusPaidByDropEdit, "CompanyData.OB_CusPaidBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_CusPaidBy)));
			this.OB_CusPaidByDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4b1cae14-68ba-4107-8ab5-c7cce1b4feb7", "Paid By");
			this.OB_CusPaidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(819, 136, true);
			this.OB_CusPaidByDropEdit.Name = "CusPaidByDropEdit";
			this.OB_CusPaidByDropEdit.PreBoundMaxLength = 3;
			this.OB_CusPaidByDropEdit.ShouldResizeByMaxLength = true;
			this.OB_CusPaidByDropEdit.ShowDescriptionBox = false;
			this.OB_CusPaidByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.OB_CusPaidByDropEdit.TabIndex = 23;
			// 
			// ConsigneeDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ConsigneeDetailsUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 582, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IMCartageDetailsGroupBox.ResumeLayout(false);
			this.IMCartageDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedPartiesGrid)).EndInit();
			this.RelatedPartiesGrid.ResumeLayout(false);
			this.RelatedPartiesGrid.PerformLayout();
			this.CurrencyUpliftGroupBox.ResumeLayout(false);
			this.CurrencyUpliftGroupBox.PerformLayout();
			this.ConfigurationGroupBox.ResumeLayout(false);
			this.ConfigurationGroupBox.PerformLayout();
			this.SCRConsigneeDefaultDropEdit.ResumeLayout(true);
			this.SCRConsigneeDefaultDropEdit.PerformLayout();
			this.ACRConsigneeDefaultDropEdit.ResumeLayout(true);
			this.ACRConsigneeDefaultDropEdit.PerformLayout();
			this.OM_IMSendSeaImportDocsDropEdit.ResumeLayout(true);
			this.OM_IMSendSeaImportDocsDropEdit.PerformLayout();
			this.OM_IMSendImportDocsDropEdit.ResumeLayout(true);
			this.OM_IMSendImportDocsDropEdit.PerformLayout();
			this.OM_IMDocumentAddressPreferenceDropEdit.ResumeLayout(true);
			this.OM_IMDocumentAddressPreferenceDropEdit.PerformLayout();
			this.ProductImportAuditDropEdit.ResumeLayout(true);
			this.ProductImportAuditDropEdit.PerformLayout();
			this.OM_ConsigneeAuthorityToLeaveDropEdit.ResumeLayout(true);
			this.OM_ConsigneeAuthorityToLeaveDropEdit.PerformLayout();
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.ResumeLayout(true);
			this.OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit.PerformLayout();
			this.OM_IMEnablePromptToCreateProductsDropEdit.ResumeLayout(true);
			this.OM_IMEnablePromptToCreateProductsDropEdit.PerformLayout();
			this.OM_IMPaymentMethodDropEdit.ResumeLayout(true);
			this.OM_IMPaymentMethodDropEdit.PerformLayout();
			this.OM_IMDefaultINCOTermDropEdit.ResumeLayout(true);
			this.OM_IMDefaultINCOTermDropEdit.PerformLayout();
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.ResumeLayout(true);
			this.OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox.PerformLayout();
			this.OM_IMImporterCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_IMImporterCategoryBoundDropEdit.PerformLayout();
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.ResumeLayout(true);
			this.OM_IMMergeCustomsInvoiceLinesByBoundDropEdit.PerformLayout();
			this.ASNDefaultGroupBox.ResumeLayout(false);
			this.ASNDefaultGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ASNDefaultFieldTypeGrid)).EndInit();
			this.ASNDefaultFieldTypeGrid.ResumeLayout(false);
			this.ASNDefaultFieldTypeGrid.PerformLayout();
			this.OrderManagementGroupBox.ResumeLayout(false);
			this.OrderManagementGroupBox.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.DetentionTabControl.ResumeLayout(false);
			this.DetentionTabControl.PerformLayout();
			this.PaymentDetailsTabControl.ResumeLayout(false);
			this.PaymentDetailsTabControl.PerformLayout();
			this.CustomsDefaultsGroupBox.ResumeLayout(false);
			this.CustomsDefaultsGroupBox.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.OB_CusPaidByDropEdit.ResumeLayout(true);
			this.OB_CusPaidByDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZGroupBox IMCartageDetailsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox CurrencyUpliftGroupBox;
		protected ZGroupBox ConfigurationGroupBox;
		protected ZGroupBox ASNDefaultGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_IMDefaultINCOTermDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OM_IMImporterRequiresOrderNumbersOnDocsCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox OM_RS_NKIMDefaultServiceLevelBoundCodeFindBox;
		protected Enterprise.ZArchitecture.ZCalcEdit OM_IMSeaDepotFreeDaysBoundCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit OM_IMAirDepotFreeDaysBoundCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit OM_IMCopySeaBillsBoundCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_IMImporterCategoryBoundDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OM_IMJobRequireOrderTrackLinkBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_IMMergeCustomsInvoiceLinesByBoundDropEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit OM_IMOriginalSeaBillsBoundCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_IMPaymentMethodDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZButton OwnersRefInitialNumberButton;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox AutoImporterJobRefCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OB_ARUseSystemDefaultUpliftsBoundCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OB_ARUseSystemDefaultUpliftsMinimumsCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ProductImportAuditDropEdit;
		protected ZDropEdit OM_IMAutoPopulateOwnerRefWithOrderNumsDropEdit;
		protected ZDropEdit OM_IMDocumentAddressPreferenceDropEdit;
		protected ZCheckBox OM_IMDefaultToNewOrdersToNextOrderNumCheckBox;
		protected ZCheckBox OM_IMAllowAttachedOrderXMLUpdateCheckBox;
		protected ZTextBox OM_IMLastOrderReferenceTextBox;
		protected ZGrid RelatedPartiesGrid;
		private ZGroupBox OrderManagementGroupBox;
		private ZPanel zPanel1;
		private ZPanel BottomPanel;
		private ZTabControl DetentionTabControl;
		private ZTabPage ContainerPenaltiesTabPage;
		private ZTabPage ContainerFillingRatioTolerancesTabPage;
		private ZTabPage CTOStoragesTabPage;
		private ZGroupBox CustomsDefaultsGroupBox;
		protected ZTabControl CustomsDefaultsTabControl;
		protected ZGrid ASNDefaultFieldTypeGrid;
		protected ZTabControl PaymentDetailsTabControl;
		protected ZTabPage AUPaymentDetailsTabPage;
		protected ZCheckBox exportBillAgentChargesDirectCheckBox;
		protected ZDropEdit OM_ConsigneeAuthorityToLeaveDropEdit;
		protected ZCheckBox OM_IMOwnsProductsCheckBox;
		protected ZCheckBox OM_IMDisallowOrdersCheckBox;
		private System.ComponentModel.IContainer components;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZLinkLabel cfxUpliftEditLink;
		protected ZCheckBox OM_IMBalanceInvoicePackageCheckBox;
		protected ZCheckBox ImporterOverrideCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_IMSendImportDocsDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit OM_IMSendSeaImportDocsDropEdit;
		protected ZDropEdit OB_CusPaidByDropEdit;
		protected ZDropEdit SCRConsigneeDefaultDropEdit;
		protected ZDropEdit ACRConsigneeDefaultDropEdit;
		protected ZCalcEdit ContainerMinFillingRatioTolerancesCalcEdit;
		protected ZCalcEdit ContainerMaxFillingRatioTolerancesCalcEdit;
		protected ZGrid ContainerDetentionGrid;
		protected ZGrid CTOStoragesGrid;
		protected ZCheckBox OM_IMShowDutyOnWarehouseEntriesCheckBox;
		protected ZCheckBox OM_IMEftQuarantineFromImportCheckBox;
		protected ZTextBox OM_IMEFTBankAccountBoundTextBox;
		protected ZTextBox OM_IMEFTBankBSBBoundTextBox;
		protected ZCalcEdit OM_IMMinEFTAmountBoundCalcEdit;
		protected ZCalcEdit OM_IMMaxEFTAmountBoundCalcEdit;
		protected ZCheckBox OM_IMEftCustomsFromImportBoundCheckEdit;
		protected ZCheckBox OM_IMEftHoldUntilPayAuthorisedBoundCheckEdit;
		protected ZCheckBox OM_IMIsGSTDeferredBoundCheckBox;
		protected ZCheckBox IsDutyDeferredCheckBox;
		protected ZDropEdit OM_IMEnablePromptToCreateProductsDropEdit;
		protected ZCheckBox OM_IMAdvanceCargoReportingSelfFilerCheckBox;
		protected ZCheckBox OM_IMEnableVisibilityEventDeliveryCheckBox;
	}
}
