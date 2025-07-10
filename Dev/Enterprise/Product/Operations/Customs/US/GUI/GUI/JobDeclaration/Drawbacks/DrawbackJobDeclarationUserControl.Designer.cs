using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class DrawbackJobDeclarationUserControl
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

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!CustomFieldsDisplayControl.IsDisposed)
			{
				CustomFieldsDisplayControl.ForceBindingIncludingParents();
			}
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ImporterOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.ACSEntryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EnableMergeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrintSubTotalsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RejectedReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DRWBrokerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TotalClaimAmount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalDutyClaimAmount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalOtherFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalIRTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalHMFCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.US_TeamNoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_ClaimPortDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DrawbackSectionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FilingmethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_PreparerDistrictPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TotalPRDCCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalMPFCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PeriodToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PeriodFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NAFTADrawbackCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NAFTAClaimIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PetroliumClaimIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PreInspectionIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WaiverNoticeIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExporterSummaryIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AcceleratedClaimIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EarliestExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EstimatedEntryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.USMCAClaimIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportingCarrierFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TENoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IntendedPortOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ContractNumberszGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContractNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BondGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshBondButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.US_SuretyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_BondTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MiscOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DashLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EntryFilerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DrawbackDelcarationPurposeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransfereeOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.RulingNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertOfManufactureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransferorOrganizationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.AllocateImportEntryNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACSDrawbackPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ACSRightPanelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ACERightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ACENoticeOfIntentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACEIntendedPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ProcessorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProcessorDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ProcessorPhoneNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessorBadgeNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessorNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestructionResultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEExainationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExaminationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExaminationPhoneNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExaminationBadgeNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExaminationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ACELocationOfDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ACEBondGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACERefreshBondButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ACEWaivingReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEBondDesignationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEAccountNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ACEBondAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACESuretyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ACEBondTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEDrawbackPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ACEEntryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACEPeriodToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ACEPeriodFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ACEEarliestExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ACEEstimatedEntryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ACETotalOilSpillTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BillOfFormulaCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DestroyedValuationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RetailSalesSubstitutionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuperfundTaxCertificationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnUsedWineCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEDrawbackTeamDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEClaimPortDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEPrintSubTotalsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEEnableMergeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACENAFTADrawbackCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExaminationWitnessCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACENAFTAClaimCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OilSpillTaxIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ManufacturingPetroleumCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ElectronicPetroleumCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEWaiverNoticeIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OneTimeWaiverIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEAcceleratedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEBrokerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ACETotalClaimedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACETotalOtherFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACETotalDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACETotalIRTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACETotalMPFCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACETotalHMFCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACEMethodFilingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEDrawbackDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommercialRulingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DrawbackProvisionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACEProcessingPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomFieldsDisplayControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterOrganisationControl.SuspendLayout();
			this.ACSEntryGroupBox.SuspendLayout();
			this.RejectedReasonDropEdit.SuspendLayout();
			this.DRWBrokerGuidFindBox.SuspendLayout();
			this.US_TeamNoDropEdit.SuspendLayout();
			this.US_ClaimPortDropEdit.SuspendLayout();
			this.FilingmethodDropEdit.SuspendLayout();
			this.US_PreparerDistrictPortFindBox.SuspendLayout();
			this.PeriodToDateEdit.SuspendLayout();
			this.PeriodFromDateEdit.SuspendLayout();
			this.EntryTypeDropEdit.SuspendLayout();
			this.NAFTADrawbackCountryDropEdit.SuspendLayout();
			this.EarliestExportDateEdit.SuspendLayout();
			this.EstimatedEntryDateDateEdit.SuspendLayout();
			this.ExportingCarrierFindBox.SuspendLayout();
			this.IntendedPortOfExportCodeFindBox.SuspendLayout();
			this.ContractNumberszGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContractNumbersGrid)).BeginInit();
			this.ContractNumbersGrid.SuspendLayout();
			this.BondGroupBox.SuspendLayout();
			this.US_BondTypeDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.DrawbackDelcarationPurposeDropEdit.SuspendLayout();
			this.AdditionalDetailsGroupBox.SuspendLayout();
			this.TransfereeOrganisationControl.SuspendLayout();
			this.TransferorOrganizationControl.SuspendLayout();
			this.MessageTypeDropEdit.SuspendLayout();
			this.ACSDrawbackPanel.SuspendLayout();
			this.ACSRightPanelPanel.SuspendLayout();
			this.ACERightPanel.SuspendLayout();
			this.ACENoticeOfIntentGroupBox.SuspendLayout();
			this.ACEIntendedPortCodeFindBox.SuspendLayout();
			this.ProcessorGroupBox.SuspendLayout();
			this.ProcessorDateDateEdit.SuspendLayout();
			this.DestructionResultDropEdit.SuspendLayout();
			this.ACEExainationGroupBox.SuspendLayout();
			this.ExaminationDateEdit.SuspendLayout();
			this.ACEBondGroupBox.SuspendLayout();
			this.ACEWaivingReasonDropEdit.SuspendLayout();
			this.ACEBondDesignationCodeDropEdit.SuspendLayout();
			this.ACEBondTypeDropEdit.SuspendLayout();
			this.ACEDrawbackPanel.SuspendLayout();
			this.ACEEntryGroupBox.SuspendLayout();
			this.ACEPeriodToDateEdit.SuspendLayout();
			this.ACEPeriodFromDateEdit.SuspendLayout();
			this.ACEEarliestExportDateEdit.SuspendLayout();
			this.ACEEstimatedEntryDateDateEdit.SuspendLayout();
			this.ACEDrawbackTeamDropEdit.SuspendLayout();
			this.ACEClaimPortDropEdit.SuspendLayout();
			this.ACENAFTADrawbackCountryDropEdit.SuspendLayout();
			this.ACEBrokerGuidFindBox.SuspendLayout();
			this.ACEMethodFilingDropEdit.SuspendLayout();
			this.CommercialRulingDropEdit.SuspendLayout();
			this.DrawbackProvisionDropEdit.SuspendLayout();
			this.ACEProcessingPortCodeFindBox.SuspendLayout();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.CustomFieldsDisplayControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|c1eeb997-7fb0-4d2f-8d48-f8bb76786d88", "The Identifier of a Customs Document that represents a declaration to Customs by a Party concerning either Goods or Persons that may cross the border.");
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 17, true);
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.ExportDeclarationNumberBoundTextBox.TabIndex = 2;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Controls.Add(this.MessageTypeDropEdit);
			this.DeclarationDetailsGroupBox.Controls.Add(this.AllocateImportEntryNumberButton);
			this.DeclarationDetailsGroupBox.Controls.Add(this.DrawbackDelcarationPurposeDropEdit);
			this.DeclarationDetailsGroupBox.Controls.Add(this.DashLabel);
			this.DeclarationDetailsGroupBox.Controls.Add(this.EntryFilerTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.MessageStatusDescriptionTextBox);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 152, true);
			this.DeclarationDetailsGroupBox.TabIndex = 1;
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.MessageStatusDescriptionTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.EntryFilerTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.DashLabel, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.DrawbackDelcarationPurposeDropEdit, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.AllocateImportEntryNumberButton, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.MessageTypeDropEdit, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|f4f509d5-6346-4cf5-9a67-ab7e791adc6e", "Status", "Entry Status Description", "The current status of this Declaration Message.");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 45, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 20, true);
			this.StatusTextBox.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_Importer)));
			this.ImporterOrganisationControl.BindToMiscellaneousFields = "JE_ImporterMiscFields";
			this.ImporterOrganisationControl.BindToOrganisations = "Lookups.ImportersList";
			this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8816f992-c9e0-4dff-9ecf-593f9f86d438", "Drawback Claimant/Transferor");
			this.ImporterOrganisationControl.Captions = new string[] {
        "Drawback Claimant/Transferor"};
			this.ImporterOrganisationControl.IsCaptionOverridden = false;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ImporterOrganisationControl.TabIndex = 0;
			// 
			// ACSEntryGroupBox
			// 
			this.ACSEntryGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("18920a33-19c8-49f2-b75e-4648f81710fd", "Drawback Claim Details");
			this.ACSEntryGroupBox.Controls.Add(this.EnableMergeCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.PrintSubTotalsCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.RejectedReasonDropEdit);
			this.ACSEntryGroupBox.Controls.Add(this.DRWBrokerGuidFindBox);
			this.ACSEntryGroupBox.Controls.Add(this.TotalClaimAmount);
			this.ACSEntryGroupBox.Controls.Add(this.TotalDutyClaimAmount);
			this.ACSEntryGroupBox.Controls.Add(this.TotalOtherFeesCalcEdit);
			this.ACSEntryGroupBox.Controls.Add(this.TotalIRTaxCalcEdit);
			this.ACSEntryGroupBox.Controls.Add(this.TotalHMFCalcEdit);
			this.ACSEntryGroupBox.Controls.Add(this.US_TeamNoDropEdit);
			this.ACSEntryGroupBox.Controls.Add(this.US_ClaimPortDropEdit);
			this.ACSEntryGroupBox.Controls.Add(this.DrawbackSectionTextBox);
			this.ACSEntryGroupBox.Controls.Add(this.FilingmethodDropEdit);
			this.ACSEntryGroupBox.Controls.Add(this.US_PreparerDistrictPortFindBox);
			this.ACSEntryGroupBox.Controls.Add(this.TotalPRDCCalcEdit);
			this.ACSEntryGroupBox.Controls.Add(this.TotalMPFCalcEdit);
			this.ACSEntryGroupBox.Controls.Add(this.PeriodToDateEdit);
			this.ACSEntryGroupBox.Controls.Add(this.PeriodFromDateEdit);
			this.ACSEntryGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.ACSEntryGroupBox.Controls.Add(this.EntryTypeDropEdit);
			this.ACSEntryGroupBox.Controls.Add(this.NAFTADrawbackCountryDropEdit);
			this.ACSEntryGroupBox.Controls.Add(this.NAFTAClaimIndCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.PetroliumClaimIndCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.PreInspectionIndCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.WaiverNoticeIndCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.ExporterSummaryIndCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.AcceleratedClaimIndCheckBox);
			this.ACSEntryGroupBox.Controls.Add(this.EarliestExportDateEdit);
			this.ACSEntryGroupBox.Controls.Add(this.EstimatedEntryDateDateEdit);
			this.ACSEntryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ACSEntryGroupBox.Name = "ACSEntryGroupBox";
			this.ACSEntryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 434, true);
			this.ACSEntryGroupBox.TabIndex = 1;
			this.ACSEntryGroupBox.TabStop = false;
			this.ACSEntryGroupBox.Text = "Drawback Claim Details";
			// 
			// EnableMergeCheckBox
			// 
			this.EnableMergeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableMergeCheckBox, "US_DRWEnableMerge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWEnableMerge)));
			this.EnableMergeCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bf7dc5b2-5100-4950-874e-18fc7f1f6982", "Enable Merge");
			this.EnableMergeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableMergeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableMergeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 335, true);
			this.EnableMergeCheckBox.Name = "EnableMergeCheckBox";
			this.EnableMergeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.EnableMergeCheckBox.TabIndex = 28;
			this.EnableMergeCheckBox.UseVisualStyleBackColor = true;
			// 
			// PrintSubTotalsCheckBox
			// 
			this.PrintSubTotalsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintSubTotalsCheckBox, "US_DRWPrintSubTotals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWPrintSubTotals)));
			this.PrintSubTotalsCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("688c3ab8-8ca9-4e0d-be47-5fdc462cbe93", "Print Sub-Totals", "Print Sub-totals on 7551", "");
			this.PrintSubTotalsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PrintSubTotalsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintSubTotalsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 335, true);
			this.PrintSubTotalsCheckBox.Name = "PrintSubTotalsCheckBox";
			this.PrintSubTotalsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PrintSubTotalsCheckBox.TabIndex = 24;
			this.PrintSubTotalsCheckBox.UseVisualStyleBackColor = true;
			// 
			// RejectedReasonDropEdit
			// 
			this.RejectedReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RejectedReasonDropEdit, "US_DRWRejectedMerchandiseReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWRejectedMerchandiseReason)));
			this.RejectedReasonDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0bc75225-f9f2-43ec-a92d-1e6b6dd7a52f", "Rejected Merchandise Reason");
			this.RejectedReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 43, true);
			this.RejectedReasonDropEdit.Name = "RejectedReasonDropEdit";
			this.RejectedReasonDropEdit.ShouldResizeByMaxLength = true;
			this.RejectedReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.RejectedReasonDropEdit.TabIndex = 1;
			// 
			// DRWBrokerGuidFindBox
			// 
			this.DRWBrokerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DRWBrokerGuidFindBox, "JE_OH_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_NotifyParty)));
			this.DRWBrokerGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7da2e750-a604-42fd-9876-e753663df0d1", "4811 Party");
			this.DRWBrokerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 242, true);
			this.DRWBrokerGuidFindBox.Name = "DRWBrokerGuidFindBox";
			this.DRWBrokerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.DRWBrokerGuidFindBox.TabIndex = 15;
			// 
			// TotalClaimAmount
			// 
			this.TotalClaimAmount.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalClaimAmount, "TotalClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalClaimAmount)));
			this.TotalClaimAmount.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|71606261-fc43-4690-bb77-025929129d5a", "Tot Claim", "Total Claimed", "");
			this.TotalClaimAmount.DecimalPlaces = 2;
			this.TotalClaimAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 217, true);
			this.TotalClaimAmount.Name = "TotalClaimAmount";
			this.TotalClaimAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalClaimAmount.TabIndex = 14;
			this.TotalClaimAmount.Text = "0.00";
			this.TotalClaimAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalDutyClaimAmount
			// 
			this.TotalDutyClaimAmount.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalDutyClaimAmount, "TotalDutyClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalDutyClaimAmount)));
			this.TotalDutyClaimAmount.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|78cca598-e088-49c8-91e2-8cb489694270", "Tot Duty Claim", "Total Duty Claimed", "");
			this.TotalDutyClaimAmount.DecimalPlaces = 2;
			this.TotalDutyClaimAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 192, true);
			this.TotalDutyClaimAmount.Name = "TotalDutyClaimAmount";
			this.TotalDutyClaimAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalDutyClaimAmount.TabIndex = 12;
			this.TotalDutyClaimAmount.Text = "0.00";
			this.TotalDutyClaimAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalOtherFeesCalcEdit
			// 
			this.TotalOtherFeesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalOtherFeesCalcEdit, "TotalOtherClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalOtherClaimAmount)));
			this.TotalOtherFeesCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|351c5fff-0318-4489-81d8-649e6081a2e7", "Total Other Fees Claimed", "Total Other Fees Claimed", "Total Other Fees Claimed", "");
			this.TotalOtherFeesCalcEdit.DecimalPlaces = 2;
			this.TotalOtherFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 192, true);
			this.TotalOtherFeesCalcEdit.Name = "TotalOtherFeesCalcEdit";
			this.TotalOtherFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalOtherFeesCalcEdit.TabIndex = 11;
			this.TotalOtherFeesCalcEdit.Text = "0.00";
			this.TotalOtherFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalIRTaxCalcEdit
			// 
			this.TotalIRTaxCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalIRTaxCalcEdit, "TotalTaxClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalTaxClaimAmount)));
			this.TotalIRTaxCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|10b91f8a-2779-4b9c-a0bc-03be5cff5301", "Total I.R. Tax Claimed", "Total I.R. Tax Claimed", "Total I.R. Tax Claimed", "");
			this.TotalIRTaxCalcEdit.DecimalPlaces = 2;
			this.TotalIRTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 167, true);
			this.TotalIRTaxCalcEdit.Name = "TotalIRTaxCalcEdit";
			this.TotalIRTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalIRTaxCalcEdit.TabIndex = 10;
			this.TotalIRTaxCalcEdit.Text = "0.00";
			this.TotalIRTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalHMFCalcEdit
			// 
			this.TotalHMFCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalHMFCalcEdit, "TotalHMFClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalHMFClaimAmount)));
			this.TotalHMFCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|32fdba46-c5f5-48f6-99be-2b8be24c89ec", "Total HMF Claimed", "Total HMF Claimed", "Total HMF Claimed", "");
			this.TotalHMFCalcEdit.DecimalPlaces = 2;
			this.TotalHMFCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 142, true);
			this.TotalHMFCalcEdit.Name = "TotalHMFCalcEdit";
			this.TotalHMFCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalHMFCalcEdit.TabIndex = 8;
			this.TotalHMFCalcEdit.Text = "0.00";
			this.TotalHMFCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_TeamNoDropEdit
			// 
			this.US_TeamNoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_TeamNoDropEdit, "US_TeamNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TeamNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_TeamNoForDrawbackCodeList)));
			this.US_TeamNoDropEdit.BindToList = "AddInfoLookups+US_TeamNoForDrawbackCodeList";
			this.US_TeamNoDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|ba8ea544-d8ea-4fa7-8d21-15a633fbf0d0", "Drawback Team", "Drawback Team", "The valid Team Code associated with the Claim Port, see the pull down list.\r\n\r\n.");
			this.US_TeamNoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 410, true);
			this.US_TeamNoDropEdit.Name = "US_TeamNoDropEdit";
			this.US_TeamNoDropEdit.PreBoundMaxLength = 3;
			this.US_TeamNoDropEdit.ShouldResizeByMaxLength = true;
			this.US_TeamNoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.US_TeamNoDropEdit.TabIndex = 27;
			// 
			// US_ClaimPortDropEdit
			// 
			this.US_ClaimPortDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_ClaimPortDropEdit, "US_ClaimPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ClaimPort)));
			this.US_ClaimPortDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|b2557dc8-9455-4af7-a3e8-472833009c9a", "Claim Port", "Claim Port", "District/port where the claim is filed.  Must be one of the valid drawback sites, see the pull down list.");
			this.US_ClaimPortDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 384, true);
			this.US_ClaimPortDropEdit.Name = "US_ClaimPortDropEdit";
			this.US_ClaimPortDropEdit.PreBoundMaxLength = 4;
			this.US_ClaimPortDropEdit.ShouldResizeByMaxLength = true;
			this.US_ClaimPortDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.US_ClaimPortDropEdit.TabIndex = 26;
			// 
			// DrawbackSectionTextBox
			// 
			this.DrawbackSectionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DrawbackSectionTextBox, "US_DRWSection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWSection)));
			this.DrawbackSectionTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|12837390-d868-484d-b140-7cb384d9988d", "Drawback Section", "Drawback Section", "Drawback Section", "Indicates the appropriate section(s) of the law that your claim is filed under. Your claim may qualify for more than one provision, e.g. a combination 1313(a) and 1313(b), and 1313(p) can be added to a claim for (a) or (b). In addition, packaging, under section 1313(q) may be added to a claim for (a), (b), (c), (j), (p). All other sections of the law must be filed as a separate claim.");
			this.DrawbackSectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 268, true);
			this.DrawbackSectionTextBox.Name = "DrawbackSectionTextBox";
			this.DrawbackSectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.DrawbackSectionTextBox.TabIndex = 16;
			// 
			// FilingmethodDropEdit
			// 
			this.FilingmethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilingmethodDropEdit, "US_DRWFilingMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWFilingMethod)));
			this.FilingmethodDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|42149546-f75a-49f5-b857-344ad4552953", "Method", "Filing Method", "Method of Filing", "Method of Filing can be either Manual or ABI (Auto).");
			this.FilingmethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 142, true);
			this.FilingmethodDropEdit.Name = "FilingmethodDropEdit";
			this.FilingmethodDropEdit.PreBoundMaxLength = 1;
			this.FilingmethodDropEdit.ShouldResizeByMaxLength = true;
			this.FilingmethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FilingmethodDropEdit.TabIndex = 7;
			// 
			// US_PreparerDistrictPortFindBox
			// 
			this.US_PreparerDistrictPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_PreparerDistrictPortFindBox, "US_PreparerDistrictPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PreparerDistrictPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.RegionDistrictPorts)));
			this.US_PreparerDistrictPortFindBox.BindToList = "AddInfoLookups+RegionDistrictPorts";
			this.US_PreparerDistrictPortFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|35f9e9ad-a6ca-4b16-b33f-48647e0ef1cc", "License Port", "Any district/port within the region where the filer is actively permitted.  May be \"9900\" for National Permit Holders using National Permit.");
			this.US_PreparerDistrictPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 358, true);
			this.US_PreparerDistrictPortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.US_PreparerDistrictPortFindBox.Name = "US_PreparerDistrictPortFindBox";
			this.US_PreparerDistrictPortFindBox.PreBoundMaxLength = 4;
			this.US_PreparerDistrictPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.US_PreparerDistrictPortFindBox.TabIndex = 25;
			// 
			// TotalPRDCCalcEdit
			// 
			this.TotalPRDCCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalPRDCCalcEdit, "US_DRWTotalPRDC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWTotalPRDC)));
			this.TotalPRDCCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|334901cf-f8d7-4974-941f-6583ab0471a4", "Puerto Rico Drawback", "Puerto Rico Drawback", "Puerto Rico Drawback Claimed", "If you are listing import entries filed in Puerto Rico using port code 49NN, enter the 99% figure for those imports only.");
			this.TotalPRDCCalcEdit.DecimalPlaces = 2;
			this.TotalPRDCCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 217, true);
			this.TotalPRDCCalcEdit.Name = "TotalPRDCCalcEdit";
			this.TotalPRDCCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalPRDCCalcEdit.TabIndex = 13;
			this.TotalPRDCCalcEdit.Text = "0.00";
			this.TotalPRDCCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalMPFCalcEdit
			// 
			this.TotalMPFCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalMPFCalcEdit, "TotalMPFClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalMPFClaimAmount)));
			this.TotalMPFCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|2b2fca40-9df4-4765-8191-abe1595b4e27", "MPF", "Total MPF", "Total MPF Claimed", "If a Merchandise Processing Fee (MPF) is involved, enter the amount of MPF you have computed and are claiming as a refund. MPF claimed under drawback is refunded at 99%. The MPF can only be refunded on Unused Merchandise Drawback claims 1313(j)(1) and (j)(2). The top of the CBP7551 must be marked \"MPF Refund Requested\" by the claimant.");
			this.TotalMPFCalcEdit.DecimalPlaces = 2;
			this.TotalMPFCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 167, true);
			this.TotalMPFCalcEdit.Name = "TotalMPFCalcEdit";
			this.TotalMPFCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.TotalMPFCalcEdit.TabIndex = 9;
			this.TotalMPFCalcEdit.Text = "0.00";
			this.TotalMPFCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodToDateEdit
			// 
			this.PeriodToDateEdit.AllowDrop = true;
			this.PeriodToDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PeriodToDateEdit, "US_DRWDatePeriodTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWDatePeriodTo)));
			this.PeriodToDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|01269cd2-216e-4925-874f-2af7a0b3f34b", "To", "Period Covered  - To", "Drawback Period Covered  - To", "The last date of the period that this drawback covers.");
			this.PeriodToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 117, true);
			this.PeriodToDateEdit.Name = "PeriodToDateEdit";
			this.PeriodToDateEdit.TabIndex = 6;
			// 
			// PeriodFromDateEdit
			// 
			this.PeriodFromDateEdit.AllowDrop = true;
			this.PeriodFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PeriodFromDateEdit, "US_DRWDatePeriodFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWDatePeriodFrom)));
			this.PeriodFromDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|d5eaae08-8a30-4f4b-bb05-329256144f88", "From", "Period Covered - From", "Drawback Period Covered  - From", "The first date of the period that this drawback covers.");
			this.PeriodFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 117, true);
			this.PeriodFromDateEdit.Name = "PeriodFromDateEdit";
			this.PeriodFromDateEdit.TabIndex = 5;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "JE_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = null;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 67, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 2;
			// 
			// EntryTypeDropEdit
			// 
			this.EntryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryTypeDropEdit, "US_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_EntryTypeList)));
			this.EntryTypeDropEdit.BindToList = "AddInfoLookups+US_EntryTypeList";
			this.EntryTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|2702d868-5778-4e24-bd91-b5b0b9d8a34e", "Claim Type", "Claim Type", "A code indicating the type of Drawback claim, see the pull-down list.");
			this.EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 18, true);
			this.EntryTypeDropEdit.Name = "EntryTypeDropEdit";
			this.EntryTypeDropEdit.PreBoundMaxLength = 2;
			this.EntryTypeDropEdit.ShouldResizeByMaxLength = true;
			this.EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.EntryTypeDropEdit.TabIndex = 0;
			// 
			// NAFTADrawbackCountryDropEdit
			// 
			this.NAFTADrawbackCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NAFTADrawbackCountryDropEdit, "US_NAFTADrawbackCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_NAFTADrawbackCountry)));
			this.NAFTADrawbackCountryDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|ae22788d-72ac-4cf2-b2cd-642a0d35b688", "NAFTA Drawback Country", "NAFTA Drawback Country Code", "A a valid NAFTA ISO country code if the NAFTA claim indicator is ticked, see the pull down list.");
			this.NAFTADrawbackCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 332, true);
			this.NAFTADrawbackCountryDropEdit.Name = "NAFTADrawbackCountryDropEdit";
			this.NAFTADrawbackCountryDropEdit.PreBoundMaxLength = 2;
			this.NAFTADrawbackCountryDropEdit.ShouldResizeByMaxLength = true;
			this.NAFTADrawbackCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.NAFTADrawbackCountryDropEdit.TabIndex = 23;
			// 
			// NAFTAClaimIndCheckBox
			// 
			this.NAFTAClaimIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NAFTAClaimIndCheckBox, "US_NAFTAClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_NAFTAClaimInd)));
			this.NAFTAClaimIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|9b1096b4-440c-40d5-8953-52321502583c", "NAFTA Claim Indicator", "NAFTA Claim Indicator", "Tick to indicate a NAFTA claim.");
			this.NAFTAClaimIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NAFTAClaimIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NAFTAClaimIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 293, true);
			this.NAFTAClaimIndCheckBox.Name = "NAFTAClaimIndCheckBox";
			this.NAFTAClaimIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NAFTAClaimIndCheckBox.TabIndex = 18;
			this.NAFTAClaimIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// PetroliumClaimIndCheckBox
			// 
			this.PetroliumClaimIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PetroliumClaimIndCheckBox, "US_PetroleumClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PetroleumClaimInd)));
			this.PetroliumClaimIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|539e8e2b-64f8-4f90-bb08-3a5717ef1ffa", "Petroleum Claim Indicator", "Petroleum Claim Indicator", "Tick if petroleum or petroleum product.");
			this.PetroliumClaimIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PetroliumClaimIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PetroliumClaimIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 312, true);
			this.PetroliumClaimIndCheckBox.Name = "PetroliumClaimIndCheckBox";
			this.PetroliumClaimIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PetroliumClaimIndCheckBox.TabIndex = 21;
			this.PetroliumClaimIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// PreInspectionIndCheckBox
			// 
			this.PreInspectionIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PreInspectionIndCheckBox, "US_PreInspectionInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PreInspectionInd)));
			this.PreInspectionIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|0062d093-1e3b-4e61-8a9c-936bd77991de", "Pre Inspection Indicator", "Pre Inspection Indicator", "Tick if pre inspection is indicated.");
			this.PreInspectionIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PreInspectionIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PreInspectionIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 312, true);
			this.PreInspectionIndCheckBox.Name = "PreInspectionIndCheckBox";
			this.PreInspectionIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PreInspectionIndCheckBox.TabIndex = 20;
			this.PreInspectionIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// WaiverNoticeIndCheckBox
			// 
			this.WaiverNoticeIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WaiverNoticeIndCheckBox, "US_WaiverNoticeInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_WaiverNoticeInd)));
			this.WaiverNoticeIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|f6438325-4f98-4e1e-9481-6d5fdc62f0d4", "Waiver Notice Indicator", "Waiver of Prior Notice Indicator", "Tick to indicate waiver of prior notice is claimed.");
			this.WaiverNoticeIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.WaiverNoticeIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WaiverNoticeIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 312, true);
			this.WaiverNoticeIndCheckBox.Name = "WaiverNoticeIndCheckBox";
			this.WaiverNoticeIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.WaiverNoticeIndCheckBox.TabIndex = 22;
			this.WaiverNoticeIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExporterSummaryIndCheckBox
			// 
			this.ExporterSummaryIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExporterSummaryIndCheckBox, "US_ExporterSummaryInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ExporterSummaryInd)));
			this.ExporterSummaryIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|9395fd01-c52f-4ae1-a84d-8cdf8d345e46", "Exporter Summary Indicator", "Exporter Summary Procedure Indicator", "Tick to indicate that exporter summary procedure is used.");
			this.ExporterSummaryIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExporterSummaryIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExporterSummaryIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 293, true);
			this.ExporterSummaryIndCheckBox.Name = "ExporterSummaryIndCheckBox";
			this.ExporterSummaryIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExporterSummaryIndCheckBox.TabIndex = 17;
			this.ExporterSummaryIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// AcceleratedClaimIndCheckBox
			// 
			this.AcceleratedClaimIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AcceleratedClaimIndCheckBox, "US_AcceleratedClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_AcceleratedClaimInd)));
			this.AcceleratedClaimIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|9ee8d488-770a-4bb2-88ce-752d3629792b", "Acc. Claim Ind.", "Accelerated Claim Indicator", "Tick to indicate an accelerated drawback claim.");
			this.AcceleratedClaimIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AcceleratedClaimIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AcceleratedClaimIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 293, true);
			this.AcceleratedClaimIndCheckBox.Name = "AcceleratedClaimIndCheckBox";
			this.AcceleratedClaimIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AcceleratedClaimIndCheckBox.TabIndex = 19;
			this.AcceleratedClaimIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// EarliestExportDateEdit
			// 
			this.EarliestExportDateEdit.AllowDrop = true;
			this.EarliestExportDateEdit.AutoCompleteMonthThreshold = 1;
			this.EarliestExportDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EarliestExportDateEdit, "US_EarliestExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EarliestExportDate)));
			this.EarliestExportDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|8d41acc3-e189-436a-8283-2ccc49c7e8c0", "Earliest Export Date", "Earliest Export Date", "Optional date of earliest export involved in claim.  If used, must be within three years of estimated claim date.");
			this.EarliestExportDateEdit.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.EarliestExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 92, true);
			this.EarliestExportDateEdit.Name = "EarliestExportDateEdit";
			this.EarliestExportDateEdit.TabIndex = 4;
			// 
			// EstimatedEntryDateDateEdit
			// 
			this.EstimatedEntryDateDateEdit.AllowDrop = true;
			this.EstimatedEntryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedEntryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedEntryDateDateEdit, "US_EstimatedEntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EstimatedEntryDate)));
			this.EstimatedEntryDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|d8e3f430-2a65-4635-b1de-79e6a3eb882e", "Estimated Claim Date", "Estimated Claim Date", "The estimated date when the full paper claim will be lodged. This date must be within 30 calendar days of the ABI transmission.");
			this.EstimatedEntryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 92, true);
			this.EstimatedEntryDateDateEdit.Name = "EstimatedEntryDateDateEdit";
			this.EstimatedEntryDateDateEdit.TabIndex = 3;
			// 
			// USMCAClaimIndCheckBox
			// 
			this.USMCAClaimIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.USMCAClaimIndCheckBox, "US_USMCAClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_USMCAClaimInd)));
			this.USMCAClaimIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|222F03BF-7015-442A-9BE4-EF6495FFB263", "USMCA Claim Indicator", "USMCA Claim Indicator", "Tick to indicate a USMCA claim.");
			this.USMCAClaimIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.USMCAClaimIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.USMCAClaimIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 336, true);
			this.USMCAClaimIndCheckBox.Name = "USMCAClaimIndCheckBox";
			this.USMCAClaimIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.USMCAClaimIndCheckBox.TabIndex = 26;
			this.USMCAClaimIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExportingCarrierFindBox
			// 
			this.ExportingCarrierFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportingCarrierFindBox, "US_UI_NKCarrierSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_UI_NKCarrierSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.USCarrierList)));
			this.ExportingCarrierFindBox.BindToList = "Lookups+USCarrierList";
			this.ExportingCarrierFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d97c1bde-83c9-4322-831f-f3756b3a8dab", "Exporting Carrier SCAC");
			this.ExportingCarrierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 584, true);
			this.ExportingCarrierFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Carrier;
			this.ExportingCarrierFindBox.Name = "ExportingCarrierFindBox";
			this.ExportingCarrierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.ExportingCarrierFindBox.TabIndex = 6;
			// 
			// TENoTextBox
			// 
			this.BindingSource.SetBindingMember(this.TENoTextBox, "US_DRWTENo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWTENo)));
			this.TENoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b4461eb4-fe20-49e0-a6a2-7188099d78c9", "T&E No.");
			this.TENoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 559, true);
			this.TENoTextBox.Name = "TENoTextBox";
			this.TENoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.TENoTextBox.TabIndex = 5;
			// 
			// IntendedPortOfExportCodeFindBox
			// 
			this.IntendedPortOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntendedPortOfExportCodeFindBox, "US_DRWIntendedPortOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWIntendedPortOfExport)));
			this.IntendedPortOfExportCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4d8ffe19-0f33-4295-9ced-ad88ba26fced", "Intended Port of Export");
			this.IntendedPortOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 372, true);
			this.IntendedPortOfExportCodeFindBox.Name = "IntendedPortOfExportCodeFindBox";
			this.IntendedPortOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.IntendedPortOfExportCodeFindBox.TabIndex = 4;
			// 
			// ContractNumberszGroupBox
			// 
			this.ContractNumberszGroupBox.Controls.Add(this.ContractNumbersGrid);
			this.ContractNumberszGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 93, true);
			this.ContractNumberszGroupBox.Name = "ContractNumberszGroupBox";
			this.ContractNumberszGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 178, true);
			this.ContractNumberszGroupBox.TabIndex = 4;
			this.ContractNumberszGroupBox.TabStop = false;
			this.ContractNumberszGroupBox.Text = "Contract Numbers";
			// 
			// ContractNumbersGrid
			// 
			this.ContractNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContractNumbersGrid, "ContractNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ContractNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ContractNumber)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ContractNumbers)).SyncRoot)).CY_Data)));
			this.ContractNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Drawback Contract Numbers";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ContractNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContractNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContractNumbersGrid.GridId = "bbfcc2db-1342-4cee-acf3-4e721181c563";
			this.ContractNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContractNumbersGrid.LayoutKey = "ContractNumbersGrid";
			this.ContractNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContractNumbersGrid.Name = "ContractNumbersGrid";
			this.ContractNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 159, true);
			this.ContractNumbersGrid.TabIndex = 1;
			// 
			// BondGroupBox
			// 
			this.BondGroupBox.Controls.Add(this.RefreshBondButton);
			this.BondGroupBox.Controls.Add(this.US_SuretyCodeTextBox);
			this.BondGroupBox.Controls.Add(this.US_BondTypeDropEdit);
			this.BondGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 275, true);
			this.BondGroupBox.Name = "BondGroupBox";
			this.BondGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 88, true);
			this.BondGroupBox.TabIndex = 5;
			this.BondGroupBox.TabStop = false;
			this.BondGroupBox.Text = "Bond";
			// 
			// RefreshBondButton
			// 
			this.RefreshBondButton.IsCaptionOverridden = true;
			this.RefreshBondButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 41, true);
			this.RefreshBondButton.Name = "RefreshBondButton";
			this.RefreshBondButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshBondButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.RefreshBondButton.TabIndex = 2;
			this.RefreshBondButton.Text = "Refresh Bond Details";
			this.RefreshBondButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshBondButton.ToolTipCaption = null;
			this.RefreshBondButton.UseVisualStyleBackColor = true;
			this.RefreshBondButton.Click += new System.EventHandler(this.RefreshBondButton_Click);
			// 
			// US_SuretyCodeTextBox
			// 
			this.US_SuretyCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.US_SuretyCodeTextBox, "US_SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SuretyCode)));
			this.US_SuretyCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|bead48a0-216e-4a77-9576-d3401351816d", "Surety Code", "Surety Code", "Surety identification.");
			this.US_SuretyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 43, true);
			this.US_SuretyCodeTextBox.Name = "US_SuretyCodeTextBox";
			this.US_SuretyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.US_SuretyCodeTextBox.TabIndex = 1;
			// 
			// US_BondTypeDropEdit
			// 
			this.US_BondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_BondTypeDropEdit, "US_BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_BondType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_BondTypeList)));
			this.US_BondTypeDropEdit.BindToList = "AddInfoLookups+US_BondTypeList";
			this.US_BondTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|20adcc99-7df5-4e0f-a94e-fba34d6bd2bb", "Bond Type", "Bond Type", "A Valid Bond Type for drawback, see the pull down list (cannot be 0 if accelerated drawback or exporter\'s summary procedure is claimed).");
			this.US_BondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 17, true);
			this.US_BondTypeDropEdit.Name = "US_BondTypeDropEdit";
			this.US_BondTypeDropEdit.PreBoundMaxLength = 1;
			this.US_BondTypeDropEdit.ShouldResizeByMaxLength = true;
			this.US_BondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.US_BondTypeDropEdit.TabIndex = 0;
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.ClientReferenceTextBox);
			this.MiscOptionsGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.MiscOptionsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.MiscOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.MiscOptionsGroupBox.Name = "MiscOptionsGroupBox";
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 90, true);
			this.MiscOptionsGroupBox.TabIndex = 0;
			this.MiscOptionsGroupBox.TabStop = false;
			this.MiscOptionsGroupBox.Text = "Miscellaneous Options";
			// 
			// ClientReferenceTextBox
			// 
			this.ClientReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ClientReferenceTextBox, "JE_OwnerRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OwnerRef)));
			this.ClientReferenceTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|dbcd78fe-0888-4d4c-b4c0-d921419f99bf", "Client Ref", "Client Reference", "");
			this.ClientReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 67, true);
			this.ClientReferenceTextBox.Name = "ClientReferenceTextBox";
			this.ClientReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.ClientReferenceTextBox.TabIndex = 3;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_GS_NKCusAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.CusAgents)));
			this.BrokerCodeFindBox.BindToList = "Lookups+CusAgents";
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|5b872cf8-a797-4f00-809c-83d462beed3e", "Broker", "Customs Broker", "Customs Broker responsible for this Claim.");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 43, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Lookups.BranchCollection)));
			this.BranchGuidFindBox.BindToList = "Lookups.BranchCollection";
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 18, true);
			this.BranchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.BranchGuidFindBox.TabIndex = 0;
			// 
			// MessageStatusDescriptionTextBox
			// 
			this.MessageStatusDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MessageStatusDescriptionTextBox, "JE_MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MessageStatusDescription)));
			this.MessageStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|f20fae32-e175-4bb2-90b0-489cabadfbcd", "Message Status");
			this.MessageStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 72, true);
			this.MessageStatusDescriptionTextBox.Name = "MessageStatusDescriptionTextBox";
			this.MessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 20, true);
			this.MessageStatusDescriptionTextBox.TabIndex = 5;
			// 
			// DashLabel
			// 
			this.DashLabel.AutoSize = true;
			this.DashLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DashLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 20, true);
			this.DashLabel.Name = "DashLabel";
			this.DashLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.DashLabel.TabIndex = 1;
			this.DashLabel.Text = "-";
			// 
			// EntryFilerTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryFilerTextBox, "US_EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryFilerCode)));
			this.EntryFilerTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("DrawbackJobDeclarationUserControl|b4465835-4bcf-490a-ab91-cd7bc5fedb06", "Entry Number");
			this.EntryFilerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 17, true);
			this.EntryFilerTextBox.Name = "EntryFilerTextBox";
			this.EntryFilerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.EntryFilerTextBox.TabIndex = 0;
			// 
			// DrawbackDelcarationPurposeDropEdit
			// 
			this.DrawbackDelcarationPurposeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DrawbackDelcarationPurposeDropEdit, "US_DRWPurpose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWPurpose)));
			this.DrawbackDelcarationPurposeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("99a6e5aa-20a6-4970-8239-cd39b2abd0f2", "Purpose");
			this.DrawbackDelcarationPurposeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 98, true);
			this.DrawbackDelcarationPurposeDropEdit.Name = "DrawbackDelcarationPurposeDropEdit";
			this.DrawbackDelcarationPurposeDropEdit.ShouldResizeByMaxLength = true;
			this.DrawbackDelcarationPurposeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.DrawbackDelcarationPurposeDropEdit.TabIndex = 7;
			// 
			// AdditionalDetailsGroupBox
			// 
			this.AdditionalDetailsGroupBox.Controls.Add(this.TransfereeOrganisationControl);
			this.AdditionalDetailsGroupBox.Controls.Add(this.RulingNoTextBox);
			this.AdditionalDetailsGroupBox.Controls.Add(this.CertOfManufactureTextBox);
			this.AdditionalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.AdditionalDetailsGroupBox.Name = "AdditionalDetailsGroupBox";
			this.AdditionalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 270, true);
			this.AdditionalDetailsGroupBox.TabIndex = 6;
			this.AdditionalDetailsGroupBox.TabStop = false;
			// 
			// TransfereeOrganisationControl
			// 
			this.TransfereeOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransfereeOrganisationControl, "US_DRWTransferee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWTransferee)));
			this.TransfereeOrganisationControl.BindToMiscellaneousFields = "JE_TransfereeMiscFields";
			this.TransfereeOrganisationControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bfc19616-e8dd-4666-a729-c5d80d8ad6af", "Transferee");
			this.TransfereeOrganisationControl.Captions = new string[] {
        "Transferee"};
			this.TransfereeOrganisationControl.IsCaptionOverridden = false;
			this.TransfereeOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.TransfereeOrganisationControl.Name = "TransfereeOrganisationControl";
			this.TransfereeOrganisationControl.PopupCaption = "";
			this.TransfereeOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.TransfereeOrganisationControl.TabIndex = 0;
			// 
			// RulingNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RulingNoTextBox, "US_DRWRulingNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWRulingNo)));
			this.RulingNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("87344361-2e5b-42b8-aaf8-522e182f5612", "Ruling No.");
			this.RulingNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RulingNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 204, true);
			this.RulingNoTextBox.Name = "RulingNoTextBox";
			this.RulingNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RulingNoTextBox.TabIndex = 2;
			// 
			// CertOfManufactureTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertOfManufactureTextBox, "US_DRWCertOfManufacture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWCertOfManufacture)));
			this.CertOfManufactureTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("dc8ec4b0-1f8b-4034-b541-e67abdfd1631", "CM&D No.");
			this.CertOfManufactureTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertOfManufactureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 178, true);
			this.CertOfManufactureTextBox.Name = "CertOfManufactureTextBox";
			this.CertOfManufactureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CertOfManufactureTextBox.TabIndex = 1;
			// 
			// TransferorOrganizationControl
			// 
			this.TransferorOrganizationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransferorOrganizationControl, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_Importer)));
			this.TransferorOrganizationControl.BindToMiscellaneousFields = "JE_ImporterMiscFields";
			this.TransferorOrganizationControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2a6e94c5-d3cf-4446-90b5-9665485dd816", "Transferee");
			this.TransferorOrganizationControl.Captions = new string[] {
        "Transferee"};
			this.TransferorOrganizationControl.IsCaptionOverridden = true;
			this.TransferorOrganizationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.TransferorOrganizationControl.Name = "TransferorOrganizationControl";
			this.TransferorOrganizationControl.PopupCaption = "";
			this.TransferorOrganizationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.TransferorOrganizationControl.TabIndex = 0;
			// 
			// AllocateImportEntryNumberButton
			// 
			this.AllocateImportEntryNumberButton.IsCaptionOverridden = true;
			this.AllocateImportEntryNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 15, true);
			this.AllocateImportEntryNumberButton.Name = "AllocateImportEntryNumberButton";
			this.AllocateImportEntryNumberButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocateImportEntryNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AllocateImportEntryNumberButton.TabIndex = 3;
			this.AllocateImportEntryNumberButton.Text = "Allocate";
			this.AllocateImportEntryNumberButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AllocateImportEntryNumberButton.ToolTipCaption = null;
			this.AllocateImportEntryNumberButton.UseVisualStyleBackColor = true;
			this.AllocateImportEntryNumberButton.Click += new System.EventHandler(this.AllocateImportEntryNumberButton_Click);
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "JE_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_ApplicationCode)));
			this.MessageTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("684fd09c-44c6-4a37-bee0-77b909b9fdeb", "Message Type");
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 98, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageTypeDropEdit.ShouldResizeByMaxLength = true;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.MessageTypeDropEdit.TabIndex = 6;
			// 
			// ACSDrawbackPanel
			// 
			this.ACSDrawbackPanel.Controls.Add(this.ACSEntryGroupBox);
			this.ACSDrawbackPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 165, true);
			this.ACSDrawbackPanel.Name = "ACSDrawbackPanel";
			this.ACSDrawbackPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 440, true);
			this.ACSDrawbackPanel.TabIndex = 2;
			// 
			// ACSRightPanelPanel
			// 
			this.ACSRightPanelPanel.Controls.Add(this.ACERightPanel);
			this.ACSRightPanelPanel.Controls.Add(this.IntendedPortOfExportCodeFindBox);
			this.ACSRightPanelPanel.Controls.Add(this.AdditionalDetailsGroupBox);
			this.ACSRightPanelPanel.Controls.Add(this.BondGroupBox);
			this.ACSRightPanelPanel.Controls.Add(this.MiscOptionsGroupBox);
			this.ACSRightPanelPanel.Controls.Add(this.ContractNumberszGroupBox);
			this.ACSRightPanelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 165, true);
			this.ACSRightPanelPanel.Name = "ACSRightPanelPanel";
			this.ACSRightPanelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 394, true);
			this.ACSRightPanelPanel.TabIndex = 3;
			// 
			// ACERightPanel
			// 
			this.ACERightPanel.Controls.Add(this.ACENoticeOfIntentGroupBox);
			this.ACERightPanel.Controls.Add(this.ACEBondGroupBox);
			this.ACERightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.ACERightPanel.Name = "ACERightPanel";
			this.ACERightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 296, true);
			this.ACERightPanel.TabIndex = 3;
			// 
			// ACENoticeOfIntentGroupBox
			// 
			this.ACENoticeOfIntentGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a450b1db-5dd3-41b7-b761-9a45bae49ca6", "Notice of Intent");
			this.ACENoticeOfIntentGroupBox.Controls.Add(this.ACEIntendedPortCodeFindBox);
			this.ACENoticeOfIntentGroupBox.Controls.Add(this.ProcessorGroupBox);
			this.ACENoticeOfIntentGroupBox.Controls.Add(this.DestructionResultDropEdit);
			this.ACENoticeOfIntentGroupBox.Controls.Add(this.ACEExainationGroupBox);
			this.ACENoticeOfIntentGroupBox.Controls.Add(this.ACELocationOfDescTextBox);
			this.ACENoticeOfIntentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, -2, true);
			this.ACENoticeOfIntentGroupBox.Name = "ACENoticeOfIntentGroupBox";
			this.ACENoticeOfIntentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 195, true);
			this.ACENoticeOfIntentGroupBox.TabIndex = 1;
			this.ACENoticeOfIntentGroupBox.TabStop = false;
			this.ACENoticeOfIntentGroupBox.Text = "Notice of Intent";
			// 
			// ACEIntendedPortCodeFindBox
			// 
			this.ACEIntendedPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEIntendedPortCodeFindBox, "US_DRWIntendedPortOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWIntendedPortOfExport)));
			this.ACEIntendedPortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6aa46e77-56aa-48d6-94f8-5e5cbef24428", "Intended Port of Export");
			this.ACEIntendedPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 62, true);
			this.ACEIntendedPortCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ACEIntendedPortCodeFindBox.Name = "ACEIntendedPortCodeFindBox";
			this.ACEIntendedPortCodeFindBox.PreBoundMaxLength = 4;
			this.ACEIntendedPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.ACEIntendedPortCodeFindBox.TabIndex = 3;
			// 
			// ProcessorGroupBox
			// 
			this.ProcessorGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7138e07e-c9f9-4394-9568-ae588f84b39a", "Processor");
			this.ProcessorGroupBox.Controls.Add(this.ProcessorDateDateEdit);
			this.ProcessorGroupBox.Controls.Add(this.ProcessorPhoneNoTextBox);
			this.ProcessorGroupBox.Controls.Add(this.ProcessorBadgeNoTextBox);
			this.ProcessorGroupBox.Controls.Add(this.ProcessorNameTextBox);
			this.ProcessorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 85, true);
			this.ProcessorGroupBox.Name = "ProcessorGroupBox";
			this.ProcessorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 108, true);
			this.ProcessorGroupBox.TabIndex = 4;
			this.ProcessorGroupBox.TabStop = false;
			this.ProcessorGroupBox.Text = "Processor";
			// 
			// ProcessorDateDateEdit
			// 
			this.ProcessorDateDateEdit.AllowDrop = true;
			this.ProcessorDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ProcessorDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ProcessorDateDateEdit, "US_DRWProcDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWProcDate)));
			this.ProcessorDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bdd42b0f-4f46-47dc-a644-8b019fd7a48f", "Date");
			this.ProcessorDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 85, true);
			this.ProcessorDateDateEdit.Name = "ProcessorDateDateEdit";
			this.ProcessorDateDateEdit.TabIndex = 3;
			// 
			// ProcessorPhoneNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessorPhoneNoTextBox, "US_DRWProcPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWProcPhone)));
			this.ProcessorPhoneNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3c2f83ae-71a8-431d-aae6-54d029d3b527", "Phone No.");
			this.ProcessorPhoneNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 62, true);
			this.ProcessorPhoneNoTextBox.Name = "ProcessorPhoneNoTextBox";
			this.ProcessorPhoneNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ProcessorPhoneNoTextBox.TabIndex = 2;
			// 
			// ProcessorBadgeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessorBadgeNoTextBox, "US_DRWProcBadge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWProcBadge)));
			this.ProcessorBadgeNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e63b74a6-55fe-41aa-b790-e43080c04f18", "Badge No.");
			this.ProcessorBadgeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 39, true);
			this.ProcessorBadgeNoTextBox.Name = "ProcessorBadgeNoTextBox";
			this.ProcessorBadgeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ProcessorBadgeNoTextBox.TabIndex = 1;
			// 
			// ProcessorNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessorNameTextBox, "US_DRWProcName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWProcName)));
			this.ProcessorNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d7f95753-80b5-49a6-a1a4-99d7efa9e872", "Name");
			this.ProcessorNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 16, true);
			this.ProcessorNameTextBox.Name = "ProcessorNameTextBox";
			this.ProcessorNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ProcessorNameTextBox.TabIndex = 0;
			// 
			// DestructionResultDropEdit
			// 
			this.DestructionResultDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestructionResultDropEdit, "US_DRWDestructionResult");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWDestructionResult)));
			this.DestructionResultDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f108794b-60c9-4c51-bfb8-ca41c979f9f9", "Result of Examination", "Results of Examination or Witness of Destruction");
			this.DestructionResultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 38, true);
			this.DestructionResultDropEdit.Name = "DestructionResultDropEdit";
			this.DestructionResultDropEdit.PreBoundMaxLength = 1;
			this.DestructionResultDropEdit.ShouldResizeByMaxLength = true;
			this.DestructionResultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.DestructionResultDropEdit.TabIndex = 2;
			// 
			// ACEExainationGroupBox
			// 
			this.ACEExainationGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("93e52aca-3c71-41ff-9355-3cef4a9a2c51", "Examination");
			this.ACEExainationGroupBox.Controls.Add(this.ExaminationDateEdit);
			this.ACEExainationGroupBox.Controls.Add(this.ExaminationPhoneNoTextBox);
			this.ACEExainationGroupBox.Controls.Add(this.ExaminationBadgeNoTextBox);
			this.ACEExainationGroupBox.Controls.Add(this.ExaminationNameTextBox);
			this.ACEExainationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 85, true);
			this.ACEExainationGroupBox.Name = "ACEExainationGroupBox";
			this.ACEExainationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 108, true);
			this.ACEExainationGroupBox.TabIndex = 5;
			this.ACEExainationGroupBox.TabStop = false;
			this.ACEExainationGroupBox.Text = "Examination";
			// 
			// ExaminationDateEdit
			// 
			this.ExaminationDateEdit.AllowDrop = true;
			this.ExaminationDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExaminationDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExaminationDateEdit, "US_DRWExamDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWExamDate)));
			this.ExaminationDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("16d99288-c308-4ae5-81f7-55a09f177426", "Date");
			this.ExaminationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 85, true);
			this.ExaminationDateEdit.Name = "ExaminationDateEdit";
			this.ExaminationDateEdit.TabIndex = 3;
			// 
			// ExaminationPhoneNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExaminationPhoneNoTextBox, "US_DRWExamPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWExamPhone)));
			this.ExaminationPhoneNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9d144aeb-8142-4365-9f00-748d5b1470bc", "Phone No.");
			this.ExaminationPhoneNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 62, true);
			this.ExaminationPhoneNoTextBox.Name = "ExaminationPhoneNoTextBox";
			this.ExaminationPhoneNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExaminationPhoneNoTextBox.TabIndex = 2;
			// 
			// ExaminationBadgeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExaminationBadgeNoTextBox, "US_DRWExamBadge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWExamBadge)));
			this.ExaminationBadgeNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2a271d44-ce7c-49cc-a1bc-a54695a2a14e", "Badge No.");
			this.ExaminationBadgeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 39, true);
			this.ExaminationBadgeNoTextBox.Name = "ExaminationBadgeNoTextBox";
			this.ExaminationBadgeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExaminationBadgeNoTextBox.TabIndex = 1;
			// 
			// ExaminationNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExaminationNameTextBox, "US_DRWExamName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWExamName)));
			this.ExaminationNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e5f35807-26a0-45cc-a431-122fa1f13d1f", "Name");
			this.ExaminationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 16, true);
			this.ExaminationNameTextBox.Name = "ExaminationNameTextBox";
			this.ExaminationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExaminationNameTextBox.TabIndex = 0;
			// 
			// ACELocationOfDescTextBox
			// 
			this.ACELocationOfDescTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACELocationOfDescTextBox, "US_DRWLocatOfDest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWLocatOfDest)));
			this.ACELocationOfDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("72ab4991-8141-484f-a822-71a07a260978", "Location of Destruction");
			this.ACELocationOfDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 14, true);
			this.ACELocationOfDescTextBox.Name = "ACELocationOfDescTextBox";
			this.ACELocationOfDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.ACELocationOfDescTextBox.TabIndex = 1;
			// 
			// ACEBondGroupBox
			// 
			this.ACEBondGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("135729bd-5e2d-4e95-bb28-178f1d154335", "Bond");
			this.ACEBondGroupBox.Controls.Add(this.ACERefreshBondButton);
			this.ACEBondGroupBox.Controls.Add(this.ACEWaivingReasonDropEdit);
			this.ACEBondGroupBox.Controls.Add(this.ACEBondDesignationCodeDropEdit);
			this.ACEBondGroupBox.Controls.Add(this.ACEAccountNoTextBox);
			this.ACEBondGroupBox.Controls.Add(this.ACEBondAmountCalcEdit);
			this.ACEBondGroupBox.Controls.Add(this.ACESuretyCodeTextBox);
			this.ACEBondGroupBox.Controls.Add(this.ACEBondTypeDropEdit);
			this.ACEBondGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 195, true);
			this.ACEBondGroupBox.Name = "ACEBondGroupBox";
			this.ACEBondGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 98, true);
			this.ACEBondGroupBox.TabIndex = 2;
			this.ACEBondGroupBox.TabStop = false;
			this.ACEBondGroupBox.Text = "Bond";
			// 
			// ACERefreshBondButton
			// 
			this.ACERefreshBondButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6ca32296-1308-430e-b5dd-0a2035df7cba", "Refresh Bond Details");
			this.ACERefreshBondButton.IsCaptionOverridden = true;
			this.ACERefreshBondButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 73, true);
			this.ACERefreshBondButton.Name = "ACERefreshBondButton";
			this.ACERefreshBondButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ACERefreshBondButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.ACERefreshBondButton.TabIndex = 6;
			this.ACERefreshBondButton.Text = "Refresh Bond Details";
			this.ACERefreshBondButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ACERefreshBondButton.ToolTipCaption = null;
			this.ACERefreshBondButton.UseVisualStyleBackColor = true;
			this.ACERefreshBondButton.Click += new System.EventHandler(this.RefreshBondButton_Click);
			// 
			// ACEWaivingReasonDropEdit
			// 
			this.ACEWaivingReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEWaivingReasonDropEdit, "US_BondWaiverCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_BondWaiverCode)));
			this.ACEWaivingReasonDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("374f7ec7-2c60-42c1-ae49-d9e4cf10820d", "Waiving Reason");
			this.ACEWaivingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 35, true);
			this.ACEWaivingReasonDropEdit.Name = "ACEWaivingReasonDropEdit";
			this.ACEWaivingReasonDropEdit.PreBoundMaxLength = 3;
			this.ACEWaivingReasonDropEdit.ShouldResizeByMaxLength = true;
			this.ACEWaivingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.ACEWaivingReasonDropEdit.TabIndex = 2;
			// 
			// ACEBondDesignationCodeDropEdit
			// 
			this.ACEBondDesignationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEBondDesignationCodeDropEdit, "US_BondDesignationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_BondDesignationCode)));
			this.ACEBondDesignationCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0f52a52e-b705-4764-9e87-0f251489bcbf", "Designation Code");
			this.ACEBondDesignationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 78, true);
			this.ACEBondDesignationCodeDropEdit.Name = "ACEBondDesignationCodeDropEdit";
			this.ACEBondDesignationCodeDropEdit.PreBoundMaxLength = 1;
			this.ACEBondDesignationCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ACEBondDesignationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.ACEBondDesignationCodeDropEdit.TabIndex = 5;
			// 
			// ACEAccountNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ACEAccountNoTextBox, "US_BondProducerAccNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_BondProducerAccNo)));
			this.ACEAccountNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0b1db20d-53ee-4ce6-97c5-1e957d42d41f", "Account No.");
			this.ACEAccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 57, true);
			this.ACEAccountNoTextBox.Name = "ACEAccountNoTextBox";
			this.ACEAccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.ACEAccountNoTextBox.TabIndex = 4;
			// 
			// ACEBondAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ACEBondAmountCalcEdit, "US_BondAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_BondAmount)));
			this.ACEBondAmountCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("97362ca0-7b48-4ad0-aded-422110d72d10", "Bond Amount");
			this.ACEBondAmountCalcEdit.DecimalPlaces = 0;
			this.ACEBondAmountCalcEdit.Decimals = 0;
			this.ACEBondAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 34, true);
			this.ACEBondAmountCalcEdit.Name = "ACEBondAmountCalcEdit";
			this.ACEBondAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.ACEBondAmountCalcEdit.TabIndex = 3;
			this.ACEBondAmountCalcEdit.Text = "0";
			this.ACEBondAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACESuretyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ACESuretyCodeTextBox, "US_SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_SuretyCode)));
			this.ACESuretyCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89560152-703f-469b-8ea1-cba2b509c9e9", "Surety Code");
			this.ACESuretyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 14, true);
			this.ACESuretyCodeTextBox.Name = "ACESuretyCodeTextBox";
			this.ACESuretyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.ACESuretyCodeTextBox.TabIndex = 1;
			// 
			// ACEBondTypeDropEdit
			// 
			this.ACEBondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEBondTypeDropEdit, "US_BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_BondType)));
			this.ACEBondTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4c36ddbe-5eee-466c-ac79-cde76352d770", "Bond Type");
			this.ACEBondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 14, true);
			this.ACEBondTypeDropEdit.Name = "ACEBondTypeDropEdit";
			this.ACEBondTypeDropEdit.PreBoundMaxLength = 1;
			this.ACEBondTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ACEBondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.ACEBondTypeDropEdit.TabIndex = 0;
			// 
			// ACEDrawbackPanel
			// 
			this.ACEDrawbackPanel.Controls.Add(this.ACEEntryGroupBox);
			this.ACEDrawbackPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 165, true);
			this.ACEDrawbackPanel.Name = "ACEDrawbackPanel";
			this.ACEDrawbackPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 470, true);
			this.ACEDrawbackPanel.TabIndex = 2;
			// 
			// ACEEntryGroupBox
			// 
			this.ACEEntryGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("48680f17-d77c-401d-8028-4644463ccccf", "ACE Drawback Claim Details");
			this.ACEEntryGroupBox.Controls.Add(this.ACEPeriodToDateEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEPeriodFromDateEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEEarliestExportDateEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEEstimatedEntryDateDateEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalOilSpillTaxCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.BillOfFormulaCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.DestroyedValuationCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.RetailSalesSubstitutionCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.SuperfundTaxCertificationCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACEProcessingPortCodeFindBox);
			this.ACEEntryGroupBox.Controls.Add(this.UnUsedWineCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACEDrawbackTeamDropEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEClaimPortDropEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEPrintSubTotalsCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACEEnableMergeCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACENAFTADrawbackCountryDropEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ExaminationWitnessCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACENAFTAClaimCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.USMCAClaimIndCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.OilSpillTaxIndCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ManufacturingPetroleumCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ElectronicPetroleumCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACEWaiverNoticeIndCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.OneTimeWaiverIndCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACEAcceleratedCheckBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACEBrokerGuidFindBox);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalClaimedCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalOtherFeesCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalDutyCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalIRTaxCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalMPFCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACETotalHMFCalcEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEMethodFilingDropEdit);
			this.ACEEntryGroupBox.Controls.Add(this.ACEDrawbackDescriptionTextBox);
			this.ACEEntryGroupBox.Controls.Add(this.CommercialRulingDropEdit);
			this.ACEEntryGroupBox.Controls.Add(this.DrawbackProvisionDropEdit);
			this.ACEEntryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ACEEntryGroupBox.Name = "ACEEntryGroupBox";
			this.ACEEntryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 470, true);
			this.ACEEntryGroupBox.TabIndex = 0;
			this.ACEEntryGroupBox.TabStop = false;
			// 
			// ACEPeriodToDateEdit
			// 
			this.ACEPeriodToDateEdit.AllowDrop = true;
			this.ACEPeriodToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ACEPeriodToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ACEPeriodToDateEdit, "US_DRWDatePeriodTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWDatePeriodTo)));
			this.ACEPeriodToDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ba00047a-b7d4-4480-9a78-5210d1c1e6c4", "Period Covered  - To");
			this.ACEPeriodToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 117, true);
			this.ACEPeriodToDateEdit.Name = "ACEPeriodToDateEdit";
			this.ACEPeriodToDateEdit.TabIndex = 7;
			// 
			// ACEPeriodFromDateEdit
			// 
			this.ACEPeriodFromDateEdit.AllowDrop = true;
			this.ACEPeriodFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.ACEPeriodFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ACEPeriodFromDateEdit, "US_DRWDatePeriodFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWDatePeriodFrom)));
			this.ACEPeriodFromDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5cf2fd9e-376b-40b3-b63d-3d9b87f48378", "Period Covered  - From");
			this.ACEPeriodFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 117, true);
			this.ACEPeriodFromDateEdit.Name = "ACEPeriodFromDateEdit";
			this.ACEPeriodFromDateEdit.TabIndex = 6;
			// 
			// ACEEarliestExportDateEdit
			// 
			this.ACEEarliestExportDateEdit.AllowDrop = true;
			this.ACEEarliestExportDateEdit.AutoCompleteMonthThreshold = 1;
			this.ACEEarliestExportDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ACEEarliestExportDateEdit, "US_EarliestExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EarliestExportDate)));
			this.ACEEarliestExportDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("75805dba-92d6-4904-b758-543325e70e64", "Earliest Export Date");
			this.ACEEarliestExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 92, true);
			this.ACEEarliestExportDateEdit.Name = "ACEEarliestExportDateEdit";
			this.ACEEarliestExportDateEdit.TabIndex = 5;
			// 
			// ACEEstimatedEntryDateDateEdit
			// 
			this.ACEEstimatedEntryDateDateEdit.AllowDrop = true;
			this.ACEEstimatedEntryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ACEEstimatedEntryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ACEEstimatedEntryDateDateEdit, "US_EstimatedEntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EstimatedEntryDate)));
			this.ACEEstimatedEntryDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c886c935-4d98-4895-a987-1fcee5513060", "Estimated Claim Date");
			this.ACEEstimatedEntryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 92, true);
			this.ACEEstimatedEntryDateDateEdit.Name = "ACEEstimatedEntryDateDateEdit";
			this.ACEEstimatedEntryDateDateEdit.TabIndex = 4;
			// 
			// ACETotalOilSpillTaxCalcEdit
			// 
			this.ACETotalOilSpillTaxCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalOilSpillTaxCalcEdit, "US_DRWTotalPRDC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWTotalPRDC)));
			this.ACETotalOilSpillTaxCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8317d0c4-05a4-4da6-827f-598b880fb9a9", "Puerto Rico Drawback Claimed");
			this.ACETotalOilSpillTaxCalcEdit.DecimalPlaces = 2;
			this.ACETotalOilSpillTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 218, true);
			this.ACETotalOilSpillTaxCalcEdit.Name = "ACETotalOilSpillTaxCalcEdit";
			this.ACETotalOilSpillTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalOilSpillTaxCalcEdit.TabIndex = 14;
			this.ACETotalOilSpillTaxCalcEdit.Text = "0.00";
			this.ACETotalOilSpillTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BillOfFormulaCheckBox
			// 
			this.BillOfFormulaCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BillOfFormulaCheckBox, "US_DRWBillOfFormula");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWBillOfFormula)));
			this.BillOfFormulaCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bcbaffba-57a1-4ef0-8a45-07d8afafb9aa", "Bill of Materials/Formula");
			this.BillOfFormulaCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BillOfFormulaCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BillOfFormulaCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 360, true);
			this.BillOfFormulaCheckBox.Name = "BillOfFormulaCheckBox";
			this.BillOfFormulaCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.BillOfFormulaCheckBox.TabIndex = 30;
			this.BillOfFormulaCheckBox.UseVisualStyleBackColor = true;
			// 
			// DestroyedValuationCheckBox
			// 
			this.DestroyedValuationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DestroyedValuationCheckBox, "US_DRWDestroyedValuation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWDestroyedValuation)));
			this.DestroyedValuationCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8d2458f0-5650-43d4-b205-b668ff2ffeb2", "Destroyed Valuation", "Valuation of Destroyed Merchandise");
			this.DestroyedValuationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DestroyedValuationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DestroyedValuationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 383, true);
			this.DestroyedValuationCheckBox.Name = "DestroyedValuationCheckBox";
			this.DestroyedValuationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DestroyedValuationCheckBox.TabIndex = 32;
			this.DestroyedValuationCheckBox.UseVisualStyleBackColor = true;
			// 
			// RetailSalesSubstitutionCheckBox
			// 
			this.RetailSalesSubstitutionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RetailSalesSubstitutionCheckBox, "USD_RetailSalesSubstitutionIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).USD_RetailSalesSubstitutionIndicator)));
			this.RetailSalesSubstitutionCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0164262A-B381-4764-B1C2-32D9FEDC5BB6", "Retail Sales Substitution Ind.", "Retail Sales Substitution Indicator");
			this.RetailSalesSubstitutionCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RetailSalesSubstitutionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RetailSalesSubstitutionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 406, true);
			this.RetailSalesSubstitutionCheckBox.Name = "RetailSalesSubstitutionCheckBox";
			this.RetailSalesSubstitutionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RetailSalesSubstitutionCheckBox.TabIndex = 34;
			this.RetailSalesSubstitutionCheckBox.UseVisualStyleBackColor = true;
			// 
			// SuperfundTaxCertificationCheckBox
			// 
			this.SuperfundTaxCertificationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SuperfundTaxCertificationCheckBox, "US_DRWSuperfundInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWSuperfundInd)));
			this.SuperfundTaxCertificationCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("838e8072-abd5-4cf1-b4b3-8ab9952baf88", "Super-fund Tax Certification");
			this.SuperfundTaxCertificationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SuperfundTaxCertificationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SuperfundTaxCertificationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 429, true);
			this.SuperfundTaxCertificationCheckBox.Name = "SuperfundTaxCertificationCheckBox";
			this.SuperfundTaxCertificationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SuperfundTaxCertificationCheckBox.TabIndex = 36;
			this.SuperfundTaxCertificationCheckBox.UseVisualStyleBackColor = true;
			// 
			// UnUsedWineCheckBox
			// 
			this.UnUsedWineCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UnUsedWineCheckBox, "US_DRWUnUsedWine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWUnUsedWine)));
			this.UnUsedWineCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f89e926c-e7f4-49f2-a007-79fb17fbd359", "Substituted Unused Wine");
			this.UnUsedWineCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UnUsedWineCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnUsedWineCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 313, true);
			this.UnUsedWineCheckBox.Name = "UnUsedWineCheckBox";
			this.UnUsedWineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UnUsedWineCheckBox.TabIndex = 25;
			this.UnUsedWineCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEDrawbackTeamDropEdit
			// 
			this.ACEDrawbackTeamDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEDrawbackTeamDropEdit, "US_TeamNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_TeamNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_TeamNoForDrawbackCodeList)));
			this.ACEDrawbackTeamDropEdit.BindToList = "AddInfoLookups+US_TeamNoForDrawbackCodeList";
			this.ACEDrawbackTeamDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8613ffcf-561c-49ce-8163-b5d2741aba91", "Drawback Team");
			this.ACEDrawbackTeamDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 414, true);
			this.ACEDrawbackTeamDropEdit.Name = "ACEDrawbackTeamDropEdit";
			this.ACEDrawbackTeamDropEdit.PreBoundMaxLength = 3;
			this.ACEDrawbackTeamDropEdit.ShouldResizeByMaxLength = true;
			this.ACEDrawbackTeamDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ACEDrawbackTeamDropEdit.TabIndex = 33;
			// 
			// ACEClaimPortDropEdit
			// 
			this.ACEClaimPortDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEClaimPortDropEdit, "US_ClaimPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ClaimPort)));
			this.ACEClaimPortDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("069420ec-5e9f-44f5-809d-a4d24cabe7eb", "Claim Port");
			this.ACEClaimPortDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 388, true);
			this.ACEClaimPortDropEdit.Name = "ACEClaimPortDropEdit";
			this.ACEClaimPortDropEdit.PreBoundMaxLength = 4;
			this.ACEClaimPortDropEdit.ShouldResizeByMaxLength = true;
			this.ACEClaimPortDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ACEClaimPortDropEdit.TabIndex = 31;
			// 
			// ACEPrintSubTotalsCheckBox
			// 
			this.ACEPrintSubTotalsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACEPrintSubTotalsCheckBox, "US_DRWPrintSubTotals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWPrintSubTotals)));
			this.ACEPrintSubTotalsCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7cfe6e5b-3481-4cfa-93ff-ee665c4867cb", "Print Sub-totals on 7551");
			this.ACEPrintSubTotalsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ACEPrintSubTotalsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACEPrintSubTotalsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 336, true);
			this.ACEPrintSubTotalsCheckBox.Name = "ACEPrintSubTotalsCheckBox";
			this.ACEPrintSubTotalsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ACEPrintSubTotalsCheckBox.TabIndex = 28;
			this.ACEPrintSubTotalsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEEnableMergeCheckBox
			// 
			this.ACEEnableMergeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACEEnableMergeCheckBox, "US_DRWEnableMerge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWEnableMerge)));
			this.ACEEnableMergeCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6008df4a-b5d4-4cc5-b94b-b0f6046723f6", "Enable Merge");
			this.ACEEnableMergeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ACEEnableMergeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACEEnableMergeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 336, true);
			this.ACEEnableMergeCheckBox.Name = "ACEEnableMergeCheckBox";
			this.ACEEnableMergeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ACEEnableMergeCheckBox.TabIndex = 27;
			this.ACEEnableMergeCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACENAFTADrawbackCountryDropEdit
			// 
			this.ACENAFTADrawbackCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACENAFTADrawbackCountryDropEdit, "US_NAFTADrawbackCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_NAFTADrawbackCountry)));
			this.ACENAFTADrawbackCountryDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2f410bf7-606f-46f6-b4ce-5ddf54a7f526", "USMCA Drawback Ctry/Rgn. Code");
			this.ACENAFTADrawbackCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 362, true);
			this.ACENAFTADrawbackCountryDropEdit.Name = "ACENAFTADrawbackCountryDropEdit";
			this.ACENAFTADrawbackCountryDropEdit.PreBoundMaxLength = 2;
			this.ACENAFTADrawbackCountryDropEdit.ShouldResizeByMaxLength = true;
			this.ACENAFTADrawbackCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.ACENAFTADrawbackCountryDropEdit.TabIndex = 29;
			// 
			// ExaminationWitnessCheckBox
			// 
			this.ExaminationWitnessCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExaminationWitnessCheckBox, "US_DRWExamWitness");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWExamWitness)));
			this.ExaminationWitnessCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("92f0520c-ca98-4e9a-83e9-94ac42090065", "Notice of Intent Ind.");
			this.ExaminationWitnessCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExaminationWitnessCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExaminationWitnessCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 312, true);
			this.ExaminationWitnessCheckBox.Name = "ExaminationWitnessCheckBox";
			this.ExaminationWitnessCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExaminationWitnessCheckBox.TabIndex = 24;
			this.ExaminationWitnessCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACENAFTAClaimCheckBox
			// 
			this.ACENAFTAClaimCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACENAFTAClaimCheckBox, "US_NAFTAClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_NAFTAClaimInd)));
			this.ACENAFTAClaimCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("052c3815-4742-407d-b965-5b29c5b201fe", "NAFTA Claim Ind.", "NAFTA Drawback Claim Indicator");
			this.ACENAFTAClaimCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ACENAFTAClaimCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACENAFTAClaimCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 313, true);
			this.ACENAFTAClaimCheckBox.Name = "ACENAFTAClaimCheckBox";
			this.ACENAFTAClaimCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ACENAFTAClaimCheckBox.TabIndex = 23;
			this.ACENAFTAClaimCheckBox.UseVisualStyleBackColor = true;
			// 
			// OilSpillTaxIndCheckBox
			// 
			this.OilSpillTaxIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OilSpillTaxIndCheckBox, "US_DRWOilSpillTaxCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWOilSpillTaxCert)));
			this.OilSpillTaxIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2ee61bee-99cc-4258-9132-e0d5203f28aa", "Oil Spill Tax Ind.", "Oil Spill Tax Indicator");
			this.OilSpillTaxIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OilSpillTaxIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OilSpillTaxIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 289, true);
			this.OilSpillTaxIndCheckBox.Name = "OilSpillTaxIndCheckBox";
			this.OilSpillTaxIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OilSpillTaxIndCheckBox.TabIndex = 22;
			this.OilSpillTaxIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// ManufacturingPetroleumCheckBox
			// 
			this.ManufacturingPetroleumCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManufacturingPetroleumCheckBox, "US_DRWElectManufPetroleumCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWElectManufPetroleumCert)));
			this.ManufacturingPetroleumCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7d3f6788-b16c-471b-9362-667b97b9b82e", "Manufacturing Petroleum", "Electronic Manufacturing Petroleum Certification");
			this.ManufacturingPetroleumCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ManufacturingPetroleumCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ManufacturingPetroleumCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 289, true);
			this.ManufacturingPetroleumCheckBox.Name = "ManufacturingPetroleumCheckBox";
			this.ManufacturingPetroleumCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ManufacturingPetroleumCheckBox.TabIndex = 21;
			this.ManufacturingPetroleumCheckBox.UseVisualStyleBackColor = true;
			// 
			// ElectronicPetroleumCheckBox
			// 
			this.ElectronicPetroleumCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ElectronicPetroleumCheckBox, "US_DRWElectPetroleumCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWElectPetroleumCert)));
			this.ElectronicPetroleumCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7223fb86-d1a0-4229-99b3-86e2c2d00510", "Electronic Petroleum Ind.", "Electronic Petroleum Certification");
			this.ElectronicPetroleumCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ElectronicPetroleumCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ElectronicPetroleumCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 290, true);
			this.ElectronicPetroleumCheckBox.Name = "ElectronicPetroleumCheckBox";
			this.ElectronicPetroleumCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ElectronicPetroleumCheckBox.TabIndex = 20;
			this.ElectronicPetroleumCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEWaiverNoticeIndCheckBox
			// 
			this.ACEWaiverNoticeIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACEWaiverNoticeIndCheckBox, "US_WaiverNoticeInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_WaiverNoticeInd)));
			this.ACEWaiverNoticeIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("19f5919c-467f-4c4e-9a76-143bd31cc380", "Waiver Notice Ind.", "Waiver Prior Notice Indicator");
			this.ACEWaiverNoticeIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ACEWaiverNoticeIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACEWaiverNoticeIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 268, true);
			this.ACEWaiverNoticeIndCheckBox.Name = "ACEWaiverNoticeIndCheckBox";
			this.ACEWaiverNoticeIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ACEWaiverNoticeIndCheckBox.TabIndex = 19;
			this.ACEWaiverNoticeIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// OneTimeWaiverIndCheckBox
			// 
			this.OneTimeWaiverIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OneTimeWaiverIndCheckBox, "US_DRWOneTimeWaiverInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWOneTimeWaiverInd)));
			this.OneTimeWaiverIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("78e4a9a9-7497-4f07-bad1-edc9fd8f60ca", "One Time Waiver Ind.", "One Time Waiver Indicator");
			this.OneTimeWaiverIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OneTimeWaiverIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OneTimeWaiverIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 268, true);
			this.OneTimeWaiverIndCheckBox.Name = "OneTimeWaiverIndCheckBox";
			this.OneTimeWaiverIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OneTimeWaiverIndCheckBox.TabIndex = 18;
			this.OneTimeWaiverIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEAcceleratedCheckBox
			// 
			this.ACEAcceleratedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACEAcceleratedCheckBox, "US_AcceleratedClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_AcceleratedClaimInd)));
			this.ACEAcceleratedCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eae56472-8e65-4cf2-b609-198e1ba347d2", "Acc. Payment Request Ind.", "Accelerated Payment Request Indicator");
			this.ACEAcceleratedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ACEAcceleratedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACEAcceleratedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 268, true);
			this.ACEAcceleratedCheckBox.Name = "ACEAcceleratedCheckBox";
			this.ACEAcceleratedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ACEAcceleratedCheckBox.TabIndex = 17;
			this.ACEAcceleratedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEBrokerGuidFindBox
			// 
			this.ACEBrokerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEBrokerGuidFindBox, "JE_OH_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_OH_NotifyParty)));
			this.ACEBrokerGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eb087953-a00e-4594-ad35-ec9fb9d05332", "4811 Party");
			this.ACEBrokerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 241, true);
			this.ACEBrokerGuidFindBox.Name = "ACEBrokerGuidFindBox";
			this.ACEBrokerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.ACEBrokerGuidFindBox.TabIndex = 16;
			// 
			// ACETotalClaimedCalcEdit
			// 
			this.ACETotalClaimedCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalClaimedCalcEdit, "TotalClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalClaimAmount)));
			this.ACETotalClaimedCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("86d4e579-8604-411c-946e-142604a96703", "Total Claimed");
			this.ACETotalClaimedCalcEdit.DecimalPlaces = 2;
			this.ACETotalClaimedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 217, true);
			this.ACETotalClaimedCalcEdit.Name = "ACETotalClaimedCalcEdit";
			this.ACETotalClaimedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalClaimedCalcEdit.TabIndex = 15;
			this.ACETotalClaimedCalcEdit.Text = "0.00";
			this.ACETotalClaimedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACETotalOtherFeesCalcEdit
			// 
			this.ACETotalOtherFeesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalOtherFeesCalcEdit, "TotalOtherClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalOtherClaimAmount)));
			this.ACETotalOtherFeesCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a174ffc8-2b23-490c-b980-d4c848c4437e", "Total Other Fees Claimed");
			this.ACETotalOtherFeesCalcEdit.DecimalPlaces = 2;
			this.ACETotalOtherFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 192, true);
			this.ACETotalOtherFeesCalcEdit.Name = "ACETotalOtherFeesCalcEdit";
			this.ACETotalOtherFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalOtherFeesCalcEdit.TabIndex = 13;
			this.ACETotalOtherFeesCalcEdit.Text = "0.00";
			this.ACETotalOtherFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACETotalDutyCalcEdit
			// 
			this.ACETotalDutyCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalDutyCalcEdit, "TotalDutyClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalDutyClaimAmount)));
			this.ACETotalDutyCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f824b2b8-b419-4929-b941-970125bcf54c", "Total Duty Claimed");
			this.ACETotalDutyCalcEdit.DecimalPlaces = 2;
			this.ACETotalDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 192, true);
			this.ACETotalDutyCalcEdit.Name = "ACETotalDutyCalcEdit";
			this.ACETotalDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalDutyCalcEdit.TabIndex = 12;
			this.ACETotalDutyCalcEdit.Text = "0.00";
			this.ACETotalDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACETotalIRTaxCalcEdit
			// 
			this.ACETotalIRTaxCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalIRTaxCalcEdit, "TotalTaxClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalTaxClaimAmount)));
			this.ACETotalIRTaxCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9cba674d-e2d8-4dce-a90d-c70ba2d236a9", "Total I.R. Tax Claimed");
			this.ACETotalIRTaxCalcEdit.DecimalPlaces = 2;
			this.ACETotalIRTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 167, true);
			this.ACETotalIRTaxCalcEdit.Name = "ACETotalIRTaxCalcEdit";
			this.ACETotalIRTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalIRTaxCalcEdit.TabIndex = 11;
			this.ACETotalIRTaxCalcEdit.Text = "0.00";
			this.ACETotalIRTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACETotalMPFCalcEdit
			// 
			this.ACETotalMPFCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalMPFCalcEdit, "TotalMPFClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalMPFClaimAmount)));
			this.ACETotalMPFCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f4a809a7-fc16-4ded-a6d1-a3f09d01ed73", "Total MPF Claimed");
			this.ACETotalMPFCalcEdit.DecimalPlaces = 2;
			this.ACETotalMPFCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 167, true);
			this.ACETotalMPFCalcEdit.Name = "ACETotalMPFCalcEdit";
			this.ACETotalMPFCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalMPFCalcEdit.TabIndex = 10;
			this.ACETotalMPFCalcEdit.Text = "0.00";
			this.ACETotalMPFCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACETotalHMFCalcEdit
			// 
			this.ACETotalHMFCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACETotalHMFCalcEdit, "TotalHMFClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).TotalHMFClaimAmount)));
			this.ACETotalHMFCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d9e71f1a-bbd9-436b-b342-a5b2e37d79ef", "Total HMF Claimed");
			this.ACETotalHMFCalcEdit.DecimalPlaces = 2;
			this.ACETotalHMFCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 142, true);
			this.ACETotalHMFCalcEdit.Name = "ACETotalHMFCalcEdit";
			this.ACETotalHMFCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ACETotalHMFCalcEdit.TabIndex = 9;
			this.ACETotalHMFCalcEdit.Text = "0.00";
			this.ACETotalHMFCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACEMethodFilingDropEdit
			// 
			this.ACEMethodFilingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEMethodFilingDropEdit, "US_DRWFilingMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWFilingMethod)));
			this.ACEMethodFilingDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5aea4e4f-b237-48e7-9651-7ed1b78406b2", "Method of Filing");
			this.ACEMethodFilingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 142, true);
			this.ACEMethodFilingDropEdit.Name = "ACEMethodFilingDropEdit";
			this.ACEMethodFilingDropEdit.PreBoundMaxLength = 1;
			this.ACEMethodFilingDropEdit.ShouldResizeByMaxLength = true;
			this.ACEMethodFilingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ACEMethodFilingDropEdit.TabIndex = 8;
			// 
			// ACEDrawbackDescriptionTextBox
			// 
			this.ACEDrawbackDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ACEDrawbackDescriptionTextBox, "JE_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_GoodsDescription)));
			this.ACEDrawbackDescriptionTextBox.CaptionResourceString = null;
			this.ACEDrawbackDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 67, true);
			this.ACEDrawbackDescriptionTextBox.Name = "ACEDrawbackDescriptionTextBox";
			this.ACEDrawbackDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.ACEDrawbackDescriptionTextBox.TabIndex = 3;
			// 
			// CommercialRulingDropEdit
			// 
			this.CommercialRulingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialRulingDropEdit, "US_DRWCommRuling");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWCommRuling)));
			this.CommercialRulingDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d03fd6fe-42f5-4d35-90a0-530798d25083", "Commercial Ruling");
			this.CommercialRulingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 42, true);
			this.CommercialRulingDropEdit.Name = "CommercialRulingDropEdit";
			this.CommercialRulingDropEdit.PreBoundMaxLength = 1;
			this.CommercialRulingDropEdit.ShouldResizeByMaxLength = true;
			this.CommercialRulingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.CommercialRulingDropEdit.TabIndex = 2;
			// 
			// DrawbackProvisionDropEdit
			// 
			this.DrawbackProvisionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DrawbackProvisionDropEdit, "US_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.US_EntryTypeList)));
			this.DrawbackProvisionDropEdit.BindToList = "AddInfoLookups+US_EntryTypeList";
			this.DrawbackProvisionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6df5321a-995a-4a11-b8af-9a5461abdbfe", "Drawback Provision");
			this.DrawbackProvisionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 18, true);
			this.DrawbackProvisionDropEdit.Name = "DrawbackProvisionDropEdit";
			this.DrawbackProvisionDropEdit.PreBoundMaxLength = 2;
			this.DrawbackProvisionDropEdit.ShouldResizeByMaxLength = true;
			this.DrawbackProvisionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.DrawbackProvisionDropEdit.TabIndex = 1;
			// 
			// ACEProcessingPortCodeFindBox
			// 
			this.ACEProcessingPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACEProcessingPortCodeFindBox, "US_PreparerDistrictPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PreparerDistrictPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).AddInfoLookups.ACEDrawbackProcessingPortCodeList)));
			this.ACEProcessingPortCodeFindBox.BindToList = "AddInfoLookups+ACEDrawbackProcessingPortCodeList";
			this.ACEProcessingPortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6e932079-8f80-4494-a147-c7295f8ffce1", "Processing Port");
			this.ACEProcessingPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 440, true);
			this.ACEProcessingPortCodeFindBox.Name = "ACEProcessingPortCodeFindBox";
			this.ACEProcessingPortCodeFindBox.PreBoundMaxLength = 4;
			this.ACEProcessingPortCodeFindBox.ShouldResizeByMaxLength = true;
			this.ACEProcessingPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ACEProcessingPortCodeFindBox.TabIndex = 35;
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("20fa9ceb-d35d-4785-b9f6-e3a20af7d059", "Custom");
			this.CustomFieldsGroupBox.Controls.Add(this.CustomFieldsDisplayControl);
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 8, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 591, true);
			this.CustomFieldsGroupBox.TabIndex = 14;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// CustomFieldsDisplayControl
			// 
			this.CustomFieldsDisplayControl.AllowDrop = true;
			this.CustomFieldsDisplayControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsDisplayControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomFieldsDisplayControl.Name = "CustomFieldsDisplayControl";
			this.CustomFieldsDisplayControl.NothingSetupMessageLabelText = "To make use of this area, please setup Drawback customized fields against Workflo" +
    "w Templates.";
			this.CustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 572, true);
			this.CustomFieldsDisplayControl.TabIndex = 0;
			// 
			// DrawbackJobDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CustomFieldsGroupBox);
			this.Controls.Add(this.ACEDrawbackPanel);
			this.Controls.Add(this.ACSRightPanelPanel);
			this.Controls.Add(this.ACSDrawbackPanel);
			this.Controls.Add(this.ExportingCarrierFindBox);
			this.Controls.Add(this.TransferorOrganizationControl);
			this.Controls.Add(this.TENoTextBox);
			this.Controls.Add(this.ImporterOrganisationControl);
			this.Name = "DrawbackJobDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1268, 631, true);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.TENoTextBox, 0);
			this.Controls.SetChildIndex(this.TransferorOrganizationControl, 0);
			this.Controls.SetChildIndex(this.ExportingCarrierFindBox, 0);
			this.Controls.SetChildIndex(this.ACSDrawbackPanel, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ACSRightPanelPanel, 0);
			this.Controls.SetChildIndex(this.ACEDrawbackPanel, 0);
			this.Controls.SetChildIndex(this.CustomFieldsGroupBox, 0);
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.ACSEntryGroupBox.ResumeLayout(false);
			this.ACSEntryGroupBox.PerformLayout();
			this.RejectedReasonDropEdit.ResumeLayout(true);
			this.RejectedReasonDropEdit.PerformLayout();
			this.DRWBrokerGuidFindBox.ResumeLayout(true);
			this.DRWBrokerGuidFindBox.PerformLayout();
			this.US_TeamNoDropEdit.ResumeLayout(true);
			this.US_TeamNoDropEdit.PerformLayout();
			this.US_ClaimPortDropEdit.ResumeLayout(true);
			this.US_ClaimPortDropEdit.PerformLayout();
			this.FilingmethodDropEdit.ResumeLayout(true);
			this.FilingmethodDropEdit.PerformLayout();
			this.US_PreparerDistrictPortFindBox.ResumeLayout(true);
			this.US_PreparerDistrictPortFindBox.PerformLayout();
			this.PeriodToDateEdit.ResumeLayout(true);
			this.PeriodToDateEdit.PerformLayout();
			this.PeriodFromDateEdit.ResumeLayout(true);
			this.PeriodFromDateEdit.PerformLayout();
			this.EntryTypeDropEdit.ResumeLayout(true);
			this.EntryTypeDropEdit.PerformLayout();
			this.NAFTADrawbackCountryDropEdit.ResumeLayout(true);
			this.NAFTADrawbackCountryDropEdit.PerformLayout();
			this.EarliestExportDateEdit.ResumeLayout(true);
			this.EarliestExportDateEdit.PerformLayout();
			this.EstimatedEntryDateDateEdit.ResumeLayout(true);
			this.EstimatedEntryDateDateEdit.PerformLayout();
			this.ExportingCarrierFindBox.ResumeLayout(true);
			this.ExportingCarrierFindBox.PerformLayout();
			this.IntendedPortOfExportCodeFindBox.ResumeLayout(true);
			this.IntendedPortOfExportCodeFindBox.PerformLayout();
			this.ContractNumberszGroupBox.ResumeLayout(false);
			this.ContractNumberszGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContractNumbersGrid)).EndInit();
			this.ContractNumbersGrid.ResumeLayout(false);
			this.ContractNumbersGrid.PerformLayout();
			this.BondGroupBox.ResumeLayout(false);
			this.BondGroupBox.PerformLayout();
			this.US_BondTypeDropEdit.ResumeLayout(true);
			this.US_BondTypeDropEdit.PerformLayout();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.DrawbackDelcarationPurposeDropEdit.ResumeLayout(true);
			this.DrawbackDelcarationPurposeDropEdit.PerformLayout();
			this.AdditionalDetailsGroupBox.ResumeLayout(false);
			this.AdditionalDetailsGroupBox.PerformLayout();
			this.TransfereeOrganisationControl.ResumeLayout(true);
			this.TransfereeOrganisationControl.PerformLayout();
			this.TransferorOrganizationControl.ResumeLayout(true);
			this.TransferorOrganizationControl.PerformLayout();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.ACSDrawbackPanel.ResumeLayout(false);
			this.ACSDrawbackPanel.PerformLayout();
			this.ACSRightPanelPanel.ResumeLayout(false);
			this.ACSRightPanelPanel.PerformLayout();
			this.ACERightPanel.ResumeLayout(false);
			this.ACERightPanel.PerformLayout();
			this.ACENoticeOfIntentGroupBox.ResumeLayout(false);
			this.ACENoticeOfIntentGroupBox.PerformLayout();
			this.ACEIntendedPortCodeFindBox.ResumeLayout(true);
			this.ACEIntendedPortCodeFindBox.PerformLayout();
			this.ProcessorGroupBox.ResumeLayout(false);
			this.ProcessorGroupBox.PerformLayout();
			this.ProcessorDateDateEdit.ResumeLayout(true);
			this.ProcessorDateDateEdit.PerformLayout();
			this.DestructionResultDropEdit.ResumeLayout(true);
			this.DestructionResultDropEdit.PerformLayout();
			this.ACEExainationGroupBox.ResumeLayout(false);
			this.ACEExainationGroupBox.PerformLayout();
			this.ExaminationDateEdit.ResumeLayout(true);
			this.ExaminationDateEdit.PerformLayout();
			this.ACEBondGroupBox.ResumeLayout(false);
			this.ACEBondGroupBox.PerformLayout();
			this.ACEWaivingReasonDropEdit.ResumeLayout(true);
			this.ACEWaivingReasonDropEdit.PerformLayout();
			this.ACEBondDesignationCodeDropEdit.ResumeLayout(true);
			this.ACEBondDesignationCodeDropEdit.PerformLayout();
			this.ACEBondTypeDropEdit.ResumeLayout(true);
			this.ACEBondTypeDropEdit.PerformLayout();
			this.ACEDrawbackPanel.ResumeLayout(false);
			this.ACEDrawbackPanel.PerformLayout();
			this.ACEEntryGroupBox.ResumeLayout(false);
			this.ACEEntryGroupBox.PerformLayout();
			this.ACEPeriodToDateEdit.ResumeLayout(true);
			this.ACEPeriodToDateEdit.PerformLayout();
			this.ACEPeriodFromDateEdit.ResumeLayout(true);
			this.ACEPeriodFromDateEdit.PerformLayout();
			this.ACEEarliestExportDateEdit.ResumeLayout(true);
			this.ACEEarliestExportDateEdit.PerformLayout();
			this.ACEEstimatedEntryDateDateEdit.ResumeLayout(true);
			this.ACEEstimatedEntryDateDateEdit.PerformLayout();
			this.ACEDrawbackTeamDropEdit.ResumeLayout(true);
			this.ACEDrawbackTeamDropEdit.PerformLayout();
			this.ACEClaimPortDropEdit.ResumeLayout(true);
			this.ACEClaimPortDropEdit.PerformLayout();
			this.ACENAFTADrawbackCountryDropEdit.ResumeLayout(true);
			this.ACENAFTADrawbackCountryDropEdit.PerformLayout();
			this.ACEBrokerGuidFindBox.ResumeLayout(true);
			this.ACEBrokerGuidFindBox.PerformLayout();
			this.ACEMethodFilingDropEdit.ResumeLayout(true);
			this.ACEMethodFilingDropEdit.PerformLayout();
			this.CommercialRulingDropEdit.ResumeLayout(true);
			this.CommercialRulingDropEdit.PerformLayout();
			this.DrawbackProvisionDropEdit.ResumeLayout(true);
			this.DrawbackProvisionDropEdit.PerformLayout();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.CustomFieldsGroupBox.PerformLayout();
			this.CustomFieldsDisplayControl.ResumeLayout(true);
			this.CustomFieldsDisplayControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous ImporterOrganisationControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ACSEntryGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit NAFTADrawbackCountryDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NAFTAClaimIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox USMCAClaimIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PetroliumClaimIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PreInspectionIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox WaiverNoticeIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ExporterSummaryIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AcceleratedClaimIndCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EarliestExportDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EstimatedEntryDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContractNumberszGroupBox;
		private Enterprise.ZArchitecture.ZGrid ContractNumbersGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEdit EntryTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_ClaimPortDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_TeamNoDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox US_PreparerDistrictPortFindBox;
		public Enterprise.ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BondGroupBox;
		private Enterprise.ZArchitecture.ZTextBox US_SuretyCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_BondTypeDropEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox MiscOptionsGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox MessageStatusDescriptionTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PeriodToDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PeriodFromDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalPRDCCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalMPFCalcEdit;
		public Enterprise.ZArchitecture.ZTextBox DrawbackSectionTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FilingmethodDropEdit;
		private ZArchitecture.ZCalcEdit TotalOtherFeesCalcEdit;
		private ZArchitecture.ZCalcEdit TotalIRTaxCalcEdit;
		private ZArchitecture.ZCalcEdit TotalHMFCalcEdit;
		private ZArchitecture.ZCalcEdit TotalDutyClaimAmount;
		private ZArchitecture.ZLabel DashLabel;
		private ZArchitecture.ZTextBox EntryFilerTextBox;
		public ZArchitecture.ZTextBox ClientReferenceTextBox;
		private ZArchitecture.ZCalcEdit TotalClaimAmount;
		private ZArchitecture.GUI.ZButton RefreshBondButton;
		private ZArchitecture.GUI.ZDropEdit DrawbackDelcarationPurposeDropEdit;
		private ZArchitecture.GUI.ZGroupBox AdditionalDetailsGroupBox;
		private ZArchitecture.ZTextBox RulingNoTextBox;
		private ZArchitecture.ZTextBox CertOfManufactureTextBox;
		private Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous TransfereeOrganisationControl;
		private Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous TransferorOrganizationControl;
		private ZArchitecture.GUI.ZGuidFindBox DRWBrokerGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit RejectedReasonDropEdit;
		private ZArchitecture.ZTextBox TENoTextBox;
		private ZArchitecture.GUI.ZCodeFindBox IntendedPortOfExportCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ExportingCarrierFindBox;
		internal ZArchitecture.GUI.ZButton AllocateImportEntryNumberButton;
		private ZArchitecture.GUI.ZCheckBox PrintSubTotalsCheckBox;
		private ZArchitecture.GUI.ZCheckBox EnableMergeCheckBox;
		private ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		private ZArchitecture.GUI.ZPanel ACSDrawbackPanel;
		private ZArchitecture.GUI.ZPanel ACSRightPanelPanel;
		private ZArchitecture.GUI.ZPanel ACEDrawbackPanel;
		private ZArchitecture.GUI.ZGroupBox ACEEntryGroupBox;
		private ZArchitecture.GUI.ZDropEdit DrawbackProvisionDropEdit;
		private ZArchitecture.GUI.ZCheckBox ACEWaiverNoticeIndCheckBox;
		private ZArchitecture.GUI.ZCheckBox OneTimeWaiverIndCheckBox;
		private ZArchitecture.GUI.ZCheckBox ACEAcceleratedCheckBox;
		private ZArchitecture.ZTextBox ACELocationOfDescTextBox;
		private ZArchitecture.GUI.ZGuidFindBox ACEBrokerGuidFindBox;
		private ZArchitecture.ZCalcEdit ACETotalClaimedCalcEdit;
		private ZArchitecture.ZCalcEdit ACETotalOtherFeesCalcEdit;
		private ZArchitecture.ZCalcEdit ACETotalDutyCalcEdit;
		private ZArchitecture.ZCalcEdit ACETotalIRTaxCalcEdit;
		private ZArchitecture.ZCalcEdit ACETotalMPFCalcEdit;
		private ZArchitecture.ZCalcEdit ACETotalHMFCalcEdit;
		private ZArchitecture.GUI.ZDropEdit ACEMethodFilingDropEdit;
		public ZArchitecture.ZTextBox ACEDrawbackDescriptionTextBox;
		private ZArchitecture.GUI.ZDropEdit CommercialRulingDropEdit;
		private ZArchitecture.GUI.ZCheckBox ACEPrintSubTotalsCheckBox;
		private ZArchitecture.GUI.ZCheckBox ACEEnableMergeCheckBox;
		private ZArchitecture.GUI.ZDropEdit ACENAFTADrawbackCountryDropEdit;
		private ZArchitecture.GUI.ZCheckBox ExaminationWitnessCheckBox;
		private ZArchitecture.GUI.ZCheckBox ACENAFTAClaimCheckBox;
		private ZArchitecture.GUI.ZCheckBox OilSpillTaxIndCheckBox;
		private ZArchitecture.GUI.ZCheckBox ManufacturingPetroleumCheckBox;
		private ZArchitecture.GUI.ZCheckBox ElectronicPetroleumCheckBox;
		private ZArchitecture.GUI.ZDropEdit ACEDrawbackTeamDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACEClaimPortDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACEProcessingPortCodeFindBox;
		private ZArchitecture.GUI.ZPanel ACERightPanel;
		private ZArchitecture.GUI.ZButton ACERefreshBondButton;
		private ZArchitecture.GUI.ZGroupBox ProcessorGroupBox;
		private ZArchitecture.GUI.ZDateEdit ProcessorDateDateEdit;
		private ZArchitecture.ZTextBox ProcessorPhoneNoTextBox;
		private ZArchitecture.ZTextBox ProcessorBadgeNoTextBox;
		private ZArchitecture.ZTextBox ProcessorNameTextBox;
		private ZArchitecture.GUI.ZGroupBox ACEBondGroupBox;
		private ZArchitecture.GUI.ZDropEdit ACEWaivingReasonDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACEBondDesignationCodeDropEdit;
		private ZArchitecture.ZTextBox ACEAccountNoTextBox;
		private ZArchitecture.ZCalcEdit ACEBondAmountCalcEdit;
		private ZArchitecture.ZTextBox ACESuretyCodeTextBox;
		private ZArchitecture.GUI.ZDropEdit ACEBondTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox ACEExainationGroupBox;
		private ZArchitecture.GUI.ZDateEdit ExaminationDateEdit;
		private ZArchitecture.ZTextBox ExaminationPhoneNoTextBox;
		private ZArchitecture.ZTextBox ExaminationBadgeNoTextBox;
		private ZArchitecture.ZTextBox ExaminationNameTextBox;
		private ZArchitecture.GUI.ZCheckBox UnUsedWineCheckBox;
		private ZArchitecture.GUI.ZCheckBox BillOfFormulaCheckBox;
		private ZArchitecture.GUI.ZGroupBox CustomFieldsGroupBox;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CustomFieldsDisplayControl;
		private ZArchitecture.GUI.ZCheckBox DestroyedValuationCheckBox;
		private ZArchitecture.GUI.ZCheckBox RetailSalesSubstitutionCheckBox;
		private ZArchitecture.GUI.ZCheckBox SuperfundTaxCertificationCheckBox;
		private ZArchitecture.GUI.ZDropEdit DestructionResultDropEdit;
		private ZArchitecture.ZCalcEdit ACETotalOilSpillTaxCalcEdit;
		private ZArchitecture.GUI.ZGroupBox ACENoticeOfIntentGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox ACEIntendedPortCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit ACEPeriodToDateEdit;
		private ZArchitecture.GUI.ZDateEdit ACEPeriodFromDateEdit;
		private ZArchitecture.GUI.ZDateEdit ACEEarliestExportDateEdit;
		private ZArchitecture.GUI.ZDateEdit ACEEstimatedEntryDateDateEdit;
	}
}
