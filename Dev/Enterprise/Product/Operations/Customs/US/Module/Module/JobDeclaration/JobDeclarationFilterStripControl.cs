using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl()
		{
			InitializeComponent();
			InitializeGridColumns();
		}

		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(module, gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			InitializeGridColumns();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, JobInvoicingConsumerTypes.Brokerage.Code, false, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		protected override ZBool ShouldSetColorContextKeyFromParentModuleID => true;

		protected override ZBool ShouldAddCustomFieldsToGrid => false;

		protected override ZArchitecture.GUI.ZFilterStrip NewZFilterStrip() => new AdditionalFilterStrip();

		protected override ZBool ShouldPerformSearch()
		{
			if (FilterBusinessObject.ActiveModuleFilters.Any(x => x.Code == DeclarationFilterConstants.InvoiceExportDate && !x.IsEmpty))
			{
				const string message = "The search you have requested has ‘Invoice Export Date – Slow Search’. \r\n It might cause performance loss for all users. Do you want to proceed?";
				return Globals.Message.Show(message, "Slow Search", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
			}

			return base.ShouldPerformSearch();
		}

		void InitializeGridColumns()
		{
			var importerEINTextBox = new ZTextBoxColumnStyleInfo();
			importerEINTextBox.Caption = "Importer EIN";
			importerEINTextBox.ColumnName = JobDeclaration.Schema.ImporterEIN;
			importerEINTextBox.IsVisible = false;
			importerEINTextBox.ToolTip = "Importer EIN";
			FilteredGrid.ColumnStyles.Add(importerEINTextBox);

			var importerOfRecordEINTextBox = new ZTextBoxColumnStyleInfo();
			importerOfRecordEINTextBox.Caption = "Importer of Record EIN";
			importerOfRecordEINTextBox.ColumnName = JobDeclaration.Schema.ImporterOfRecordEIN;
			importerOfRecordEINTextBox.IsVisible = false;
			importerOfRecordEINTextBox.ToolTip = "Importer of Record EIN";
			ControlDpiScalingHelper.SetWidth(ref importerOfRecordEINTextBox, 125, true);
			FilteredGrid.ColumnStyles.Add(importerOfRecordEINTextBox);

			var consolidatedEntryJobNoTextBox = new ZTextBoxColumnStyleInfo();
			consolidatedEntryJobNoTextBox.Caption = "Consolidated Job No";
			consolidatedEntryJobNoTextBox.ColumnName = JobDeclaration.Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber;
			consolidatedEntryJobNoTextBox.IsVisible = false;
			consolidatedEntryJobNoTextBox.ToolTip = "Consolidated Job No";
			ControlDpiScalingHelper.SetWidth(ref consolidatedEntryJobNoTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(consolidatedEntryJobNoTextBox);

			var preparerDistrictPortTextBox = new ZTextBoxColumnStyleInfo();
			preparerDistrictPortTextBox.Caption = "Preparer District Port";
			preparerDistrictPortTextBox.ColumnName = JobDeclaration.Schema.US_PreparerDistrictPort;
			preparerDistrictPortTextBox.IsVisible = false;
			preparerDistrictPortTextBox.ToolTip = "Preparer District Port";
			ControlDpiScalingHelper.SetWidth(ref preparerDistrictPortTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(preparerDistrictPortTextBox);

			var pgaStatusTextBox = new ZTextBoxColumnStyleInfo();
			pgaStatusTextBox.Caption = "PGA Status";
			pgaStatusTextBox.ColumnName = "PGAStatus";
			ControlDpiScalingHelper.SetWidth(ref pgaStatusTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(pgaStatusTextBox);

			var pgaStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			pgaStatusDescTextBox.Caption = "PGA Status Desc.";
			pgaStatusDescTextBox.ColumnName = "PGAStatusDesc";
			pgaStatusDescTextBox.AllowMultipleMacroses = true;
			ControlDpiScalingHelper.SetWidth(ref pgaStatusDescTextBox, 190, true);
			FilteredGrid.ColumnStyles.Add(pgaStatusDescTextBox);

			var incompleteDispositionsCodesTextBox = new ZTextBoxColumnStyleInfo();
			incompleteDispositionsCodesTextBox.Caption = "Incomplete Disp. Codes";
			incompleteDispositionsCodesTextBox.ColumnName = JobDeclaration.Schema.IncompleteDispositionsCode;
			ControlDpiScalingHelper.SetWidth(ref incompleteDispositionsCodesTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(incompleteDispositionsCodesTextBox);

			var incompleteDispositionsDescTextBox = new ZTextBoxColumnStyleInfo();
			incompleteDispositionsDescTextBox.Caption = "Incomplete Disp. Desc.";
			incompleteDispositionsDescTextBox.ColumnName = JobDeclaration.Schema.IncompleteDispositionsDescription;
			ControlDpiScalingHelper.SetWidth(ref incompleteDispositionsDescTextBox, 180, true);
			FilteredGrid.ColumnStyles.Add(incompleteDispositionsDescTextBox);

			var disStatusTextBox = new ZTextBoxColumnStyleInfo();
			disStatusTextBox.Caption = "DIS Status";
			disStatusTextBox.ColumnName = JobDeclaration.Schema.DISStatus;
			disStatusTextBox.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|00C10A7F-E42A-4AA1-A07B-9FA60ACD4663", "DIS Status");
			ControlDpiScalingHelper.SetWidth(ref disStatusTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(disStatusTextBox);

			var disStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			disStatusDescriptionTextBox.Caption = "DIS Status Description";
			disStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.DISStatusDescription;
			disStatusDescriptionTextBox.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|00C10A7F-E42A-4AA1-A07B-9FA60ACD4663", "DIS Status");
			ControlDpiScalingHelper.SetWidth(ref disStatusDescriptionTextBox, 120, true);
			FilteredGrid.ColumnStyles.Add(disStatusDescriptionTextBox);

			var messageStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			messageStatusDescriptionTextBox.Caption = "Message Status Description";
			messageStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.JE_MessageStatusDescription;
			messageStatusDescriptionTextBox.ToolTip = "Description of the Message Status";
			ControlDpiScalingHelper.SetWidth(ref messageStatusDescriptionTextBox, 120, true);
			FilteredGrid.ColumnStyles.Add(messageStatusDescriptionTextBox);

			var enableCRLZCheckBox = new ZCheckBoxColumnStyleInfo();
			enableCRLZCheckBox.Caption = "Enable CRL";
			enableCRLZCheckBox.IsVisible = false;
			enableCRLZCheckBox.ColumnName = USAddInfoSchema.Constants.US_EnableCRL;
			enableCRLZCheckBox.ToolTip = "Enable Cargo Release";
			FilteredGrid.ColumnStyles.Add(enableCRLZCheckBox);

			var enableAIIZCheckBox = new ZCheckBoxColumnStyleInfo();
			enableAIIZCheckBox.Caption = "Enable AII";
			enableAIIZCheckBox.IsVisible = false;
			enableAIIZCheckBox.ColumnName = USAddInfoSchema.Constants.US_EnableAII;
			enableAIIZCheckBox.ToolTip = "Enable Electronic Invoice";
			FilteredGrid.ColumnStyles.Add(enableAIIZCheckBox);

			var ultimateConsigneeZGuidFindBox = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ultimateConsigneeZGuidFindBox.BindToList = "Lookups.ImportersList";
			ultimateConsigneeZGuidFindBox.Caption = "Consignee/Ultimate Consignee";
			ultimateConsigneeZGuidFindBox.IsVisible = false;
			ultimateConsigneeZGuidFindBox.ColumnName = JobDeclaration.Schema.ConsigneeAddressOrgPK;
			ControlDpiScalingHelper.SetWidth(ref ultimateConsigneeZGuidFindBox, 110, true);
			ultimateConsigneeZGuidFindBox.ToolTip = "Consignee/Ultimate Consignee";
			FilteredGrid.ColumnStyles.Add(ultimateConsigneeZGuidFindBox);

			var importerOfRecordZGuidFindBox = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			importerOfRecordZGuidFindBox.BindToList = "Lookups.ImportersList";
			importerOfRecordZGuidFindBox.Caption = "Importer Of Record";
			importerOfRecordZGuidFindBox.IsVisible = false;
			importerOfRecordZGuidFindBox.ColumnName = JobDeclaration.Schema.IOROrgPK;
			ControlDpiScalingHelper.SetWidth(ref importerOfRecordZGuidFindBox, 110, true);
			importerOfRecordZGuidFindBox.ToolTip = "Importer Of Record";
			FilteredGrid.ColumnStyles.Add(importerOfRecordZGuidFindBox);

			var entryTypeZTextBox = new ZTextBoxColumnStyleInfo();
			entryTypeZTextBox.Caption = "Entry Type";
			entryTypeZTextBox.IsVisible = false;
			entryTypeZTextBox.ColumnName = USAddInfoSchema.Constants.US_EntryType;
			entryTypeZTextBox.ToolTip = "Entry Type";
			ControlDpiScalingHelper.SetWidth(ref entryTypeZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(entryTypeZTextBox);

			var orderRefsZTextBox = new ZTextBoxColumnStyleInfo();
			orderRefsZTextBox.Caption = "Order Refs";
			orderRefsZTextBox.IsVisible = false;
			orderRefsZTextBox.ColumnName = "DocsAndCartage+JP_OrderItemsAsString";
			orderRefsZTextBox.ToolTip = "Order Refs";
			ControlDpiScalingHelper.SetWidth(ref orderRefsZTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(orderRefsZTextBox);

			var forwarderZGuidFindBox = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			forwarderZGuidFindBox.BindToList = "Lookups.ForwarderList";
			forwarderZGuidFindBox.Caption = "Forwarder";
			forwarderZGuidFindBox.IsVisible = false;
			forwarderZGuidFindBox.ColumnName = JobDeclaration.Schema.JE_OH_Forwarder;
			ControlDpiScalingHelper.SetWidth(ref forwarderZGuidFindBox, 90, true);
			forwarderZGuidFindBox.ToolTip = "Forwarder";
			FilteredGrid.ColumnStyles.Add(forwarderZGuidFindBox);

			var carrierZGuidFindBox = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			carrierZGuidFindBox.BindToList = "Lookups.ShippingLineList";
			carrierZGuidFindBox.Caption = "Carrier";
			carrierZGuidFindBox.IsVisible = false;
			carrierZGuidFindBox.ColumnName = JobDeclaration.Schema.JE_OH_ShippingLine;
			ControlDpiScalingHelper.SetWidth(ref carrierZGuidFindBox, 90, true);
			carrierZGuidFindBox.ToolTip = "Carrier";
			FilteredGrid.ColumnStyles.Add(carrierZGuidFindBox);

			var carrierSCACZTextBox = new ZTextBoxColumnStyleInfo();
			carrierSCACZTextBox.Caption = "Carrier SCAC";
			carrierSCACZTextBox.IsVisible = false;
			carrierSCACZTextBox.ColumnName = JobDeclaration.Schema.CarrierSCAC;
			carrierSCACZTextBox.ToolTip = "Carrier SCAC";
			ControlDpiScalingHelper.SetWidth(ref carrierSCACZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(carrierSCACZTextBox);

			var estEntryZDateEdit = new ZDateEditColumnStyleInfo();
			estEntryZDateEdit.Caption = "Est Entry Date";
			estEntryZDateEdit.IsVisible = false;
			estEntryZDateEdit.ColumnName = USAddInfoSchema.Constants.US_EstimatedEntryDate;
			ControlDpiScalingHelper.SetWidth(ref estEntryZDateEdit, 100, true);
			estEntryZDateEdit.ToolTip = "Estimated Entry Date";
			FilteredGrid.ColumnStyles.Add(estEntryZDateEdit);

			var paymentTypeZTextBox = new ZTextBoxColumnStyleInfo();
			paymentTypeZTextBox.Caption = "Payment Type";
			paymentTypeZTextBox.IsVisible = false;
			paymentTypeZTextBox.ColumnName = USAddInfoSchema.Constants.US_PaymentType;
			paymentTypeZTextBox.ToolTip = "Payment Type";
			ControlDpiScalingHelper.SetWidth(ref paymentTypeZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(paymentTypeZTextBox);

			var prelimStmtPrintZDateEdit = new ZDateEditColumnStyleInfo();
			prelimStmtPrintZDateEdit.Caption = "Prelim Stmt Print Date";
			prelimStmtPrintZDateEdit.IsVisible = false;
			prelimStmtPrintZDateEdit.ColumnName = USAddInfoSchema.Constants.US_PreliminaryStatementPrintDate;
			ControlDpiScalingHelper.SetWidth(ref prelimStmtPrintZDateEdit, 120, true);
			prelimStmtPrintZDateEdit.ToolTip = "Preliminary Statement Print Date";
			prelimStmtPrintZDateEdit.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(prelimStmtPrintZDateEdit);

			var periodicStmtMthZTextBox = new ZTextBoxColumnStyleInfo();
			periodicStmtMthZTextBox.Caption = "Periodic Statement Month";
			periodicStmtMthZTextBox.IsVisible = false;
			periodicStmtMthZTextBox.ColumnName = USAddInfoSchema.Constants.US_PeriodicStatementMM;
			periodicStmtMthZTextBox.ToolTip = "Periodic Statement Month";
			ControlDpiScalingHelper.SetWidth(ref periodicStmtMthZTextBox, 134, true);
			FilteredGrid.ColumnStyles.Add(periodicStmtMthZTextBox);

			var suretyCodeZTextBox = new ZTextBoxColumnStyleInfo();
			suretyCodeZTextBox.Caption = "Surety Code";
			suretyCodeZTextBox.IsVisible = false;
			suretyCodeZTextBox.ColumnName = USAddInfoSchema.Constants.US_SuretyCode;
			suretyCodeZTextBox.ToolTip = "Surety Code";
			ControlDpiScalingHelper.SetWidth(ref suretyCodeZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(suretyCodeZTextBox);

			var aDDCVDSuretyCodeZTextBox = new ZTextBoxColumnStyleInfo();
			aDDCVDSuretyCodeZTextBox.Caption = "ADD/CVD Surety Code";
			aDDCVDSuretyCodeZTextBox.IsVisible = false;
			aDDCVDSuretyCodeZTextBox.ColumnName = USAddInfoSchema.Constants.US_ADDCVDSuretyCode;
			aDDCVDSuretyCodeZTextBox.ToolTip = "ADD/CVD Surety Code";
			ControlDpiScalingHelper.SetWidth(ref aDDCVDSuretyCodeZTextBox, 120, true);
			FilteredGrid.ColumnStyles.Add(aDDCVDSuretyCodeZTextBox);

			var liveEntryIndicatorZTextBox = new ZTextBoxColumnStyleInfo();
			liveEntryIndicatorZTextBox.Caption = "Live Entry Ind";
			liveEntryIndicatorZTextBox.IsVisible = false;
			liveEntryIndicatorZTextBox.ColumnName = USAddInfoSchema.Constants.US_LiveEntryIndicator;
			liveEntryIndicatorZTextBox.ToolTip = "Live Entry Indicator";
			ControlDpiScalingHelper.SetWidth(ref liveEntryIndicatorZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(liveEntryIndicatorZTextBox);

			var conInfIndicatorZTextBox = new ZTextBoxColumnStyleInfo();
			conInfIndicatorZTextBox.Caption = "Con/Inf Ind";
			conInfIndicatorZTextBox.IsVisible = false;
			conInfIndicatorZTextBox.ColumnName = USAddInfoSchema.Constants.US_ConsolidatedInformalIndicator;
			conInfIndicatorZTextBox.ToolTip = "Consolidated / Informal Indicator";
			ControlDpiScalingHelper.SetWidth(ref conInfIndicatorZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(conInfIndicatorZTextBox);

			var generalOrderNoZTextBox = new ZTextBoxColumnStyleInfo();
			generalOrderNoZTextBox.Caption = "General Order #";
			generalOrderNoZTextBox.IsVisible = false;
			generalOrderNoZTextBox.ColumnName = USAddInfoSchema.Constants.US_GeneralOrderNo;
			generalOrderNoZTextBox.ToolTip = "General Order Number";
			ControlDpiScalingHelper.SetWidth(ref generalOrderNoZTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(generalOrderNoZTextBox);

			var bondProducerAccNoZTextBox = new ZTextBoxColumnStyleInfo();
			bondProducerAccNoZTextBox.Caption = "Bond Producer Acc #";
			bondProducerAccNoZTextBox.IsVisible = false;
			bondProducerAccNoZTextBox.ColumnName = USAddInfoSchema.Constants.US_BondProducerAccNo;
			bondProducerAccNoZTextBox.ToolTip = "Bond Producer Account Number";
			ControlDpiScalingHelper.SetWidth(ref bondProducerAccNoZTextBox, 120, true);
			FilteredGrid.ColumnStyles.Add(bondProducerAccNoZTextBox);

			var releaseStatusTextBox = new ZTextBoxColumnStyleInfo();
			releaseStatusTextBox.Caption = "Release Status";
			releaseStatusTextBox.IsVisible = false;
			releaseStatusTextBox.ColumnName = JobDeclaration.Schema.ReleaseStatus;
			releaseStatusTextBox.ToolTip = "Release Status";
			ControlDpiScalingHelper.SetWidth(ref releaseStatusTextBox, 85, true);
			FilteredGrid.ColumnStyles.Add(releaseStatusTextBox);

			var releaseStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			releaseStatusDescTextBox.Caption = "Release Status Desc";
			releaseStatusDescTextBox.IsVisible = false;
			releaseStatusDescTextBox.ColumnName = JobDeclaration.Schema.ReleaseStatusDesc;
			releaseStatusDescTextBox.ToolTip = "Release Status Description";
			ControlDpiScalingHelper.SetWidth(ref releaseStatusDescTextBox, 130, true);
			FilteredGrid.ColumnStyles.Add(releaseStatusDescTextBox);

			var cRLStatusTextBox = new ZTextBoxColumnStyleInfo();
			cRLStatusTextBox.Caption = "CRL Status";
			cRLStatusTextBox.IsVisible = false;
			cRLStatusTextBox.ColumnName = JobDeclaration.Schema.CargoReleaseStatus;
			cRLStatusTextBox.ToolTip = "Cargo Release Status";
			ControlDpiScalingHelper.SetWidth(ref cRLStatusTextBox, 64, true);
			FilteredGrid.ColumnStyles.Add(cRLStatusTextBox);

			var cRLStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			cRLStatusDescTextBox.Caption = "CRL Status Desc";
			cRLStatusDescTextBox.IsVisible = false;
			cRLStatusDescTextBox.ColumnName = JobDeclaration.Schema.CargoReleaseStatusDesc;
			cRLStatusDescTextBox.ToolTip = "Cargo Release Status Description";
			ControlDpiScalingHelper.SetWidth(ref cRLStatusDescTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(cRLStatusDescTextBox);

			var eNSStatusTextBox = new ZTextBoxColumnStyleInfo();
			eNSStatusTextBox.Caption = "ENS Status";
			eNSStatusTextBox.IsVisible = false;
			eNSStatusTextBox.ColumnName = JobDeclaration.Schema.EntrySummaryStatus;
			eNSStatusTextBox.ToolTip = "Entry Summary Status";
			ControlDpiScalingHelper.SetWidth(ref eNSStatusTextBox, 64, true);
			FilteredGrid.ColumnStyles.Add(eNSStatusTextBox);

			var eNSStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			eNSStatusDescTextBox.Caption = "ENS Status Desc";
			eNSStatusDescTextBox.IsVisible = false;
			eNSStatusDescTextBox.ColumnName = JobDeclaration.Schema.EntrySummaryStatusDesc;
			eNSStatusDescTextBox.ToolTip = "Entry Summary Status Description";
			ControlDpiScalingHelper.SetWidth(ref eNSStatusDescTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(eNSStatusDescTextBox);

			var inBondClosedDate = new ZTextBoxColumnStyleInfo();
			inBondClosedDate.Caption = "In-Bond Closed Date";
			inBondClosedDate.IsVisible = false;
			inBondClosedDate.ColumnName = JobDeclaration.Schema.InBondClosedDate;
			inBondClosedDate.ToolTip = "In-Bond Closed Date";
			ControlDpiScalingHelper.SetWidth(ref inBondClosedDate, 120, true);
			FilteredGrid.ColumnStyles.Add(inBondClosedDate);

			var inBondEntryType = new ZTextBoxColumnStyleInfo();
			inBondEntryType.Caption = "In-Bond Entry Type";
			inBondEntryType.IsVisible = false;
			inBondEntryType.ColumnName = JobDeclaration.Schema.InBondEntryTypes;
			inBondEntryType.ToolTip = "In-Bond Entry Type";
			ControlDpiScalingHelper.SetWidth(ref inBondEntryType, 100, true);
			FilteredGrid.ColumnStyles.Add(inBondEntryType);

			var eXPStatusTextBox = new ZTextBoxColumnStyleInfo();
			eXPStatusTextBox.Caption = "EXP Status";
			eXPStatusTextBox.IsVisible = false;
			eXPStatusTextBox.ColumnName = JobDeclaration.Schema.ExportStatus;
			eXPStatusTextBox.ToolTip = "Export Status";
			ControlDpiScalingHelper.SetWidth(ref eXPStatusTextBox, 63, true);
			FilteredGrid.ColumnStyles.Add(eXPStatusTextBox);

			ZTextBoxColumnStyleInfo exportDate = new ZDateEditColumnStyleInfo();
			exportDate.Caption = "Export Date";
			exportDate.IsVisible = false;
			exportDate.ColumnName = JobDeclaration.Schema.US_DateOfExport;
			exportDate.ToolTip = "Export Date";
			ControlDpiScalingHelper.SetWidth(ref exportDate, 100, true);
			FilteredGrid.ColumnStyles.Add(exportDate);

			var eXPStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			eXPStatusDescTextBox.Caption = "EXP Status Desc";
			eXPStatusDescTextBox.IsVisible = false;
			eXPStatusDescTextBox.ColumnName = JobDeclaration.Schema.ExportStatusDesc;
			eXPStatusDescTextBox.ToolTip = "Export Status Description";
			ControlDpiScalingHelper.SetWidth(ref eXPStatusDescTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(eXPStatusDescTextBox);

			#region NAFTA

			var nAFTAReconIndicator = new ZTextBoxColumnStyleInfo();
			nAFTAReconIndicator.Caption = "FTA Recon";
			nAFTAReconIndicator.IsVisible = false;
			nAFTAReconIndicator.ColumnName = JobDeclaration.Schema.US_NAFTAReconIndicator;
			nAFTAReconIndicator.ToolTip = "FTA Recon";
			ControlDpiScalingHelper.SetWidth(ref nAFTAReconIndicator, 170, true);
			FilteredGrid.ColumnStyles.Add(nAFTAReconIndicator);

			#endregion

			var reconIssue = new ZTextBoxColumnStyleInfo();
			reconIssue.Caption = "Recon Issue";
			reconIssue.IsVisible = false;
			reconIssue.ColumnName = JobDeclaration.Schema.US_OtherReconIndicator;
			reconIssue.ToolTip = "Recon Issue";
			ControlDpiScalingHelper.SetWidth(ref reconIssue, 100, true);
			FilteredGrid.ColumnStyles.Add(reconIssue);

			var reconIssueDescr = new ZTextBoxColumnStyleInfo();
			reconIssueDescr.Caption = "Recon Issue Descr";
			reconIssueDescr.IsVisible = false;
			reconIssueDescr.ColumnName = JobDeclaration.Schema.OtherReconIndicatorDescription;
			reconIssueDescr.ToolTip = "Recon Issue Description";
			ControlDpiScalingHelper.SetWidth(ref reconIssueDescr, 140, true);
			FilteredGrid.ColumnStyles.Add(reconIssueDescr);

			var entrySubmittedDate = new ZDateEditColumnStyleInfo();
			entrySubmittedDate.Caption = "Entry Submitted Date";
			entrySubmittedDate.IsVisible = false;
			entrySubmittedDate.ColumnName = JobDeclaration.Schema.EntrySubmittedDate;
			entrySubmittedDate.ToolTip = "Entry Submitted Date";
			ControlDpiScalingHelper.SetWidth(ref entrySubmittedDate, 120, true);
			FilteredGrid.ColumnStyles.Add(entrySubmittedDate);

			var entryReleaseDate = new ZDateEditColumnStyleInfo();
			entryReleaseDate.Caption = "Release Date";
			entryReleaseDate.IsVisible = false;
			entryReleaseDate.ColumnName = JobDeclaration.Schema.JE_EntryAuthorisationDate;
			entryReleaseDate.ToolTip = "Release Date";
			ControlDpiScalingHelper.SetWidth(ref entryReleaseDate, 100, true);
			entryReleaseDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(entryReleaseDate);

			var locationOfGoodsZTextBox = new ZTextBoxColumnStyleInfo();
			locationOfGoodsZTextBox.Caption = "Location Of Goods";
			locationOfGoodsZTextBox.IsVisible = false;
			locationOfGoodsZTextBox.ColumnName = JobDeclaration.Schema.US_US_NKLocationOfGoods;
			locationOfGoodsZTextBox.ToolTip = "Location of Goods";
			ControlDpiScalingHelper.SetWidth(ref locationOfGoodsZTextBox, 110, true);
			FilteredGrid.ColumnStyles.Add(locationOfGoodsZTextBox);

			var eIStatusZTextBox = new ZTextBoxColumnStyleInfo();
			eIStatusZTextBox.Caption = "EI Status";
			eIStatusZTextBox.IsVisible = false;
			eIStatusZTextBox.ColumnName = JobDeclaration.Schema.ElectronicInvoiceStatus;
			eIStatusZTextBox.ToolTip = "Electronic Invoice Status";
			ControlDpiScalingHelper.SetWidth(ref eIStatusZTextBox, 55, true);
			FilteredGrid.ColumnStyles.Add(eIStatusZTextBox);

			var eIStatusDescriptionZTextBox = new ZTextBoxColumnStyleInfo();
			eIStatusDescriptionZTextBox.Caption = "EI Status Desc";
			eIStatusDescriptionZTextBox.IsVisible = false;
			eIStatusDescriptionZTextBox.ColumnName = JobDeclaration.Schema.ElectronicInvoiceStatusDescription;
			eIStatusDescriptionZTextBox.ToolTip = "Electronic Invoice Status Description";
			ControlDpiScalingHelper.SetWidth(ref eIStatusDescriptionZTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(eIStatusDescriptionZTextBox);

			var paymentDueDate = new ZDateEditColumnStyleInfo();
			paymentDueDate.Caption = "Payment Due Date";
			paymentDueDate.IsVisible = false;
			paymentDueDate.ColumnName = USAddInfoSchema.Constants.US_PaymentDueDate;
			paymentDueDate.ToolTip = "Payment of Duty, Fees and Charges Due Date";
			ControlDpiScalingHelper.SetWidth(ref paymentDueDate, 101, true);
			paymentDueDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(paymentDueDate);

			var statementPaidDate = new ZDateEditColumnStyleInfo();
			statementPaidDate.Caption = "Statement Paid Date";
			statementPaidDate.IsVisible = false;
			statementPaidDate.ColumnName = JobDeclaration.Schema.StatementPaidDate;
			statementPaidDate.ToolTip = "Statement Payment Date";
			ControlDpiScalingHelper.SetWidth(ref statementPaidDate, 120, true);
			statementPaidDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(statementPaidDate);

			var totalPayableCalcEdit = new ZCalcEditColumnStyleInfo();
			totalPayableCalcEdit.Caption = "Total Duty & Fees";
			totalPayableCalcEdit.Decimals = 2;
			totalPayableCalcEdit.IsVisible = false;
			totalPayableCalcEdit.ColumnName = JobDeclaration.Schema.TotalPayable;
			totalPayableCalcEdit.ToolTip = "Total Amount of Duty and Fees Payable";
			ControlDpiScalingHelper.SetWidth(ref totalPayableCalcEdit, 100, true);
			FilteredGrid.ColumnStyles.Add(totalPayableCalcEdit);

			var statementNoTextBox = new ZTextBoxColumnStyleInfo();
			statementNoTextBox.Caption = "Statement No";
			statementNoTextBox.IsVisible = false;
			statementNoTextBox.ColumnName = JobDeclaration.Schema.StatementNo;
			statementNoTextBox.ToolTip = "Statement Number";
			ControlDpiScalingHelper.SetWidth(ref statementNoTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(statementNoTextBox);

			var statementStatusTextBox = new ZTextBoxColumnStyleInfo();
			statementStatusTextBox.Caption = "Statement Status";
			statementStatusTextBox.IsVisible = false;
			statementStatusTextBox.ColumnName = JobDeclaration.Schema.StatementStatus;
			statementStatusTextBox.ToolTip = "Statement Status";
			ControlDpiScalingHelper.SetWidth(ref statementStatusTextBox, 110, true);
			FilteredGrid.ColumnStyles.Add(statementStatusTextBox);

			var statementStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			statementStatusDescTextBox.Caption = "Statement Status Desc";
			statementStatusDescTextBox.IsVisible = false;
			statementStatusDescTextBox.ColumnName = JobDeclaration.Schema.StatementStatusDesc;
			statementStatusDescTextBox.ToolTip = "Statement Status Description";
			ControlDpiScalingHelper.SetWidth(ref statementStatusDescTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(statementStatusDescTextBox);

			var paymentStatusTextBox = new ZTextBoxColumnStyleInfo();
			paymentStatusTextBox.Caption = "Payment Status";
			paymentStatusTextBox.IsVisible = false;
			paymentStatusTextBox.ColumnName = JobDeclaration.Schema.PaymentStatus;
			paymentStatusTextBox.ToolTip = "Payment Status";
			ControlDpiScalingHelper.SetWidth(ref paymentStatusTextBox, 110, true);
			FilteredGrid.ColumnStyles.Add(paymentStatusTextBox);

			var paymentStatusDescTextBox = new ZTextBoxColumnStyleInfo();
			paymentStatusDescTextBox.Caption = "Payment Status Desc";
			paymentStatusDescTextBox.IsVisible = false;
			paymentStatusDescTextBox.ColumnName = JobDeclaration.Schema.PaymentStatusDesc;
			paymentStatusDescTextBox.ToolTip = "Payment Status Description";
			ControlDpiScalingHelper.SetWidth(ref paymentStatusDescTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(paymentStatusDescTextBox);

			var bLUStatusZTextBox = new ZTextBoxColumnStyleInfo();
			bLUStatusZTextBox.Caption = "BLU Status";
			bLUStatusZTextBox.IsVisible = false;
			bLUStatusZTextBox.ColumnName = JobDeclaration.Schema.BLUStatus;
			bLUStatusZTextBox.ToolTip = "Bill of Lading Update Message Status";
			ControlDpiScalingHelper.SetWidth(ref bLUStatusZTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(bLUStatusZTextBox);

			var bLUStatusDescriptionZTextBox = new ZTextBoxColumnStyleInfo();
			bLUStatusDescriptionZTextBox.Caption = "BLU Status Desc";
			bLUStatusDescriptionZTextBox.IsVisible = false;
			bLUStatusDescriptionZTextBox.ColumnName = JobDeclaration.Schema.BLUStatusDescription;
			bLUStatusDescriptionZTextBox.ToolTip = "Bill of Lading Update Message Status Description";
			ControlDpiScalingHelper.SetWidth(ref bLUStatusDescriptionZTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(bLUStatusDescriptionZTextBox);

			var fDAMsgStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fDAMsgStatusDescriptionTextBox.Caption = "OGA FDA Msg.Status Desc";
			fDAMsgStatusDescriptionTextBox.IsVisible = false;
			fDAMsgStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.FDAMsgStatusDescription;
			fDAMsgStatusDescriptionTextBox.ToolTip = "Description of the OGA FDA Message Status";
			ControlDpiScalingHelper.SetWidth(ref fDAMsgStatusDescriptionTextBox, 150, true);
			FilteredGrid.ColumnStyles.Add(fDAMsgStatusDescriptionTextBox);

			var fDAStatusTextBox = new ZTextBoxColumnStyleInfo();
			fDAStatusTextBox.Caption = "OGA FDA Status";
			fDAStatusTextBox.IsVisible = false;
			fDAStatusTextBox.ColumnName = JobDeclaration.Schema.FDAStatus;
			fDAStatusTextBox.ToolTip = "OGA FDA Status Code";
			ControlDpiScalingHelper.SetWidth(ref fDAStatusTextBox, 110, true);
			FilteredGrid.ColumnStyles.Add(fDAStatusTextBox);

			var fDAStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fDAStatusDescriptionTextBox.Caption = "OGA FDA Status Description";
			fDAStatusDescriptionTextBox.IsVisible = false;
			fDAStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.FDAStatusDescription;
			fDAStatusDescriptionTextBox.ToolTip = "OGA FDA Status Description";
			ControlDpiScalingHelper.SetWidth(ref fDAStatusDescriptionTextBox, 160, true);
			FilteredGrid.ColumnStyles.Add(fDAStatusDescriptionTextBox);

			var iSFBillStatusTextBox = new ZTextBoxColumnStyleInfo();
			iSFBillStatusTextBox.Caption = "ISF Bill Status";
			iSFBillStatusTextBox.IsVisible = false;
			iSFBillStatusTextBox.ColumnName = JobDeclaration.Schema.ISFBillStatus;
			ControlDpiScalingHelper.SetWidth(ref iSFBillStatusTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(iSFBillStatusTextBox);

			var iSFBillStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			iSFBillStatusDescriptionTextBox.Caption = "ISF Bill Status Description";
			iSFBillStatusDescriptionTextBox.IsVisible = false;
			iSFBillStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.ISFBillStatusDescription;
			ControlDpiScalingHelper.SetWidth(ref iSFBillStatusDescriptionTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(iSFBillStatusDescriptionTextBox);

			var entryFilerCodeTextBox = new ZTextBoxColumnStyleInfo();
			entryFilerCodeTextBox.Caption = "Filer";
			entryFilerCodeTextBox.IsVisible = false;
			entryFilerCodeTextBox.ColumnName = "US_EntryFilerCode";
			entryFilerCodeTextBox.ToolTip = "Entry Filer Code";
			ControlDpiScalingHelper.SetWidth(ref entryFilerCodeTextBox, 31, true);
			FilteredGrid.ColumnStyles.Add(entryFilerCodeTextBox);

			var iORNameTextBox = new ZTextBoxColumnStyleInfo();
			iORNameTextBox.Caption = "Importer of Record Name";
			iORNameTextBox.IsVisible = false;
			iORNameTextBox.ColumnName = JobDeclaration.Schema.IORName;
			iORNameTextBox.ToolTip = "Importer of Record Name";
			ControlDpiScalingHelper.SetWidth(ref iORNameTextBox, 160, true);
			FilteredGrid.ColumnStyles.Add(iORNameTextBox);

			var sPIAuditDateColumn = new ZDateEditColumnStyleInfo();
			sPIAuditDateColumn.Caption = "SPI Audit Date";
			sPIAuditDateColumn.IsVisible = false;
			sPIAuditDateColumn.ColumnName = JobDeclaration.Schema.SPIAuditDate;
			sPIAuditDateColumn.ToolTip = "SPI Audit Date";
			ControlDpiScalingHelper.SetWidth(ref sPIAuditDateColumn, 90, true);
			sPIAuditDateColumn.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(sPIAuditDateColumn);

			var sPIAuditUserTextColumn = new ZTextBoxColumnStyleInfo();
			sPIAuditUserTextColumn.Caption = "SPI Auditor";
			sPIAuditUserTextColumn.IsVisible = false;
			sPIAuditUserTextColumn.ColumnName = JobDeclaration.Schema.SPIAuditUser;
			sPIAuditUserTextColumn.ToolTip = "SPI Audit User";
			ControlDpiScalingHelper.SetWidth(ref sPIAuditUserTextColumn, 70, true);
			FilteredGrid.ColumnStyles.Add(sPIAuditUserTextColumn);

			var sPIAuditReferenceTextBoxColumn = new ZTextBoxColumnStyleInfo();
			sPIAuditReferenceTextBoxColumn.Caption = "SPI Audit Reference";
			sPIAuditReferenceTextBoxColumn.IsVisible = false;
			sPIAuditReferenceTextBoxColumn.ColumnName = JobDeclaration.Schema.SPIAuditReference;
			ControlDpiScalingHelper.SetWidth(ref sPIAuditReferenceTextBoxColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(sPIAuditReferenceTextBoxColumn);

			var sPIAuditUserNameTextBoxColumn = new ZTextBoxColumnStyleInfo();
			sPIAuditUserNameTextBoxColumn.Caption = "SPI Auditor Name";
			sPIAuditUserNameTextBoxColumn.ColumnName = JobDeclaration.Schema.SPIAuditUserName;
			sPIAuditUserNameTextBoxColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref sPIAuditUserNameTextBoxColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(sPIAuditUserNameTextBoxColumn);

			var fDAAuditDateColumn = new ZDateEditColumnStyleInfo();
			fDAAuditDateColumn.Caption = "FDA Audit Date";
			fDAAuditDateColumn.IsVisible = false;
			fDAAuditDateColumn.ColumnName = JobDeclaration.Schema.FDAAuditDate;
			fDAAuditDateColumn.ToolTip = "FDA Audit Date";
			ControlDpiScalingHelper.SetWidth(ref fDAAuditDateColumn, 90, true);
			fDAAuditDateColumn.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(fDAAuditDateColumn);

			var fDAAuditUserTextColumn = new ZTextBoxColumnStyleInfo();
			fDAAuditUserTextColumn.Caption = "FDA Auditor";
			fDAAuditUserTextColumn.IsVisible = false;
			fDAAuditUserTextColumn.ColumnName = JobDeclaration.Schema.FDAAuditUser;
			fDAAuditUserTextColumn.ToolTip = "FDA Audit User";
			ControlDpiScalingHelper.SetWidth(ref fDAAuditUserTextColumn, 70, true);
			FilteredGrid.ColumnStyles.Add(fDAAuditUserTextColumn);

			var fDAAuditReferenceTextBoxColumn = new ZTextBoxColumnStyleInfo();
			fDAAuditReferenceTextBoxColumn.Caption = "FDA Audit Reference";
			fDAAuditReferenceTextBoxColumn.IsVisible = false;
			fDAAuditReferenceTextBoxColumn.ColumnName = JobDeclaration.Schema.FDAAuditReference;
			ControlDpiScalingHelper.SetWidth(ref fDAAuditReferenceTextBoxColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(fDAAuditReferenceTextBoxColumn);

			var fDAAuditUserNameTextBoxColumn = new ZTextBoxColumnStyleInfo();
			fDAAuditUserNameTextBoxColumn.Caption = "FDA Auditor Name";
			fDAAuditUserNameTextBoxColumn.ColumnName = JobDeclaration.Schema.FDAAuditUserName;
			fDAAuditUserNameTextBoxColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref fDAAuditUserNameTextBoxColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(fDAAuditUserNameTextBoxColumn);

			var cwoAuditDateColumn = new ZDateEditColumnStyleInfo();
			cwoAuditDateColumn.Caption = "CW Audit Date";
			cwoAuditDateColumn.IsVisible = false;
			cwoAuditDateColumn.ColumnName = JobDeclaration.Schema.CWAuditDate;
			ControlDpiScalingHelper.SetWidth(ref cwoAuditDateColumn, 90, true);
			cwoAuditDateColumn.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(cwoAuditDateColumn);

			var cwoAuditUserTextColumn = new ZTextBoxColumnStyleInfo();
			cwoAuditUserTextColumn.Caption = "CW Auditor";
			cwoAuditUserTextColumn.IsVisible = false;
			cwoAuditUserTextColumn.ColumnName = JobDeclaration.Schema.CWAuditUser;
			ControlDpiScalingHelper.SetWidth(ref cwoAuditUserTextColumn, 70, true);
			FilteredGrid.ColumnStyles.Add(cwoAuditUserTextColumn);

			var cwoAuditReferenceTextBoxColumn = new ZTextBoxColumnStyleInfo();
			cwoAuditReferenceTextBoxColumn.Caption = "CW Audit Reference";
			cwoAuditReferenceTextBoxColumn.IsVisible = false;
			cwoAuditReferenceTextBoxColumn.ColumnName = JobDeclaration.Schema.CWAuditReference;
			ControlDpiScalingHelper.SetWidth(ref cwoAuditReferenceTextBoxColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(cwoAuditReferenceTextBoxColumn);

			var cwoAuditUserNameTextBoxColumn = new ZTextBoxColumnStyleInfo();
			cwoAuditUserNameTextBoxColumn.Caption = "CW Auditor Name";
			cwoAuditUserNameTextBoxColumn.ColumnName = JobDeclaration.Schema.CWAuditUserName;
			cwoAuditUserNameTextBoxColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref cwoAuditUserNameTextBoxColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(cwoAuditUserNameTextBoxColumn);

			var liquidationDate = new ZDateEditColumnStyleInfo();
			liquidationDate.Caption = "Liquidation Date";
			liquidationDate.IsVisible = false;
			liquidationDate.ColumnName = JobDeclaration.Schema.LiquidationDate;
			liquidationDate.ToolTip = "Liquidation Date";
			ControlDpiScalingHelper.SetWidth(ref liquidationDate, 120, true);
			liquidationDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(liquidationDate);

			var bIRDRefNoTextColumn = new ZTextBoxColumnStyleInfo();
			bIRDRefNoTextColumn.Caption = "BIRD Ref No";
			bIRDRefNoTextColumn.IsVisible = false;
			bIRDRefNoTextColumn.ColumnName = JobDeclaration.Schema.US_BRDRefNo;
			bIRDRefNoTextColumn.ToolTip = "BIRD Originating Broker Ref";
			ControlDpiScalingHelper.SetWidth(ref bIRDRefNoTextColumn, 71, true);
			FilteredGrid.ColumnStyles.Add(bIRDRefNoTextColumn);

			var loadingSchedK = new ZTextBoxColumnStyleInfo();
			loadingSchedK.Caption = "Loading (Sched D/K)";
			loadingSchedK.IsVisible = false;
			loadingSchedK.ColumnName = JobDeclaration.Schema.US_SchDLoading;
			loadingSchedK.ToolTip = "Loading Port(Sched D/K)";
			ControlDpiScalingHelper.SetWidth(ref loadingSchedK, 130, true);
			FilteredGrid.ColumnStyles.Add(loadingSchedK);

			var dischargeSchedD = new ZTextBoxColumnStyleInfo();
			dischargeSchedD.Caption = "Discharge (Sched D/K)";
			dischargeSchedD.IsVisible = false;
			dischargeSchedD.ColumnName = JobDeclaration.Schema.US_SchDArrival;
			dischargeSchedD.ToolTip = "Discharge Port(Sched D/K)";
			ControlDpiScalingHelper.SetWidth(ref dischargeSchedD, 130, true);
			FilteredGrid.ColumnStyles.Add(dischargeSchedD);

			var entrySchedD = new ZTextBoxColumnStyleInfo();
			entrySchedD.Caption = "Entry Port (Sched D)";
			entrySchedD.IsVisible = false;
			entrySchedD.ColumnName = JobDeclaration.Schema.US_SchDEntry;
			entrySchedD.ToolTip = "Entry Port (Sched D)";
			ControlDpiScalingHelper.SetWidth(ref entrySchedD, 130, true);
			FilteredGrid.ColumnStyles.Add(entrySchedD);

			var exportPort = new ZTextBoxColumnStyleInfo();
			exportPort.Caption = "Export Port";
			exportPort.IsVisible = false;
			exportPort.ColumnName = JobDeclaration.Schema.US_SchDExport;
			exportPort.ToolTip = "Export Port";
			FilteredGrid.ColumnStyles.Add(exportPort);

			var entryMode = new ZTextBoxColumnStyleInfo();
			entryMode.Caption = "Mode";
			entryMode.IsVisible = false;
			entryMode.ColumnName = JobDeclaration.Schema.US_EntryMode;
			entryMode.ToolTip = "Mode";
			ControlDpiScalingHelper.SetWidth(ref entryMode, 60, true);
			FilteredGrid.ColumnStyles.Add(entryMode);

			var paperlessEntry = new ZTextBoxColumnStyleInfo();
			paperlessEntry.Caption = "Paperless";
			paperlessEntry.IsVisible = false;
			paperlessEntry.ColumnName = JobDeclaration.Schema.US_PaperlessEntry;
			paperlessEntry.ToolTip = "Is Paperless Entry";
			ControlDpiScalingHelper.SetWidth(ref paperlessEntry, 70, true);
			FilteredGrid.ColumnStyles.Add(paperlessEntry);

			var tIBExpiryDate = new ZDateEditColumnStyleInfo();
			tIBExpiryDate.Caption = "TIB Expiry Date";
			tIBExpiryDate.IsVisible = false;
			tIBExpiryDate.ColumnName = JobDeclaration.Schema.TIBExpiryDate;
			tIBExpiryDate.ToolTip = "TIB Expiry Date";
			ControlDpiScalingHelper.SetWidth(ref tIBExpiryDate, 92, true);
			tIBExpiryDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(tIBExpiryDate);

			var tIBNumOfExtensions = new ZTextBoxColumnStyleInfo();
			tIBNumOfExtensions.Caption = "TIB No Of Extensions";
			tIBNumOfExtensions.IsVisible = false;
			tIBNumOfExtensions.ColumnName = JobDeclaration.Schema.TIBNumOfExtensions;
			tIBNumOfExtensions.ToolTip = "TIB Number Of Extensions";
			ControlDpiScalingHelper.SetWidth(ref tIBNumOfExtensions, 115, true);
			FilteredGrid.ColumnStyles.Add(tIBNumOfExtensions);

			var tIBClosedDateColumn = new ZDateEditColumnStyleInfo();
			tIBClosedDateColumn.Caption = "TIB Closed Date";
			tIBClosedDateColumn.IsVisible = false;
			tIBClosedDateColumn.ColumnName = JobDeclaration.Schema.TIBClosedDate;
			tIBClosedDateColumn.ToolTip = "TIB Closed Date";
			ControlDpiScalingHelper.SetWidth(ref tIBClosedDateColumn, 92, true);
			tIBClosedDateColumn.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(tIBClosedDateColumn);

			var tIBCloseUserTextColumn = new ZTextBoxColumnStyleInfo();
			tIBCloseUserTextColumn.Caption = "TIB Closed User";
			tIBCloseUserTextColumn.IsVisible = false;
			tIBCloseUserTextColumn.ColumnName = JobDeclaration.Schema.TIBClosedUser;
			tIBCloseUserTextColumn.ToolTip = "User Who Closed TIB Entry";
			ControlDpiScalingHelper.SetWidth(ref tIBCloseUserTextColumn, 100, true);
			FilteredGrid.ColumnStyles.Add(tIBCloseUserTextColumn);

			var tIBCloseUserNameTextColumn = new ZTextBoxColumnStyleInfo();
			tIBCloseUserNameTextColumn.Caption = "TIB Closed User Name";
			tIBCloseUserNameTextColumn.IsVisible = false;
			tIBCloseUserNameTextColumn.ColumnName = JobDeclaration.Schema.TIBClosedUserName;
			tIBCloseUserNameTextColumn.ToolTip = "User Who Closed TIB Entry";
			ControlDpiScalingHelper.SetWidth(ref tIBCloseUserNameTextColumn, 120, true);
			FilteredGrid.ColumnStyles.Add(tIBCloseUserNameTextColumn);

			var tIBReferenceTextColumn = new ZTextBoxColumnStyleInfo();
			tIBReferenceTextColumn.Caption = "TIB Closed Reference";
			tIBReferenceTextColumn.IsVisible = false;
			tIBReferenceTextColumn.ColumnName = JobDeclaration.Schema.TIBClosedReference;
			tIBReferenceTextColumn.ToolTip = "TIB Closed Reference";
			ControlDpiScalingHelper.SetWidth(ref tIBReferenceTextColumn, 130, true);
			FilteredGrid.ColumnStyles.Add(tIBReferenceTextColumn);

			var paymentByBroker = new ZTextBoxColumnStyleInfo();
			paymentByBroker.Caption = "Payment By Broker";
			paymentByBroker.IsVisible = false;
			paymentByBroker.ColumnName = JobDeclaration.Schema.BrokerToPayIndicator;
			paymentByBroker.ToolTip = "Payment By Broker Indicator";
			ControlDpiScalingHelper.SetWidth(ref paymentByBroker, 105, true);
			FilteredGrid.ColumnStyles.Add(paymentByBroker);

			var applicationCode = new ZTextBoxColumnStyleInfo();
			applicationCode.Caption = "Message Mode";
			applicationCode.IsVisible = false;
			applicationCode.ColumnName = JobDeclaration.Schema.JE_ApplicationCode;
			applicationCode.ToolTip = "Message Mode";
			ControlDpiScalingHelper.SetWidth(ref applicationCode, 90, true);
			FilteredGrid.ColumnStyles.Add(applicationCode);

			var notifyParty = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			notifyParty.Caption = "Notify Party";
			notifyParty.IsVisible = false;
			notifyParty.ColumnName = JobDeclaration.Schema.JE_OH_NotifyParty;
			notifyParty.ToolTip = "Notify Party";
			ControlDpiScalingHelper.SetWidth(ref notifyParty, 100, true);
			FilteredGrid.ColumnStyles.Add(notifyParty);

			var soldToParty = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			soldToParty.Caption = "Sold To Party";
			soldToParty.IsVisible = false;
			soldToParty.ColumnName = JobDeclaration.Schema.SoldToPartyOrgPK;
			soldToParty.ToolTip = "Sold To Party";
			ControlDpiScalingHelper.SetWidth(ref soldToParty, 100, true);
			FilteredGrid.ColumnStyles.Add(soldToParty);

			var pscIndicator = new ZCheckBoxColumnStyleInfo();
			pscIndicator.Caption = "PSC";
			pscIndicator.IsVisible = false;
			pscIndicator.ColumnName = JobDeclaration.Schema.US_PSC;
			pscIndicator.ToolTip = "PSC Indicator";
			ControlDpiScalingHelper.SetWidth(ref pscIndicator, 30, true);
			FilteredGrid.ColumnStyles.Add(pscIndicator);

			var supersedingBond = new ZCheckBoxColumnStyleInfo();
			supersedingBond.Caption = "Superseding Bond";
			supersedingBond.IsVisible = false;
			supersedingBond.ColumnName = JobDeclaration.Schema.US_BondSuperseding;
			supersedingBond.ToolTip = "Superseding Bond";
			ControlDpiScalingHelper.SetWidth(ref supersedingBond, 100, true);
			FilteredGrid.ColumnStyles.Add(supersedingBond);

			var bondWaivingReason = new ZTextBoxColumnStyleInfo();
			bondWaivingReason.Caption = "Bond Waiving Reason";
			bondWaivingReason.IsVisible = false;
			bondWaivingReason.ColumnName = JobDeclaration.Schema.US_BondWaiverCode;
			bondWaivingReason.ToolTip = "Bond Waiving Reason";
			ControlDpiScalingHelper.SetWidth(ref bondWaivingReason, 120, true);
			FilteredGrid.ColumnStyles.Add(bondWaivingReason);

			var presentationDate = new ZDateEditColumnStyleInfo();
			presentationDate.Caption = "Presentation Date";
			presentationDate.IsVisible = false;
			presentationDate.ColumnName = JobDeclaration.Schema.US_PresentationDate;
			presentationDate.ToolTip = "Presentation Date";
			ControlDpiScalingHelper.SetWidth(ref presentationDate, 100, true);
			presentationDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			FilteredGrid.ColumnStyles.Add(presentationDate);

			var entryDate = new ZDateEditColumnStyleInfo();
			entryDate.Caption = "Date at Entry Port";
			entryDate.IsVisible = false;
			entryDate.ColumnName = JobDeclaration.Schema.US_EntryDate;
			entryDate.ToolTip = "Date at Entry Port";
			ControlDpiScalingHelper.SetWidth(ref entryDate, 105, true);
			FilteredGrid.ColumnStyles.Add(entryDate);

			var checkNo = new ZTextBoxColumnStyleInfo();
			checkNo.Caption = "Check No";
			checkNo.IsVisible = false;
			checkNo.ColumnName = JobDeclaration.Schema.US_CheckNo;
			checkNo.ToolTip = "Check Number";
			ControlDpiScalingHelper.SetWidth(ref checkNo, 100, true);
			FilteredGrid.ColumnStyles.Add(checkNo);

			var jobStatus = new ZTextBoxColumnStyleInfo();
			jobStatus.Caption = "Job Status";
			jobStatus.IsVisible = false;
			jobStatus.ColumnName = "Job+JH_Status";
			jobStatus.ToolTip = "Job Status";
			ControlDpiScalingHelper.SetWidth(ref jobStatus, 90, true);
			FilteredGrid.ColumnStyles.Add(jobStatus);

			var shipperReferenceNumber = new ZTextBoxColumnStyleInfo();
			shipperReferenceNumber.Caption = DeclarationFilterConstants.ShipperReferenceNumber;
			shipperReferenceNumber.ColumnName = JobDeclaration.Schema.ShipperReferenceNumber;
			shipperReferenceNumber.ToolTip = "Shipper Reference Number (AES)";
			ControlDpiScalingHelper.SetWidth(ref shipperReferenceNumber, 120, true);
			FilteredGrid.ColumnStyles.Add(shipperReferenceNumber);

			var fTZAdmissionStatusTextBox = new ZTextBoxColumnStyleInfo();
			fTZAdmissionStatusTextBox.Caption = "FTZ Adm. Status";
			fTZAdmissionStatusTextBox.IsVisible = false;
			fTZAdmissionStatusTextBox.ColumnName = JobDeclaration.Schema.AdmissionStatus;
			fTZAdmissionStatusTextBox.ToolTip = "FTZ Admission Status Code";
			ControlDpiScalingHelper.SetWidth(ref fTZAdmissionStatusTextBox, 90, true);
			FilteredGrid.ColumnStyles.Add(fTZAdmissionStatusTextBox);

			var fTZAdmissionStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fTZAdmissionStatusDescriptionTextBox.Caption = "FTZ Adm. Status Description";
			fTZAdmissionStatusDescriptionTextBox.IsVisible = false;
			fTZAdmissionStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.AdmissionStatusDescription;
			fTZAdmissionStatusDescriptionTextBox.ToolTip = "FTZ Admission Status Description";
			ControlDpiScalingHelper.SetWidth(ref fTZAdmissionStatusDescriptionTextBox, 160, true);
			FilteredGrid.ColumnStyles.Add(fTZAdmissionStatusDescriptionTextBox);

			var fTZConcurrenceStatusTextBox = new ZTextBoxColumnStyleInfo();
			fTZConcurrenceStatusTextBox.Caption = "FTZ Concur. Status";
			fTZConcurrenceStatusTextBox.IsVisible = false;
			fTZConcurrenceStatusTextBox.ColumnName = JobDeclaration.Schema.FTZConcurrenceStatus;
			fTZConcurrenceStatusTextBox.ToolTip = "FTZ Concurrence Status Code";
			ControlDpiScalingHelper.SetWidth(ref fTZConcurrenceStatusTextBox, 100, true);
			FilteredGrid.ColumnStyles.Add(fTZConcurrenceStatusTextBox);

			var fTZConcurrenceStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fTZConcurrenceStatusDescriptionTextBox.Caption = "FTZ Concur. Status Description";
			fTZConcurrenceStatusDescriptionTextBox.IsVisible = false;
			fTZConcurrenceStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.FTZConcurrenceStatusDescription;
			fTZConcurrenceStatusDescriptionTextBox.ToolTip = "FTZ Concurrence Status Description";
			ControlDpiScalingHelper.SetWidth(ref fTZConcurrenceStatusDescriptionTextBox, 170, true);
			FilteredGrid.ColumnStyles.Add(fTZConcurrenceStatusDescriptionTextBox);

			var fTZDeliveryOfGoodsStatusTextBox = new ZTextBoxColumnStyleInfo();
			fTZDeliveryOfGoodsStatusTextBox.Caption = "FTZ Delivery Of Goods Status";
			fTZDeliveryOfGoodsStatusTextBox.IsVisible = false;
			fTZDeliveryOfGoodsStatusTextBox.ColumnName = JobDeclaration.Schema.FTZDeliveryOfGoodsStatus;
			fTZDeliveryOfGoodsStatusTextBox.ToolTip = "FTZ Delivery Of Goods Status Code";
			ControlDpiScalingHelper.SetWidth(ref fTZDeliveryOfGoodsStatusTextBox, 160, true);
			FilteredGrid.ColumnStyles.Add(fTZDeliveryOfGoodsStatusTextBox);

			var fTZDeliveryOfGoodsStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fTZDeliveryOfGoodsStatusDescriptionTextBox.Caption = "FTZ Delivery Of Goods Status Description";
			fTZDeliveryOfGoodsStatusDescriptionTextBox.IsVisible = false;
			fTZDeliveryOfGoodsStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.FTZDeliveryOfGoodsStatusDescription;
			fTZDeliveryOfGoodsStatusDescriptionTextBox.ToolTip = "FTZ Delivery Of Goods Status Description";
			ControlDpiScalingHelper.SetWidth(ref fTZDeliveryOfGoodsStatusDescriptionTextBox, 220, true);
			FilteredGrid.ColumnStyles.Add(fTZDeliveryOfGoodsStatusDescriptionTextBox);

			var fTZGoodsArrivalStatusTextBox = new ZTextBoxColumnStyleInfo();
			fTZGoodsArrivalStatusTextBox.Caption = "FTZ Goods Arrival Status";
			fTZGoodsArrivalStatusTextBox.IsVisible = false;
			fTZGoodsArrivalStatusTextBox.ColumnName = JobDeclaration.Schema.FTZArrivalStatus;
			fTZGoodsArrivalStatusTextBox.ToolTip = "FTZ Goods Arrival Status Code";
			ControlDpiScalingHelper.SetWidth(ref fTZGoodsArrivalStatusTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(fTZGoodsArrivalStatusTextBox);

			var fTZGoodsArrivalStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fTZGoodsArrivalStatusDescriptionTextBox.Caption = "FTZ Goods Arrival Status Description";
			fTZGoodsArrivalStatusDescriptionTextBox.IsVisible = false;
			fTZGoodsArrivalStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.FTZArrivalStatusDescription;
			fTZGoodsArrivalStatusDescriptionTextBox.ToolTip = "FTZ Goods Arrival Status Description";
			ControlDpiScalingHelper.SetWidth(ref fTZGoodsArrivalStatusDescriptionTextBox, 190, true);
			FilteredGrid.ColumnStyles.Add(fTZGoodsArrivalStatusDescriptionTextBox);

			var fTZPTTStatusTextBox = new ZTextBoxColumnStyleInfo();
			fTZPTTStatusTextBox.Caption = "FTZ PTT Status";
			fTZPTTStatusTextBox.IsVisible = false;
			fTZPTTStatusTextBox.ColumnName = JobDeclaration.Schema.FTZPTTStatus;
			fTZPTTStatusTextBox.ToolTip = "FTZ PTT Status Code";
			ControlDpiScalingHelper.SetWidth(ref fTZPTTStatusTextBox, 90, true);
			FilteredGrid.ColumnStyles.Add(fTZPTTStatusTextBox);

			var fTZPTTStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			fTZPTTStatusDescriptionTextBox.Caption = "FTZ PTT Status Description";
			fTZPTTStatusDescriptionTextBox.IsVisible = false;
			fTZPTTStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.FTZPTTStatusDescription;
			fTZPTTStatusDescriptionTextBox.ToolTip = "FTZ PTT Status Description";
			ControlDpiScalingHelper.SetWidth(ref fTZPTTStatusDescriptionTextBox, 160, true);
			FilteredGrid.ColumnStyles.Add(fTZPTTStatusDescriptionTextBox);

			var fTZAdmNumberTextBox = new ZTextBoxColumnStyleInfo();
			fTZAdmNumberTextBox.Caption = "FTZ Admission Number";
			fTZAdmNumberTextBox.IsVisible = false;
			fTZAdmNumberTextBox.ColumnName = JobDeclaration.Schema.FTZAdmissionNumberFormatted;
			fTZAdmNumberTextBox.ToolTip = "FTZ Admission Number";
			ControlDpiScalingHelper.SetWidth(ref fTZAdmNumberTextBox, 130, true);
			FilteredGrid.ColumnStyles.Add(fTZAdmNumberTextBox);

			var anticipatedLiquidationDateEdit = new ZDateEditColumnStyleInfo();
			anticipatedLiquidationDateEdit.Caption = "Anticip. Liquidation Date";
			anticipatedLiquidationDateEdit.IsVisible = false;
			anticipatedLiquidationDateEdit.ColumnName = JobDeclaration.Schema.US_AnticipatedLiquidationDate;
			ControlDpiScalingHelper.SetWidth(ref anticipatedLiquidationDateEdit, 140, true);
			anticipatedLiquidationDateEdit.ToolTip = "Anticipated Liquidation Date";
			FilteredGrid.ColumnStyles.Add(anticipatedLiquidationDateEdit);

			var collectionDateEdit = new ZDateEditColumnStyleInfo();
			collectionDateEdit.Caption = "Collection Date";
			collectionDateEdit.IsVisible = false;
			collectionDateEdit.ColumnName = JobDeclaration.Schema.US_CollectionDate;
			ControlDpiScalingHelper.SetWidth(ref collectionDateEdit, 100, true);
			collectionDateEdit.ToolTip = "Collection Date";
			FilteredGrid.ColumnStyles.Add(collectionDateEdit);

			var sEBillStatusTextBox = new ZTextBoxColumnStyleInfo();
			sEBillStatusTextBox.Caption = "ACE CRL Bill Status";
			sEBillStatusTextBox.IsVisible = false;
			sEBillStatusTextBox.ColumnName = JobDeclaration.Schema.SimplifiedEntryBillStatus;
			sEBillStatusTextBox.ToolTip = "ACE Cargo Release Bill Status";
			ControlDpiScalingHelper.SetWidth(ref sEBillStatusTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(sEBillStatusTextBox);

			var sEBillStatusDescriptionTextBox = new ZTextBoxColumnStyleInfo();
			sEBillStatusDescriptionTextBox.Caption = "ACE CRL Bill Status Desc";
			sEBillStatusDescriptionTextBox.IsVisible = false;
			sEBillStatusDescriptionTextBox.ColumnName = JobDeclaration.Schema.SimplifiedEntryBillStatusDescription;
			sEBillStatusDescriptionTextBox.ToolTip = "ACE Cargo Release Bill Status Description";
			ControlDpiScalingHelper.SetWidth(ref sEBillStatusDescriptionTextBox, 140, true);
			FilteredGrid.ColumnStyles.Add(sEBillStatusDescriptionTextBox);

			var holdExamStatusTextBox = new ZTextBoxColumnStyleInfo();
			holdExamStatusTextBox.Caption = "Hold/Exam";
			holdExamStatusTextBox.IsVisible = false;
			holdExamStatusTextBox.ColumnName = JobDeclaration.Schema.HLDOrEXMStatus;
			holdExamStatusTextBox.ToolTip = "Hold/Exam";
			ControlDpiScalingHelper.SetWidth(ref holdExamStatusTextBox, 80, true);
			FilteredGrid.ColumnStyles.Add(holdExamStatusTextBox);

			var deferredTaxDueZDateEdit = new ZDateEditColumnStyleInfo();
			deferredTaxDueZDateEdit.Caption = "Def. Tax Due Date";
			deferredTaxDueZDateEdit.IsVisible = false;
			deferredTaxDueZDateEdit.ColumnName = USAddInfoSchema.Constants.US_DeferredTaxDueDate;
			ControlDpiScalingHelper.SetWidth(ref deferredTaxDueZDateEdit, 110, true);
			deferredTaxDueZDateEdit.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			deferredTaxDueZDateEdit.ToolTip = "Deferred Tax Due Date";
			FilteredGrid.ColumnStyles.Add(deferredTaxDueZDateEdit);

			var taxDeferIndicator = new ZTextBoxColumnStyleInfo();
			taxDeferIndicator.Caption = "Tax Deferrable Ind.";
			taxDeferIndicator.IsVisible = false;
			taxDeferIndicator.ColumnName = USAddInfoSchema.Constants.US_TaxDeferIndicator;
			taxDeferIndicator.ToolTip = "Tax Deferrable Indicator";
			ControlDpiScalingHelper.SetWidth(ref taxDeferIndicator, 110, true);
			FilteredGrid.ColumnStyles.Add(taxDeferIndicator);

			var impSpecialistTeamTextBox = new ZTextBoxColumnStyleInfo();
			impSpecialistTeamTextBox.Caption = "Import Specialist Team";
			impSpecialistTeamTextBox.IsVisible = false;
			impSpecialistTeamTextBox.ColumnName = USAddInfoSchema.Constants.US_TeamNo;
			impSpecialistTeamTextBox.ToolTip = "Import Specialist Team Number";
			ControlDpiScalingHelper.SetWidth(ref impSpecialistTeamTextBox, 125, true);
			FilteredGrid.ColumnStyles.Add(impSpecialistTeamTextBox);

			var cargoReleaseTypeTextBox = new ZTextBoxColumnStyleInfo();
			cargoReleaseTypeTextBox.Caption = "Cargo Release Type";
			cargoReleaseTypeTextBox.IsVisible = false;
			cargoReleaseTypeTextBox.ColumnName = USAddInfoSchema.Constants.US_CargoReleaseType;
			cargoReleaseTypeTextBox.ToolTip = "Cargo Release Type";
			ControlDpiScalingHelper.SetWidth(ref cargoReleaseTypeTextBox, 110, true);
			FilteredGrid.ColumnStyles.Add(cargoReleaseTypeTextBox);

			var filingOption = new ZTextBoxColumnStyleInfo();
			filingOption.Caption = "Filing Opt.";
			filingOption.IsVisible = false;
			filingOption.ColumnName = USAddInfoSchema.Constants.US_CommodityFilingOption;
			filingOption.ToolTip = "Filing Option";
			FilteredGrid.ColumnStyles.Add(filingOption);

			var earliestExportDate = new ZDateEditColumnStyleInfo();
			earliestExportDate.Caption = "Earliest Inv. Exp. Date";
			earliestExportDate.IsVisible = false;
			earliestExportDate.ColumnName = JobDeclaration.Schema.EarliestExportDate;
			ControlDpiScalingHelper.SetWidth(ref earliestExportDate, 140, true);
			earliestExportDate.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			earliestExportDate.ToolTip = "Earliest Invoice Export Date";
			FilteredGrid.ColumnStyles.Add(earliestExportDate);

			var soldEnRoute = new ZTextBoxColumnStyleInfo();
			soldEnRoute.Caption = "Sold En Route";
			soldEnRoute.IsVisible = false;
			soldEnRoute.ColumnName = USAddInfoSchema.Constants.US_SoldEnRouteIndicator;
			soldEnRoute.ToolTip = "Sold En Route";
			FilteredGrid.ColumnStyles.Add(soldEnRoute);

			var eBondMessageStatus = new ZTextBoxColumnStyleInfo();
			eBondMessageStatus.Caption = "EBond Message Status";
			eBondMessageStatus.IsVisible = false;
			eBondMessageStatus.ColumnName = USAddInfoSchema.Constants.US_InsuranceDisposition;
			ControlDpiScalingHelper.SetWidth(ref eBondMessageStatus, 120, true);
			eBondMessageStatus.ToolTip = "EBond Message Status";
			FilteredGrid.ColumnStyles.Add(eBondMessageStatus);

			var baseBondDispositionCode = new ZTextBoxColumnStyleInfo();
			baseBondDispositionCode.Caption = "Basic STB Disposition";
			baseBondDispositionCode.IsVisible = false;
			baseBondDispositionCode.ColumnName = USAddInfoSchema.Constants.US_BondDispositionCode;
			ControlDpiScalingHelper.SetWidth(ref baseBondDispositionCode, 120, true);
			baseBondDispositionCode.ToolTip = "Basic STB Disposition";
			FilteredGrid.ColumnStyles.Add(baseBondDispositionCode);

			var additionalBondDispositionCode = new ZTextBoxColumnStyleInfo();
			additionalBondDispositionCode.Caption = "Additional Bond Disposition";
			additionalBondDispositionCode.IsVisible = false;
			additionalBondDispositionCode.ColumnName = USAddInfoSchema.Constants.US_BondDispositionCode2;
			ControlDpiScalingHelper.SetWidth(ref additionalBondDispositionCode, 150, true);
			additionalBondDispositionCode.ToolTip = "Additional bond Disposition";
			FilteredGrid.ColumnStyles.Add(additionalBondDispositionCode);

			var splitShipmentIndicator = new ZCheckBoxColumnStyleInfo();
			splitShipmentIndicator.Caption = "Contains Split Ship.";
			splitShipmentIndicator.IsVisible = false;
			splitShipmentIndicator.ColumnName = JobDeclaration.Schema.IsSplitShipment;
			splitShipmentIndicator.ToolTip = "Contains Split Shipments";
			ControlDpiScalingHelper.SetWidth(ref splitShipmentIndicator, 105, true);
			FilteredGrid.ColumnStyles.Add(splitShipmentIndicator);

			var pgaReplaceUpdate = new ZTextBoxColumnStyleInfo();
			pgaReplaceUpdate.Caption = "PGA Replace Update Needed";
			pgaReplaceUpdate.IsVisible = false;
			pgaReplaceUpdate.ColumnName = JobDeclaration.Constants.GenAddOnColumnFieldName.US_PGAReplaceUpdateNeeded;
			pgaReplaceUpdate.ToolTip = "PGA Replace Update Needed";
			ControlDpiScalingHelper.SetWidth(ref pgaReplaceUpdate, 105, true);
			FilteredGrid.ColumnStyles.Add(pgaReplaceUpdate);

			var pgaCorrectionStatus = new ZTextBoxColumnStyleInfo();
			pgaCorrectionStatus.Caption = "PGA Correction Status";
			pgaCorrectionStatus.IsVisible = false;
			pgaCorrectionStatus.ColumnName = JobDeclaration.Constants.GenAddOnColumnFieldName.US_PGACorrectionStatus;
			pgaCorrectionStatus.ToolTip = "PGA Correction Status";
			ControlDpiScalingHelper.SetWidth(ref pgaCorrectionStatus, 105, true);
			FilteredGrid.ColumnStyles.Add(pgaCorrectionStatus);

			var pgaCorrectionStatusDesc = new ZTextBoxColumnStyleInfo();
			pgaCorrectionStatusDesc.Caption = "PGA Correction Status Desc.";
			pgaCorrectionStatusDesc.IsVisible = false;
			pgaCorrectionStatusDesc.ColumnName = JobDeclaration.Schema.US_PGACorrectionStatusDesc;
			pgaCorrectionStatusDesc.ToolTip = "PGA Correction Status Desc.";
			ControlDpiScalingHelper.SetWidth(ref pgaCorrectionStatusDesc, 213, true);
			FilteredGrid.ColumnStyles.Add(pgaCorrectionStatusDesc);

			var quotaStatus = new ZTextBoxColumnStyleInfo();
			quotaStatus.Caption = "Quota Status";
			quotaStatus.IsVisible = false;
			quotaStatus.ColumnName = JobDeclaration.Constants.GenAddOnColumnFieldName.US_QuotaStatus;
			quotaStatus.ToolTip = "Quota Status";
			ControlDpiScalingHelper.SetWidth(ref quotaStatus, 105, true);
			FilteredGrid.ColumnStyles.Add(quotaStatus);

			var quotaStatusDesc = new ZTextBoxColumnStyleInfo();
			quotaStatusDesc.Caption = "Quota Status Desc.";
			quotaStatusDesc.IsVisible = false;
			quotaStatusDesc.ColumnName = JobDeclaration.Schema.US_QuotaStatusDesc;
			quotaStatusDesc.ToolTip = "Quota Status Desc.";
			ControlDpiScalingHelper.SetWidth(ref quotaStatusDesc, 213, true);
			FilteredGrid.ColumnStyles.Add(quotaStatusDesc);

			#region Warehouse Details
			var whsEntryFiler = new ZTextBoxColumnStyleInfo();
			whsEntryFiler.Caption = "WHS Entry Filer";
			whsEntryFiler.IsVisible = false;
			whsEntryFiler.ColumnName = JobDeclaration.Schema.US_WHSEntryFilerCode;
			whsEntryFiler.ToolTip = "Warehouse Entry Filer Code";
			whsEntryFiler.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|D0B14F47-5B07-4929-BA99-931506596C02", "Warehouse Details");
			ControlDpiScalingHelper.SetWidth(ref whsEntryFiler, 100, true);
			FilteredGrid.ColumnStyles.Add(whsEntryFiler);

			var whsEntryNumber = new ZTextBoxColumnStyleInfo();
			whsEntryNumber.Caption = "WHS Entry Number";
			whsEntryNumber.IsVisible = false;
			whsEntryNumber.ColumnName = JobDeclaration.Schema.US_WHSEntryNumber;
			whsEntryNumber.ToolTip = "Warehouse Entry Number";
			whsEntryNumber.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|D0B14F47-5B07-4929-BA99-931506596C02", "Warehouse Details");
			ControlDpiScalingHelper.SetWidth(ref whsEntryNumber, 110, true);
			FilteredGrid.ColumnStyles.Add(whsEntryNumber);

			var whsQtyInWH = new ZCalcEditColumnStyleInfo();
			whsQtyInWH.Caption = "Qty in W/H";
			whsQtyInWH.IsVisible = false;
			whsQtyInWH.ColumnName = JobDeclaration.Schema.US_QtyInWHBeforeWithdrawal;
			whsQtyInWH.ToolTip = "Bonded Amount: quantity in the warehouse account before the withdrawal.";
			whsQtyInWH.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|D0B14F47-5B07-4929-BA99-931506596C02", "Warehouse Details");
			ControlDpiScalingHelper.SetWidth(ref whsQtyInWH, 80, true);
			FilteredGrid.ColumnStyles.Add(whsQtyInWH);

			var whsWithdrawQty = new ZCalcEditColumnStyleInfo();
			whsWithdrawQty.Caption = "Withdraw Qty";
			whsWithdrawQty.IsVisible = false;
			whsWithdrawQty.ColumnName = JobDeclaration.Schema.US_QtyBeingWithdrawn;
			whsWithdrawQty.ToolTip = "Withdrawal: quantity being withdrawn in this entry.";
			whsWithdrawQty.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|D0B14F47-5B07-4929-BA99-931506596C02", "Warehouse Details");
			ControlDpiScalingHelper.SetWidth(ref whsWithdrawQty, 90, true);
			FilteredGrid.ColumnStyles.Add(whsWithdrawQty);

			var whsBalance = new ZCalcEditColumnStyleInfo();
			whsBalance.Caption = "Balance";
			whsBalance.IsVisible = false;
			whsBalance.ColumnName = JobDeclaration.Schema.US_QtyInWHAfterWithdrawal;
			whsBalance.ToolTip = "Balance: quantity remaining in the warehouse after withdrawal entry.";
			whsBalance.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|D0B14F47-5B07-4929-BA99-931506596C02", "Warehouse Details");
			ControlDpiScalingHelper.SetWidth(ref whsBalance, 70, true);
			FilteredGrid.ColumnStyles.Add(whsBalance);

			var whsIsFinalWithdrawal = new ZCheckBoxColumnStyleInfo();
			whsIsFinalWithdrawal.Caption = "Final Withdrawal";
			whsIsFinalWithdrawal.IsVisible = false;
			whsIsFinalWithdrawal.ColumnName = JobDeclaration.Schema.US_IsFinalWHS;
			whsIsFinalWithdrawal.ToolTip = "Warehouse Final Withdrawal";
			whsIsFinalWithdrawal.GroupName = Enterprise.Customs.US.Module.Res.GetData("JobDeclarationFilterStrip|D0B14F47-5B07-4929-BA99-931506596C02", "Warehouse Details");
			ControlDpiScalingHelper.SetWidth(ref whsIsFinalWithdrawal, 90, true);
			FilteredGrid.ColumnStyles.Add(whsIsFinalWithdrawal);
			#endregion

			#region AES Disposition Codes

			var aesResponseCode = new ZTextBoxColumnStyleInfo();
			aesResponseCode.Caption = "AES Response Code (current)";
			aesResponseCode.IsVisible = false;
			aesResponseCode.ColumnName = JobDeclaration.Schema.AESResponseCode;
			aesResponseCode.ToolTip = "AES Response Code (current)";
			ControlDpiScalingHelper.SetWidth(ref aesResponseCode, 90, true);
			FilteredGrid.ColumnStyles.Add(aesResponseCode);

			var aesResponseCodeDescription = new ZTextBoxColumnStyleInfo();
			aesResponseCodeDescription.Caption = "AES Response Description (current)";
			aesResponseCodeDescription.IsVisible = false;
			aesResponseCodeDescription.ColumnName = JobDeclaration.Schema.AESResponseCodeDescription;
			aesResponseCodeDescription.ToolTip = "AES Response Description (current)";
			ControlDpiScalingHelper.SetWidth(ref aesResponseCodeDescription, 90, true);
			FilteredGrid.ColumnStyles.Add(aesResponseCodeDescription);

			var aesSeverity = new ZTextBoxColumnStyleInfo();
			aesSeverity.Caption = "AES Severity (current)";
			aesSeverity.IsVisible = false;
			aesSeverity.ColumnName = JobDeclaration.Schema.AESSeverity;
			aesSeverity.ToolTip = "AES Severity (current)";
			ControlDpiScalingHelper.SetWidth(ref aesSeverity, 90, true);
			FilteredGrid.ColumnStyles.Add(aesSeverity);

			var aesSeverityDescription = new ZTextBoxColumnStyleInfo();
			aesSeverityDescription.Caption = "AES Severity Description (current)";
			aesSeverityDescription.IsVisible = false;
			aesSeverityDescription.ColumnName = JobDeclaration.Schema.AESSeverityDescription;
			aesSeverityDescription.ToolTip = "AES Severity Description (current)";
			ControlDpiScalingHelper.SetWidth(ref aesSeverityDescription, 90, true);
			FilteredGrid.ColumnStyles.Add(aesSeverityDescription);

			#endregion

			ReOrderGridColumns(FilteredGrid);
		}

		protected void ReOrderGridColumns(ZGrid grid)
		{
			var newColumnOrder = GetNewColumnOrderForGrid(grid);

			if (newColumnOrder != null && newColumnOrder.Count > 0)
			{
				grid.ColumnStyles.Clear();
				grid.ColumnStyles.AddRange(newColumnOrder);
			}
		}

		protected ZString[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new ArrayList();
					columns.AddRange(DefaultColumnsForGrid);

					columns.Add((ZString)JobDeclaration.Schema.US_EntryFilerCode);

					columns.Add((ZString)JobDeclarationSchema.Constants.JE_GS_NKCusAgent);
					columns.Add((ZString)JobDeclaration.Schema.US_EntryDate);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_ContainerMode);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_DateOfArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_DateAtFinalDestination);
					columns.Add((ZString)USAddInfoSchema.Constants.US_DateOfExport);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_OwnerRef);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RL_NKPortOfFirstArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading);
					columns.Add((ZString)JobDeclaration.Schema.JE_ETAOfDischarge);
					columns.Add((ZString)JobDeclaration.Schema.JE_ETDOfLoading);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalNoOfPacks);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalNoOfPacksPackType);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalVolume);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalVolumeUnit);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalWeight);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalWeightUnit);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_MessageStatus);
					columns.Add((ZString)JobDeclaration.Schema.JE_MessageStatusDescription);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_ScreeningStatus);

					columns.Add((ZString)JobDeclaration.Schema.JE_OH_Forwarder);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ShippingLine);

					columns.Add((ZString)JobDeclaration.Schema.JE_ApplicationCode);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_NotifyParty);
					columns.Add((ZString)JobDeclaration.Schema.SoldToPartyOrgPK);
					columns.Add((ZString)JobDeclaration.Schema.US_PSC);
					columns.Add((ZString)JobDeclaration.Schema.US_BondSuperseding);
					columns.Add((ZString)JobDeclaration.Schema.US_BondWaiverCode);

					columns.Add((ZString)"Job+JH_Status");
					columns.Add((ZString)"Job+JH_HoldReason");
					columns.Add((ZString)"Job+JH_ProfitLossReasonCode");
					columns.Add((ZString)"Job+JH_TotalProfitRevenueMargin");

					columns.Add((ZString)JobDeclaration.Schema.US_PresentationDate);
					columns.Add((ZString)USAddInfoSchema.Constants.US_EnableCRL);
					columns.Add((ZString)USAddInfoSchema.Constants.US_EnableAII);
					columns.Add((ZString)JobDeclaration.Schema.ConsigneeAddressOrgPK);
					columns.Add((ZString)JobDeclaration.Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber);
					columns.Add((ZString)JobDeclaration.Schema.IOROrgPK);
					columns.Add((ZString)USAddInfoSchema.Constants.US_EntryType);
					columns.Add((ZString)USAddInfoSchema.Constants.US_EstimatedEntryDate);
					columns.Add((ZString)USAddInfoSchema.Constants.US_PaymentType);
					columns.Add((ZString)JobDeclaration.Schema.BrokerToPayIndicator);
					columns.Add((ZString)USAddInfoSchema.Constants.US_PreliminaryStatementPrintDate);
					columns.Add((ZString)USAddInfoSchema.Constants.US_PeriodicStatementMM);
					columns.Add((ZString)USAddInfoSchema.Constants.US_SuretyCode);
					columns.Add((ZString)USAddInfoSchema.Constants.US_TeamNo);
					columns.Add((ZString)USAddInfoSchema.Constants.US_CargoReleaseType);
					columns.Add((ZString)JobDeclaration.Schema.IsSplitShipment);
					columns.Add((ZString)USAddInfoSchema.Constants.US_ADDCVDSuretyCode);
					columns.Add((ZString)USAddInfoSchema.Constants.US_LiveEntryIndicator);
					columns.Add((ZString)USAddInfoSchema.Constants.US_ConsolidatedInformalIndicator);
					columns.Add((ZString)USAddInfoSchema.Constants.US_GeneralOrderNo);
					columns.Add((ZString)USAddInfoSchema.Constants.US_BondProducerAccNo);
					columns.Add((ZString)USAddInfoSchema.Constants.US_US_NKLocationOfGoods);
					columns.Add((ZString)USAddInfoSchema.Constants.US_PaymentDueDate);

					columns.Add((ZString)"DocsAndCartage+JP_OrderItemsAsString");
					columns.Add((ZString)"WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+Milestones+LastMilestone+P9_Description");
					columns.Add((ZString)"WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding");
					columns.Add((ZString)"WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+Milestones+NextMilestone+P9_Description");
					columns.Add((ZString)"WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding");

					columns.Add((ZString)"WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+DescriptionWithReference");
					columns.Add((ZString)"WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_ActualDate");
					columns.Add((ZString)"WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+Milestones+CurrentCompanyLastMilestone+DescriptionWithReference");
					columns.Add((ZString)"WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDate");
					columns.Add((ZString)"WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+DescriptionWithReference");
					columns.Add((ZString)"WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_ScheduledDate");
					columns.Add((ZString)"WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+Milestones+CurrentCompanyNextMilestone+DescriptionWithReference");
					columns.Add((ZString)"WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDate");

					columns.Add((ZString)"PGAStatus");
					columns.Add((ZString)"PGAStatusDesc");

					columns.Add((ZString)JobDeclaration.Schema.CarrierSCAC);
					columns.Add((ZString)JobDeclaration.Schema.CargoReleaseStatus);
					columns.Add((ZString)JobDeclaration.Schema.CargoReleaseStatusDesc);
					columns.Add((ZString)JobDeclaration.Schema.SimplifiedEntryBillStatus);
					columns.Add((ZString)JobDeclaration.Schema.SimplifiedEntryBillStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.HLDOrEXMStatus);
					columns.Add((ZString)JobDeclaration.Schema.EntrySummaryStatus);
					columns.Add((ZString)JobDeclaration.Schema.EntrySummaryStatusDesc);
					columns.Add((ZString)JobDeclaration.Schema.InBondClosedDate);
					columns.Add((ZString)JobDeclaration.Schema.InBondEntryTypes);
					columns.Add((ZString)JobDeclaration.Schema.IncompleteDispositionsCode);
					columns.Add((ZString)JobDeclaration.Schema.IncompleteDispositionsDescription);
					columns.Add((ZString)JobDeclaration.Schema.ExportStatus);
					columns.Add((ZString)JobDeclaration.Schema.ExportStatusDesc);
					columns.Add((ZString)JobDeclaration.Schema.US_NAFTAReconIndicator);
					columns.Add((ZString)JobDeclaration.Schema.EntrySubmittedDate);
					columns.Add((ZString)JobDeclaration.Schema.JE_EntryAuthorisationDate);
					columns.Add((ZString)JobDeclaration.Schema.ElectronicInvoiceStatus);
					columns.Add((ZString)JobDeclaration.Schema.ElectronicInvoiceStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.StatementPaidDate);
					columns.Add((ZString)JobDeclaration.Schema.TotalPayable);
					columns.Add((ZString)JobDeclaration.Schema.StatementNo);
					columns.Add((ZString)JobDeclaration.Schema.StatementStatus);
					columns.Add((ZString)JobDeclaration.Schema.StatementStatusDesc);
					columns.Add((ZString)JobDeclaration.Schema.PaymentStatus);
					columns.Add((ZString)JobDeclaration.Schema.PaymentStatusDesc);
					columns.Add((ZString)JobDeclaration.Schema.BLUStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.FDAMsgStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.FDAStatus);
					columns.Add((ZString)JobDeclaration.Schema.FDAStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.ISFBillStatus);
					columns.Add((ZString)JobDeclaration.Schema.ISFBillStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.BrokerName);
					columns.Add((ZString)JobDeclaration.Schema.ImporterName);
					columns.Add((ZString)JobDeclaration.Schema.SupplierName);
					columns.Add((ZString)JobDeclaration.Schema.ForwarderName);
					columns.Add((ZString)JobDeclaration.Schema.IORName);
					columns.Add((ZString)JobDeclaration.Schema.UltimateConsigneeName);
					columns.Add((ZString)JobDeclaration.Schema.US_TotalEnteredValue);
					columns.Add((ZString)JobDeclaration.Schema.TotalOutstandingAmount);
					columns.Add((ZString)JobDeclaration.Schema.TotalInvoicedAmount);
					columns.Add((ZString)JobDeclaration.Schema.TotalBilledAmount);
					columns.Add((ZString)JobDeclaration.Schema.LiquidationDate);

					columns.Add((ZString)JobDeclaration.Schema.AuditDate);
					columns.Add((ZString)JobDeclaration.Schema.AuditReference);
					columns.Add((ZString)JobDeclaration.Schema.AuditLogUser);
					columns.Add((ZString)JobDeclaration.Schema.AuditLogUserName);
					columns.Add((ZString)JobDeclaration.Schema.CWAuditDate);
					columns.Add((ZString)JobDeclaration.Schema.CWAuditReference);
					columns.Add((ZString)JobDeclaration.Schema.CWAuditUser);
					columns.Add((ZString)JobDeclaration.Schema.CWAuditUserName);
					columns.Add((ZString)JobDeclaration.Schema.SPIAuditDate);
					columns.Add((ZString)JobDeclaration.Schema.SPIAuditReference);
					columns.Add((ZString)JobDeclaration.Schema.SPIAuditUser);
					columns.Add((ZString)JobDeclaration.Schema.SPIAuditUserName);
					columns.Add((ZString)JobDeclaration.Schema.FDAAuditDate);
					columns.Add((ZString)JobDeclaration.Schema.FDAAuditReference);
					columns.Add((ZString)JobDeclaration.Schema.FDAAuditUser);
					columns.Add((ZString)JobDeclaration.Schema.FDAAuditUserName);
					columns.Add((ZString)JobDeclaration.Schema.TIBClosedDate);
					columns.Add((ZString)JobDeclaration.Schema.TIBClosedReference);
					columns.Add((ZString)JobDeclaration.Schema.TIBClosedUser);
					columns.Add((ZString)JobDeclaration.Schema.TIBClosedUserName);
					columns.Add((ZString)JobDeclaration.Schema.TIBExpiryDate);
					columns.Add((ZString)JobDeclaration.Schema.TIBNumOfExtensions);

					columns.Add((ZString)JobDeclaration.Schema.US_OtherReconIndicator);
					columns.Add((ZString)JobDeclaration.Schema.OtherReconIndicatorDescription);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ExternalBroker);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ControllingAgent);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ControllingCustomer);
					columns.Add((ZString)JobDeclaration.Schema.US_BRDRefNo);
					columns.Add((ZString)JobDeclaration.Schema.US_SchDLoading);
					columns.Add((ZString)JobDeclaration.Schema.US_SchDArrival);
					columns.Add((ZString)JobDeclaration.Schema.ReleaseStatusDesc);
					columns.Add((ZString)JobDeclaration.Schema.US_PaperlessEntry);
					columns.Add((ZString)JobDeclaration.Schema.US_SchDEntry);
					columns.Add((ZString)JobDeclaration.Schema.US_SchDExport);

					columns.Add((ZString)JobDeclaration.Schema.BLUStatus);
					columns.Add((ZString)JobDeclaration.Schema.FDAMsgStatus);
					columns.Add((ZString)JobDeclaration.Schema.ReleaseStatus);
					columns.Add(new ZString(JobDeclaration.Schema.FreightContainerMode));
					columns.Add((ZString)JobDeclaration.Schema.US_CheckNo);
					columns.Add((ZString)JobDeclaration.Schema.ShipperReferenceNumber);
					columns.Add((ZString)JobDeclaration.Schema.US_AnticipatedLiquidationDate);
					columns.Add((ZString)JobDeclaration.Schema.US_CollectionDate);
					columns.Add((ZString)JobDeclaration.Schema.US_DeferredTaxDueDate);
					columns.Add((ZString)JobDeclaration.Schema.US_TaxDeferIndicator);
					columns.Add((ZString)JobDeclaration.Schema.US_CommodityFilingOption);
					columns.Add((ZString)JobDeclaration.Schema.EarliestExportDate);
					columns.Add((ZString)JobDeclaration.Schema.US_SoldEnRouteIndicator);

					columns.Add((ZString)JobDeclaration.Schema.FTZAdmissionNumberFormatted);
					columns.Add((ZString)JobDeclaration.Schema.AdmissionStatus);
					columns.Add((ZString)JobDeclaration.Schema.AdmissionStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.FTZArrivalStatus);
					columns.Add((ZString)JobDeclaration.Schema.FTZArrivalStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.FTZConcurrenceStatus);
					columns.Add((ZString)JobDeclaration.Schema.FTZConcurrenceStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.FTZDeliveryOfGoodsStatus);
					columns.Add((ZString)JobDeclaration.Schema.FTZDeliveryOfGoodsStatusDescription);
					columns.Add((ZString)JobDeclaration.Schema.FTZPTTStatus);
					columns.Add((ZString)JobDeclaration.Schema.FTZPTTStatusDescription);

					columns.Add((ZString)JobDeclaration.Schema.US_BondDispositionCode);
					columns.Add((ZString)JobDeclaration.Schema.US_BondDispositionCode2);
					columns.Add((ZString)JobDeclaration.Schema.US_InsuranceDisposition);

					columns.Add((ZString)JobDeclaration.Schema.DISStatus);
					columns.Add((ZString)JobDeclaration.Schema.DISStatusDescription);

					columns.Add((ZString)JobDeclaration.Schema.ImporterEIN);
					columns.Add((ZString)JobDeclaration.Schema.ImporterOfRecordEIN);
					columns.Add((ZString)JobDeclaration.Schema.US_PreparerDistrictPort);
					columns.Add((ZString)JobDeclaration.Constants.GenAddOnColumnFieldName.US_PGAReplaceUpdateNeeded);
					columns.Add((ZString)JobDeclaration.Constants.GenAddOnColumnFieldName.US_PGACorrectionStatus);
					columns.Add((ZString)JobDeclaration.Schema.US_PGACorrectionStatusDesc);
					columns.Add((ZString)JobDeclaration.Constants.GenAddOnColumnFieldName.US_QuotaStatus);
					columns.Add((ZString)JobDeclaration.Schema.US_QuotaStatusDesc);

					columns.Add((ZString)JobDeclaration.Schema.BillingBranch);
					columns.Add((ZString)JobDeclaration.Schema.BillingDepartment);
					columns.Add((ZString)JobDeclaration.Schema.BillingOperator);
					columns.Add((ZString)JobDeclaration.Schema.BillingTaxBranch);

					columns.Add((ZString)JobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded);
					columns.Add((ZString)JobDeclaration.Schema.DeliveryOrPickupCartageCoPK);

					columns.Add((ZString)JobDeclaration.Schema.US_WHSEntryFilerCode);
					columns.Add((ZString)JobDeclaration.Schema.US_WHSEntryNumber);
					columns.Add((ZString)JobDeclaration.Schema.US_QtyInWHBeforeWithdrawal);
					columns.Add((ZString)JobDeclaration.Schema.US_QtyBeingWithdrawn);
					columns.Add((ZString)JobDeclaration.Schema.US_QtyInWHAfterWithdrawal);
					columns.Add((ZString)JobDeclaration.Schema.US_IsFinalWHS);

					columns.Add((ZString)JobDeclaration.Schema.AESResponseCode);
					columns.Add((ZString)JobDeclaration.Schema.AESResponseCodeDescription);
					columns.Add((ZString)JobDeclaration.Schema.AESSeverity);
					columns.Add((ZString)JobDeclaration.Schema.AESSeverityDescription);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemCreateUser);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemCreateTimeUtc);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemLastEditUser);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemLastEditTimeUtc);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCode);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientName);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddressAsString);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddressShortCode);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddress1);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddress2);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCity);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientState);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCountry);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.RelatedTransportBookingsJobNumbers);

					columnNamesInSortOrder = (ZString[])columns.ToArray(typeof(ZString));
				}

				return columnNamesInSortOrder;
			}
		}
		ZString[] columnNamesInSortOrder;

		protected virtual void ModifyColumnProperties(ZGridColumnInfo column)
		{
			if (column != null)
			{
				switch (column.ColumnName)
				{
					case JobDeclarationSchema.Constants.JE_MessageType:
						ControlDpiScalingHelper.SetWidth(ref column, 40, true);
						break;
					case JobDeclarationSchema.Constants.JE_GB:
					case JobDeclarationSchema.Constants.JE_TransportMode:
						ControlDpiScalingHelper.SetWidth(ref column, 60, true);
						break;
					case JobDeclarationSchema.Constants.JE_RL_NKPortOfFirstArrival:
						ControlDpiScalingHelper.SetWidth(ref column, 66, true);
						break;
					case JobDeclarationSchema.Constants.JE_TotalNoOfPacks:
						ControlDpiScalingHelper.SetWidth(ref column, 68, true);
						break;
					case JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival:
						column.Caption = DeclarationFilterConstants.ColumnnCaptions.Discharge;
						ControlDpiScalingHelper.SetWidth(ref column, 70, true);
						break;
					case JobDeclarationSchema.Constants.JE_HouseBill:
					case JobDeclarationSchema.Constants.JE_MasterBill:
						ControlDpiScalingHelper.SetWidth(ref column, 80, true);
						break;
					case JobDeclarationSchema.Constants.JE_MessageStatus:
						ControlDpiScalingHelper.SetWidth(ref column, 87, true);
						break;
					case JobDeclarationSchema.Constants.JE_VesselName:
					case JobDeclarationSchema.Constants.JE_VoyageFlightNo:
					case JobDeclarationSchema.Constants.JE_ContainerMode:
						ControlDpiScalingHelper.SetWidth(ref column, 90, true);
						break;
					case JobDeclaration.Schema.JE_MessageStatusDescription:
						ControlDpiScalingHelper.SetWidth(ref column, 120, true);
						break;
					case JobDeclarationSchema.Constants.JE_GoodsDescription:
						ControlDpiScalingHelper.SetWidth(ref column, 130, true);
						break;
					case JobDeclarationSchema.Constants.JE_EntryAuthorisationDate:
						column.Caption = DeclarationFilterConstants.ColumnnCaptions.ReleaseDate;
						break;
					default:
						break;
				}

				foreach (var defaultColumn in DefaultColumnsForGrid)
				{
					if (defaultColumn == column.ColumnName)
					{
						column.IsVisible = true;
						return;
					}
				}

				column.IsVisible = false;
			}
		}

		ArrayList GetNewColumnOrderForGrid(ZGrid grid)
		{
			var columnNames = ColumnNamesInSortOrder;
			var result = new ArrayList(columnNames.Length);

			foreach (string columnName in columnNames)
			{
				var column = grid.GetColumnStyle(columnName);
				ModifyColumnProperties(column);
				result.Add(column);
			}

			return result;
		}

		void AddColumnByColumnStyle(ArrayList columns, string columnName)
		{
			if (grid.GetColumnStyle(columnName) != null)
			{
				columns.Add((ZString)columnName);
			}
		}

		ZString[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = new ZString[] {
						(ZString)JobDeclarationSchema.Constants.JE_GB,
						(ZString)JobDeclarationSchema.Constants.JE_MessageType,
						(ZString)JobDeclarationSchema.Constants.JE_TransportMode,
						(ZString)JobDeclarationSchema.Constants.JE_DeclarationReference,
						(ZString)JobDeclarationSchema.Constants.JE_VesselName,
						(ZString)JobDeclarationSchema.Constants.JE_VoyageFlightNo,
						(ZString)JobDeclarationSchema.Constants.JE_RL_NKOrigin,
						(ZString)JobDeclarationSchema.Constants.JE_RL_NKFinalDestination,
						(ZString)JobDeclarationSchema.Constants.JE_MasterBill,
						(ZString)JobDeclarationSchema.Constants.JE_HouseBill,
						(ZString)JobDeclarationSchema.Constants.JE_ScreeningStatus,
						(ZString)JobDeclarationSchema.Constants.JE_OH_Importer,
						(ZString)JobDeclarationSchema.Constants.JE_OH_Supplier,
						(ZString)JobDeclarationSchema.Constants.JE_OwnerRef,
						(ZString)JobDeclaration.Schema.OrderNumbers,
						(ZString)JobDeclaration.Schema.DeclarationNumber,
						(ZString)JobDeclarationSchema.Constants.JE_GoodsDescription
					};
				}

				return defaultColumnsForGrid;
			}
		}
		ZString[] defaultColumnsForGrid;
	}
}
