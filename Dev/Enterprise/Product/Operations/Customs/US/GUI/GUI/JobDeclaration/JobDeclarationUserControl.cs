using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.EntryNumber;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Schema;
using Bill = Enterprise.Customs.Business.Bill;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class USJobDeclarationUserControl : Customs.GUI.BaseCustomsDeclarationUserControl
	{
		public USJobDeclarationUserControl()
		{
			InitializeComponent();
			this.DDTCCategoryXXIDeterminationNumberTextBox.Visible = ZZCustomsFunctionality.IsAESJurisdictionNumberEffective;

			FTZPanel.AllowOverlap(EntryNumberDividerLabel);
			FTZPanel.AllowOverlap(Box29Button);
			FTZPanel.AllowOverlap(CentralizedExamSiteFindBox);
			FTZPanel.AllowOverlap(ImportEntryNumberTextBox);
			FTZPanel.AllowOverlap(EntryFilerCodeTextBox);
			HMFApplicableZDropEdit.AllowOutsideOfParent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				MakeServiceLevelInvisible();
				ChangeWarehouseWithdrawalPanel(USJobDeclaration.IsACE, USJobDeclaration.IsReWarehouse);
				ChangeJE_TransportModeHintExtension();
				ChangeMonthlyFilingControlsVisibility();
				BondedWarehouseDocAddressRelatedFieldChanged(this, EventArgs.Empty);
				UpdateAllocateButtonVisibility();
				ChangeUNLOCOPortsComponentVisibility();
			}
		}

		protected override void SetContainerCountAndNoOfPieces()
		{
			base.SetContainerCountAndNoOfPieces();
			this.JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
		}

		public override BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				base.JobDeclaration = value;
				SetRightTabControlSelectTab();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Declaration_OnPGARelatedDataLoadProgress(object sender, BusinessObject.LoadChildEditableObjectsProgressEventArgs e)
		{
			if (e != null)
			{
				if (e.IsFinished)
				{
					if (pgaRelatedDataLoadProgressForm != null)
					{
						pgaRelatedDataLoadProgressForm.Dispose();
					}
				}
				else
				{
					if (pgaRelatedDataLoadProgressForm == null)
					{
						pgaRelatedDataLoadProgressForm = new ProgressForm();
						pgaRelatedDataLoadProgressForm.TopMost = true;
						pgaRelatedDataLoadProgressForm.ShowCancelButton = false;
						pgaRelatedDataLoadProgressForm.Show();
						Application.DoEvents();
					}

					if (pgaRelatedDataLoadProgressForm.PercentComplete != e.Percentage)
					{
						pgaRelatedDataLoadProgressForm.SetStatusAndPercentComplete(e.Status, e.Percentage);
						Application.DoEvents();
					}
				}
			}
		}

		void Declaration_ShowMessageOnGUI(object sender, ShowMessageOnGUIEventArgs e)
		{
			if (e != null)
			{
				Globals.Message.Show(e.Message);
			}
		}

		ProgressForm pgaRelatedDataLoadProgressForm;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var declaration = (JobDeclaration)CurrentDataItem;
			UnhookDeclaration(declaration);
			pgaRelatedDataLoadProgressForm?.Dispose();
		}

		void UnhookDeclaration(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.OnPGARelatedDataLoadProgress -= Declaration_OnPGARelatedDataLoadProgress;
				declaration.ShowMessageOnGUI -= Declaration_ShowMessageOnGUI;
				declaration.US_PSCInfo.ValueChanged -= US_PSCInfo_ValueChanged;
				declaration.US_EntryTypeInfo.ValueChanged -= US_EntryType_ValueChanged;
				declaration.US_EnableCRLInfo.ValueChanged -= US_CargoReleaseTypeInfo_ValueChanged;
				declaration.FTZZoneIDInfo.ValueChanged -= FTZZoneIDInfo_ValueChanged;
				declaration.AdmissionStatusInfo.ValueChanged -= AdmissionStatusInfo_ValueChanged;
				declaration.US_EnableSPNInfo.ValueChanged -= US_EnableSPNInfo_ValueChanged;
				declaration.US_F_PNModeInfo.ValueChanged -= US_F_PNModeInfo_ValueChanged;
				declaration.IOROrgPKInfo.ValueChanged -= IOROrgPKInfo_ValueChanged;
				declaration.US_NonAMSInfo.ValueChanged -= US_NonAMSInfo_ValueChanged;
				declaration.JE_RL_NKPortOfArrivalInfo.ValueChanged -= ChangeArrivalUNLOCOPortsComponentType;
				declaration.JE_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeLoadingUNLOCOPortsComponentType;
				declaration.US_RL_NKPortOfExportInfo.ValueChanged -= ChangeExportUNLOCOPortsComponentType;
				declaration.MarkPGAStatusToBeDeletedWarningChecker = null;
				declaration.OnFTZZoneIDChangingEvent = null;
				if (declaration.WarehouseDocAddress != null)
				{
					declaration.WarehouseDocAddress.OrganisationPKInfo.ValueChanged -= WarehouseDocAddressOrganisationPKInfo_ValueChanged;
				}
			}
		}

		bool MarkPGAStatusToBeDeletedWarningChecker(string warning)
		{
			return Globals.Message.Show(warning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var declaration = (JobDeclaration)CurrentDataItem;

			if (declaration != null)
			{
				declaration.OnPGARelatedDataLoadProgress += Declaration_OnPGARelatedDataLoadProgress;
				declaration.ShowMessageOnGUI += Declaration_ShowMessageOnGUI;
				if (declaration.IsInDatabase && !declaration.ShouldSynchroniseWithShipment())
				{
					foreach (Bill bill in declaration.LowestBills)
					{
						declaration.Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, bill.PK);
					}
				}
				declaration.MarkPGAStatusToBeDeletedWarningChecker = MarkPGAStatusToBeDeletedWarningChecker;
				declaration.US_PSCInfo.ValueChanged += US_PSCInfo_ValueChanged;
				declaration.US_EntryTypeInfo.ValueChanged += US_EntryType_ValueChanged;
				declaration.US_CargoReleaseTypeInfo.ValueChanged += US_CargoReleaseTypeInfo_ValueChanged;
				declaration.US_EnableSPNInfo.ValueChanged += US_EnableSPNInfo_ValueChanged;
				declaration.FTZZoneIDInfo.ValueChanged += FTZZoneIDInfo_ValueChanged;
				declaration.AdmissionStatusInfo.ValueChanged += AdmissionStatusInfo_ValueChanged;
				declaration.US_F_PNModeInfo.ValueChanged += US_F_PNModeInfo_ValueChanged;
				declaration.IOROrgPKInfo.ValueChanged += IOROrgPKInfo_ValueChanged;
				declaration.US_NonAMSInfo.ValueChanged += US_NonAMSInfo_ValueChanged;
				declaration.JE_RL_NKPortOfArrivalInfo.ValueChanged += ChangeArrivalUNLOCOPortsComponentType;
				declaration.JE_RL_NKPortOfLoadingInfo.ValueChanged += ChangeLoadingUNLOCOPortsComponentType;
				declaration.US_RL_NKPortOfExportInfo.ValueChanged += ChangeExportUNLOCOPortsComponentType;
				ChangeACEControlsVisibility();
				if (declaration.WarehouseDocAddress != null)
				{
					declaration.WarehouseDocAddress.OrganisationPKInfo.ValueChanged += WarehouseDocAddressOrganisationPKInfo_ValueChanged;
				}
				FTZZoneIDChangePreCondition(declaration);
			}
		}

		void ChangeArrivalUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeArrivalUNLOCOPortsComponentVisibility();
		}

		void ChangeArrivalUNLOCOPortsComponentVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null && !declaration.IsDeleted)
			{
				PortOfDischargeSchDDropEdit.Visible = declaration.US_SchDArrivalTypeIsDropEdit;
				PortOfDischargeSchDFindBox.Visible = !declaration.US_SchDArrivalTypeIsDropEdit;
			}
		}

		void ChangeLoadingUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeLoadingUNLOCOPortsComponentVisibility();
		}

		void ChangeLoadingUNLOCOPortsComponentVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null && !declaration.IsDeleted)
			{
				PortOfLoadingSchDDropEdit.Visible = declaration.US_SchDLoadingTypeIsDropEdit;
				PortOfLoadingSchDFindBox.Visible = !declaration.US_SchDLoadingTypeIsDropEdit;
			}
		}

		void ChangeExportUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeExportUNLOCOPortsComponentVisibility();
		}

		void ChangeExportUNLOCOPortsComponentVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null && !declaration.IsDeleted)
			{
				PortOfExportSchDCodeDropEdit.Visible = declaration.US_SchDExportTypeIsDropEdit;
				PortOfExportSchDCodeFindBox.Visible = !declaration.US_SchDExportTypeIsDropEdit;
			}
		}

		void ChangeUNLOCOPortsComponentVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null && !declaration.IsDeleted)
			{
				ChangeArrivalUNLOCOPortsComponentVisibility();
				ChangeLoadingUNLOCOPortsComponentVisibility();
				ChangeExportUNLOCOPortsComponentVisibility();
			}
		}

		void LicenseTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ECCNControlVisibility(USJobDeclaration.IsExport);
		}

		void ECCNControlVisibility(bool isVisible)
		{
			if (isVisible)
			{
				var declaration = (JobDeclaration)CurrentDataItem;
				var hasAvailableECCNNumbers = declaration != null && declaration.HasAvailableECCNNumbers;
				ECCNCodeFindBox.Visible = hasAvailableECCNNumbers;
				ECCNTextBox.Visible = !hasAvailableECCNNumbers;
			}
		}

		void US_EnableSPNInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null && declaration.IsFTZAdmission)
			{
				FTZSPNIDTypeDropEdit.Visible = declaration.IsFTZPGAStandAlonePriorNotice;
			}

			UpdateSEControlsVisibility();
		}

		void US_F_PNModeInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				FTZSPNIDTypeDropEdit.Visible = declaration.IsFTZPGAStandAlonePriorNotice;
			}
		}

		void IOROrgPKInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateAllocateButtonVisibility();
		}

		void US_NonAMSInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateExpressTrackingCheckBoxVisibility();
		}

		void WarehouseDocAddressOrganisationPKInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateAllocateButtonVisibility();
		}

		void FTZZoneIDChangePreCondition(JobDeclaration declaration)
		{
			declaration.OnFTZZoneIDChangingEvent += () =>
			{
				var result = true;
				var supporter = declaration.FTZControlNumberSupporter;
				if (supporter != null)
				{
					var existingNumber = supporter.GetExistingNumber();
					result = existingNumber.IsEmpty || Globals.Message.Show(ZString.Format("There is already an FTZ Control Number ({0}) allocated for this job. Are you sure you wish to continue?. ", existingNumber), "FTZ Control Number exists", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
				}
				return result;
			};
		}

		void US_CargoReleaseTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateSEControlsVisibility();
			UpdateFixedTransportModeControlsVisibility();
			UpdateACECargoCertificationControlsVisibility();
			UpdateExpressTrackingCheckBoxVisibility();
		}

		void UpdateACECargoCertificationControlsVisibility()
		{
			if (USJobDeclaration != null)
			{
				US_PGAExpeditedReleaseCheckBox.Visible = USJobDeclaration.IsACECargoCertificationMode;
				US_ImmediateDeliveryCheckBox.Visible = USJobDeclaration.IsACECargoCertificationMode;
				US_PGAExpeditedReleaseCheckBox.Visible = USJobDeclaration.IsACECargoCertificationMode;
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			ChangeACEControlsVisibility();
		}

		protected override void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			base.JE_MessageTypeInfo_ValueChanged(sender, e);
			UpdateFixedTransportModeControlsVisibility();
			ChangeMonthlyFilingControlsVisibility();
			var isExport = USJobDeclaration.IsExport;
			RightTabControl.SelectedTab = isExport ? OrganisationsTabPage : USOrganisationsTabPage;
			var shouldShowApplicationCode = isExport || USJobDeclaration.IsFTZAdmission;
			JE_ApplicationCodeBoundDropEdit.Enabled = shouldShowApplicationCode;
			JE_ApplicationCodeBoundDropEdit.Visible = shouldShowApplicationCode;
		}

		void JE_TransportMode_ValueChanged(object sender, EventArgs e)
		{
			ChangeJE_TransportModeHintExtension();
			ChangeMonthlyFilingControlsVisibility();
			UpdateFixedTransportModeControlsVisibility();
			UpdateSEControlsVisibility();
			UpdateExpressTrackingCheckBoxVisibility();
		}

		void US_UI_NKCarrierSCAC_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideCarrierNameTextBox();
		}

		void JE_OH_ShippingLine_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideCarrierNameTextBox();
		}

		void ChangeJE_TransportModeHintExtension()
		{
			if (USJobDeclaration.IsSea || USJobDeclaration.IsPost)
			{
				JE_MasterBillForSeaBoundTextBox.Extensions.Remove<IHintExtension>();
				JE_MasterBillForSeaBoundTextBox.Extensions.Add(USJobDeclaration.IsPost ? new USMailRefHintExtension() : new HintExtension());
			}
		}

		void ChangeMonthlyFilingControlsVisibility()
		{
			if (USJobDeclaration.IsImport && USJobDeclaration.IsFixedTransportInstallations)
			{
				US_EnableSPNCheckBox.Visible = false;
				US_MonthlyFilingCheckBox.Visible = true;
			}
			else
			{
				US_EnableSPNCheckBox.Visible = true;
				US_MonthlyFilingCheckBox.Visible = false;
				USJobDeclaration.US_MonthlyFiling = false;
			}
		}

		void ChangeACEControlsVisibility()
		{
			UpdateSEControlsVisibility();

			SetVoyageFlightNoVisible();

			PSCCheckBox.Visible = USJobDeclaration.IsACE;
			UpdatePSCControlsVisibility();
			SetLableOfForms();
			UpdateFixedTransportModeControlsVisibility();
			US_PGAExpeditedReleaseCheckBox.Visible = USJobDeclaration.IsACE;
			US_ImmediateDeliveryCheckBox.Visible = USJobDeclaration.IsACE;
			US_EnableAIICheckBox.Visible = !USJobDeclaration.IsACECargoCertificationMode;
			InvoiceByRequestCheckBox.Visible = !USJobDeclaration.IsACECargoCertificationMode;
			ChangeWarehouseWithdrawalPanel(USJobDeclaration.IsACE, USJobDeclaration.IsReWarehouse);
			JobMiscOrgsControl.ACERelatedOrgControlVisibilityChanged(USJobDeclaration.IsACSCargoCertificationMode);
			UpdateExpressTrackingCheckBoxVisibility();
		}

		void SetLableOfForms()
		{
			if (USJobDeclaration.IsACE)
			{
				EnableENSCheckBox.Text = "Enable " + StatusUserControl.Form7501NameForACE_Short;
				US_EnableCRLCheckBox.Text = "Enable " + StatusUserControl.Form3461NameForACE_Short;
			}
			else
			{
				EnableENSCheckBox.Text = "Enable " + StatusUserControl.Form7501Name;
				US_EnableCRLCheckBox.Text = "Enable " + StatusUserControl.Form3461Name;
			}
		}

		void US_PSCInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdatePSCControlsVisibility();
		}

		void UpdateSEControlsVisibility()
		{
			var isACE = USJobDeclaration != null && USJobDeclaration.IsACE;
			var isACECargoCertificationMode = USJobDeclaration != null && USJobDeclaration.IsACECargoCertificationMode;
			var isSplitShipmentReleaseVisible = USJobDeclaration != null && USJobDeclaration.AreSplitDetailsRelevant && isACECargoCertificationMode;
			var isStandAlonePriorNotice = USJobDeclaration != null && USJobDeclaration.US_EnableSPN;
			NonAMSCheckBox.Visible = isACECargoCertificationMode;
			SEReleasePanel.Visible = isSplitShipmentReleaseVisible && !isStandAlonePriorNotice;
			SEReleaseAndStandAlonePriorNoticePanel.Visible = isStandAlonePriorNotice && isACE;
			SplitShipmentReleaseCodeForSPNDropEdit.Visible = isSplitShipmentReleaseVisible;

			US_EnableAIICheckBox.Visible = USJobDeclaration != null && !USJobDeclaration.IsACECargoCertificationMode;
			InvoiceByRequestCheckBox.Visible = USJobDeclaration != null && !USJobDeclaration.IsACECargoCertificationMode;
			JobMiscOrgsControl?.ACERelatedOrgControlVisibilityChanged(USJobDeclaration?.IsACSCargoCertificationMode ?? false);
		}

		void UpdateExpressTrackingCheckBoxVisibility()
		{
			ExpressTrackingCheckBox.Visible = USJobDeclaration != null && USJobDeclaration.IsExpressTrackingNumberRelevant;
			JE_MasterBillExpressTrackingValueChanged(this, EventArgs.Empty);
		}

		void JE_MasterBillExpressTrackingValueChanged(object sender, EventArgs e)
		{
			var isMasterBillExpressTrackingNumber = USJobDeclaration != null && USJobDeclaration.IsExpressTrackingNumberRelevant && USJobDeclaration.JE_MasterBillExpressTracking;
			JE_MasterBillForAirBoundTextBox.Visible = IsJE_MasterBillForAirBoundTextBoxVisible && !isMasterBillExpressTrackingNumber;
			ExpressTrackingNumberTextBox.Visible = isMasterBillExpressTrackingNumber;
			if (isMasterBillExpressTrackingNumber)
			{
				ExpressTrackingNumberTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		void UpdatePSCControlsVisibility()
		{
			if (USJobDeclaration.IsFormalImport && !USJobDeclaration.IsImportByExternalBroker)
			{
				AllocateImportEntryNumberButton.Visible = !USJobDeclaration.IsACE || !USJobDeclaration.US_PSC;
			}
			else
			{
				AllocateImportEntryNumberButton.Visible = false;
			}

			AccLiqCheckBox.Visible = USJobDeclaration.IsACE && USJobDeclaration.US_PSC;
		}

		void US_EntryType_ValueChanged(object sender, EventArgs e)
		{
			ChangeWarehouseWithdrawalPanel(USJobDeclaration.IsACE, USJobDeclaration.IsReWarehouse);
			UpdateSEControlsVisibility();
			UpdateACECargoCertificationControlsVisibility();
			UpdateExpressTrackingCheckBoxVisibility();
			UpdateFixedTransportModeControlsVisibility();
		}

		void ChangeWarehouseWithdrawalPanel(bool isACE, bool isReWarehouse)
		{
			this.WarehouseWithdrawalLabel.Text = isReWarehouse ? "Re-Warehouse" : "Warehouse Withdrawal";
			this.WHSDistrictPortCodeFindBox.Visible = isACE || !isReWarehouse;
			this.IsFinalWHSCheckBox.Visible = !isReWarehouse;
			this.QtyInWhBeforeWithdrawalCalcEdit.Visible = !isReWarehouse;
			this.QtyBeingWithdrawnCalcEdit.Visible = !isReWarehouse;
			this.QtyInWHAfterWithdrawalCalcEdit.Visible = !isReWarehouse;
		}

		void FTZZoneIDInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateAllocateButtonVisibility();
		}

		void AdmissionStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateAllocateButtonVisibility();
		}

		void UpdateAllocateButtonVisibility()
		{
			var declaration = USJobDeclaration;
			AllocateButton.Visible = declaration.IsFTZAdmission && declaration.FTZControlNumberIsAutoAllocated;
		}

		void UpdateFixedTransportModeControlsVisibility()
		{
			var isACECargoCertificationAndFixedTransportRelevant = USJobDeclaration != null && USJobDeclaration.IsACECargoCertificationAndFixedTransportRelevant;
			PipelineNameTextBox.Visible = isACECargoCertificationAndFixedTransportRelevant;

			var isACECargoCertificationAndFixedTransportRelevantAndIsNotConsumptionFTZ = isACECargoCertificationAndFixedTransportRelevant && !USJobDeclaration.IsConsumptionFTZ;
			BatchTicketTextBox.Visible = isACECargoCertificationAndFixedTransportRelevantAndIsNotConsumptionFTZ;
			if (isACECargoCertificationAndFixedTransportRelevantAndIsNotConsumptionFTZ)
			{
				BatchTicketTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		internal JobDeclaration USJobDeclaration
		{
			get { return (JobDeclaration)JobDeclaration; }
		}

		protected override void SetRightTabControlSelectTab()
		{
			if (RightTabControl is ZTabControl rightTabControl && OrganisationsTabPage is ZTabPage organisationsTabPage)
			{
				rightTabControl.SelectedTab = organisationsTabPage.TabVisible ? organisationsTabPage : USOrganisationsTabPage;
			}
		}

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			JobDeclaration usDeclaration = (JobDeclaration)declaration;
			usDeclaration.US_SoldEnRouteIndicatorInfo.ValueChanged += US_SoldEnRouteIndicatorInfo_ValueChanged;
			usDeclaration.JE_MasterBillIssuerSCACInfo.ValueChanged += JE_MasterBillIssuerSCACInfo_ValueChanged;
			usDeclaration.OnBondedWarehouseRelatedFieldChanged += BondedWarehouseDocAddressRelatedFieldChanged;
			usDeclaration.JE_TransportModeInfo.ValueChanged += JE_TransportMode_ValueChanged;
			usDeclaration.US_UI_NKCarrierSCACInfo.ValueChanged += US_UI_NKCarrierSCAC_ValueChanged;
			usDeclaration.JE_OH_ShippingLineInfo.ValueChanged += JE_OH_ShippingLine_ValueChanged;
			usDeclaration.US_LicenseTypeInfo.ValueChanged += LicenseTypeInfo_ValueChanged;
			JE_MasterBillIssuerSCACInfo_ValueChanged(this, EventArgs.Empty);
			BondedWarehouseDocAddressRelatedFieldChanged(this, EventArgs.Empty);
			usDeclaration.JE_MasterBillExpressTrackingInfo.ValueChanged += JE_MasterBillExpressTrackingValueChanged;
			usDeclaration.US_TIBMotorVehiclesInfo.ValueChanged += TIBMotorVehiclesDropList_ValueChanged;
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			JobDeclaration usDeclaration = (JobDeclaration)declaration;
			usDeclaration.US_SoldEnRouteIndicatorInfo.ValueChanged -= US_SoldEnRouteIndicatorInfo_ValueChanged;
			usDeclaration.JE_MasterBillIssuerSCACInfo.ValueChanged -= JE_MasterBillIssuerSCACInfo_ValueChanged;
			usDeclaration.OnBondedWarehouseRelatedFieldChanged -= BondedWarehouseDocAddressRelatedFieldChanged;
			usDeclaration.JE_TransportModeInfo.ValueChanged -= JE_TransportMode_ValueChanged;
			usDeclaration.US_UI_NKCarrierSCACInfo.ValueChanged -= US_UI_NKCarrierSCAC_ValueChanged;
			usDeclaration.JE_OH_ShippingLineInfo.ValueChanged -= JE_OH_ShippingLine_ValueChanged;
			usDeclaration.JE_MasterBillExpressTrackingInfo.ValueChanged -= JE_MasterBillExpressTrackingValueChanged;
			usDeclaration.US_TIBMotorVehiclesInfo.ValueChanged -= TIBMotorVehiclesDropList_ValueChanged;
			usDeclaration.US_LicenseTypeInfo.ValueChanged -= LicenseTypeInfo_ValueChanged;
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		void US_SoldEnRouteIndicatorInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ExportAESTIRShipmentTypePanel.Visible)
			{
				bool isSoldEnRoute = USJobDeclaration.US_SoldEnRouteIndicator == YesNoDefaultList.Codes.Yes;
				US_RN_NKFirstPortOfCallCountryCodeFindBox.Visible = isSoldEnRoute;
				US_FirstPortOfCallCityTextBox.Visible = isSoldEnRoute;
			}
		}

		void JE_MasterBillIssuerSCACInfo_ValueChanged(object sender, EventArgs e)
		{
			HandleMasterBillControlVisibility();
			SetVoyageFlightNoVisible();
		}

		void BondedWarehouseDocAddressRelatedFieldChanged(object sender, EventArgs e)
		{
			var declaration = USJobDeclaration;
			var bondedWarehouseEditable = declaration != null && declaration.BondedWarehouseEditable;
			JobMainOrgsControl.BondedWarehouseDocAddressRelatedFieldChanged(bondedWarehouseEditable);
			BondedWarehouseDocAddressControl.Visible = bondedWarehouseEditable;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (USJobDeclaration != null && !USJobDeclaration.IsDeleted)
			{
				base.OnVisibleChanged(e);
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			this.BeginInvokeSafe(() =>
			{
				this.SuspendLayout();
				base.HandleDeclarationControlVisibilityChangedCore();
				JE_MessageSubTypeBoundDropDownEdit.Visible = false;

				if (USJobDeclaration.IsExport)
				{
					ExportDeclarationNumberBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "ITN";
					StatusTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Desc";
					ImportDateOfFirstArrivalDateEdit.Visible = false;
					JE_DateOfArrivalBoundDateEdit.Visible = true;
					DestinationStateDropEdit.Visible = false;
					JE_ContainerCountCalcEdit.Visible = false;
					OriginalITNTextBox.Visible = true;
				}
				else
				{
					ExportDeclarationNumberBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Entry Number";
					StatusTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Customs Status";
					ImportDateOfFirstArrivalDateEdit.Visible = true;
					JE_DateOfArrivalBoundDateEdit.Visible = false;
					DestinationStateDropEdit.Visible = true;
					OriginalITNTextBox.Visible = false;
				}

				ChangeACEControlsVisibility();
				UpdatePSCControlsVisibility();
				ShowOrHideFTZControls(USJobDeclaration.IsFTZAdmission);
				ShowOrHideExportControls(USJobDeclaration.IsExport);
				ShowOrHideNonExportControls(!USJobDeclaration.IsExport);
				WarehouseWithdrawalPanel.Visible = USJobDeclaration.IsExWarehouse || USJobDeclaration.IsReWarehouse;
				TIBPanel.Visible = USJobDeclaration.IsTemporaryImportationBond;
				if (TIBPanel.Visible)
				{
					TIBMotorVehiclesDropList_ValueChanged(null, EventArgs.Empty);
				}

				ChangeModuleIDsForDynamicScheduleDOrK();
				ShowOrHideDocsTabPage();
				ShowOrHideDDTCDetailsTabPage();
				ShowOrHideFTZNumber();
				ShowOrHideJourneyTextBox();
				ShowOrHideCarrierNameTextBox();
				ChangeUNLOCOPortsComponentVisibility();

				this.ScreeningStatusDropEdit.Visible = !JobDeclaration.IsDrawback && JobDeclaration.Shipment == null && !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;
				this.ScreenButton.Visible = !JobDeclaration.IsDrawback && JobDeclaration.Shipment == null && !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;

				ResizeRightTabControl(USJobDeclaration.IsExport);
				JobMainOrgsControl.UpdateControlCaption(USJobDeclaration.IsACE);
				this.ResumeLayout();
			});
		}

		void TIBMotorVehiclesDropList_ValueChanged(object value, EventArgs empty)
		{
			TIBMVNonConformingCheckBox.Visible = USJobDeclaration.US_TIBMotorVehicles == YesNoList.Codes.Yes;
		}

		void ShowOrHideFTZControls(bool isVisibleForFTZ)
		{
			FTZPanel.Visible = isVisibleForFTZ;
			FTZSPNCheckBox.Visible = isVisibleForFTZ;
			IncludePTTCheckBox.Visible = isVisibleForFTZ;
			PTTWithoutExceptionCheckBox.Visible = isVisibleForFTZ;
			FTZAdmissionTypeDropEdit.Visible = isVisibleForFTZ;
			DirectDeliveryIndicatorCheckBox.Visible = isVisibleForFTZ;
			ImportShipmentTypePanel.Visible = !isVisibleForFTZ;
			ConsolidatedSummaryCheckBox.Visible = !isVisibleForFTZ;
			FTZSPNIDTypeDropEdit.Visible = USJobDeclaration.IsFTZPGAStandAlonePriorNotice;

			if (isVisibleForFTZ)
			{
				ControlDpiScalingHelper.SetHeight(ref ImportStatusGroupBox, 130, true);
				PurchasedDropEdit.Visible = false;
				ManualEntryCheckBox.Visible = false;
				IORAuthAgentPanel.Visible = false;

				this.BindingSource.SetBindingMember(this.PortOfEntryScheduleCodeFindBox, "");
				PortOfEntryScheduleCodeFindBox.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|F08997C3-FD81-4B14-A657-84F24383C062", "FTZ Port");
				this.BindingSource.SetBindingMember(this.PortOfEntryScheduleCodeFindBox, "US_SchDEntry");

				BindingSource.SetBindingMember(this.EstimatedEntryDateDateEdit, "JE_DateOfFirstArrival");
				EstimatedEntryDateDateEdit.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|BDDA44BF-179B-4068-BFA5-3098BDECD11E", "First Arr. Date");
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(ref ImportStatusGroupBox, 191, true);
				PurchasedDropEdit.Visible = true;
				ManualEntryCheckBox.Visible = true;
				IORAuthAgentPanel.Visible = true;

				this.BindingSource.SetBindingMember(this.PortOfEntryScheduleCodeFindBox, "");
				PortOfEntryScheduleCodeFindBox.CaptionResourceString = new ResourceStringData("USJobDeclarationUserControl|E4DA5F52-5AD7-49E7-AC13-D022E0344E01", "Entry Port");
				this.BindingSource.SetBindingMember(this.PortOfEntryScheduleCodeFindBox, "US_SchDEntry");

				BindingSource.SetBindingMember(this.EstimatedEntryDateDateEdit, "US_EstimatedEntryDate");
				EstimatedEntryDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USJobDeclarationUserControl|27b7ec47-3685-42ca-822f-7818fc9996fb", "Est. Entry Date", "Est. Entry Date", "Estimated Entry Date", "");
			}

			JobMainOrgsControl.ShowOrHideFTZControls(isVisibleForFTZ, USJobDeclaration.IsConsumptionFTZ);
		}

		protected override bool IsJE_MasterBillForAirBoundTextBoxVisible
		{
			get { return base.IsJE_MasterBillForAirBoundTextBoxVisible && !USJobDeclaration.IsConsumptionFTZ && !USJobDeclaration.JE_MasterBillExpressTracking; }
		}

		protected override bool IsJE_MasterBillForSeaBoundTextBoxVisible
		{
			get { return USJobDeclaration.IsMasterBillRelevant && !USJobDeclaration.IsAir && !(USJobDeclaration.IsPost && USJobDeclaration.IsExport) && !USJobDeclaration.IsConsumptionFTZ; }
		}

		protected override bool IsVoyageFlightNoVisible
		{
			get
			{
				return USJobDeclaration.IsVoyageFlightNumberVisible;
			}
		}

		void ResizeRightTabControl(bool isExport)
		{
			RightTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			if (isExport)
			{
				ControlDpiScalingHelper.SetHeight(ref DeclarationDetailsGroupBox, ShipmentTypeGroupBox.Bottom - DeclarationDetailsGroupBox.Top, false);
				ControlDpiScalingHelper.SetHeight(ref RightTabControl, DeclarationDetailsGroupBox.Top - RightTabControl.Top, false);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(ref ImportStatusGroupBox, ShipmentTypeGroupBox.Bottom - ImportStatusGroupBox.Top, false);
				ControlDpiScalingHelper.SetHeight(ref RightTabControl, ShipmentTypeGroupBox.Bottom - RightTabControl.Top, false);
				if (!USJobDeclaration.IsFTZAdmission)
				{
					ConsolidatedSummaryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(US_ImmediateDeliveryCheckBox.Left, ImportShipmentTypePanel.Bottom, false);
					ConsolidatedSummaryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 17, true);
				}
			}
			if (USJobDeclaration.IsImport && JobDeclaration.Shipment == null)
			{
				this.InsuranceValueCalcFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(78, 177, true);
				this.InsuranceValueCalcFindBox.Visible = true;
				this.ScreeningStatusDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(78, 200, true);
				this.ScreenButton.Location = ControlDpiScalingHelper.NewScaledPoint(203, 200, true);
				this.ShipmentDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(443, 224, true);
				this.ImportStatusGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(254, 418, true);
			}
			else
			{
				this.InsuranceValueCalcFindBox.Visible = false;
				this.ScreeningStatusDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(78, 177, true);
				this.ScreenButton.Location = ControlDpiScalingHelper.NewScaledPoint(203, 177, true);
				this.ShipmentDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(443, 201, true);
				this.ImportStatusGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(254, 399, true);
			}
		}

		void ShowOrHideFTZNumber()
		{
			var isFTZNumberVisible = USJobDeclaration != null && USJobDeclaration.IsFormalImport && USJobDeclaration.IsConsumptionFTZ;
			MasterBillForFTZTextBox.Visible = isFTZNumberVisible;
			FTZTextBox.Visible = isFTZNumberVisible;
			if (isFTZNumberVisible)
			{
				MasterBillForFTZTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		void ShowOrHideJourneyTextBox()
		{
			JourneyTextBox.Visible = USJobDeclaration.TransportMode == Core.Constants.TransportModes.Rail;
		}

		void ShowOrHideCarrierNameTextBox()
		{
			CarrierNameTextBox.Visible = USJobDeclaration.IsUnknownCarrierSCACForExport;
		}

		void ChangeModuleIDsForDynamicScheduleDOrK()
		{
			AddInfoJobDeclarationLookups lookups = ((JobDeclaration)JobDeclaration).AddInfoLookups;

			PortOfLoadingSchDFindBox.ModuleID = ZMetaData.GetModuleId(lookups.LoadingSchDList);
			PortOfDischargeSchDFindBox.ModuleID = ZMetaData.GetModuleId(lookups.DischargeSchDList);
		}

		#region Docs Tab Visiblity
		void ShowOrHideDocsTabPage()
		{
			DocsTabPage.TabVisible = (!USJobDeclaration.IsExport || USJobDeclaration.IsStandAlone);
		}
		#endregion

		#region DDTC Details Tab Visiblity
		void ShowOrHideDDTCDetailsTabPage()
		{
			DDTCDetailsTabPage.TabVisible = USJobDeclaration.IsExport;
		}
		#endregion

		#region Controls Visibility and Locations

		void ShowOrHideExportControls(bool isVisible)
		{
			DeclarationDetailsGroupBox.Visible = isVisible;
			InbondTypeDropEdit.Visible = isVisible;
			ImportEntryNoTextBox.Visible = isVisible;
			InbondTypeDropEdit.Visible = isVisible;
			ImportEntryNoTextBox.Visible = isVisible;
			TransportReferenceTextBox.Visible = isVisible;
			ForeignTradeZoneTextBox.Visible = isVisible;
			StateOfOriginDropEdit.Visible = isVisible;
			LicenseNoTextBox.Visible = isVisible;
			LicenseTypeCodeFindBox.Visible = isVisible;
			ExportCodeDropEdit.Visible = isVisible;
			PortOfExportPanel.Visible = isVisible;
			ExportShipmentTypePanel.Visible = false;
			ExportAESTIRShipmentTypePanel.Visible = isVisible;
			SEDGroupBox.Visible = isVisible;
			if (ExportAESTIRShipmentTypePanel.Visible)
			{
				US_SoldEnRouteIndicatorInfo_ValueChanged(null, EventArgs.Empty);
			}
			ECCNControlVisibility(isVisible);
			ShowOrHideOrganisationsTabPage(isVisible);
		}

		void ShowOrHideOrganisationsTabPage(bool isExport)
		{
			OrganisationsTabPage.TabVisible = isExport;
			USOrganisationsTabPage.TabVisible = !isExport;
		}

		void ShowOrHideNonExportControls(bool isVisible)
		{
			EntryTypeDropEdit.Visible = isVisible;
			ImportStatusGroupBox.Visible = isVisible;
			PortOfImportPanel.Visible = isVisible;
			MasterBillIssuerSCACFindBox.Visible = isVisible;
			HouseBillIssuerSCACFindBox.Visible = isVisible;
			PrimaryITNumberTextBox.Visible = isVisible;
			ITDateDateEdit.Visible = isVisible;
			EstimatedEntryDateDateEdit.Visible = isVisible;

			HMFPanel.Visible = JobDeclaration.IsImport && !JobDeclaration.IsExWarehouse;
			ImportShipmentTypePanel.Visible = !JobDeclaration.IsExport && !USJobDeclaration.IsFTZAdmission;
			ConsolidatedSummaryCheckBox.Visible = !JobDeclaration.IsExport && !USJobDeclaration.IsFTZAdmission;
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible
		{
			get { return USJobDeclaration.IsContainerSupported; }
		}

		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (JobDeclaration is JobDeclaration declaration)
				{
					UnhookDeclaration(declaration);
				}
			}
			base.Dispose(isNotFinalizing);
		}

		void Box29Button_Click(object sender, EventArgs e)
		{
			IBox29Supportable box29Data = JobDeclaration as IBox29Supportable;
			if (box29Data != null)
			{
				Box29Data box29DataForForm = new Box29Data(box29Data);
				Box29Form box29Form = new Box29Form(box29DataForForm);
				ZFormModaliser.Show(box29Form, this.ParentForm);
			}
		}

		void AllocateImportEntryNumberButton_Click(object sender, EventArgs e)
		{
			ZForm mainForm = FindForm() as ZForm;
			if (mainForm != null)
			{
				var args = new AllocateEventHandlerArgs();
				args.FireSaveButton = () => mainForm.FireSaveButton();
				args.TopBizObjForFormHasChanges = mainForm.BusinessEntityForHasChanges;
				args.PerformActionBeforeShowingForm = null;
				new AllocateNumberButtonClickEventHandler().Allocate(USJobDeclaration.GetAllocateNumberSupporter(), args);
			}
		}

		void AllocateButton_Click(object sender, EventArgs e)
		{
			ZForm mainForm = FindForm() as ZForm;
			if (mainForm != null)
			{
				var args = new AllocateEventHandlerArgs();
				args.FireSaveButton = () => mainForm.FireSaveButton();
				args.TopBizObjForFormHasChanges = mainForm.BusinessEntityForHasChanges;
				args.PerformActionBeforeShowingForm = null;
				new AllocateNumberButtonClickEventHandler().Allocate(((JobDeclaration)JobDeclaration).FTZControlNumberSupporter, args);
			}
		}

		internal ZTabPage DocsTabPageInternal => DocsTabPage;

		internal ZTabPage OrdersTabPageInternal => OrdersTabPage;
	}
}
