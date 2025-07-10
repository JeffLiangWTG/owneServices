using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportClassificationUserControl
	{
		ZCalcEdit cbmaDefaultTaxRateCalcEdit;
		ZDropEdit ttbRateDesignationCodeDropEdit;
		readonly ZGrid relatedGrid;
		ZTabPage oDSTSCATabPage;
		ZTabPage vNETabPage;
		internal ZTabPage pSTTabPage;
		internal PSTUserControl pstUserControl;
		internal PSTDisclaimPivotControl pstDisclaimControl;
		VNEUserControl vneUserControl;
		internal ZTabPage cPSCTabPage;
		CPSCUserControl cPSCControl;
		CPSCDisclaimPivotControl cPSCDisclaimPivotControl;
		ODSAndTSCAControl odsAndTSCAControl;
		ZTabPage oMCTabPage;
		OMCUserControl oMCControl;
		internal ZTabPage DEATabPage;
		internal ZTabPage hfcTabPage;
		internal HFCUserControl hfcUserControl;

		internal DEAUserControl DEAControl;
		internal ZTabPage AttributesTabPage;
		KSplitContainer attributesLeftSplitContainer;
		ZGroupBox attribute1GroupBox;
		ZGrid attributes1Grid;
		KSplitContainer attributesRightSplitContainer;
		ZGroupBox attributes2GroupBox;
		ZGrid attributes2Grid;
		ZGroupBox attributes3GroupBox;
		ZGrid attributes3Grid;
		ZDropEdit pIRPRulingTypeDropEdit;
		ZDropEdit productExclusionDropEdit;
		ZTextBox exclusionNumberTextBox;
		ZTextBox pIRPRulingNoTextBox;
		ZCodeFindBox countryOfExportCodeBox;
		ZGroupBox aDDCVDMiscGroupBox;
		ZCheckBox isCVDBondedCheckBox;
		ZCheckBox isADDBondedCheckBox;
		ZCheckBox cVDApplicableCheckBox;
		ZCheckBox aDDApplicableCheckBox;
		ZDropEdit cVDDepositRateDropEdit;
		ZDropEdit aDDDepositRateDropEdit;
		ZTextBox aDDDepositRateZTextBox;
		ZTextBox cVDDepositRateZTextBox;
		ZCodeFindBox aDDCodeFindBox;
		ZCodeFindBox cVDCodeFindBox;
		ZDropEdit nonReimburseDecDropEdit;
		ZTextBox aDDDecIDTextBox;
		ZTabPage cWTabPage;
		ZGrid cWOGrid;
		ZDropEdit reconIndicatorDropEdit;
		ZCheckBox nAFTAReconIndicatorCheckBox;
		ZTabPage aTFTabPage;
		ATFUserControl atfUserControl;

		public ZTabControl ImportTabControl;
		ZTabPage detailsTabPage;
		ZCodeFindBox uS_UC_NKCountryOfOriginCodeFindBox;
		ZDropEdit sPIProgramDropEdit;
		ZDropEdit productClaimSetsDropEdit;
		internal ZTabPage LicenceNoTabPage;
		ZTextBox agricultureLicNoTextBox;
		ZTextBox woolLicenceTextBox;
		ZTextBox cASugarCertNoTextBox;
		ZTextBox cottonCertNoTextBox;
		ZTextBox cBTPACertificateTextBox;
		ZTextBox miscLicNoTextBox;
		internal ZTabPage aMSTabPage;
		internal AMSUserControl amsUserControl;
		internal AMSDisclaimPivotControl amsDisclaimControl;

		ZTabPage laceyTabPage;
		ZTabPage tTBTabPage;
		ZTabPage fWSTabPage;
		ZTabPage nMFSTabPage;

		ZPanel laceyLeftPanel;
		ACELaceyActUserControl laceyUserControl;
		TTBUserControl ttbUserControl;
		FWSUserControl fwsUserControl;
		NMFSUserControl nmfsUserControl;

		ZTabPage otherTabPage;
		ZDropEdit tSCAIndicatorDropEdit;
		ZAddressControl manufacturerAddressControl;
		ZAddressControl exporterAddressControl;
		ZCalcEdit activeIngredientCalcEdit;
		ZDropEdit zoneStatusDropEdit;
		ZCalcFindBox unitCalcFindBox;
		ZCalcEdit cD_9802CalcEdit;
		ZCalcFindBox ammvUnitCalcFindBox;
		ZCalcEdit ammvPercentageCalcEdit;
		ZCheckBox nAFTANetCostCheckBox;
		ZCalcFindBox cD_98InvValueCalcFindBox;
		ZDropEdit uS_CottonFeeExemptDropEdit;
		ZCheckBox cD_CottonCertificateApplyCheckBox;
		ZDropEdit rateTypeDropEdit;
		ZDropEdit taxRateSDropEdit;
		ZDropEdit taxCodeDropEdit;
		ZCheckBox flavorContentCreditIndicatorCheckBox;
		ZDropEdit taxApplyDropEdit;
		ZTabPage oGAPGATabPage;
		OGAPGARequirementsControl ogapgaRequirementsControl;
		ZCalcEdit taxRateCalcEdit;
		ZTabPage pGAFDATabPage;
		ACEFDAUserControl acefdaUserControl;
		ZGroupBox pGAFDAGroupBox;
		ZTabPage nHTSATabPage;
		NHTSAUserControl nhtsaUserControl;
		ZTabPage aPHISTabPage;
		APHISUserControl aphisUserControl;
		internal ZTabPage DDTCTabPage;
		internal DDTCProductUserControl ddtcUserControl;
		Customs.GUI.LongTextControl cI_UsageCommentTextBox;
		Customs.GUI.LongTextControl classificationDescriptionTextBox;
		ZPanel permitLicenseBottomPanel;
		ZPanel steelAluminumPanel;
		ZGroupBox aluminumSmeltGroupBox;
		ZGroupBox steelIronGroupBox;
		ZCheckBox primaryCountryNotApplicableCheckBox;
		ZCodeFindBox primaryCountryCodeFindBox;
		ZCheckBox secondaryCountryNotApplicableCheckBox;
		ZCodeFindBox secondaryCountryCodeFindBox;
		ZCodeFindBox castCountryCodeFindBox;
		ZCodeFindBox certificateOfOriginFindBox;
		ZCodeFindBox meltedCountryFindBox;

		internal ZTabPage additionalTariffsTabPage;
		Customs.Common.GUI.TariffFindBox additionalTariff1FindBox;
		Customs.Common.GUI.TariffFindBox additionalTariff2FindBox;
		Customs.Common.GUI.TariffFindBox additionalTariff3FindBox;
		Customs.Common.GUI.TariffFindBox additionalTariff4FindBox;
		Customs.Common.GUI.TariffFindBox additionalTariff5FindBox;

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			this.ImportTabControl = new ZTabControl();
			this.detailsTabPage = new ZTabPage();
			this.cbmaDefaultTaxRateCalcEdit = new ZCalcEdit();
			this.ttbRateDesignationCodeDropEdit = new ZDropEdit();
			this.nAFTAReconIndicatorCheckBox = new ZCheckBox();
			this.reconIndicatorDropEdit = new ZDropEdit();
			this.countryOfExportCodeBox = new ZCodeFindBox();
			this.rateTypeDropEdit = new ZDropEdit();
			this.taxRateSDropEdit = new ZDropEdit();
			this.taxCodeDropEdit = new ZDropEdit();
			this.flavorContentCreditIndicatorCheckBox = new ZCheckBox();
			this.taxApplyDropEdit = new ZDropEdit();
			this.taxRateCalcEdit = new ZCalcEdit();
			this.cD_98InvValueCalcFindBox = new ZCalcFindBox();
			this.unitCalcFindBox = new ZCalcFindBox();
			this.cD_9802CalcEdit = new ZCalcEdit();
			this.ammvUnitCalcFindBox = new ZCalcFindBox();
			this.ammvPercentageCalcEdit = new ZCalcEdit();
			this.uS_UC_NKCountryOfOriginCodeFindBox = new ZCodeFindBox();
			this.sPIProgramDropEdit = new ZDropEdit();
			this.productClaimSetsDropEdit = new ZDropEdit();
			this.LicenceNoTabPage = new ZTabPage();
			this.aDDCVDMiscGroupBox = new ZGroupBox();
			this.aDDDecIDTextBox = new ZTextBox();
			this.nonReimburseDecDropEdit = new ZDropEdit();
			this.cVDCodeFindBox = new ZCodeFindBox();
			this.isCVDBondedCheckBox = new ZCheckBox();
			this.isADDBondedCheckBox = new ZCheckBox();
			this.cVDApplicableCheckBox = new ZCheckBox();
			this.aDDApplicableCheckBox = new ZCheckBox();
			this.cVDDepositRateDropEdit = new ZDropEdit();
			this.aDDDepositRateDropEdit = new ZDropEdit();
			this.aDDDepositRateZTextBox = new ZTextBox();
			this.cVDDepositRateZTextBox = new ZTextBox();
			this.aDDCodeFindBox = new ZCodeFindBox();
			this.pIRPRulingTypeDropEdit = new ZDropEdit();
			this.productExclusionDropEdit = new ZDropEdit();
			this.exclusionNumberTextBox = new ZTextBox();
			this.pIRPRulingNoTextBox = new ZTextBox();
			this.uS_CottonFeeExemptDropEdit = new ZDropEdit();
			this.cD_CottonCertificateApplyCheckBox = new ZCheckBox();
			this.agricultureLicNoTextBox = new ZTextBox();
			this.woolLicenceTextBox = new ZTextBox();
			this.cASugarCertNoTextBox = new ZTextBox();
			this.cottonCertNoTextBox = new ZTextBox();
			this.cBTPACertificateTextBox = new ZTextBox();
			this.miscLicNoTextBox = new ZTextBox();
			this.otherTabPage = new ZTabPage();
			this.nAFTANetCostCheckBox = new ZCheckBox();
			this.tSCAIndicatorDropEdit = new ZDropEdit();
			this.manufacturerAddressControl = new ZAddressControl();
			this.exporterAddressControl = new ZAddressControl();
			this.activeIngredientCalcEdit = new ZCalcEdit();
			this.zoneStatusDropEdit = new ZDropEdit();
			this.cWTabPage = new ZTabPage();
			this.cWOGrid = new ZGrid();
			this.AttributesTabPage = new ZTabPage();
			this.attributesLeftSplitContainer = new KSplitContainer();
			this.attribute1GroupBox = new ZGroupBox();
			this.attributes1Grid = new ZGrid();
			this.attributesRightSplitContainer = new KSplitContainer();
			this.attributes2GroupBox = new ZGroupBox();
			this.attributes2Grid = new ZGrid();
			this.attributes3GroupBox = new ZGroupBox();
			this.attributes3Grid = new ZGrid();
			this.oGAPGATabPage = new ZTabPage();
			this.ogapgaRequirementsControl = new OGAPGARequirementsControl();
			this.pGAFDATabPage = new ZTabPage();
			this.pGAFDAGroupBox = new ZGroupBox();
			this.acefdaUserControl = new ACEFDAUserControl();
			this.nHTSATabPage = new ZTabPage();
			this.nhtsaUserControl = new NHTSAUserControl();
			this.DDTCTabPage = new ZTabPage();
			this.ddtcUserControl = new DDTCProductUserControl();
			this.laceyTabPage = new ZTabPage();
			this.laceyLeftPanel = new ZPanel();
			this.laceyUserControl = new ACELaceyActUserControl();
			this.oDSTSCATabPage = new ZTabPage();
			this.odsAndTSCAControl = new ODSAndTSCAControl();
			this.vNETabPage = new ZTabPage();
			this.vneUserControl = new VNEUserControl();
			this.pSTTabPage = new ZTabPage();
			this.pstUserControl = new PSTUserControl();
			this.pstDisclaimControl = new PSTDisclaimPivotControl();
			this.hfcTabPage = new ZTabPage();
			this.hfcUserControl = new HFCUserControl();
			this.aTFTabPage = new ZTabPage();
			this.atfUserControl = new ATFUserControl();
			this.aMSTabPage = new ZTabPage();
			this.amsUserControl = new AMSUserControl();
			this.amsDisclaimControl = new AMSDisclaimPivotControl();
			this.tTBTabPage = new ZTabPage();
			this.ttbUserControl = new TTBUserControl();
			this.fWSTabPage = new ZTabPage();
			this.fwsUserControl = new FWSUserControl();
			this.nMFSTabPage = new ZTabPage();
			this.nmfsUserControl = new NMFSUserControl();
			this.cPSCTabPage = new ZTabPage();
			this.cPSCControl = new CPSCUserControl();
			this.cPSCDisclaimPivotControl = new CPSCDisclaimPivotControl();
			this.oMCTabPage = new ZTabPage();
			this.oMCControl = new OMCUserControl();
			this.DEATabPage = new ZTabPage();
			this.DEAControl = new DEAUserControl();
			this.aPHISTabPage = new ZTabPage();
			this.aphisUserControl = new APHISUserControl();
			this.cI_UsageCommentTextBox = new Customs.GUI.LongTextControl();
			this.classificationDescriptionTextBox = new Customs.GUI.LongTextControl();
			this.permitLicenseBottomPanel = new ZPanel();
			this.steelAluminumPanel = new ZPanel();
			this.aluminumSmeltGroupBox = new ZGroupBox();
			this.steelIronGroupBox = new ZGroupBox();
			this.primaryCountryNotApplicableCheckBox = new ZCheckBox();
			this.primaryCountryCodeFindBox = new ZCodeFindBox();
			this.secondaryCountryNotApplicableCheckBox = new ZCheckBox();
			this.secondaryCountryCodeFindBox = new ZCodeFindBox();
			this.castCountryCodeFindBox = new ZCodeFindBox();
			this.certificateOfOriginFindBox = new ZCodeFindBox();
			this.meltedCountryFindBox = new ZCodeFindBox();
			this.additionalTariffsTabPage = new ZTabPage();
			this.additionalTariff1FindBox = new Customs.Common.GUI.TariffFindBox();
			this.additionalTariff2FindBox = new Customs.Common.GUI.TariffFindBox();
			this.additionalTariff3FindBox = new Customs.Common.GUI.TariffFindBox();
			this.additionalTariff4FindBox = new Customs.Common.GUI.TariffFindBox();
			this.additionalTariff5FindBox = new Customs.Common.GUI.TariffFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImportTabControl.SuspendLayout();
			this.detailsTabPage.SuspendLayout();
			this.ttbRateDesignationCodeDropEdit.SuspendLayout();
			this.cbmaDefaultTaxRateCalcEdit.SuspendLayout();
			this.classificationDescriptionTextBox.SuspendLayout();
			this.reconIndicatorDropEdit.SuspendLayout();
			this.countryOfExportCodeBox.SuspendLayout();
			this.rateTypeDropEdit.SuspendLayout();
			this.taxRateSDropEdit.SuspendLayout();
			this.taxCodeDropEdit.SuspendLayout();
			this.flavorContentCreditIndicatorCheckBox.SuspendLayout();
			this.taxApplyDropEdit.SuspendLayout();
			this.cD_98InvValueCalcFindBox.SuspendLayout();
			this.unitCalcFindBox.SuspendLayout();
			this.ammvUnitCalcFindBox.SuspendLayout();
			this.uS_UC_NKCountryOfOriginCodeFindBox.SuspendLayout();
			this.sPIProgramDropEdit.SuspendLayout();
			this.productClaimSetsDropEdit.SuspendLayout();
			this.LicenceNoTabPage.SuspendLayout();
			this.aDDCVDMiscGroupBox.SuspendLayout();
			this.nonReimburseDecDropEdit.SuspendLayout();
			this.cVDCodeFindBox.SuspendLayout();
			this.cVDDepositRateDropEdit.SuspendLayout();
			this.aDDDepositRateDropEdit.SuspendLayout();
			this.aDDDepositRateZTextBox.SuspendLayout();
			this.cVDDepositRateZTextBox.SuspendLayout();
			this.aDDCodeFindBox.SuspendLayout();
			this.pIRPRulingTypeDropEdit.SuspendLayout();
			this.productExclusionDropEdit.SuspendLayout();
			this.exclusionNumberTextBox.SuspendLayout();
			this.uS_CottonFeeExemptDropEdit.SuspendLayout();
			this.otherTabPage.SuspendLayout();
			this.tSCAIndicatorDropEdit.SuspendLayout();
			this.manufacturerAddressControl.SuspendLayout();
			this.exporterAddressControl.SuspendLayout();
			this.zoneStatusDropEdit.SuspendLayout();
			this.cWTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cWOGrid)).BeginInit();
			this.cWOGrid.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributesLeftSplitContainer)).BeginInit();
			this.attributesLeftSplitContainer.Panel1.SuspendLayout();
			this.attributesLeftSplitContainer.Panel2.SuspendLayout();
			this.attributesLeftSplitContainer.SuspendLayout();
			this.attribute1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes1Grid)).BeginInit();
			this.attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributesRightSplitContainer)).BeginInit();
			this.attributesRightSplitContainer.Panel1.SuspendLayout();
			this.attributesRightSplitContainer.Panel2.SuspendLayout();
			this.attributesRightSplitContainer.SuspendLayout();
			this.attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes2Grid)).BeginInit();
			this.attributes2Grid.SuspendLayout();
			this.attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes3Grid)).BeginInit();
			this.attributes3Grid.SuspendLayout();
			this.oGAPGATabPage.SuspendLayout();
			this.ogapgaRequirementsControl.SuspendLayout();
			this.pGAFDATabPage.SuspendLayout();
			this.pGAFDAGroupBox.SuspendLayout();
			this.acefdaUserControl.SuspendLayout();
			this.nHTSATabPage.SuspendLayout();
			this.nhtsaUserControl.SuspendLayout();
			this.laceyTabPage.SuspendLayout();
			this.laceyLeftPanel.SuspendLayout();
			this.laceyUserControl.SuspendLayout();
			this.oDSTSCATabPage.SuspendLayout();
			this.odsAndTSCAControl.SuspendLayout();
			this.vNETabPage.SuspendLayout();
			this.vneUserControl.SuspendLayout();
			this.pSTTabPage.SuspendLayout();
			this.pstUserControl.SuspendLayout();
			this.pstDisclaimControl.SuspendLayout();
			this.hfcTabPage.SuspendLayout();
			this.hfcUserControl.SuspendLayout();
			this.aTFTabPage.SuspendLayout();
			this.atfUserControl.SuspendLayout();
			this.aMSTabPage.SuspendLayout();
			this.amsUserControl.SuspendLayout();
			this.amsDisclaimControl.SuspendLayout();
			this.tTBTabPage.SuspendLayout();
			this.ttbUserControl.SuspendLayout();
			this.fWSTabPage.SuspendLayout();
			this.fwsUserControl.SuspendLayout();
			this.nMFSTabPage.SuspendLayout();
			this.nmfsUserControl.SuspendLayout();
			this.cPSCTabPage.SuspendLayout();
			this.cPSCControl.SuspendLayout();
			this.cPSCDisclaimPivotControl.SuspendLayout();
			this.oMCTabPage.SuspendLayout();
			this.oMCControl.SuspendLayout();
			this.DEATabPage.SuspendLayout();
			this.DEAControl.SuspendLayout();
			this.aPHISTabPage.SuspendLayout();
			this.aphisUserControl.SuspendLayout();
			this.permitLicenseBottomPanel.SuspendLayout();
			this.steelAluminumPanel.SuspendLayout();
			this.aluminumSmeltGroupBox.SuspendLayout();
			this.steelIronGroupBox.SuspendLayout();
			this.primaryCountryCodeFindBox.SuspendLayout();
			this.secondaryCountryCodeFindBox.SuspendLayout();
			this.castCountryCodeFindBox.SuspendLayout();
			this.certificateOfOriginFindBox.SuspendLayout();
			this.meltedCountryFindBox.SuspendLayout();
			this.additionalTariffsTabPage.SuspendLayout();
			this.additionalTariff1FindBox.SuspendLayout();
			this.additionalTariff2FindBox.SuspendLayout();
			this.additionalTariff3FindBox.SuspendLayout();
			this.additionalTariff4FindBox.SuspendLayout();
			this.additionalTariff5FindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusClassPartPivot);
			// 
			// ImportTabControl
			// 
			this.ImportTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportTabControl.Controls.Add(this.detailsTabPage);
			this.ImportTabControl.Controls.Add(this.additionalTariffsTabPage);
			this.ImportTabControl.Controls.Add(this.LicenceNoTabPage);
			this.ImportTabControl.Controls.Add(this.otherTabPage);
			this.ImportTabControl.Controls.Add(this.cWTabPage);
			this.ImportTabControl.Controls.Add(this.AttributesTabPage);
			this.ImportTabControl.Controls.Add(this.oGAPGATabPage);
			this.ImportTabControl.Controls.Add(this.pGAFDATabPage);
			this.ImportTabControl.Controls.Add(this.nHTSATabPage);
			this.ImportTabControl.Controls.Add(this.laceyTabPage);
			this.ImportTabControl.Controls.Add(this.oDSTSCATabPage);
			this.ImportTabControl.Controls.Add(this.vNETabPage);
			this.ImportTabControl.Controls.Add(this.pSTTabPage);
			this.ImportTabControl.Controls.Add(this.hfcTabPage);
			this.ImportTabControl.Controls.Add(this.aTFTabPage);
			this.ImportTabControl.Controls.Add(this.aMSTabPage);
			this.ImportTabControl.Controls.Add(this.tTBTabPage);
			this.ImportTabControl.Controls.Add(this.fWSTabPage);
			this.ImportTabControl.Controls.Add(this.nMFSTabPage);
			this.ImportTabControl.Controls.Add(this.cPSCTabPage);
			this.ImportTabControl.Controls.Add(this.oMCTabPage);
			this.ImportTabControl.Controls.Add(this.DEATabPage);
			this.ImportTabControl.Controls.Add(this.aPHISTabPage);
			this.ImportTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 0, true);
			this.ImportTabControl.Name = "ImportTabControl";
			this.ImportTabControl.SelectedIndex = 0;
			this.ImportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 299, true);
			this.ImportTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			//
			this.detailsTabPage.Controls.Add(this.cbmaDefaultTaxRateCalcEdit);
			this.detailsTabPage.Controls.Add(this.ttbRateDesignationCodeDropEdit);
			this.detailsTabPage.Controls.Add(this.classificationDescriptionTextBox);
			this.detailsTabPage.Controls.Add(this.cI_UsageCommentTextBox);
			this.detailsTabPage.Controls.Add(this.nAFTAReconIndicatorCheckBox);
			this.detailsTabPage.Controls.Add(this.reconIndicatorDropEdit);
			this.detailsTabPage.Controls.Add(this.countryOfExportCodeBox);
			this.detailsTabPage.Controls.Add(this.rateTypeDropEdit);
			this.detailsTabPage.Controls.Add(this.taxRateSDropEdit);
			this.detailsTabPage.Controls.Add(this.taxCodeDropEdit);
			this.detailsTabPage.Controls.Add(this.flavorContentCreditIndicatorCheckBox);
			this.detailsTabPage.Controls.Add(this.taxApplyDropEdit);
			this.detailsTabPage.Controls.Add(this.taxRateCalcEdit);
			this.detailsTabPage.Controls.Add(this.cD_98InvValueCalcFindBox);
			this.detailsTabPage.Controls.Add(this.unitCalcFindBox);
			this.detailsTabPage.Controls.Add(this.cD_9802CalcEdit);
			this.detailsTabPage.Controls.Add(this.ammvUnitCalcFindBox);
			this.detailsTabPage.Controls.Add(this.ammvPercentageCalcEdit);
			this.detailsTabPage.Controls.Add(this.uS_UC_NKCountryOfOriginCodeFindBox);
			this.detailsTabPage.Controls.Add(this.sPIProgramDropEdit);
			this.detailsTabPage.Controls.Add(this.productClaimSetsDropEdit);
			this.detailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.detailsTabPage.Name = "DetailsTabPage";
			this.detailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.detailsTabPage.TabIndex = 0;
			this.detailsTabPage.Text = "Details";
			// 
			// CI_UsageCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.cI_UsageCommentTextBox, "CI_UsageComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CI_UsageComment)));
			this.cI_UsageCommentTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("76FBA7EA-CE63-4FE8-81FC-6EFD1D06C36C", "Usage Comment");
			this.cI_UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 220, true);
			this.cI_UsageCommentTextBox.Name = "CI_UsageCommentTextBox";
			this.cI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 20, true);
			this.cI_UsageCommentTextBox.TabIndex = 19;
			//
			// CI_DescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.classificationDescriptionTextBox, "CI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CI_Description)));
			this.classificationDescriptionTextBox.Name = "classificationDescriptionTextBox";
			this.classificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 248, true);
			this.classificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 20, true);
			classificationDescriptionTextBox.TabIndex = 20;
			// 
			// NAFTAReconIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.nAFTAReconIndicatorCheckBox, "CD_NAFTARecon");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_NAFTARecon)));
			this.nAFTAReconIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.nAFTAReconIndicatorCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.nAFTAReconIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.nAFTAReconIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 196, true);
			this.nAFTAReconIndicatorCheckBox.Name = "NAFTAReconIndicatorCheckBox";
			this.nAFTAReconIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.nAFTAReconIndicatorCheckBox.TabIndex = 18;
			this.nAFTAReconIndicatorCheckBox.Text = "FTA Recon";
			this.nAFTAReconIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReconIndicatorDropEdit
			// 
			this.reconIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reconIndicatorDropEdit, "CD_ReconIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_ReconIssue)));
			this.reconIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|edf716bb-620f-4152-9d9d-fba839572ba9", "Recon. Issue");
			this.reconIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 196, true);
			this.reconIndicatorDropEdit.Name = "ReconIndicatorDropEdit";
			this.reconIndicatorDropEdit.PreBoundMaxLength = 2;
			this.reconIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.reconIndicatorDropEdit.TabIndex = 17;
			// 
			// CountryOfExportCodeBox
			// 
			this.countryOfExportCodeBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryOfExportCodeBox, "CD_UC_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_UC_NKCountryOfExport)));
			this.countryOfExportCodeBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|b031ff7f-d0c7-4139-98a6-7f94344f0487", "Country of Export");
			this.countryOfExportCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 4, true);
			this.countryOfExportCodeBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.countryOfExportCodeBox.Name = "CountryOfExportCodeBox";
			this.countryOfExportCodeBox.PopupCaption = null;
			this.countryOfExportCodeBox.PreBoundMaxLength = 2;
			this.countryOfExportCodeBox.ShowDescriptionBox = false;
			this.countryOfExportCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 17, true);
			this.countryOfExportCodeBox.TabIndex = 0;
			// 
			// RateTypeDropEdit
			// 
			this.rateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rateTypeDropEdit, "CD_TaxRateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_TaxRateType)));
			this.rateTypeDropEdit.BindToList = null;
			this.rateTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|e4b93083-0c80-4c61-9df4-0a9f91982d2f", "Rate Type");
			this.rateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 124, true);
			this.rateTypeDropEdit.Name = "RateTypeDropEdit";
			this.rateTypeDropEdit.PreBoundMaxLength = 1;
			this.rateTypeDropEdit.ShowDescriptionBox = false;
			this.rateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.rateTypeDropEdit.TabIndex = 11;
			// 
			// TaxRateSDropEdit
			// 
			this.taxRateSDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxRateSDropEdit, "CD_TaxRateDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_TaxRateDesc)));
			this.taxRateSDropEdit.BindToList = null;
			this.taxRateSDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|b727202e-0a4d-4ea4-a09b-5895bd0d0945", "Tax Rate");
			this.taxRateSDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.taxRateSDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 148, true);
			this.taxRateSDropEdit.Name = "TaxRateSDropEdit";
			this.taxRateSDropEdit.PreBoundMaxLength = 20;
			this.taxRateSDropEdit.ShowDescriptionBox = false;
			this.taxRateSDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.taxRateSDropEdit.TabIndex = 12;
			//
			// ttbRateDesignationCodeDropEdit
			//
			this.ttbRateDesignationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ttbRateDesignationCodeDropEdit, "CD_TTBRateDesignationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_TTBRateDesignationCode)));
			this.ttbRateDesignationCodeDropEdit.BindToList = null;
			this.ttbRateDesignationCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|67e2d5be-5459-4d73-8ea7-50176472b308", "CBMA Rate Desig.", "CBMA Rate Designation Code");
			this.ttbRateDesignationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 172, true);
			this.ttbRateDesignationCodeDropEdit.Name = "TTBRateDesignationCodeDropEdit";
			this.ttbRateDesignationCodeDropEdit.PreBoundMaxLength = 3;
			this.ttbRateDesignationCodeDropEdit.ShowDescriptionBox = false;
			this.ttbRateDesignationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.ttbRateDesignationCodeDropEdit.TabIndex = 15;
			//
			// cbmaDefaultTaxRateCalcEdit
			//
			this.BindingSource.SetBindingMember(this.cbmaDefaultTaxRateCalcEdit, "CD_CBMADefaultTaxRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CusClassPartPivot)(null)).CD_CBMADefaultTaxRate)));
			this.cbmaDefaultTaxRateCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|686d97b3-6fba-48cf-bb11-1254ef0c35b2", "CBMA Rate");
			this.cbmaDefaultTaxRateCalcEdit.DecimalPlaces = 8;
			this.cbmaDefaultTaxRateCalcEdit.Decimals = 8;
			this.cbmaDefaultTaxRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 172, true);
			this.cbmaDefaultTaxRateCalcEdit.Name = "CBMADefaultTaxRateCalcEdit";
			this.cbmaDefaultTaxRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 17, true);
			this.cbmaDefaultTaxRateCalcEdit.TabIndex = 16;
			this.cbmaDefaultTaxRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxCodeDropEdit
			// 
			this.taxCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxCodeDropEdit, "CD_TaxCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_TaxCode)));
			this.taxCodeDropEdit.BindToList = null;
			this.taxCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|9092aa77-7ec8-422c-80dc-aa859da7945e", "Tax Code");
			this.taxCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 124, true);
			this.taxCodeDropEdit.Name = "TaxCodeDropEdit";
			this.taxCodeDropEdit.PreBoundMaxLength = 3;
			this.taxCodeDropEdit.ShowDescriptionBox = false;
			this.taxCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.taxCodeDropEdit.TabIndex = 10;
			//
			// FlavorContentCreditIndicatorCheckBox
			//
			this.BindingSource.SetBindingMember(this.flavorContentCreditIndicatorCheckBox, "CD_FlavorContentCreditIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_FlavorContentCreditIndicator)));
			this.flavorContentCreditIndicatorCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USSpell|ImportClassificationUserControl|7FA2296B-76AF-44EF-8DCC-8C0C684FE020", "Flavor Content Credit Ind.", "Flavor Content Credit Indicator");
			this.flavorContentCreditIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.flavorContentCreditIndicatorCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.flavorContentCreditIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.flavorContentCreditIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 148, true);
			this.flavorContentCreditIndicatorCheckBox.Name = "FlavorContentCreditIndicatorCheckBox";
			this.flavorContentCreditIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.flavorContentCreditIndicatorCheckBox.UseVisualStyleBackColor = true;
			this.flavorContentCreditIndicatorCheckBox.TabIndex = 14;
			// 
			// TaxApplyDropEdit
			// 
			this.taxApplyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxApplyDropEdit, "CD_TaxApplicability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_TaxApplicability)));
			this.taxApplyDropEdit.BindToList = null;
			this.taxApplyDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|1b80b9e7-56ed-4fd3-85ea-7021e1e3cfaa", "Tax Applicability");
			this.taxApplyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 124, true);
			this.taxApplyDropEdit.Name = "TaxApplyDropEdit";
			this.taxApplyDropEdit.PreBoundMaxLength = 3;
			this.taxApplyDropEdit.ShowDescriptionBox = false;
			this.taxApplyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.taxApplyDropEdit.TabIndex = 9;
			// 
			// TaxRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.taxRateCalcEdit, "CD_TaxRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CusClassPartPivot)(null)).CD_TaxRate)));
			this.taxRateCalcEdit.DecimalPlaces = 8;
			this.taxRateCalcEdit.Decimals = 8;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.taxRateCalcEdit, false);
			this.taxRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 148, true);
			this.taxRateCalcEdit.Name = "TaxRateCalcEdit";
			this.taxRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.taxRateCalcEdit.TabIndex = 13;
			this.taxRateCalcEdit.Text = "0.00000000";
			this.taxRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// _98InvValueCalcFindBox
			// 
			this.cD_98InvValueCalcFindBox.AllowDrop = true;
			this.cD_98InvValueCalcFindBox.BindToAmount = "CD_9802ValuePerUnit";
			this.cD_98InvValueCalcFindBox.BindToUnit = "CD_RX_NK9802ValuePerUnitCurr";
			this.cD_98InvValueCalcFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|ccce3737-5c97-4dbf-9558-1c70f0bbf78d", "US/Orig Inv. / Unit");
			this.cD_98InvValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.cD_98InvValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 100, true);
			this.cD_98InvValueCalcFindBox.Name = "_98InvValueCalcFindBox";
			this.cD_98InvValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.cD_98InvValueCalcFindBox.TabIndex = 8;
			// 
			// UnitCalcFindBox
			// 
			this.unitCalcFindBox.AllowDrop = true;
			this.unitCalcFindBox.BindToAmount = "CD_PerUnitCost";
			this.unitCalcFindBox.BindToUnit = "CD_RX_NKPerUnitCostCurr";
			this.unitCalcFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|db28cbae-87ed-445a-96cc-a5faf5b45e09", "Price / Unit");
			this.unitCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.unitCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 52, true);
			this.unitCalcFindBox.Name = "UnitCalcFindBox";
			this.unitCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.unitCalcFindBox.TabIndex = 4;
			// 
			// _9802CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.cD_9802CalcEdit, "CD_9802USDValuePerUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CusClassPartPivot)(null)).CD_9802USDValuePerUnit)));
			this.cD_9802CalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|097486f2-77cb-4834-9ea1-7f696b2a6b7d", "US/Orig Val / Unit");
			this.cD_9802CalcEdit.DecimalPlaces = 2;
			this.cD_9802CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 76, true);
			this.cD_9802CalcEdit.Name = "_9802CalcEdit";
			this.cD_9802CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.cD_9802CalcEdit.TabIndex = 6;
			this.cD_9802CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmmvUnitCalcFindBox
			//
			this.ammvUnitCalcFindBox.AllowDrop = true;
			this.ammvUnitCalcFindBox.BindToAmount = "CD_AMMVPerUnit";
			this.ammvUnitCalcFindBox.BindToUnit = "CD_AMMVPerUnitCurrency";
			this.ammvUnitCalcFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|ef482ffb-0f63-4ebf-b967-3559e48c8575", "Assist/Unit", "Assist/AMMV/Unit", "");
			this.ammvUnitCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ammvUnitCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 52, true);
			this.ammvUnitCalcFindBox.Name = "AmmvUnitCalcFindBox";
			this.ammvUnitCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ammvUnitCalcFindBox.TabIndex = 5;
			// 
			// AmmvPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ammvPercentageCalcEdit, "CD_AMMVPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CusClassPartPivot)(null)).CD_AMMVPercentage)));
			this.ammvPercentageCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|38D9511D-5F65-41C5-9D7E-10150C09B287", "Assist Percentage", "Assist/AMMV Percentage", "");
			this.ammvPercentageCalcEdit.DecimalPlaces = 4;
			this.ammvPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 76, true);
			this.ammvPercentageCalcEdit.Name = "AmmvPercentageCalcEdit";
			this.ammvPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.ammvPercentageCalcEdit.TabIndex = 7;
			this.ammvPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_UC_NKCountryOfOriginCodeFindBox
			// 
			this.uS_UC_NKCountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uS_UC_NKCountryOfOriginCodeFindBox, "CD_UC_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_UC_NKCountryOfOrigin)));
			this.uS_UC_NKCountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|537c7842-f467-40c5-aa6f-ac38fa1c24d8", "Country of Origin");
			this.uS_UC_NKCountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 28, true);
			this.uS_UC_NKCountryOfOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.uS_UC_NKCountryOfOriginCodeFindBox.Name = "US_UC_NKCountryOfOriginCodeFindBox";
			this.uS_UC_NKCountryOfOriginCodeFindBox.PopupCaption = null;
			this.uS_UC_NKCountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.uS_UC_NKCountryOfOriginCodeFindBox.ShowDescriptionBox = false;
			this.uS_UC_NKCountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 17, true);
			this.uS_UC_NKCountryOfOriginCodeFindBox.TabIndex = 2;
			// 
			// SPIProgramDropEdit
			// 
			this.sPIProgramDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sPIProgramDropEdit, "CD_SPI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_SPI)));
			this.sPIProgramDropEdit.BindToList = null;
			this.sPIProgramDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|9f7bea60-3633-49d1-83fd-d5a999a1f721", "SPI Indicator");
			this.sPIProgramDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 4, true);
			this.sPIProgramDropEdit.Name = "SPIProgramDropEdit";
			this.sPIProgramDropEdit.PreBoundMaxLength = 3;
			this.sPIProgramDropEdit.ShowDescriptionBox = false;
			this.sPIProgramDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.sPIProgramDropEdit.TabIndex = 1;
			// 
			// ProductClaimSetsDropEdit
			// 
			this.productClaimSetsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productClaimSetsDropEdit, "CD_ProductClaim");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_ProductClaim)));
			this.productClaimSetsDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|8191de11-81af-4a41-8528-8325c0f60c1e", "Product Claim/Sets");
			this.productClaimSetsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 28, true);
			this.productClaimSetsDropEdit.Name = "ProductClaimSetsDropEdit";
			this.productClaimSetsDropEdit.PreBoundMaxLength = 1;
			this.productClaimSetsDropEdit.ShowDescriptionBox = false;
			this.productClaimSetsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.productClaimSetsDropEdit.TabIndex = 3;
			// 
			// additionalTariffsTabPage
			// 
			this.additionalTariffsTabPage.Controls.Add(this.additionalTariff1FindBox);
			this.additionalTariffsTabPage.Controls.Add(this.additionalTariff2FindBox);
			this.additionalTariffsTabPage.Controls.Add(this.additionalTariff3FindBox);
			this.additionalTariffsTabPage.Controls.Add(this.additionalTariff4FindBox);
			this.additionalTariffsTabPage.Controls.Add(this.additionalTariff5FindBox);
			this.additionalTariffsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.additionalTariffsTabPage.Name = "additionalTariffsTabPage";
			this.additionalTariffsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.additionalTariffsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.additionalTariffsTabPage.TabIndex = 4;
			this.additionalTariffsTabPage.Text = "Additional Tariffs";
			// 
			// additionalTariff1FindBox
			// 
			this.additionalTariff1FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalTariff1FindBox, "SupFormattedAdditionalTariff1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).SupFormattedAdditionalTariff1)));
			this.additionalTariff1FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 10, true);
			this.additionalTariff1FindBox.Name = "additionalTariff1FindBox";
			this.additionalTariff1FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.additionalTariff1FindBox.ParentType = null;
			this.additionalTariff1FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.additionalTariff1FindBox.TabIndex = 1;
			// 
			// additionalTariff2FindBox
			// 
			this.additionalTariff2FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalTariff2FindBox, "SupFormattedAdditionalTariff2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).SupFormattedAdditionalTariff2)));
			this.additionalTariff2FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 40, true);
			this.additionalTariff2FindBox.Name = "additionalTariff2FindBox";
			this.additionalTariff2FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.additionalTariff2FindBox.ParentType = null;
			this.additionalTariff2FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.additionalTariff2FindBox.TabIndex = 1;
			// 
			// additionalTariff3FindBox
			// 
			this.additionalTariff3FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalTariff3FindBox, "SupFormattedAdditionalTariff3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).SupFormattedAdditionalTariff3)));
			this.additionalTariff3FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 70, true);
			this.additionalTariff3FindBox.Name = "additionalTariff3FindBox";
			this.additionalTariff3FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.additionalTariff3FindBox.ParentType = null;
			this.additionalTariff3FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.additionalTariff3FindBox.TabIndex = 1;
			// 
			// additionalTariff4FindBox
			// 
			this.additionalTariff4FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalTariff4FindBox, "SupFormattedAdditionalTariff4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).SupFormattedAdditionalTariff4)));
			this.additionalTariff4FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 100, true);
			this.additionalTariff4FindBox.Name = "additionalTariff4FindBox";
			this.additionalTariff4FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.additionalTariff4FindBox.ParentType = null;
			this.additionalTariff4FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.additionalTariff4FindBox.TabIndex = 1;
			// 
			// additionalTariff5FindBox
			// 
			this.additionalTariff5FindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalTariff5FindBox, "SupFormattedAdditionalTariff5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).SupFormattedAdditionalTariff5)));
			this.additionalTariff5FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 130, true);
			this.additionalTariff5FindBox.Name = "additionalTariff5FindBox";
			this.additionalTariff5FindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.additionalTariff5FindBox.ParentType = null;
			this.additionalTariff5FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.additionalTariff5FindBox.TabIndex = 1;
			// 
			// LicenceNoTabPage
			// 
			this.LicenceNoTabPage.Controls.Add(this.exclusionNumberTextBox);
			this.LicenceNoTabPage.Controls.Add(this.productExclusionDropEdit);
			this.LicenceNoTabPage.Controls.Add(this.permitLicenseBottomPanel);
			this.LicenceNoTabPage.Controls.Add(this.pIRPRulingTypeDropEdit);
			this.LicenceNoTabPage.Controls.Add(this.pIRPRulingNoTextBox);
			this.LicenceNoTabPage.Controls.Add(this.uS_CottonFeeExemptDropEdit);
			this.LicenceNoTabPage.Controls.Add(this.cD_CottonCertificateApplyCheckBox);
			this.LicenceNoTabPage.Controls.Add(this.agricultureLicNoTextBox);
			this.LicenceNoTabPage.Controls.Add(this.woolLicenceTextBox);
			this.LicenceNoTabPage.Controls.Add(this.cASugarCertNoTextBox);
			this.LicenceNoTabPage.Controls.Add(this.cottonCertNoTextBox);
			this.LicenceNoTabPage.Controls.Add(this.cBTPACertificateTextBox);
			this.LicenceNoTabPage.Controls.Add(this.miscLicNoTextBox);
			this.LicenceNoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LicenceNoTabPage.Name = "LicenceNoTabPage";
			this.LicenceNoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LicenceNoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.LicenceNoTabPage.TabIndex = 4;
			this.LicenceNoTabPage.Text = "Permits/Licenses";
			// 
			// ADDCVDMiscGroupBox
			// 
			this.aDDCVDMiscGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|7f4691b9-7f3b-4648-b559-e5331294870b", "ADD/CVD Details");
			this.aDDCVDMiscGroupBox.Controls.Add(this.aDDDecIDTextBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.nonReimburseDecDropEdit);
			this.aDDCVDMiscGroupBox.Controls.Add(this.cVDCodeFindBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.isCVDBondedCheckBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.isADDBondedCheckBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.cVDApplicableCheckBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.aDDApplicableCheckBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.cVDDepositRateDropEdit);
			this.aDDCVDMiscGroupBox.Controls.Add(this.aDDDepositRateDropEdit);
			this.aDDCVDMiscGroupBox.Controls.Add(this.aDDDepositRateZTextBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.cVDDepositRateZTextBox);
			this.aDDCVDMiscGroupBox.Controls.Add(this.aDDCodeFindBox);
			this.aDDCVDMiscGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aDDCVDMiscGroupBox.Name = "ADDCVDMiscGroupBox";
			this.aDDCVDMiscGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 84, true);
			this.aDDCVDMiscGroupBox.TabIndex = 1;
			this.aDDCVDMiscGroupBox.TabStop = false;
			// 
			// ADDDecIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.aDDDecIDTextBox, "CD_ADDDecID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_ADDDecID)));
			this.aDDDecIDTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|634c0308-d2d7-457c-8505-62e4f2710682", "Non-Reimburse. Dec ID");
			this.aDDDecIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 36, true);
			this.aDDDecIDTextBox.Name = "ADDDecIDTextBox";
			this.aDDDecIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.aDDDecIDTextBox.TabIndex = 6;
			// 
			// NonReimburseDecDropEdit
			// 
			this.nonReimburseDecDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nonReimburseDecDropEdit, "CD_ADCVDStat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_ADCVDStat)));
			this.nonReimburseDecDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|13a9891e-54bc-4dc7-adef-b0b9e23b934d", "ADD Non-Reimbursement Statement");
			this.nonReimburseDecDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 10, true);
			this.nonReimburseDecDropEdit.Name = "NonReimburseDecDropEdit";
			this.nonReimburseDecDropEdit.PreBoundMaxLength = 1;
			this.nonReimburseDecDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.nonReimburseDecDropEdit.TabIndex = 0;
			// 
			// CVDCodeFindBox
			// 
			this.cVDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cVDCodeFindBox, "CD_CVDCaseNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_CVDCaseNo)));
			this.cVDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|b57ab479-5a4e-4fc5-b5a4-e85e6eaaf9b1", "CVD Case No");
			this.cVDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 58, true);
			this.cVDCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.USCACCase;
			this.cVDCodeFindBox.Name = "CVDCodeFindBox";
			this.cVDCodeFindBox.PopupCaption = null;
			this.cVDCodeFindBox.ShowDescriptionBox = false;
			this.cVDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.cVDCodeFindBox.TabIndex = 7;
			// 
			// IsCVDBondedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isCVDBondedCheckBox, "CD_CVDBonded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_CVDBonded)));
			this.isCVDBondedCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|a5eeef7c-2553-4cc0-94f4-8cd081266ea3", "Bonded:");
			this.isCVDBondedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isCVDBondedCheckBox.EnableValidStateColor = false;
			this.isCVDBondedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isCVDBondedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 59, true);
			this.isCVDBondedCheckBox.Name = "IsCVDBondedCheckBox";
			this.isCVDBondedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 18, true);
			this.isCVDBondedCheckBox.TabIndex = 10;
			this.isCVDBondedCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsADDBondedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isADDBondedCheckBox, "CD_ADDBonded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_ADDBonded)));
			this.isADDBondedCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|bee99add-f0a1-4760-b6fb-44403ee23ffc", "Bonded:");
			this.isADDBondedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isADDBondedCheckBox.EnableValidStateColor = false;
			this.isADDBondedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isADDBondedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 37, true);
			this.isADDBondedCheckBox.Name = "IsADDBondedCheckBox";
			this.isADDBondedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 18, true);
			this.isADDBondedCheckBox.TabIndex = 5;
			this.isADDBondedCheckBox.UseVisualStyleBackColor = true;
			// 
			// CVDApplicableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.cVDApplicableCheckBox, "CD_CVDApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_CVDApplicable)));
			this.cVDApplicableCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|1e80cbf4-ae9b-45c1-bffb-e7620a83d0b5", "CVD N/A:");
			this.cVDApplicableCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.cVDApplicableCheckBox.EnableValidStateColor = false;
			this.cVDApplicableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cVDApplicableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 59, true);
			this.cVDApplicableCheckBox.Name = "CVDApplicableCheckBox";
			this.cVDApplicableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.cVDApplicableCheckBox.TabIndex = 6;
			this.cVDApplicableCheckBox.UseVisualStyleBackColor = true;
			// 
			// ADDApplicableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.aDDApplicableCheckBox, "CD_ADDApplicable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_ADDApplicable)));
			this.aDDApplicableCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|ab0c9436-547a-4d75-9547-90e8a78e7e31", "ADD N/A:");
			this.aDDApplicableCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.aDDApplicableCheckBox.EnableValidStateColor = false;
			this.aDDApplicableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aDDApplicableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 37, true);
			this.aDDApplicableCheckBox.Name = "ADDApplicableCheckBox";
			this.aDDApplicableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.aDDApplicableCheckBox.TabIndex = 1;
			this.aDDApplicableCheckBox.UseVisualStyleBackColor = true;
			// 
			// CVDDepositRateDropEdit
			// 
			this.cVDDepositRateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cVDDepositRateDropEdit, "CD_CVDDepositRateInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_CVDDepositRateInd)));
			this.cVDDepositRateDropEdit.ShowDescriptionBox = false;
			this.cVDDepositRateDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|8af1942b-0dae-4896-aa80-7565b8c7dd4e", "Rate");
			this.cVDDepositRateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 58, true);
			this.cVDDepositRateDropEdit.Name = "CVDDepositRateDropEdit";
			this.cVDDepositRateDropEdit.PreBoundMaxLength = 2;
			this.cVDDepositRateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.cVDDepositRateDropEdit.TabIndex = 8;
			// 
			// CVDDepositRateZTextBox
			//
			this.BindingSource.SetBindingMember(this.cVDDepositRateZTextBox, "CD_CVDDepositRateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_CVDDepositRateDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cVDDepositRateZTextBox, false);
			this.cVDDepositRateZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 58, true);
			this.cVDDepositRateZTextBox.Name = "CVDDepositRateZTextBox";
			this.cVDDepositRateZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.cVDDepositRateZTextBox.TabIndex = 9;
			// 
			// ADDDepositRateDropEdit
			// 
			this.aDDDepositRateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aDDDepositRateDropEdit, "CD_ADDDepositRateInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_ADDDepositRateInd)));
			this.aDDDepositRateDropEdit.ShowDescriptionBox = false;
			this.aDDDepositRateDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|3069d578-31c2-4eed-bcb1-ca59b52227e4", "Rate");
			this.aDDDepositRateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 36, true);
			this.aDDDepositRateDropEdit.Name = "ADDDepositRateDropEdit";
			this.aDDDepositRateDropEdit.PreBoundMaxLength = 2;
			this.aDDDepositRateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.aDDDepositRateDropEdit.TabIndex = 3;
			// 
			// ADDDepositRateZTextBox
			//
			this.BindingSource.SetBindingMember(this.aDDDepositRateZTextBox, "CD_ADDDepositRateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_ADDDepositRateDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.aDDDepositRateZTextBox, false);
			this.aDDDepositRateZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 36, true);
			this.aDDDepositRateZTextBox.Name = "ADDDepositRateZTextBox";
			this.aDDDepositRateZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.aDDDepositRateZTextBox.TabIndex = 4;
			// 
			// ADDCodeFindBox
			// 
			this.aDDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aDDCodeFindBox, "CD_ADDCaseNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_ADDCaseNo)));
			this.aDDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|b0519874-8a7b-4d4a-972f-6af6b0daf673", "ADD Case No");
			this.aDDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 36, true);
			this.aDDCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.USCACCase;
			this.aDDCodeFindBox.Name = "ADDCodeFindBox";
			this.aDDCodeFindBox.PopupCaption = null;
			this.aDDCodeFindBox.ShowDescriptionBox = false;
			this.aDDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.aDDCodeFindBox.TabIndex = 2;
			// 
			// PIRPRulingTypeDropEdit
			// 
			this.pIRPRulingTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pIRPRulingTypeDropEdit, "CD_RulingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_RulingType)));
			this.pIRPRulingTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|5a46a6f6-b030-43e7-ab6f-7da9b1303323", "Ruling Type");
			this.pIRPRulingTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 76, true);
			this.pIRPRulingTypeDropEdit.Name = "PIRPRulingTypeDropEdit";
			this.pIRPRulingTypeDropEdit.PreBoundMaxLength = 3;
			this.pIRPRulingTypeDropEdit.ShowDescriptionBox = false;
			this.pIRPRulingTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.pIRPRulingTypeDropEdit.TabIndex = 8;
			// 
			// PIRPRulingNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.pIRPRulingNoTextBox, "CD_RulingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_RulingNumber)));
			this.pIRPRulingNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|44c579e5-75da-49d4-9a2b-30eb544edfb1", "Ruling No.");
			this.pIRPRulingNoTextBox.EnableValidStateColor = false;
			this.pIRPRulingNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 76, true);
			this.pIRPRulingNoTextBox.Name = "PIRPRulingNoTextBox";
			this.pIRPRulingNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.pIRPRulingNoTextBox.TabIndex = 9;
			// 
			// ExclusionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.exclusionNumberTextBox, "CD_ExclusionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_ExclusionNumber)));
			this.exclusionNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|78d10432-07d6-42a6-b723-ddf918f37e00", "Exclusion No.");
			this.exclusionNumberTextBox.EnableValidStateColor = false;
			this.exclusionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 76, true);
			this.exclusionNumberTextBox.Name = "ExclusionNumberTextBox";
			this.exclusionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.exclusionNumberTextBox.TabIndex = 11;
			// 
			// ProductExclusionDropEdit
			// 
			this.productExclusionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productExclusionDropEdit, "CD_ProductExclusion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_ProductExclusion)));
			this.productExclusionDropEdit.BindToList = "AddInfoLookups+ProductExclusionList";
			this.productExclusionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|19fdfde8-b742-4322-8310-ad1a6388fea1", "Product Exclusion");
			this.productExclusionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 53, true);
			this.productExclusionDropEdit.Name = "ProductExclusionDropEdit";
			this.productExclusionDropEdit.PreBoundMaxLength = 2;
			this.productExclusionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.productExclusionDropEdit.TabIndex = 10;
			// 
			// US_CottonFeeExemptDropEdit
			// 
			this.uS_CottonFeeExemptDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uS_CottonFeeExemptDropEdit, "CD_CottonFeeExempt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_CottonFeeExempt)));
			this.uS_CottonFeeExemptDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|97da356d-4ab8-4ca4-8704-dbe061aa395b", "Cotton Fee Exempt", "Cotton fee calculation is based on HTS. Sometimes goods with cotton fees on the tariff are exempt.");
			this.uS_CottonFeeExemptDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 53, true);
			this.uS_CottonFeeExemptDropEdit.Name = "US_CottonFeeExemptDropEdit";
			this.uS_CottonFeeExemptDropEdit.PreBoundMaxLength = 1;
			this.uS_CottonFeeExemptDropEdit.ShowDescriptionBox = false;
			this.uS_CottonFeeExemptDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.uS_CottonFeeExemptDropEdit.TabIndex = 6;
			// 
			// CD_CottonCertificateApplyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.cD_CottonCertificateApplyCheckBox, "CD_CottonCertificateApply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((CusClassPartPivot)(null)).CD_CottonCertificateApply)));
			this.cD_CottonCertificateApplyCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("224883b5-4c0f-4791-bd10-3275e8758e8f", "Cotton Cert. Applies:", "Cotton Certificate Applies:");
			this.cD_CottonCertificateApplyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.cD_CottonCertificateApplyCheckBox.EnableValidStateColor = false;
			this.cD_CottonCertificateApplyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cD_CottonCertificateApplyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 30, true);
			this.cD_CottonCertificateApplyCheckBox.Name = "CD_CottonCertificateApplyCheckBox";
			this.cD_CottonCertificateApplyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.cD_CottonCertificateApplyCheckBox.TabIndex = 5;
			this.cD_CottonCertificateApplyCheckBox.UseVisualStyleBackColor = true;
			// 
			// AgricultureLicNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.agricultureLicNoTextBox, "CD_AgricultureLicenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_AgricultureLicenceNo)));
			this.agricultureLicNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|8e07fb96-1188-4e73-abd8-9086c2736294", "Agricultural Lic. No.", "Agricultural License No.", "Agricultural License Number", "");
			this.agricultureLicNoTextBox.EnableValidStateColor = false;
			this.agricultureLicNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 30, true);
			this.agricultureLicNoTextBox.Name = "AgricultureLicNoTextBox";
			this.agricultureLicNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.agricultureLicNoTextBox.TabIndex = 4;
			// 
			// WoolLicenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.woolLicenceTextBox, "CD_WoolLicenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_WoolLicenceNo)));
			this.woolLicenceTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|380d00d0-5385-44fc-bfd5-a2526477f063", "Wool Lic. No.", "Wool License No.", "Wool License Number", "");
			this.woolLicenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 7, true);
			this.woolLicenceTextBox.Name = "WoolLicenceTextBox";
			this.woolLicenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.woolLicenceTextBox.TabIndex = 1;
			// 
			// CASugarCertNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.cASugarCertNoTextBox, "CD_SugarCertificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_SugarCertificate)));
			this.cASugarCertNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|3db17610-045f-45ea-b45a-73dcc7123dc2", "CA Sugar Cert.", "CA Sugar Certificate", "CA Sugar Certificate", "");
			this.cASugarCertNoTextBox.EnableValidStateColor = false;
			this.cASugarCertNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 30, true);
			this.cASugarCertNoTextBox.Name = "CASugarCertNoTextBox";
			this.cASugarCertNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.cASugarCertNoTextBox.TabIndex = 3;
			// 
			// CottonCertNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.cottonCertNoTextBox, "CD_CottonCertificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_CottonCertificate)));
			this.cottonCertNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|99207bad-a7ee-43af-9e9d-a76661fba58c", "Cert. No.", "Cotton Certificate", "");
			this.cottonCertNoTextBox.EnableValidStateColor = false;
			this.cottonCertNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 7, true);
			this.cottonCertNoTextBox.Name = "CottonCertNoTextBox";
			this.cottonCertNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.cottonCertNoTextBox.TabIndex = 2;
			// 
			// CBTPACertificateTextBox
			// 
			this.BindingSource.SetBindingMember(this.cBTPACertificateTextBox, "CD_CBTPACertificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_CBTPACertificate)));
			this.cBTPACertificateTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|2ab77f04-f17f-4e89-ac64-8105c31994ad", "CBTPA Cert.", "CBTPA Certificate", "CBTPA Certificate", "");
			this.cBTPACertificateTextBox.EnableValidStateColor = false;
			this.cBTPACertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 7, true);
			this.cBTPACertificateTextBox.Name = "CBTPACertificateTextBox";
			this.cBTPACertificateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.cBTPACertificateTextBox.TabIndex = 0;
			// 
			// MiscLicNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.miscLicNoTextBox, "CD_MiscLicenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_MiscLicenceNo)));
			this.miscLicNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|c10da314-d7c9-4539-9db5-b39a07cb48f2", "Misc. Lic. No.", "Misc. License No.", "Miscellaneous License Number", "");
			this.miscLicNoTextBox.EnableValidStateColor = false;
			this.miscLicNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 53, true);
			this.miscLicNoTextBox.Name = "MiscLicNoTextBox";
			this.miscLicNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.miscLicNoTextBox.TabIndex = 7;
			// 
			// OtherTabPage
			// 
			this.otherTabPage.Controls.Add(this.nAFTANetCostCheckBox);
			this.otherTabPage.Controls.Add(this.tSCAIndicatorDropEdit);
			this.otherTabPage.Controls.Add(this.manufacturerAddressControl);
			this.otherTabPage.Controls.Add(this.exporterAddressControl);
			this.otherTabPage.Controls.Add(this.activeIngredientCalcEdit);
			this.otherTabPage.Controls.Add(this.zoneStatusDropEdit);
			this.otherTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.otherTabPage.Name = "OtherTabPage";
			this.otherTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.otherTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.otherTabPage.TabIndex = 5;
			this.otherTabPage.Text = "Other";
			// 
			// NAFTANetCostCheckBox
			// 
			this.BindingSource.SetBindingMember(this.nAFTANetCostCheckBox, "CD_NAFTANetCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_NAFTANetCost)));
			this.nAFTANetCostCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|9888d98e-7f9e-407b-b515-f296316705c0", "NAFTA Net Cost");
			this.nAFTANetCostCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.nAFTANetCostCheckBox.EnableValidStateColor = false;
			this.nAFTANetCostCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.nAFTANetCostCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 90, true);
			this.nAFTANetCostCheckBox.Name = "NAFTANetCostCheckBox";
			this.nAFTANetCostCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 19, true);
			this.nAFTANetCostCheckBox.TabIndex = 4;
			// 
			// TSCAIndicatorDropEdit
			// 
			this.tSCAIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tSCAIndicatorDropEdit, "US_TSCACertification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).US_TSCACertification)));
			this.tSCAIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|46e451e6-511c-4e91-a6f8-57b1aa6d838d", "TSCA Cert. Indicator");
			this.tSCAIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 116, true);
			this.tSCAIndicatorDropEdit.Name = "TSCAIndicatorDropEdit";
			this.tSCAIndicatorDropEdit.PreBoundMaxLength = 1;
			this.tSCAIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 17, true);
			this.tSCAIndicatorDropEdit.TabIndex = 5;
			// 
			// ManufacturerAddressControl
			// 
			this.manufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.manufacturerAddressControl, "CD_OA_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CusClassPartPivot)(null)).CD_OA_Manufacturer)));
			this.manufacturerAddressControl.BindToOrgList = "Lookups+Manufacturers";
			this.manufacturerAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|279cecc8-3194-4803-a757-932ff52a34f2", "Manufacturer");
			this.manufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 36, true);
			this.manufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.manufacturerAddressControl.PopupCaption = "";
			this.manufacturerAddressControl.ReadOnly = false;
			this.manufacturerAddressControl.ShowAddress = false;
			this.manufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			this.manufacturerAddressControl.TabIndex = 1;
			// 
			// ExporterAddressControl
			// 
			this.exporterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.exporterAddressControl, "CD_OA_Exporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CusClassPartPivot)(null)).CD_OA_Exporter)));
			this.exporterAddressControl.BindToOrgList = "Lookups+Exporters";
			this.exporterAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|9b92e110-ff99-4a9f-971d-f15de94c16f6", "Foreign Exporter");
			this.exporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 62, true);
			this.exporterAddressControl.Name = "ExporterAddressControl";
			this.exporterAddressControl.PopupCaption = "";
			this.exporterAddressControl.ReadOnly = false;
			this.exporterAddressControl.ShowAddress = false;
			this.exporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			this.exporterAddressControl.TabIndex = 2;
			// 
			// ActiveIngredientCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.activeIngredientCalcEdit, "CD_ActiveIngredientPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CusClassPartPivot)(null)).CD_ActiveIngredientPercentage)));
			this.activeIngredientCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|9dd3a051-9338-4d66-9ee2-984fc34d8c48", "Active Ingredient %");
			this.activeIngredientCalcEdit.DecimalPlaces = 6;
			this.activeIngredientCalcEdit.Decimals = 6;
			this.activeIngredientCalcEdit.EnableValidStateColor = false;
			this.activeIngredientCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 89, true);
			this.activeIngredientCalcEdit.Name = "ActiveIngredientCalcEdit";
			this.activeIngredientCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.activeIngredientCalcEdit.TabIndex = 3;
			this.activeIngredientCalcEdit.Text = "0.000000";
			this.activeIngredientCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZoneStatusDropEdit
			// 
			this.zoneStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zoneStatusDropEdit, "CD_ZoneStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusClassPartPivot)(null)).CD_ZoneStatus)));
			this.zoneStatusDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|ce5eaec8-d2a3-42d8-80e6-cc5a859711bf", "Zone Status");
			this.zoneStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 10, true);
			this.zoneStatusDropEdit.Name = "ZoneStatusDropEdit";
			this.zoneStatusDropEdit.PreBoundMaxLength = 1;
			this.zoneStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 17, true);
			this.zoneStatusDropEdit.TabIndex = 0;
			// 
			// CWTabPage
			// 
			this.cWTabPage.Controls.Add(this.cWOGrid);
			this.cWTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.cWTabPage.Name = "CWTabPage";
			this.cWTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.cWTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.cWTabPage.TabIndex = 8;
			this.cWTabPage.Text = "Census Warning Override";
			this.cWTabPage.UseVisualStyleBackColor = true;
			// 
			// CWOGrid
			// 
			this.cWOGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.cWOGrid, "CensusWarningOverrides");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CusClassPartPivot)(null)).CensusWarningOverrides)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CensusWarningOverride)(((System.Collections.IList)(((CusClassPartPivot)(null)).CensusWarningOverrides)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CensusWarningOverride)(((System.Collections.IList)(((CusClassPartPivot)(null)).CensusWarningOverrides)).SyncRoot)).CY_Data)));
			this.cWOGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Condition Code";
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.Caption = "Override Code";
			zDropEditColumnStyleInfo2.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.cWOGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.cWOGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.cWOGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cWOGrid.GridId = "f1d89e5e-1544-4d7c-b5d2-70c107df08fb";
			this.cWOGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.cWOGrid.LayoutKey = "CWOGrid";
			this.cWOGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cWOGrid.Name = "CWOGrid";
			this.cWOGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.cWOGrid.TabIndex = 0;
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|1b2d9592-39d7-41db-a1f1-0d852b923e49", "Applies to");
			this.AttributesTabPage.Controls.Add(this.attributesLeftSplitContainer);
			this.AttributesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AttributesTabPage.Name = "AttributesTabPage";
			this.AttributesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.AttributesTabPage.TabIndex = 7;
			// 
			// AttributesLeftSplitContainer
			// 
			this.attributesLeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributesLeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.attributesLeftSplitContainer.Name = "AttributesLeftSplitContainer";
			// 
			// AttributesLeftSplitContainer.Panel1
			// 
			this.attributesLeftSplitContainer.Panel1.Controls.Add(this.attribute1GroupBox);
			this.attributesLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 271, true);
			this.attributesLeftSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// AttributesLeftSplitContainer.Panel2
			// 
			this.attributesLeftSplitContainer.Panel2.Controls.Add(this.attributesRightSplitContainer);
			this.attributesLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(259);
			this.attributesLeftSplitContainer.TabIndex = 3;
			// 
			// Attribute1GroupBox
			// 
			this.attribute1GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|2770ef9b-8265-480e-abb1-711fb5333a9f", "Attribute 1");
			this.attribute1GroupBox.Controls.Add(this.attributes1Grid);
			this.attribute1GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attribute1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attribute1GroupBox.Name = "Attribute1GroupBox";
			this.attribute1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 271, true);
			this.attribute1GroupBox.TabIndex = 1;
			this.attribute1GroupBox.TabStop = false;
			// 
			// Attributes1Grid
			// 
			this.attributes1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributes1Grid, "Attributes1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CusClassPartPivot)(null)).Attributes1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusAttributeFilter)(((System.Collections.IList)(((CusClassPartPivot)(null)).Attributes1)).SyncRoot)).BG_AttributeValue1)));
			this.attributes1Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attributes1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.attributes1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes1Grid.GridId = "2a088bdd-e15f-47ce-a7a7-290bd9cb5ee2";
			this.attributes1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributes1Grid.LayoutKey = "zGrid1";
			this.attributes1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.attributes1Grid.Name = "Attributes1Grid";
			this.attributes1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 255, true);
			this.attributes1Grid.TabIndex = 4;
			// 
			// AttributesRightSplitContainer
			// 
			this.attributesRightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributesRightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributesRightSplitContainer.Name = "AttributesRightSplitContainer";
			// 
			// AttributesRightSplitContainer.Panel1
			// 
			this.attributesRightSplitContainer.Panel1.Controls.Add(this.attributes2GroupBox);
			this.attributesRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 271, true);
			this.attributesRightSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// AttributesRightSplitContainer.Panel2
			// 
			this.attributesRightSplitContainer.Panel2.Controls.Add(this.attributes3GroupBox);
			this.attributesRightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			this.attributesRightSplitContainer.TabIndex = 0;
			// 
			// Attributes2GroupBox
			// 
			this.attributes2GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|4047b995-c94a-407d-9508-8bfc0d51d2e1", "Attribute 2");
			this.attributes2GroupBox.Controls.Add(this.attributes2Grid);
			this.attributes2GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes2GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributes2GroupBox.Name = "Attributes2GroupBox";
			this.attributes2GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 271, true);
			this.attributes2GroupBox.TabIndex = 2;
			this.attributes2GroupBox.TabStop = false;
			// 
			// Attributes2Grid
			// 
			this.attributes2Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributes2Grid, "Attributes2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CusClassPartPivot)(null)).Attributes2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusAttributeFilter)(((System.Collections.IList)(((CusClassPartPivot)(null)).Attributes2)).SyncRoot)).BG_AttributeValue1)));
			this.attributes2Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attributes2Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.attributes2Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes2Grid.GridId = "448dc1e1-d5e6-414f-b181-afce0491d514";
			this.attributes2Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributes2Grid.LayoutKey = "zGrid1";
			this.attributes2Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.attributes2Grid.Name = "Attributes2Grid";
			this.attributes2Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 255, true);
			this.attributes2Grid.TabIndex = 4;
			// 
			// Attributes3GroupBox
			// 
			this.attributes3GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|8579b43b-84c8-48d8-995d-aef9d5a7c47e", "Attribute 3");
			this.attributes3GroupBox.Controls.Add(this.attributes3Grid);
			this.attributes3GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes3GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attributes3GroupBox.Name = "Attributes3GroupBox";
			this.attributes3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 271, true);
			this.attributes3GroupBox.TabIndex = 3;
			this.attributes3GroupBox.TabStop = false;
			// 
			// Attributes3Grid
			// 
			this.attributes3Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.attributes3Grid, "Attributes3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CusClassPartPivot)(null)).Attributes3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusAttributeFilter)(((System.Collections.IList)(((CusClassPartPivot)(null)).Attributes3)).SyncRoot)).BG_AttributeValue1)));
			this.attributes3Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attributes3Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.attributes3Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attributes3Grid.GridId = "8195dad5-d059-420b-8912-bf44eaaadaae";
			this.attributes3Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attributes3Grid.LayoutKey = "zGrid1";
			this.attributes3Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.attributes3Grid.Name = "Attributes3Grid";
			this.attributes3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 255, true);
			this.attributes3Grid.TabIndex = 4;
			// 
			// OGAPGATabPage
			// 
			this.oGAPGATabPage.Controls.Add(this.ogapgaRequirementsControl);
			this.oGAPGATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.oGAPGATabPage.Name = "OGAPGATabPage";
			this.oGAPGATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.oGAPGATabPage.TabIndex = 9;
			this.oGAPGATabPage.Text = "PGA Requirements";
			// 
			// ogapgaRequirementsControl
			// 
			this.ogapgaRequirementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ogapgaRequirementsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ogapgaRequirementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ogapgaRequirementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ogapgaRequirementsControl.Name = "ogapgaRequirementsControl";
			this.ogapgaRequirementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.ogapgaRequirementsControl.TabIndex = 0;
			// 
			// PGAFDATabPage
			// 
			this.pGAFDATabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2fcc6d11-bed5-45b3-8cc5-a379d5ace779", "PGA FDA");
			this.pGAFDATabPage.Controls.Add(this.pGAFDAGroupBox);
			this.pGAFDATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.pGAFDATabPage.Name = "PGAFDATabPage";
			this.pGAFDATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.pGAFDATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.pGAFDATabPage.TabIndex = 1;
			this.pGAFDATabPage.Text = "PGA FDA";
			this.pGAFDATabPage.UseVisualStyleBackColor = true;
			// 
			// PGAFDAGroupBox
			// 
			this.pGAFDAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("55d9b148-c423-43ae-acd9-1d2a7406ef2f", "FDA Lines");
			this.pGAFDAGroupBox.Controls.Add(this.acefdaUserControl);
			this.pGAFDAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pGAFDAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.pGAFDAGroupBox.Name = "PGAFDAGroupBox";
			this.pGAFDAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.pGAFDAGroupBox.TabIndex = 18;
			this.pGAFDAGroupBox.TabStop = false;
			this.pGAFDAGroupBox.Text = "FDA Lines";
			// 
			// acefdaUserControl
			// 
			this.acefdaUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.acefdaUserControl, "ACEFDAs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ACEFDACollection)(((CusClassPartPivot)(null)).ACEFDAs)));
			this.acefdaUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.acefdaUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.acefdaUserControl.Name = "acefdaUserControl";
			this.acefdaUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 250, true);
			this.acefdaUserControl.TabIndex = 17;
			// 
			// NHTSATabPage
			// 
			this.nHTSATabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6b48830d-5288-44a0-8277-c3df5787449a", "NHTSA");
			this.nHTSATabPage.Controls.Add(this.nhtsaUserControl);
			this.nHTSATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.nHTSATabPage.Name = "NHTSATabPage";
			this.nHTSATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.nHTSATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.nHTSATabPage.TabIndex = 11;
			this.nHTSATabPage.Text = "NHTSA";
			this.nHTSATabPage.UseVisualStyleBackColor = true;
			// 
			// nhtsaUserControl
			// 
			this.nhtsaUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nhtsaUserControl, "NHTSALines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NHTSAHeaderCollection)(((CusClassPartPivot)(null)).NHTSALines)));
			this.nhtsaUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nhtsaUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.nhtsaUserControl.Name = "nhtsaUserControl";
			this.nhtsaUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.nhtsaUserControl.TabIndex = 0;
			// 

			//
			// DDTCTabPage
			//
			this.DDTCTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5EAFE5FD-DBB7-4130-9B15-29FB4D6C48BE", "DDTC");
			this.DDTCTabPage.Controls.Add(this.ddtcUserControl);
			this.DDTCTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DDTCTabPage.Name = "DDTCTabPage";
			this.DDTCTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DDTCTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.DDTCTabPage.TabIndex = 12;
			this.DDTCTabPage.Text = "DDTC";
			this.DDTCTabPage.UseVisualStyleBackColor = true;
			// 
			// ddtcUserControl
			// 
			this.ddtcUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ddtcUserControl, "."); //
																			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
																			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.CusUSClassification)(((Enterprise.Customs.US.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.US.Business.OrgSupplierPart)(null)).Pivots)).SyncRoot)))));
			this.ddtcUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ddtcUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ddtcUserControl.Name = "ddtcUserControl";
			this.ddtcUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 271, true);
			this.ddtcUserControl.TabIndex = 0;

			// LaceyTabPage
			// 
			this.laceyTabPage.Controls.Add(this.laceyLeftPanel);
			this.laceyTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.laceyTabPage.Name = "LaceyTabPage";
			this.laceyTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.laceyTabPage.TabIndex = 6;
			this.laceyTabPage.Text = "Lacey";
			// 
			// LaceyLeftPanel
			// 
			this.laceyLeftPanel.Controls.Add(this.laceyUserControl);
			this.laceyLeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.laceyLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.laceyLeftPanel.Name = "LaceyLeftPanel";
			this.laceyLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.laceyLeftPanel.TabIndex = 17;
			// 
			// laceyUserControl
			// 
			this.laceyUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.laceyUserControl, "PGAs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PGACollection)(((CusClassPartPivot)(null)).PGAs)));
			this.laceyUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.laceyUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.laceyUserControl.Name = "laceyUserControl";
			this.laceyUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.laceyUserControl.TabIndex = 0;
			// 
			// ODSTSCATabPage
			// 
			this.oDSTSCATabPage.Controls.Add(this.odsAndTSCAControl);
			this.oDSTSCATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.oDSTSCATabPage.Name = "ODSTSCATabPage";
			this.oDSTSCATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.oDSTSCATabPage.TabIndex = 12;
			this.oDSTSCATabPage.Text = "ODS/TSCA";
			// 
			// odsAndTSCAControl
			// 
			this.odsAndTSCAControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.odsAndTSCAControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.odsAndTSCAControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.odsAndTSCAControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.odsAndTSCAControl.Name = "odsAndTSCAControl";
			this.odsAndTSCAControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.odsAndTSCAControl.TabIndex = 0;
			// 
			// VNETabPage
			// 
			this.vNETabPage.Controls.Add(this.vneUserControl);
			this.vNETabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.vNETabPage.Name = "VNETabPage";
			this.vNETabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.vNETabPage.TabIndex = 13;
			this.vNETabPage.Text = "VNE";
			// 
			// vneUserControl
			// 
			this.vneUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vneUserControl, "VehicleLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((VehicleCollection)(((CusClassPartPivot)(null)).VehicleLines)));
			this.vneUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vneUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.vneUserControl.Name = "vneUserControl";
			this.vneUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.vneUserControl.TabIndex = 0;
			// 
			// PSTTabPage
			// 
			this.pSTTabPage.Controls.Add(this.pstUserControl);
			this.pSTTabPage.Controls.Add(this.pstDisclaimControl);
			this.pSTTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.pSTTabPage.Name = "PSTTabPage";
			this.pSTTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.pSTTabPage.TabIndex = 14;
			this.pSTTabPage.Text = "PST";
			// 
			// pstUserControl
			// 
			this.pstUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pstUserControl, "PSTLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PesticideCollection)(((CusClassPartPivot)(null)).PSTLines)));
			this.pstUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pstUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pstUserControl.Name = "pstUserControl";
			this.pstUserControl.Visible = false;
			this.pstUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.pstUserControl.TabIndex = 0;
			// 
			// pstDisclaimControl
			// 
			this.pstDisclaimControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pstDisclaimControl, ".");
			this.pstDisclaimControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pstDisclaimControl.Name = "pstDisclaimControl";
			this.pstDisclaimControl.Visible = false;
			this.pstDisclaimControl.TabIndex = 0;
			// 
			// hfcTabPage
			//
			this.hfcTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("260751E7-8607-4065-B882-E23E8359E534", "HFC");
			this.hfcTabPage.Controls.Add(this.hfcUserControl);
			this.hfcTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.hfcTabPage.Name = "HFCTabPage";
			this.hfcTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.hfcTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.hfcTabPage.TabIndex = 15;
			this.hfcTabPage.Text = "HFC";
			this.hfcTabPage.UseVisualStyleBackColor = true;
			// 
			// hfcUserControl
			// 
			this.hfcUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.hfcUserControl, "HFCHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((USHFCHeaderCollection)(((CusClassPartPivot)(null)).HFCHeaders)));
			this.hfcUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hfcUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.hfcUserControl.Name = "HFCUserControl";
			this.hfcUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 272, true);
			this.hfcUserControl.TabIndex = 0;
			// 
			// ATFTabPage
			// 
			this.aTFTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DB6EBB5C-288E-4CAC-8278-DBC68104859B", "ATF");
			this.aTFTabPage.Controls.Add(this.atfUserControl);
			this.aTFTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.aTFTabPage.Name = "ATFTabPage";
			this.aTFTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.aTFTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.aTFTabPage.TabIndex = 15;
			this.aTFTabPage.Text = "ATF";
			this.aTFTabPage.UseVisualStyleBackColor = true;
			// 
			// atfUserControl
			// 
			this.atfUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.atfUserControl, "ATFLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ATFCollection)(((CusClassPartPivot)(null)).ATFLines)));
			this.atfUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.atfUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.atfUserControl.Name = "atfUserControl";
			this.atfUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.atfUserControl.TabIndex = 0;
			// 
			// AMSTabPage
			// 
			this.aMSTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CFF60094-290A-4835-8A99-DB0453B3427D", "AMS");
			this.aMSTabPage.Controls.Add(this.amsUserControl);
			this.aMSTabPage.Controls.Add(this.amsDisclaimControl);
			this.aMSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.aMSTabPage.Name = "AMSTabPage";
			this.aMSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.aMSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.aMSTabPage.TabIndex = 16;
			this.aMSTabPage.Text = "AMS";
			this.aMSTabPage.UseVisualStyleBackColor = true;
			// 
			// amsUserControl
			// 
			this.amsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.amsUserControl, "AMSLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((AMSCollection)(((CusClassPartPivot)(null)).AMSLines)));
			this.amsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.amsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.amsUserControl.Name = "amsUserControl";
			this.amsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.amsUserControl.TabIndex = 0;
			this.amsUserControl.Visible = false;
			this.amsUserControl.Tag = "MO1";
			// 
			// amsDisclaimControl
			// 
			this.amsDisclaimControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.amsDisclaimControl, ".");
			this.amsDisclaimControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.amsDisclaimControl.Name = "amsDisclaimControl";
			this.amsDisclaimControl.TabIndex = 0;
			this.amsDisclaimControl.Visible = false;
			// 
			// TTBTabPage
			// 
			this.tTBTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("FF2B94EC-88BF-4781-BB70-96131C445191", "TTB");
			this.tTBTabPage.Controls.Add(this.ttbUserControl);
			this.tTBTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.tTBTabPage.Name = "TTBTabPage";
			this.tTBTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tTBTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.tTBTabPage.TabIndex = 16;
			this.tTBTabPage.Text = "TTB";
			this.tTBTabPage.UseVisualStyleBackColor = true;
			// 
			// ttbUserControl
			// 
			this.ttbUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ttbUserControl, "TTBLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TTBLineCollection)(((CusClassPartPivot)(null)).TTBLines)));
			this.ttbUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ttbUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ttbUserControl.Name = "ttbUserControl";
			this.ttbUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.ttbUserControl.TabIndex = 0;
			// 
			// FWSTabPage
			// 
			this.fWSTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6A3B2D37-BFDA-4FF1-B0E8-47A6C4B6C3E9", "FWS");
			this.fWSTabPage.Controls.Add(this.fwsUserControl);
			this.fWSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.fWSTabPage.Name = "FWSTabPage";
			this.fWSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.fWSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.fWSTabPage.TabIndex = 16;
			this.fWSTabPage.Text = "FWS";
			this.fWSTabPage.UseVisualStyleBackColor = true;
			// 
			// fwsUserControl
			// 
			this.fwsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.fwsUserControl, "FWSLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((FWSHeaderCollection)(((CusClassPartPivot)(null)).FWSLines)));
			this.fwsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fwsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.fwsUserControl.Name = "fwsUserControl";
			this.fwsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 266, true);
			this.fwsUserControl.TabIndex = 0;
			// 
			// NMFSTabPage
			// 
			this.nMFSTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4B44D2CD-92F8-489D-943D-81534284AD7E", "NMFS");
			this.nMFSTabPage.Controls.Add(this.nmfsUserControl);
			this.nMFSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.nMFSTabPage.Name = "NMFSTabPage";
			this.nMFSTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.nMFSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.nMFSTabPage.TabIndex = 16;
			this.nMFSTabPage.Text = "NMFS";
			this.nMFSTabPage.UseVisualStyleBackColor = true;
			// 
			// nmfsUserControl
			// 
			this.nmfsUserControl.AllowDrop = true;
			this.nmfsUserControl.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nmfsUserControl, "NMFSLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NMFSLineCollection)(((CusClassPartPivot)(null)).NMFSLines)));
			this.nmfsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.nmfsUserControl.Name = "nmfsUserControl";
			this.nmfsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 266, true);
			this.nmfsUserControl.TabIndex = 0;
			// 
			// CPSCTabPage
			// 
			this.cPSCTabPage.Controls.Add(this.cPSCControl);
			this.cPSCTabPage.Controls.Add(this.cPSCDisclaimPivotControl);
			this.cPSCTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.cPSCTabPage.Name = "CPSCTabPage";
			this.cPSCTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.cPSCTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.cPSCTabPage.TabIndex = 17;
			this.cPSCTabPage.Text = "CPSC";
			this.cPSCTabPage.UseVisualStyleBackColor = true;
			// 
			// CPSCControl
			// 
			this.cPSCControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cPSCControl, "CPSCLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CPSCHeaderCollection)(((CusClassPartPivot)(null)).CPSCLines)));
			this.cPSCControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cPSCControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cPSCControl.Name = "CPSCControl";
			this.cPSCControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.cPSCControl.TabIndex = 1;
			// 
			// CPSCDisclaimPivotControl
			// 
			this.cPSCDisclaimPivotControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cPSCDisclaimPivotControl, "CPSCLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CPSCHeaderCollection)(((CusClassPartPivot)(null)).CPSCLines)));
			this.cPSCDisclaimPivotControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cPSCDisclaimPivotControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cPSCDisclaimPivotControl.Name = "CPSCDisclaimPivotControl";
			this.cPSCDisclaimPivotControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 200, true);
			this.cPSCDisclaimPivotControl.TabIndex = 1;
			this.cPSCDisclaimPivotControl.Visible = false;
			// 
			// OMCTabPage
			// 
			this.oMCTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("F00E123D-49D3-4F1F-99D2-31118760A16A", "OMC");
			this.oMCTabPage.Controls.Add(this.oMCControl);
			this.oMCTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.oMCTabPage.Name = "OMCTabPage";
			this.oMCTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.oMCTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.oMCTabPage.TabIndex = 17;
			this.oMCTabPage.Text = "OMC";
			this.oMCTabPage.UseVisualStyleBackColor = true;
			// 
			// OMCControl
			// 
			this.oMCControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.oMCControl, "OMCHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OMCHeaderCollection)(((CusClassPartPivot)(null)).OMCHeaders)));
			this.oMCControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.oMCControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.oMCControl.Name = "OMCControl";
			this.oMCControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.oMCControl.TabIndex = 1;
			// 
			// DEATabPage
			// 
			this.DEATabPage.Controls.Add(this.DEAControl);
			this.DEATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DEATabPage.Name = "DEATabPage";
			this.DEATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DEATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.DEATabPage.TabIndex = 18;
			this.DEATabPage.Text = "DEA";
			this.DEATabPage.UseVisualStyleBackColor = true;
			// 
			// DEAControl
			// 
			this.DEAControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DEAControl, "DEAHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DEAHeaderCollection)(((CusClassPartPivot)(null)).DEAHeaders)));
			this.DEAControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DEAControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DEAControl.Name = "DEAControl";
			this.DEAControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.DEAControl.TabIndex = 1;
			// 
			// APHISTabPage
			// 
			this.aPHISTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.aPHISTabPage.Controls.Add(this.aphisUserControl);
			this.aPHISTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.aPHISTabPage.Name = "APHISTabPage";
			this.aPHISTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.aPHISTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 277, true);
			this.aPHISTabPage.TabIndex = 19;
			this.aPHISTabPage.Text = "APHIS";
			// 
			// aphisUserControl
			// 
			this.aphisUserControl.AllowDrop = true;
			this.aphisUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.aphisUserControl, "APHISHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((APHISHeaderCollection)(((CusClassPartPivot)(null)).APHISHeaders)));
			this.aphisUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aphisUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.aphisUserControl.Name = "aphisUserControl";
			this.aphisUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 267, true);
			this.aphisUserControl.TabIndex = 0;
			// 
			// permitLicenseBottomPanel
			//
			this.permitLicenseBottomPanel.Controls.Add(this.aDDCVDMiscGroupBox);
			this.permitLicenseBottomPanel.Controls.Add(this.steelAluminumPanel);
			this.permitLicenseBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.permitLicenseBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 205, true);
			this.permitLicenseBottomPanel.Name = "permitLicenseBottomPanel";
			this.permitLicenseBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 170, true);
			this.permitLicenseBottomPanel.TabIndex = 10;
			// 
			// steelAluminumPanel
			//
			this.steelAluminumPanel.Controls.Add(this.aluminumSmeltGroupBox);
			this.steelAluminumPanel.Controls.Add(this.steelIronGroupBox);
			this.steelAluminumPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 85, true);
			this.steelAluminumPanel.Name = "steelAluminumPanel";
			this.steelAluminumPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.steelAluminumPanel.TabIndex = 2;
			this.steelAluminumPanel.TabStop = false;
			// 
			// aluminumSmeltGroupBox
			// 
			this.aluminumSmeltGroupBox.Controls.Add(this.primaryCountryNotApplicableCheckBox);
			this.aluminumSmeltGroupBox.Controls.Add(this.primaryCountryCodeFindBox);
			this.aluminumSmeltGroupBox.Controls.Add(this.secondaryCountryNotApplicableCheckBox);
			this.aluminumSmeltGroupBox.Controls.Add(this.secondaryCountryCodeFindBox);
			this.aluminumSmeltGroupBox.Controls.Add(this.castCountryCodeFindBox);
			this.aluminumSmeltGroupBox.Name = "aluminumSmeltGroupBox";
			this.aluminumSmeltGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.aluminumSmeltGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 80, true);
			this.aluminumSmeltGroupBox.TabIndex = 1;
			this.aluminumSmeltGroupBox.TabStop = false;
			this.aluminumSmeltGroupBox.Text = "Aluminum Smelt and Cast Country Details";
			// 
			// steelIronGroupBox
			// 
			this.steelIronGroupBox.Controls.Add(this.certificateOfOriginFindBox);
			this.steelIronGroupBox.Controls.Add(this.meltedCountryFindBox);
			this.steelIronGroupBox.Name = "SteelIronGroupBox";
			this.steelIronGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 3, true);
			this.steelIronGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 80, true);
			this.steelIronGroupBox.TabIndex = 2;
			this.steelIronGroupBox.TabStop = false;
			this.steelIronGroupBox.Text = "Steel/Iron Details";
			// 
			// primaryCountryNotApplicableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.primaryCountryNotApplicableCheckBox, "CD_PrimaryCountryNA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_PrimaryCountryNA)));
			this.primaryCountryNotApplicableCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|ad58fd63-50ab-4e6b-9806-cdab99887389", "Primary Country/Region N/A");
			this.primaryCountryNotApplicableCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.primaryCountryNotApplicableCheckBox.EnableValidStateColor = false;
			this.primaryCountryNotApplicableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.primaryCountryNotApplicableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.primaryCountryNotApplicableCheckBox.Name = "primaryCountryNotApplicableCheckBox";
			this.primaryCountryNotApplicableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 18, true);
			this.primaryCountryNotApplicableCheckBox.TabIndex = 1;
			this.primaryCountryNotApplicableCheckBox.UseVisualStyleBackColor = true;
			// 
			// primaryCountryCodeFindBox
			// 
			this.primaryCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.primaryCountryCodeFindBox, "CD_RN_NKPrimaryCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_RN_NKPrimaryCountry)));
			this.primaryCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|936c4572-e8f7-4053-87e9-190fc9f7b827", "Primary Country/Region");
			this.primaryCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 20, true);
			this.primaryCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.primaryCountryCodeFindBox.Name = "primaryCountryCodeFindBox";
			this.primaryCountryCodeFindBox.PopupCaption = null;
			this.primaryCountryCodeFindBox.PreBoundMaxLength = 2;
			this.primaryCountryCodeFindBox.ShowDescriptionBox = false;
			this.primaryCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 17, true);
			this.primaryCountryCodeFindBox.TabIndex = 2;
			// 
			// secondaryCountryNotApplicableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.secondaryCountryNotApplicableCheckBox, "CD_SecondaryCountryNA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(null)).CD_SecondaryCountryNA)));
			this.secondaryCountryNotApplicableCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|d4c76c91-0589-405f-ad95-56145036291f", "Secondary Country/Region N/A");
			this.secondaryCountryNotApplicableCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.secondaryCountryNotApplicableCheckBox.EnableValidStateColor = false;
			this.secondaryCountryNotApplicableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.secondaryCountryNotApplicableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 40, true);
			this.secondaryCountryNotApplicableCheckBox.Name = "secondaryCountryNotApplicableCheckBox";
			this.secondaryCountryNotApplicableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 18, true);
			this.secondaryCountryNotApplicableCheckBox.TabIndex = 3;
			this.secondaryCountryNotApplicableCheckBox.UseVisualStyleBackColor = true;
			// 
			// secondaryCountryCodeFindBox
			// 
			this.secondaryCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.secondaryCountryCodeFindBox, "CD_RN_NKSecondaryCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_RN_NKSecondaryCountry)));
			this.secondaryCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|e24ea65b-b669-468d-84a7-571cabeb29bb", "Secondary Country/Region");
			this.secondaryCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 40, true);
			this.secondaryCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.secondaryCountryCodeFindBox.Name = "secondaryCountryCodeFindBox";
			this.secondaryCountryCodeFindBox.PopupCaption = null;
			this.secondaryCountryCodeFindBox.PreBoundMaxLength = 2;
			this.secondaryCountryCodeFindBox.ShowDescriptionBox = false;
			this.secondaryCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 17, true);
			this.secondaryCountryCodeFindBox.TabIndex = 4;
			// 
			// castCountryCodeFindBox
			// 
			this.castCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.castCountryCodeFindBox, "CD_RN_NKCastCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_RN_NKCastCountry)));
			this.castCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportClassificationUserControl|e7174a36-5f33-4d5a-995b-64355e8962d4", "Country/Region of Cast");
			this.castCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 60, true);
			this.castCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.castCountryCodeFindBox.Name = "castCountryCodeFindBox";
			this.castCountryCodeFindBox.PopupCaption = null;
			this.castCountryCodeFindBox.PreBoundMaxLength = 2;
			this.castCountryCodeFindBox.ShowDescriptionBox = false;
			this.castCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 17, true);
			this.castCountryCodeFindBox.TabIndex = 5;
			// 
			// certificateOfOriginFindBox
			// 
			this.certificateOfOriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.certificateOfOriginFindBox, "CD_RN_NKCertificateOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_RN_NKCertificateOrigin)));
			this.certificateOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
			this.certificateOfOriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.certificateOfOriginFindBox.Name = "certificateOfOriginFindBox";
			this.certificateOfOriginFindBox.PopupCaption = null;
			this.certificateOfOriginFindBox.PreBoundMaxLength = 2;
			this.certificateOfOriginFindBox.ShowDescriptionBox = false;
			this.certificateOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 17, true);
			this.certificateOfOriginFindBox.TabIndex = 1;
			// 
			// meltedCountryFindBox
			// 
			this.meltedCountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.meltedCountryFindBox, "CD_RN_NKMeltCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(null)).CD_RN_NKMeltCountry)));
			this.meltedCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 40, true);
			this.meltedCountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.meltedCountryFindBox.Name = "meltedCountryFindBox";
			this.meltedCountryFindBox.PopupCaption = null;
			this.meltedCountryFindBox.PreBoundMaxLength = 2;
			this.meltedCountryFindBox.ShowDescriptionBox = false;
			this.meltedCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 17, true);
			this.meltedCountryFindBox.TabIndex = 2;
			// 
			// ImportClassificationUserControl
			// 
			this.Controls.Add(this.ImportTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 0, true);
			this.Name = "ImportClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 299, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImportTabControl.ResumeLayout(false);
			this.ImportTabControl.PerformLayout();
			this.detailsTabPage.ResumeLayout(false);
			this.detailsTabPage.PerformLayout();
			this.reconIndicatorDropEdit.ResumeLayout(true);
			this.reconIndicatorDropEdit.PerformLayout();
			this.ttbRateDesignationCodeDropEdit.ResumeLayout(true);
			this.ttbRateDesignationCodeDropEdit.PerformLayout();
			this.cbmaDefaultTaxRateCalcEdit.ResumeLayout(true);
			this.cbmaDefaultTaxRateCalcEdit.PerformLayout();
			this.classificationDescriptionTextBox.ResumeLayout(true);
			this.classificationDescriptionTextBox.PerformLayout();
			this.countryOfExportCodeBox.ResumeLayout(true);
			this.countryOfExportCodeBox.PerformLayout();
			this.rateTypeDropEdit.ResumeLayout(true);
			this.rateTypeDropEdit.PerformLayout();
			this.taxRateSDropEdit.ResumeLayout(true);
			this.taxRateSDropEdit.PerformLayout();
			this.taxCodeDropEdit.ResumeLayout(true);
			this.taxCodeDropEdit.PerformLayout();
			this.flavorContentCreditIndicatorCheckBox.ResumeLayout(true);
			this.flavorContentCreditIndicatorCheckBox.PerformLayout();
			this.taxApplyDropEdit.ResumeLayout(true);
			this.taxApplyDropEdit.PerformLayout();
			this.cD_98InvValueCalcFindBox.ResumeLayout(true);
			this.cD_98InvValueCalcFindBox.PerformLayout();
			this.unitCalcFindBox.ResumeLayout(true);
			this.unitCalcFindBox.PerformLayout();
			this.ammvUnitCalcFindBox.ResumeLayout(true);
			this.ammvUnitCalcFindBox.PerformLayout();
			this.uS_UC_NKCountryOfOriginCodeFindBox.ResumeLayout(true);
			this.uS_UC_NKCountryOfOriginCodeFindBox.PerformLayout();
			this.sPIProgramDropEdit.ResumeLayout(true);
			this.sPIProgramDropEdit.PerformLayout();
			this.productClaimSetsDropEdit.ResumeLayout(true);
			this.productClaimSetsDropEdit.PerformLayout();
			this.LicenceNoTabPage.ResumeLayout(false);
			this.LicenceNoTabPage.PerformLayout();
			this.aDDCVDMiscGroupBox.ResumeLayout(false);
			this.aDDCVDMiscGroupBox.PerformLayout();
			this.nonReimburseDecDropEdit.ResumeLayout(true);
			this.nonReimburseDecDropEdit.PerformLayout();
			this.cVDCodeFindBox.ResumeLayout(true);
			this.cVDCodeFindBox.PerformLayout();
			this.cVDDepositRateDropEdit.ResumeLayout(true);
			this.cVDDepositRateDropEdit.PerformLayout();
			this.aDDDepositRateDropEdit.ResumeLayout(true);
			this.aDDDepositRateDropEdit.PerformLayout();
			this.aDDDepositRateZTextBox.ResumeLayout(true);
			this.aDDDepositRateZTextBox.PerformLayout();
			this.cVDDepositRateZTextBox.ResumeLayout(true);
			this.cVDDepositRateZTextBox.PerformLayout();
			this.aDDCodeFindBox.ResumeLayout(true);
			this.aDDCodeFindBox.PerformLayout();
			this.pIRPRulingTypeDropEdit.ResumeLayout(true);
			this.pIRPRulingTypeDropEdit.PerformLayout();
			this.productExclusionDropEdit.ResumeLayout(true);
			this.productExclusionDropEdit.PerformLayout();
			this.exclusionNumberTextBox.ResumeLayout(true);
			this.exclusionNumberTextBox.PerformLayout();
			this.uS_CottonFeeExemptDropEdit.ResumeLayout(true);
			this.uS_CottonFeeExemptDropEdit.PerformLayout();
			this.otherTabPage.ResumeLayout(false);
			this.otherTabPage.PerformLayout();
			this.tSCAIndicatorDropEdit.ResumeLayout(true);
			this.tSCAIndicatorDropEdit.PerformLayout();
			this.manufacturerAddressControl.ResumeLayout(true);
			this.manufacturerAddressControl.PerformLayout();
			this.exporterAddressControl.ResumeLayout(true);
			this.exporterAddressControl.PerformLayout();
			this.zoneStatusDropEdit.ResumeLayout(true);
			this.zoneStatusDropEdit.PerformLayout();
			this.cWTabPage.ResumeLayout(false);
			this.cWTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.cWOGrid)).EndInit();
			this.cWOGrid.ResumeLayout(false);
			this.cWOGrid.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.attributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.attributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.attributesLeftSplitContainer)).EndInit();
			this.attributesLeftSplitContainer.ResumeLayout(false);
			this.attributesLeftSplitContainer.PerformLayout();
			this.attribute1GroupBox.ResumeLayout(false);
			this.attribute1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes1Grid)).EndInit();
			this.attributes1Grid.ResumeLayout(false);
			this.attributes1Grid.PerformLayout();
			this.attributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.attributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.attributesRightSplitContainer)).EndInit();
			this.attributesRightSplitContainer.ResumeLayout(false);
			this.attributesRightSplitContainer.PerformLayout();
			this.attributes2GroupBox.ResumeLayout(false);
			this.attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes2Grid)).EndInit();
			this.attributes2Grid.ResumeLayout(false);
			this.attributes2Grid.PerformLayout();
			this.attributes3GroupBox.ResumeLayout(false);
			this.attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attributes3Grid)).EndInit();
			this.attributes3Grid.ResumeLayout(false);
			this.attributes3Grid.PerformLayout();
			this.oGAPGATabPage.ResumeLayout(false);
			this.oGAPGATabPage.PerformLayout();
			this.ogapgaRequirementsControl.ResumeLayout(true);
			this.ogapgaRequirementsControl.PerformLayout();
			this.pGAFDATabPage.ResumeLayout(false);
			this.pGAFDATabPage.PerformLayout();
			this.pGAFDAGroupBox.ResumeLayout(false);
			this.pGAFDAGroupBox.PerformLayout();
			this.acefdaUserControl.ResumeLayout(true);
			this.acefdaUserControl.PerformLayout();
			this.nHTSATabPage.ResumeLayout(false);
			this.nHTSATabPage.PerformLayout();
			this.nhtsaUserControl.ResumeLayout(true);
			this.nhtsaUserControl.PerformLayout();
			this.laceyTabPage.ResumeLayout(false);
			this.laceyTabPage.PerformLayout();
			this.laceyLeftPanel.ResumeLayout(false);
			this.laceyLeftPanel.PerformLayout();
			this.laceyUserControl.ResumeLayout(true);
			this.laceyUserControl.PerformLayout();
			this.oDSTSCATabPage.ResumeLayout(false);
			this.oDSTSCATabPage.PerformLayout();
			this.odsAndTSCAControl.ResumeLayout(true);
			this.odsAndTSCAControl.PerformLayout();
			this.vNETabPage.ResumeLayout(false);
			this.vNETabPage.PerformLayout();
			this.vneUserControl.ResumeLayout(true);
			this.vneUserControl.PerformLayout();
			this.pSTTabPage.ResumeLayout(false);
			this.pSTTabPage.PerformLayout();
			this.pstUserControl.ResumeLayout(true);
			this.pstUserControl.PerformLayout();
			this.pstDisclaimControl.ResumeLayout(true);
			this.pstDisclaimControl.PerformLayout();
			this.hfcTabPage.ResumeLayout(false);
			this.hfcTabPage.PerformLayout();
			this.hfcUserControl.ResumeLayout(true);
			this.hfcUserControl.PerformLayout();
			this.aTFTabPage.ResumeLayout(false);
			this.aTFTabPage.PerformLayout();
			this.atfUserControl.ResumeLayout(true);
			this.atfUserControl.PerformLayout();
			this.aMSTabPage.ResumeLayout(false);
			this.aMSTabPage.PerformLayout();
			this.amsUserControl.ResumeLayout(true);
			this.amsUserControl.PerformLayout();
			this.amsDisclaimControl.ResumeLayout(true);
			this.amsDisclaimControl.PerformLayout();
			this.tTBTabPage.ResumeLayout(false);
			this.tTBTabPage.PerformLayout();
			this.ttbUserControl.ResumeLayout(true);
			this.ttbUserControl.PerformLayout();
			this.fWSTabPage.ResumeLayout(false);
			this.fWSTabPage.PerformLayout();
			this.fwsUserControl.ResumeLayout(true);
			this.fwsUserControl.PerformLayout();
			this.nMFSTabPage.ResumeLayout(false);
			this.nMFSTabPage.PerformLayout();
			this.nmfsUserControl.ResumeLayout(true);
			this.nmfsUserControl.PerformLayout();
			this.cPSCTabPage.ResumeLayout(false);
			this.cPSCTabPage.PerformLayout();
			this.cPSCControl.ResumeLayout(true);
			this.cPSCControl.PerformLayout();
			this.cPSCDisclaimPivotControl.ResumeLayout(true);
			this.cPSCDisclaimPivotControl.PerformLayout();
			this.oMCTabPage.ResumeLayout(false);
			this.oMCTabPage.PerformLayout();
			this.oMCControl.ResumeLayout(true);
			this.oMCControl.PerformLayout();
			this.DEATabPage.ResumeLayout(false);
			this.DEATabPage.PerformLayout();
			this.DEAControl.ResumeLayout(true);
			this.DEAControl.PerformLayout();
			this.aPHISTabPage.ResumeLayout(false);
			this.aPHISTabPage.PerformLayout();
			this.aphisUserControl.ResumeLayout(true);
			this.aphisUserControl.PerformLayout();
			this.permitLicenseBottomPanel.ResumeLayout(false);
			this.permitLicenseBottomPanel.PerformLayout();
			this.aluminumSmeltGroupBox.ResumeLayout(false);
			this.aluminumSmeltGroupBox.PerformLayout();
			this.steelIronGroupBox.ResumeLayout(false);
			this.steelIronGroupBox.PerformLayout();
			this.steelAluminumPanel.ResumeLayout(false);
			this.steelAluminumPanel.PerformLayout();
			this.primaryCountryCodeFindBox.ResumeLayout(false);
			this.primaryCountryCodeFindBox.PerformLayout();
			this.secondaryCountryCodeFindBox.ResumeLayout(false);
			this.secondaryCountryCodeFindBox.PerformLayout();
			this.castCountryCodeFindBox.ResumeLayout(false);
			this.castCountryCodeFindBox.PerformLayout();
			this.certificateOfOriginFindBox.ResumeLayout(false);
			this.certificateOfOriginFindBox.PerformLayout();
			this.meltedCountryFindBox.ResumeLayout(false);
			this.meltedCountryFindBox.PerformLayout();
			this.additionalTariffsTabPage.ResumeLayout(false);
			this.additionalTariffsTabPage.PerformLayout();
			this.additionalTariff1FindBox.ResumeLayout(false);
			this.additionalTariff1FindBox.PerformLayout();
			this.additionalTariff2FindBox.ResumeLayout(false);
			this.additionalTariff2FindBox.PerformLayout();
			this.additionalTariff3FindBox.ResumeLayout(false);
			this.additionalTariff3FindBox.PerformLayout();
			this.additionalTariff4FindBox.ResumeLayout(false);
			this.additionalTariff4FindBox.PerformLayout();
			this.additionalTariff5FindBox.ResumeLayout(false);
			this.additionalTariff5FindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
