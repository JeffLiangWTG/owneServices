using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterData.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsignorDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo carrierFindBoxColumnStyleInfo = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo4 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo penaltyTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo penaltyDescTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo detentionFreeDayExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo detentionDurationExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo storagesFreeDayExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo storagesDurationExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo();
			this.CurrencyUpliftGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.cfxUpliftEditLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartsBothImportAndExportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ApprovedToPrintCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CartageAndBrokerageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.PreAllocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreAllocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerDetentionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerPenaltiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerDetentionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CTOStorageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignorCTOStoragesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DefaultsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ExporterTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OM_EXOwnsProductsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_EXEnableVisibilityEventDeliveryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_ConsignorAuthorityToLeaveDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importBillAgentChargesDirectCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.OM_IMLastOrderReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequireLinkedOrderTrackingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.InvPriceFromLastCostDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AWBAddrDefzDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_IMPaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductExportAuditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OM_EXDefaultIncoTermBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_EXExporterCategoryBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceLevelsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ServiceLevelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OverrideServiceLevelsSettingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ServiceLevelsInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExportDocumentationDefaultsTabpage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExportsBankNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportsBankAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportsSwiftCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MethodOfPaymentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1Panel = new CargoWise.Windows.UI.KPanel();
			this.CurrencyUpliftPanel = new CargoWise.Windows.UI.KPanel();
			this.PreAllocationPanel = new CargoWise.Windows.UI.KPanel();
			this.ContainerDetentionPanel = new CargoWise.Windows.UI.KPanel();
			this.MergeCustomsInvoiceLinesByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrencyUpliftGroupBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.CartageAndBrokerageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.PreAllocationGroupBox.SuspendLayout();
			this.ContainerDetentionTabControl.SuspendLayout();
			this.ContainerPenaltiesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerDetentionGrid)).BeginInit();
			this.ContainerDetentionGrid.SuspendLayout();
			this.CTOStorageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsignorCTOStoragesGrid)).BeginInit();
			this.ConsignorCTOStoragesGrid.SuspendLayout();
			this.DefaultsTabControl.SuspendLayout();
			this.ExporterTabPage.SuspendLayout();
			this.OM_ConsignorAuthorityToLeaveDropEdit.SuspendLayout();
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.SuspendLayout();
			this.InvPriceFromLastCostDropEdit.SuspendLayout();
			this.AWBAddrDefzDropEdit.SuspendLayout();
			this.OM_IMPaymentMethodDropEdit.SuspendLayout();
			this.ProductExportAuditDropEdit.SuspendLayout();
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.SuspendLayout();
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.SuspendLayout();
			this.OM_EXDefaultIncoTermBoundDropEdit.SuspendLayout();
			this.OM_EXExporterCategoryBoundDropEdit.SuspendLayout();
			this.ServiceLevelsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceLevelsGrid)).BeginInit();
			this.ServiceLevelsGrid.SuspendLayout();
			this.ExportDocumentationDefaultsTabpage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.zGroupBox1Panel.SuspendLayout();
			this.CurrencyUpliftPanel.SuspendLayout();
			this.PreAllocationPanel.SuspendLayout();
			this.ContainerDetentionPanel.SuspendLayout();
			this.MergeCustomsInvoiceLinesByDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CurrencyUpliftGroupBox
			// 
			this.CurrencyUpliftGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|c4988b5c-38c1-4f54-aed6-3aea4abadae8", "Currency Uplift");
			this.CurrencyUpliftGroupBox.Controls.Add(this.cfxUpliftEditLink);
			this.CurrencyUpliftGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 96, true);
			this.CurrencyUpliftGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrencyUpliftGroupBox.Name = "CurrencyUpliftGroupBox";
			this.CurrencyUpliftGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 73, true);
			this.CurrencyUpliftGroupBox.TabIndex = 10;
			this.CurrencyUpliftGroupBox.TabStop = false;
			// 
			// cfxUpliftEditLink
			// 
			this.cfxUpliftEditLink.AutoSize = true;
			this.cfxUpliftEditLink.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e3c1808-017b-4338-8ac0-547edc983886", "Click here to navigate to the \'A/R\' tab");
			this.cfxUpliftEditLink.IsFontBold = false;
			this.cfxUpliftEditLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 23, true);
			this.cfxUpliftEditLink.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cfxUpliftEditLink.Name = "cfxUpliftEditLink";
			this.cfxUpliftEditLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 13, true);
			this.cfxUpliftEditLink.TabIndex = 0;
			this.cfxUpliftEditLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cfxUpliftEditLink_LinkClicked);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|3584791d-aca5-4b0e-9337-1a9f1baad5f9", "Other Details");
			this.zGroupBox1.Controls.Add(this.PartsBothImportAndExportCheckBox);
			this.zGroupBox1.Controls.Add(this.ApprovedToPrintCheckBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 87, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// PartsBothImportAndExportCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PartsBothImportAndExportCheckBox, "CountryData+OV_MakePartsBothImportAndExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CountryData.OV_MakePartsBothImportAndExport)));
			this.PartsBothImportAndExportCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PartsBothImportAndExportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PartsBothImportAndExportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 48, true);
			this.PartsBothImportAndExportCheckBox.Name = "PartsBothImportAndExportCheckBox";
			this.PartsBothImportAndExportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 24, true);
			this.PartsBothImportAndExportCheckBox.TabIndex = 20;
			// 
			// ApprovedToPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovedToPrintCheckBox, "MiscServ.OM_EXAllowedToPrintOriginalBL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXAllowedToPrintOriginalBL)));
			this.ApprovedToPrintCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ApprovedToPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApprovedToPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 22, true);
			this.ApprovedToPrintCheckBox.Name = "ApprovedToPrintCheckBox";
			this.ApprovedToPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 24, true);
			this.ApprovedToPrintCheckBox.TabIndex = 19;
			// 
			// CartageAndBrokerageGroupBox
			// 
			this.CartageAndBrokerageGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|ed56e3de-3cdf-4edf-b7a5-c7619ef64d76", "Related Parties");
			this.CartageAndBrokerageGroupBox.Controls.Add(this.zGrid1);
			this.CartageAndBrokerageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageAndBrokerageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CartageAndBrokerageGroupBox.Name = "CartageAndBrokerageGroupBox";
			this.CartageAndBrokerageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 121, true);
			this.CartageAndBrokerageGroupBox.TabIndex = 1;
			this.CartageAndBrokerageGroupBox.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "ConsignorRelatedParties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PR_PartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).Lookups.ConsignorPartyTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PartyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PR_OA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PR_FreightTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PR_FreightContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PR_OH_RelatedParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).RelatedPartyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).CompanyLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorRelatedParties)).SyncRoot)).PR_Location)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ConsignorPartyTypeList";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "PR_PartyType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|3ae7b29f-8ebf-4c4e-a803-1fab3684ceb3", "Party Type Description");
			zDropEditColumnStyleInfo2.ColumnName = "PartyTypeDescription";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidDropEditColumnStyleInfo1.ColumnName = "PR_OA";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "PR_FreightTransportMode";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.ColumnName = "PR_FreightContainerMode";
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "PR_OH_RelatedParty";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|0b012cee-22d0-4d92-ae4c-5d4c355a80b1", "Related Party Name");
			zTextBoxColumnStyleInfo1.ColumnName = "RelatedPartyName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|7e07f353-ff08-43b5-a251-8eacd0d5106a", "Company", "Company / System Level", "");
			zDropEditColumnStyleInfo5.ColumnName = "CompanyLevel";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("08e67f1d-2035-4e6d-9cf6-e8d607e767e2", "UNLOCO");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PR_Location";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "e40e9d84-7a79-41e4-9dcb-b98fe4050ed8";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 102, true);
			this.zGrid1.TabIndex = 0;
			// 
			// PreAllocationGroupBox
			// 
			this.PreAllocationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|21cb71a7-9546-4699-b402-7bf12f6766d5", "House Bill Pre-Allocation");
			this.PreAllocationGroupBox.Controls.Add(this.PreAllocationTextBox);
			this.PreAllocationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreAllocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 178, true);
			this.PreAllocationGroupBox.Name = "PreAllocationGroupBox";
			this.PreAllocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 41, true);
			this.PreAllocationGroupBox.TabIndex = 2;
			this.PreAllocationGroupBox.TabStop = false;
			// 
			// PreAllocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreAllocationTextBox, "MiscServ+OM_EXPreAllocPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXPreAllocPrefix)));
			this.PreAllocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 14, true);
			this.PreAllocationTextBox.Name = "PreAllocationTextBox";
			this.PreAllocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.PreAllocationTextBox.TabIndex = 3;
			this.PreAllocationTextBox.WordWrap = false;
			// 
			// ContainerDetentionTabControl
			// 
			this.ContainerDetentionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ContainerDetentionTabControl.Controls.Add(this.ContainerPenaltiesTabPage);
			this.ContainerDetentionTabControl.Controls.Add(this.CTOStorageTabPage);
			this.ContainerDetentionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerDetentionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerDetentionTabControl.Name = "ContainerDetentionTabControl";
			this.ContainerDetentionTabControl.SelectedIndex = 0;
			this.ContainerDetentionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 376, true);
			this.ContainerDetentionTabControl.TabIndex = 18;
			// 
			// ContainerDetentionTabPage
			// 
			this.ContainerPenaltiesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|e9824e3d-ba92-4dfd-970b-eb072afe7b11", "Container Penalties");
			this.ContainerPenaltiesTabPage.Controls.Add(this.ContainerDetentionGrid);
			this.ContainerPenaltiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerPenaltiesTabPage.Name = "ContainerDetentionTabPage";
			this.ContainerPenaltiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 349, true);
			this.ContainerPenaltiesTabPage.TabIndex = 16;
			// 
			// ContainerDetentionGrid
			// 
			this.ContainerDetentionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainerDetentionGrid, "ConsignorContainerPenalties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_OriginPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_FreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_FreeDayType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_CEX_FreeDayExclusion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorContainerPenalties)).SyncRoot)).PD_CEX_DurationExclusion)));
			this.ContainerDetentionGrid.CaptionVisible = false;
			penaltyTypeDropEditColumnStyleInfo.ColumnName = "PD_PenaltyType";
			penaltyTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			penaltyDescTextBoxColumnStyleInfo.ColumnName = "PenaltyDescription";
			penaltyDescTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			carrierFindBoxColumnStyleInfo.ColumnName = "PD_OH_Carrier";
			carrierFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "PD_OriginPortOrCountry";
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo8.ColumnName = "PD_FreeDayType";
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|54ab8218-b2fa-489f-96e6-1487b29b746a", "Last Free Day");
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			detentionFreeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			detentionFreeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			detentionDurationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			detentionDurationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ContainerDetentionGrid.ColumnStyles.Add(penaltyTypeDropEditColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(penaltyDescTextBoxColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(carrierFindBoxColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ContainerDetentionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ContainerDetentionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ContainerDetentionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainerDetentionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ContainerDetentionGrid.ColumnStyles.Add(detentionFreeDayExclusionColumnStyleInfo);
			this.ContainerDetentionGrid.ColumnStyles.Add(detentionDurationExclusionColumnStyleInfo);

			this.ContainerDetentionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerDetentionGrid.GridId = "cf3153df-05dd-448e-a812-7079c87d8dc9";
			this.ContainerDetentionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerDetentionGrid.LayoutKey = "zGrid1";
			this.ContainerDetentionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainerDetentionGrid.Name = "ContainerDetentionGrid";
			this.ContainerDetentionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 357, true);
			this.ContainerDetentionGrid.TabIndex = 4;
			// 
			// CTOStorageTabPage
			// 
			this.CTOStorageTabPage.Controls.Add(this.ConsignorCTOStoragesGrid);
			this.CTOStorageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CTOStorageTabPage.Name = "CTOStorageTabPage";
			this.CTOStorageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 349, true);
			this.CTOStorageTabPage.TabIndex = 17;
			this.CTOStorageTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|0a63584d-f985-4035-905f-098be02e8b70", "CTO Storage");
			// 
			// ConsignorCTOStoragesGrid
			// 
			this.ConsignorCTOStoragesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsignorCTOStoragesGrid, "ConsignorCTOStorages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_OH_CTO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_CreditorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_FreeDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ConsignorCTOStorages)).SyncRoot)).PD_FreeDayType)));
			this.ConsignorCTOStoragesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c58e8d02-5b94-4ef6-ae57-4c08c24a2e39", "Port/Country(Region)");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c6a11b91-7b52-45ef-96fc-32e0c66503bd", "Class");
			zDropEditColumnStyleInfo7.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "PD_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("478ac4f2-645a-41f4-9f0a-3d6c7b45aba6", "CTO");
			zOrganisationFindBoxColumnStyleInfo4.ColumnName = "PD_OH_CTO";
			zOrganisationFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("679d3542-3098-4fb5-9aba-edbef332b10c", "Free Days");
			zCalcEditColumnStyleInfo2.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("887241f4-a71a-4f28-897e-48af2bad960a", "Last Free Day");
			zDropEditColumnStyleInfo9.ColumnName = "PD_FreeDayType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			storagesFreeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			storagesFreeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			storagesDurationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			storagesDurationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo4);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(storagesFreeDayExclusionColumnStyleInfo);
			this.ConsignorCTOStoragesGrid.ColumnStyles.Add(storagesDurationExclusionColumnStyleInfo);

			this.ConsignorCTOStoragesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignorCTOStoragesGrid.GridId = "cf3153df-05dd-448e-a812-7079c87d8dc9";
			this.ConsignorCTOStoragesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsignorCTOStoragesGrid.LayoutKey = "zGrid1";
			this.ConsignorCTOStoragesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignorCTOStoragesGrid.Name = "ConsignorCTOStoragesGrid";
			this.ConsignorCTOStoragesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 349, true);
			this.ConsignorCTOStoragesGrid.TabIndex = 5;
			// 
			// DefaultsTabControl
			// 
			this.DefaultsTabControl.Controls.Add(this.ExporterTabPage);
			this.DefaultsTabControl.Controls.Add(this.ServiceLevelsTabPage);
			this.DefaultsTabControl.Controls.Add(this.ExportDocumentationDefaultsTabpage);
			this.DefaultsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultsTabControl.Name = "DefaultsTabControl";
			this.DefaultsTabControl.SelectedIndex = 0;
			this.DefaultsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 603, true);
			this.DefaultsTabControl.TabIndex = 17;
			// 
			// ExporterTabPage
			// 
			this.ExporterTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|e5ba406a-5528-4fd0-96e6-ca7b028dc009", "Exporter / Consignor Defaults");
			this.ExporterTabPage.Controls.Add(this.MergeCustomsInvoiceLinesByDropEdit);
			this.ExporterTabPage.Controls.Add(this.OM_EXOwnsProductsCheckBox);
			this.ExporterTabPage.Controls.Add(this.OM_EXEnableVisibilityEventDeliveryCheckBox);
			this.ExporterTabPage.Controls.Add(this.OM_ConsignorAuthorityToLeaveDropEdit);
			this.ExporterTabPage.Controls.Add(this.importBillAgentChargesDirectCheckBox);
			this.ExporterTabPage.Controls.Add(this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox);
			this.ExporterTabPage.Controls.Add(this.zTextBox1);
			this.ExporterTabPage.Controls.Add(this.OM_IMLastOrderReferenceTextBox);
			this.ExporterTabPage.Controls.Add(this.RequireLinkedOrderTrackingCheckBox);
			this.ExporterTabPage.Controls.Add(this.EXExporterRequiresOrderNumbersOnDocsCheckBox);
			this.ExporterTabPage.Controls.Add(this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox);
			this.ExporterTabPage.Controls.Add(this.InvPriceFromLastCostDropEdit);
			this.ExporterTabPage.Controls.Add(this.AWBAddrDefzDropEdit);
			this.ExporterTabPage.Controls.Add(this.OM_IMPaymentMethodDropEdit);
			this.ExporterTabPage.Controls.Add(this.ProductExportAuditDropEdit);
			this.ExporterTabPage.Controls.Add(this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox);
			this.ExporterTabPage.Controls.Add(this.OM_RX_NKEXDefCurrencyBoundCodeFindBox);
			this.ExporterTabPage.Controls.Add(this.OM_EXDefaultIncoTermBoundDropEdit);
			this.ExporterTabPage.Controls.Add(this.OM_EXExporterCategoryBoundDropEdit);
			this.ExporterTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.ExporterTabPage.Name = "ExporterTabPage";
			this.ExporterTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ExporterTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 579, true);
			this.ExporterTabPage.TabIndex = 0;
			// 
			// OM_EXOwnsProductsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_EXOwnsProductsCheckBox, "MiscServ.OM_EXOwnsProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXOwnsProducts)));
			this.OM_EXOwnsProductsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7A0D58E3-211B-4629-A6A5-4BDB3EC0CC17", "Always Owns Product");
			this.OM_EXOwnsProductsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_EXOwnsProductsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 291, true);
			this.OM_EXOwnsProductsCheckBox.Name = "OM_EXOwnsProductsCheckBox";
			this.OM_EXOwnsProductsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.OM_EXOwnsProductsCheckBox.TabIndex = 15;
			// 
			// OM_EXEnableVisibilityEventDeliveryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_EXEnableVisibilityEventDeliveryCheckBox, "MiscServ.OM_IsVisibilityEventEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IsVisibilityEventEnabled)));
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.Visible = FreightDataRegistry.Instance.EnableVisibilityEventDelivery.Value;
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DE937009-282B-4FFB-9CD9-67CCC1EB1CD0", "Enable Visibility Event Delivery");
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 314, true);
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.Name = "OM_EXEnableVisibilityEventDeliveryCheckBox";
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.OM_EXEnableVisibilityEventDeliveryCheckBox.TabIndex = 16;
			// 
			// OM_ConsignorAuthorityToLeaveDropEdit
			// 
			this.OM_ConsignorAuthorityToLeaveDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_ConsignorAuthorityToLeaveDropEdit, "MiscServ.OM_ConsignorAuthorityToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ConsignorAuthorityToLeave)));
			this.OM_ConsignorAuthorityToLeaveDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 216, true);
			this.OM_ConsignorAuthorityToLeaveDropEdit.Name = "OM_ConsignorAuthorityToLeaveDropEdit";
			this.OM_ConsignorAuthorityToLeaveDropEdit.PreBoundMaxLength = 3;
			this.OM_ConsignorAuthorityToLeaveDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_ConsignorAuthorityToLeaveDropEdit.TabIndex = 9;
			// 
			// importBillAgentChargesDirectCheckBox
			// 
			this.BindingSource.SetBindingMember(this.importBillAgentChargesDirectCheckBox, "CompanyData.OB_IMBillAgentChargesDirect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_IMBillAgentChargesDirect)));
			this.importBillAgentChargesDirectCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.importBillAgentChargesDirectCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 291, true);
			this.importBillAgentChargesDirectCheckBox.Name = "importBillAgentChargesDirectCheckBox";
			this.importBillAgentChargesDirectCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 17, true);
			this.importBillAgentChargesDirectCheckBox.TabIndex = 12;
			// 
			// OM_FWRequiresElectronicBOLForDirectConsolCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox, "MiscServ+OM_FWRequiresElectronicBOLForDirectConsol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_FWRequiresElectronicBOLForDirectConsol)));
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8988C3FC-615B-4FC2-A7BD-801FC7E3939B", "Require Electronic Bill of Lading");
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 314, true);
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox.Name = "OM_FWRequiresElectronicBOLForDirectConsolCheckBox";
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 17, true);
			this.OM_FWRequiresElectronicBOLForDirectConsolCheckBox.TabIndex = 13;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "MiscServ+OM_EXHandlingInstuctions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXHandlingInstuctions)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 360, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 45, true);
			this.zTextBox1.TabIndex = 17;
			// 
			// OM_IMLastOrderReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.OM_IMLastOrderReferenceTextBox, "MiscServ+OM_EXGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXGoodsDescription)));
			this.OM_IMLastOrderReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 337, true);
			this.OM_IMLastOrderReferenceTextBox.Name = "OM_IMLastOrderReferenceTextBox";
			this.OM_IMLastOrderReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_IMLastOrderReferenceTextBox.TabIndex = 16;
			// 
			// RequireLinkedOrderTrackingCheckBox
			// 
			this.RequireLinkedOrderTrackingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RequireLinkedOrderTrackingCheckBox, "MiscServ.OM_EXJobRequireOrderTrackLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXJobRequireOrderTrackLink)));
			this.RequireLinkedOrderTrackingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireLinkedOrderTrackingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 268, true);
			this.RequireLinkedOrderTrackingCheckBox.Name = "RequireLinkedOrderTrackingCheckBox";
			this.RequireLinkedOrderTrackingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 17, true);
			this.RequireLinkedOrderTrackingCheckBox.TabIndex = 14;
			// 
			// EXExporterRequiresOrderNumbersOnDocsCheckBox
			// 
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EXExporterRequiresOrderNumbersOnDocsCheckBox, "MiscServ.OM_EXExporterRequiresOrderNumbersOnDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXExporterRequiresOrderNumbersOnDocs)));
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 268, true);
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox.Name = "EXExporterRequiresOrderNumbersOnDocsCheckBox";
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 17, true);
			this.EXExporterRequiresOrderNumbersOnDocsCheckBox.TabIndex = 11;
			// 
			// OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox
			// 
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox, "MiscServ.OM_RS_NKEXDefaultServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_RS_NKEXDefaultServiceLevel)));
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 190, true);
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.Name = "OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox";
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.PopupCaption = "Select Service Level";
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.PreBoundMaxLength = 3;
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.ShouldResize = true;
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.TabIndex = 8;
			// 
			// InvPriceFromLastCostDropEdit
			// 
			this.InvPriceFromLastCostDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvPriceFromLastCostDropEdit, "MiscServ+OM_EXDefaultInvoicePriceFromProductLastCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXDefaultInvoicePriceFromProductLastCost)));
			this.InvPriceFromLastCostDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 138, true);
			this.InvPriceFromLastCostDropEdit.Name = "InvPriceFromLastCostDropEdit";
			this.InvPriceFromLastCostDropEdit.PreBoundMaxLength = 3;
			this.InvPriceFromLastCostDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.InvPriceFromLastCostDropEdit.TabIndex = 5;
			// 
			// AWBAddrDefzDropEdit
			// 
			this.AWBAddrDefzDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AWBAddrDefzDropEdit, "MiscServ+OM_EXDocumentAddressPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXDocumentAddressPreference)));
			this.AWBAddrDefzDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 112, true);
			this.AWBAddrDefzDropEdit.Name = "AWBAddrDefzDropEdit";
			this.AWBAddrDefzDropEdit.PreBoundMaxLength = 3;
			this.AWBAddrDefzDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.AWBAddrDefzDropEdit.TabIndex = 4;
			// 
			// OM_IMPaymentMethodDropEdit
			// 
			this.OM_IMPaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_IMPaymentMethodDropEdit, "MiscServ.OM_IMPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPaymentMethod)));
			this.OM_IMPaymentMethodDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|f7a94a35-227e-4f2d-abf5-dbc608fa8f29", "Payment Method");
			this.OM_IMPaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 164, true);
			this.OM_IMPaymentMethodDropEdit.Name = "OM_IMPaymentMethodDropEdit";
			this.OM_IMPaymentMethodDropEdit.PreBoundMaxLength = 3;
			this.OM_IMPaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_IMPaymentMethodDropEdit.TabIndex = 6;
			// 
			// MergeCustomsInvoiceLinesByDropEdit
			// 
			this.MergeCustomsInvoiceLinesByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MergeCustomsInvoiceLinesByDropEdit, "MiscServ.OM_EXMergeCustomsInvoiceLinesBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXMergeCustomsInvoiceLinesBy)));
			this.MergeCustomsInvoiceLinesByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 164, true);
			this.MergeCustomsInvoiceLinesByDropEdit.Name = "MergeCustomsInvoiceLinesByDropEdit";
			this.MergeCustomsInvoiceLinesByDropEdit.PreBoundMaxLength = 3;
			this.MergeCustomsInvoiceLinesByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.MergeCustomsInvoiceLinesByDropEdit.TabIndex = 7;
			// 
			// ProductExportAuditDropEdit
			// 
			this.ProductExportAuditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductExportAuditDropEdit, "MiscServ+OM_EXValidationForUnauditedClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXValidationForUnauditedClassification)));
			this.ProductExportAuditDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e68b97c5-903c-4280-950e-f205b47ef9f0", "Unaudited Export Product");
			this.ProductExportAuditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 242, true);
			this.ProductExportAuditDropEdit.Name = "ProductExportAuditDropEdit";
			this.ProductExportAuditDropEdit.PreBoundMaxLength = 3;
			this.ProductExportAuditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.ProductExportAuditDropEdit.TabIndex = 10;
			// 
			// OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox
			// 
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox, "MiscServ.OM_RN_NKEXDefaultCntryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_RN_NKEXDefaultCntryOfOrigin)));
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 34, true);
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.Name = "OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox";
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.PopupCaption = "Select Country";
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.ShouldResize = true;
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.TabIndex = 1;
			// 
			// OM_RX_NKEXDefCurrencyBoundCodeFindBox
			// 
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_RX_NKEXDefCurrencyBoundCodeFindBox, "MiscServ.OM_RX_NKEXDefCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_RX_NKEXDefCurrency)));
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 60, true);
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.Name = "OM_RX_NKEXDefCurrencyBoundCodeFindBox";
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.PopupCaption = "Select Currency";
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.ShouldResize = true;
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.TabIndex = 2;
			// 
			// OM_EXDefaultIncoTermBoundDropEdit
			// 
			this.OM_EXDefaultIncoTermBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_EXDefaultIncoTermBoundDropEdit, "MiscServ.OM_EXDefaultIncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXDefaultIncoTerm)));
			this.OM_EXDefaultIncoTermBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 86, true);
			this.OM_EXDefaultIncoTermBoundDropEdit.Name = "OM_EXDefaultIncoTermBoundDropEdit";
			this.OM_EXDefaultIncoTermBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_EXDefaultIncoTermBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_EXDefaultIncoTermBoundDropEdit.TabIndex = 3;
			// 
			// OM_EXExporterCategoryBoundDropEdit
			// 
			this.OM_EXExporterCategoryBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_EXExporterCategoryBoundDropEdit, "MiscServ.OM_EXExporterCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_EXExporterCategory)));
			this.OM_EXExporterCategoryBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 6, true);
			this.OM_EXExporterCategoryBoundDropEdit.Name = "OM_EXExporterCategoryBoundDropEdit";
			this.OM_EXExporterCategoryBoundDropEdit.PreBoundMaxLength = 3;
			this.OM_EXExporterCategoryBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.OM_EXExporterCategoryBoundDropEdit.TabIndex = 0;
			// 
			// ServiceLevelsTabPage
			// 
			this.ServiceLevelsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|87f16c06-8314-421e-b934-08a2aeaeaff1", "Service Levels");
			this.ServiceLevelsTabPage.Controls.Add(this.ServiceLevelsGrid);
			this.ServiceLevelsTabPage.Controls.Add(this.OverrideServiceLevelsSettingCheckBox);
			this.ServiceLevelsTabPage.Controls.Add(this.ServiceLevelsInfoLabel);
			this.ServiceLevelsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServiceLevelsTabPage.Name = "ServiceLevelsTabPage";
			this.ServiceLevelsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServiceLevelsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 568, true);
			this.ServiceLevelsTabPage.TabIndex = 1;
			// 
			// ServiceLevelsGrid
			// 
			this.ServiceLevelsGrid.AllowNavigation = false;
			this.ServiceLevelsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceLevelsGrid, "OrgServiceLevels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgServiceLevels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgServiceLevels)).SyncRoot)).ServiceLevel.RS_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.OrgServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgServiceLevels)).SyncRoot)).ServiceLevel.RS_DescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgServiceLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgServiceLevels)).SyncRoot)).PM_IsPublished)));
			this.ServiceLevelsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|8fdc56c2-2eb7-4169-80a2-1b33f95c7508", "Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ServiceLevel+RS_Code";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|f1870c39-e963-4335-8804-9713e714b05f", "Service Level Description");
			zTextBoxColumnStyleInfo3.ColumnName = "ServiceLevel+RS_DescriptionMultilingual";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "PM_IsPublished";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ServiceLevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServiceLevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ServiceLevelsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ServiceLevelsGrid.GridId = "ea09b018-6f52-4dfe-94c2-218ba7f28ff0";
			this.ServiceLevelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServiceLevelsGrid.LayoutKey = "ServiceLevelsGrid";
			this.ServiceLevelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 67, true);
			this.ServiceLevelsGrid.Name = "ServiceLevelsGrid";
			this.ServiceLevelsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ServiceLevelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 493, true);
			this.ServiceLevelsGrid.TabIndex = 2;
			// 
			// OverrideServiceLevelsSettingCheckBox
			// 
			this.OverrideServiceLevelsSettingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideServiceLevelsSettingCheckBox, "IsServiceLevelOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).IsServiceLevelOverridden)));
			this.OverrideServiceLevelsSettingCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|fb1b3ac5-dc4e-4f09-aac7-0fcfd4240b43", "Override System Default");
			this.OverrideServiceLevelsSettingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideServiceLevelsSettingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 33, true);
			this.OverrideServiceLevelsSettingCheckBox.Name = "OverrideServiceLevelsSettingCheckBox";
			this.OverrideServiceLevelsSettingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
			this.OverrideServiceLevelsSettingCheckBox.TabIndex = 1;
			this.OverrideServiceLevelsSettingCheckBox.UseVisualStyleBackColor = true;
			this.OverrideServiceLevelsSettingCheckBox.CheckedChanged += new System.EventHandler(this.OverrideServiceLevelsSettingCheckBox_CheckedChanged);
			// 
			// ServiceLevelsInfoLabel
			// 
			this.ServiceLevelsInfoLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|5df98eee-0fa2-4725-ba55-168e2dc5792c", "This grid controls the service levels available to this organization in WebTracker.");
			this.ServiceLevelsInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServiceLevelsInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.ServiceLevelsInfoLabel.Name = "ServiceLevelsInfoLabel";
			this.ServiceLevelsInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 23, true);
			this.ServiceLevelsInfoLabel.TabIndex = 0;
			// 
			// ExportDocumentationDefaultsTabpage
			// 
			this.ExportDocumentationDefaultsTabpage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|B9FF01DF-048D-47AA-87E2-2C64DC772318", "Export Documentation Defaults");
			this.ExportDocumentationDefaultsTabpage.Controls.Add(this.ExportsBankNameTextBox);
			this.ExportDocumentationDefaultsTabpage.Controls.Add(this.ExportsBankAccountTextBox);
			this.ExportDocumentationDefaultsTabpage.Controls.Add(this.ExportsSwiftCodeTextBox);
			this.ExportDocumentationDefaultsTabpage.Controls.Add(this.MethodOfPaymentTextBox);
			this.ExportDocumentationDefaultsTabpage.Controls.Add(this.AdditionalInformationTextBox);
			this.ExportDocumentationDefaultsTabpage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExportDocumentationDefaultsTabpage.Name = "ExportDocumentationDefaultsTabpage";
			this.ExportDocumentationDefaultsTabpage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ExportDocumentationDefaultsTabpage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 579, true);
			this.ExportDocumentationDefaultsTabpage.TabIndex = 2;
			// 
			// ExportsBankNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportsBankNameTextBox, "ExportersBankName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExportersBankName)));
			this.ExportsBankNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|C9743561-994F-48C5-9C9E-8AEAA81A690D", "Exporters Bank Name");
			this.ExportsBankNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 6, true);
			this.ExportsBankNameTextBox.Multiline = true;
			this.ExportsBankNameTextBox.Name = "ExportsBankNameTextBox";
			this.ExportsBankNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.ExportsBankNameTextBox.TabIndex = 0;
			// 
			// ExportsBankAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportsBankAccountTextBox, "ExportersBankAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExportersBankAccount)));
			this.ExportsBankAccountTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|A7479882-0CCC-4792-8F10-B78CD5B13064", "Exporters Bank Account");
			this.ExportsBankAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 34, true);
			this.ExportsBankAccountTextBox.Multiline = true;
			this.ExportsBankAccountTextBox.Name = "ExportsBankAccountTextBox";
			this.ExportsBankAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.ExportsBankAccountTextBox.TabIndex = 1;
			// 
			// ExportsSwiftCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportsSwiftCodeTextBox, "ExportersSwiftCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ExportersSwiftCode)));
			this.ExportsSwiftCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|2392EF95-BC33-4689-85EE-6B8ED29058B2", "Exporters SWIFT Code");
			this.ExportsSwiftCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 62, true);
			this.ExportsSwiftCodeTextBox.Multiline = true;
			this.ExportsSwiftCodeTextBox.Name = "ExportsSwiftCodeTextBox";
			this.ExportsSwiftCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.ExportsSwiftCodeTextBox.TabIndex = 2;
			// 
			// MethodOfPaymentTextBox
			// 
			this.BindingSource.SetBindingMember(this.MethodOfPaymentTextBox, "MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MethodOfPayment)));
			this.MethodOfPaymentTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|810D40FE-1101-44AD-AB2D-7C0637F62F5E", "Method of Payment");
			this.MethodOfPaymentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 90, true);
			this.MethodOfPaymentTextBox.Multiline = true;
			this.MethodOfPaymentTextBox.Name = "MethodOfPaymentTextBox";
			this.MethodOfPaymentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.MethodOfPaymentTextBox.TabIndex = 3;
			// 
			// AdditionalInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalInformationTextBox, "AdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AdditionalInformation)));
			this.AdditionalInformationTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorDetailsUserControl|74F969C1-152D-4E76-8934-2107E3BB287A", "Additional Information");
			this.AdditionalInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 118, true);
			this.AdditionalInformationTextBox.Multiline = true;
			this.AdditionalInformationTextBox.Name = "AdditionalInformationTextBox";
			this.AdditionalInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 20, true);
			this.AdditionalInformationTextBox.TabIndex = 4;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 734, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(121);
			this.splitContainer1.TabIndex = 18;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.CartageAndBrokerageGroupBox);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(121);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(418);

			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.ContainerDetentionPanel);
			this.splitContainer2.Panel1.Controls.Add(this.PreAllocationPanel);
			this.splitContainer2.Panel1.Controls.Add(this.CurrencyUpliftPanel);
			this.splitContainer2.Panel1.Controls.Add(this.zGroupBox1Panel);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.DefaultsTabControl);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 596, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(378);
			this.splitContainer2.TabIndex = 18;
			// 
			// zGroupBox1Panel
			// 
			this.zGroupBox1Panel.Controls.Add(this.zGroupBox1);
			this.zGroupBox1Panel.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1Panel.Name = "zGroupBox1Panel";
			this.zGroupBox1Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 87, true);
			this.zGroupBox1Panel.TabIndex = 19;
			// 
			// CurrencyUpliftPanel
			// 
			this.CurrencyUpliftPanel.Controls.Add(this.CurrencyUpliftGroupBox);
			this.CurrencyUpliftPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CurrencyUpliftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 87, true);
			this.CurrencyUpliftPanel.Name = "CurrencyUpliftPanel";
			this.CurrencyUpliftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 86, true);
			this.CurrencyUpliftPanel.TabIndex = 20;
			// 
			// PreAllocationPanel
			// 
			this.PreAllocationPanel.Controls.Add(this.PreAllocationGroupBox);
			this.PreAllocationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PreAllocationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.PreAllocationPanel.Name = "PreAllocationPanel";
			this.PreAllocationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 50, true);
			this.PreAllocationPanel.TabIndex = 21;
			// 
			// ContainerDetentionPanel
			// 
			this.ContainerDetentionPanel.Controls.Add(this.ContainerDetentionTabControl);
			this.ContainerDetentionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerDetentionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 223, true);
			this.ContainerDetentionPanel.Name = "ContainerDetentionPanel";
			this.ContainerDetentionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 373, true);
			this.ContainerDetentionPanel.TabIndex = 22;
			// 
			// ConsignorDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ConsignorDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 734, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CurrencyUpliftGroupBox.ResumeLayout(false);
			this.CurrencyUpliftGroupBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.CartageAndBrokerageGroupBox.ResumeLayout(false);
			this.CartageAndBrokerageGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.PreAllocationGroupBox.ResumeLayout(false);
			this.PreAllocationGroupBox.PerformLayout();
			this.ContainerDetentionTabControl.ResumeLayout(false);
			this.ContainerDetentionTabControl.PerformLayout();
			this.ContainerPenaltiesTabPage.ResumeLayout(false);
			this.ContainerPenaltiesTabPage.PerformLayout();
			this.CTOStorageTabPage.ResumeLayout(false);
			this.CTOStorageTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsignorCTOStoragesGrid)).EndInit();
			this.ConsignorCTOStoragesGrid.ResumeLayout(false);
			this.ConsignorCTOStoragesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerDetentionGrid)).EndInit();
			this.ContainerDetentionGrid.ResumeLayout(false);
			this.ContainerDetentionGrid.PerformLayout();
			this.DefaultsTabControl.ResumeLayout(false);
			this.DefaultsTabControl.PerformLayout();
			this.ExporterTabPage.ResumeLayout(false);
			this.ExporterTabPage.PerformLayout();
			this.OM_ConsignorAuthorityToLeaveDropEdit.ResumeLayout(true);
			this.OM_ConsignorAuthorityToLeaveDropEdit.PerformLayout();
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.ResumeLayout(true);
			this.OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox.PerformLayout();
			this.InvPriceFromLastCostDropEdit.ResumeLayout(true);
			this.InvPriceFromLastCostDropEdit.PerformLayout();
			this.AWBAddrDefzDropEdit.ResumeLayout(true);
			this.AWBAddrDefzDropEdit.PerformLayout();
			this.OM_IMPaymentMethodDropEdit.ResumeLayout(true);
			this.OM_IMPaymentMethodDropEdit.PerformLayout();
			this.ProductExportAuditDropEdit.ResumeLayout(true);
			this.ProductExportAuditDropEdit.PerformLayout();
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.ResumeLayout(true);
			this.OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox.PerformLayout();
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.ResumeLayout(true);
			this.OM_RX_NKEXDefCurrencyBoundCodeFindBox.PerformLayout();
			this.OM_EXDefaultIncoTermBoundDropEdit.ResumeLayout(true);
			this.OM_EXDefaultIncoTermBoundDropEdit.PerformLayout();
			this.OM_EXExporterCategoryBoundDropEdit.ResumeLayout(true);
			this.OM_EXExporterCategoryBoundDropEdit.PerformLayout();
			this.ServiceLevelsTabPage.ResumeLayout(false);
			this.ServiceLevelsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServiceLevelsGrid)).EndInit();
			this.ServiceLevelsGrid.ResumeLayout(false);
			this.ServiceLevelsGrid.PerformLayout();
			this.ExportDocumentationDefaultsTabpage.ResumeLayout(false);
			this.ExportDocumentationDefaultsTabpage.PerformLayout();
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
			this.MergeCustomsInvoiceLinesByDropEdit.ResumeLayout(true);
			this.MergeCustomsInvoiceLinesByDropEdit.PerformLayout();
			this.zGroupBox1Panel.ResumeLayout(false);
			this.zGroupBox1Panel.PerformLayout();
			this.CurrencyUpliftPanel.ResumeLayout(false);
			this.CurrencyUpliftPanel.PerformLayout();
			this.PreAllocationPanel.ResumeLayout(false);
			this.PreAllocationPanel.PerformLayout();
			this.ContainerDetentionPanel.ResumeLayout(false);
			this.ContainerDetentionPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZDropEdit ProductExportAuditDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox CurrencyUpliftGroupBox;
		private ZGroupBox zGroupBox1;
		protected ZCheckBox PartsBothImportAndExportCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ApprovedToPrintCheckBox;
		private ZGrid zGrid1;
		private ZGroupBox CartageAndBrokerageGroupBox;
		protected ZGroupBox PreAllocationGroupBox;
		protected ZTextBox PreAllocationTextBox;
		private ZTabPage ContainerPenaltiesTabPage;
		private ZTabPage CTOStorageTabPage;
		private ZGrid ConsignorCTOStoragesGrid;
		ZTabControl ContainerDetentionTabControl;
		private ZGrid ContainerDetentionGrid;
		protected ZTabControl DefaultsTabControl;
		private ZTabPage ExporterTabPage;
		private ZTabPage ServiceLevelsTabPage;
		private ZTabPage ExportDocumentationDefaultsTabpage;
		protected ZTextBox zTextBox1;
		protected ZTextBox OM_IMLastOrderReferenceTextBox;
		protected ZCheckBox RequireLinkedOrderTrackingCheckBox;
		protected ZCheckBox EXExporterRequiresOrderNumbersOnDocsCheckBox;
		protected ZCodeFindBox OM_RS_NKEXDefaultServiceLevelBoundCodeFindBox;
		protected ZDropEdit InvPriceFromLastCostDropEdit;
		private ZDropEdit AWBAddrDefzDropEdit;
		protected ZDropEdit OM_IMPaymentMethodDropEdit;
		protected ZCodeFindBox OM_RN_NKEXDefaultCntryOfOriginBoundCodeFindBox;
		protected ZCodeFindBox OM_RX_NKEXDefCurrencyBoundCodeFindBox;
		protected ZDropEdit OM_EXDefaultIncoTermBoundDropEdit;
		protected ZDropEdit OM_EXExporterCategoryBoundDropEdit;
		private ZLabel ServiceLevelsInfoLabel;
		private ZGrid ServiceLevelsGrid;
		private ZCheckBox OverrideServiceLevelsSettingCheckBox;
		private ZCheckBox importBillAgentChargesDirectCheckBox;
		private ZCheckBox OM_EXOwnsProductsCheckBox;
		private ZCheckBox OM_EXEnableVisibilityEventDeliveryCheckBox;
		private ZCheckBox OM_FWRequiresElectronicBOLForDirectConsolCheckBox;
		protected ZDropEdit OM_ConsignorAuthorityToLeaveDropEdit;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		protected ZTextBox ExportsBankNameTextBox;
		protected ZTextBox ExportsBankAccountTextBox;
		protected ZTextBox ExportsSwiftCodeTextBox;
		protected ZTextBox MethodOfPaymentTextBox;
		protected ZTextBox AdditionalInformationTextBox;
		private ZLinkLabel cfxUpliftEditLink;
		private System.ComponentModel.IContainer components;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private CargoWise.Windows.UI.KPanel zGroupBox1Panel;
		private CargoWise.Windows.UI.KPanel CurrencyUpliftPanel;
		private CargoWise.Windows.UI.KPanel PreAllocationPanel;
		private CargoWise.Windows.UI.KPanel ContainerDetentionPanel;
		protected ZDropEdit MergeCustomsInvoiceLinesByDropEdit;
	}
}
