using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public MiscOptionsUserControl()
		{
			InitializeComponent();
			ShowOrHideExportDetails();
			ShowOrHideBondAndCalculationCode();
			ShowOrHideDomesticCargoControl();
			SetDataSourceBinding(FDACCNStateDropEdit.GetExtension<LabelCaptionRenderer>(), "Caption", "CarrierCountryLabel", true);
			SetDataSourceBinding(FDACCNCountryCodeFindBox.GetExtension<LabelCaptionRenderer>(), "Caption", "CarrierCountryLabel", true);
			SetDataSourceBinding(_FDACANTextBox.GetExtension<LabelCaptionRenderer>(), "Caption", "CarrierNameLabel", true);
			ShowOrHideFDACountryAndStateContols();

			if (!DesignMode)
			{
				AnticipatedPortOfCrossingCodeFindBox.Visible = false;
			}
		}

		#region Control Visibility

		void ShowOrHideExportDetails()
		{
			var isExport = JobDeclaration != null && JobDeclaration.IsExport;
			var isPPQForm368Box13Compatible = JobDeclaration != null && JobDeclaration.IsPPQForm368Box13Compatible;
			AESGroupBox.Visible = isExport;
			MiscOptionsGroupBox.Visible = !isExport;
			EntryGroupBox.Visible = !isExport;
			RemoteFilingGroupBox.Visible = !isExport;
			ReConGroupBox.Visible = !isExport;
			PPQForm368Box13Button.Visible = !isExport && isPPQForm368Box13Compatible;
			BondGroupBox.Visible = !isExport;
			PaymentsGroupBox.Visible = !isExport;
			PriorNoticeGroupBox.Visible = !isExport;
		}

		void ShowOrHideBondAndCalculationCode()
		{
			var isVisible = JobDeclaration != null && JobDeclaration.IsSingleTransactionBond;
			var isContinuousBond = JobDeclaration != null && JobDeclaration.US_BondType == BondTypeList.Codes.ContinuousBond;
			var isContinuousBond2 = JobDeclaration != null && JobDeclaration.US_BondType2 == BondTypeList.Codes.ContinuousBond;
			var isSingleTransactionBond2 = JobDeclaration != null && (JobDeclaration.IsAdditionalSingleTransactionBond || JobDeclaration.US_BondType2.IsEmpty);

			BondAmountCalcEdit.Visible = isVisible;
			ACEBondAmountCalcEdit.Visible = isVisible;
			ACEBondDesignationCodeDropEdit.Visible = isVisible;
			ACEDispositionDropEdit.Visible = isVisible;
			ACEDispositionDropEdit2.Visible = isSingleTransactionBond2;
			BondNumberTextBox.Visible = isVisible;
			BondNumberTextBox2.Visible = isSingleTransactionBond2;

			BondCalcCodeDropEdit.Visible = isVisible;
			ACESEBCalcCodeDropEdit.Visible = isVisible;

			var bondProducerAccNoVisible = isVisible || isContinuousBond;
			BondProducerAccNoTextBox.Visible = bondProducerAccNoVisible;
			ACEBondProducerAccNoTextBox.Visible = bondProducerAccNoVisible;
			ACEBondAmount2CalcEdit.Visible = !isContinuousBond2;

			SupersedingCheckBox.Visible = isContinuousBond;

			ShowOrHideEBondControls();
			AdjustPositionOfWaivingReasonControls();
		}

		void ShowOrHideDomesticCargoControl()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			var isVisible = declaration != null && JobDeclaration.US_EntryType == EntryTypeList.Codes.Warehouse;
			DomesticCargoCheckBox.Visible = isVisible;
		}

		void ShowOrHideEBondControls()
		{
			var isInsuranceFunctionEnable = JobDeclaration?.IsInsuranceFunctionEnable ?? false;

			InsuranceAgentDropEdit.Visible = isInsuranceFunctionEnable;
			InsuranceDispositionTextBox.Visible = isInsuranceFunctionEnable;
			SendEBondRequestButton.Visible = isInsuranceFunctionEnable;
		}

		void AdjustPositionOfWaivingReasonControls()
		{
			var bondType = JobDeclaration?.US_BondType ?? ZString.Empty;
			var y = 42;

			switch (bondType)
			{
				case BondTypeList.Codes.ContinuousBond:
					{
						y = 88;
						break;
					}

				case BondTypeList.Codes.SingleTransactionBond:
					{
						y = 156;
						break;
					}
			}
			var buttonY = (bondType == BondTypeList.Codes.SingleTransactionBond) ? 179 : y - 1;
			WaiverCodeDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(122, y);
			ACEBondButton.Location = ControlDpiScalingHelper.NewScaledPoint(180, buttonY);
		}

		void ShowOrHideFDACountryAndStateContols()
		{
			FDACCNStateDropEdit.Visible = false;
			FDACCNCountryCodeFindBox.Visible = true;
			if (JobDeclaration != null)
			{
				FDACCNStateDropEdit.Visible = JobDeclaration.FDACCNStateCodeVisible;
				FDACCNCountryCodeFindBox.Visible = !JobDeclaration.FDACCNStateCodeVisible;
			}
		}

		void ShowOrHideFTZControls(bool isVisibleForFTZ, bool isExport, bool isPPQForm368Box13Compatible)
		{
			EntryGroupBox.Visible = !isVisibleForFTZ && !isExport;
			RemoteFilingGroupBox.Visible = !isVisibleForFTZ && !isExport;
			ReConGroupBox.Visible = !isVisibleForFTZ && !isExport;
			PPQForm368Box13Button.Visible = !isVisibleForFTZ && !isExport && isPPQForm368Box13Compatible;
			BondGroupBox.Visible = !isVisibleForFTZ && !isExport;
			PaymentsGroupBox.Visible = !isVisibleForFTZ && !isExport;
			US_ConsolidatedJobNumberFindBox.Visible = !isVisibleForFTZ && !isExport;

			if (isVisibleForFTZ)
			{
				var xValue = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(EntryGroupBox.Location.X);
				var yValue = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(EntryGroupBox.Location.Y);
				var point = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(xValue, yValue);
				PriorNoticeGroupBox.Location = point;
				ControlDpiScalingHelper.SetWidth(ref PriorNoticeGroupBox, MiscOptionsGroupBox.Width, false);

				OtherOptionsGroupBox.Visible = isVisibleForFTZ;
				ControlDpiScalingHelper.SetWidth(ref OtherOptionsGroupBox, MiscOptionsGroupBox.Width, false);

				var routingX = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(PriorNoticeGroupBox.Location.X);
				var routingY = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(PriorNoticeGroupBox.Location.Y + PriorNoticeGroupBox.Height) + 4;
				var routingPoint = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(routingX, routingY);
				OtherOptionsGroupBox.Location = routingPoint;
			}
		}

		#endregion

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				declaration.US_BondTypeInfo.ValueChanged -= US_BondTypeInfo_ValueChanged;
				declaration.US_BondType2Info.ValueChanged -= US_BondTypeInfo_ValueChanged;
				declaration.US_FDACANTypeInfo.ValueChanged -= US_FDACANTypeInfo_ValueChanged;
				declaration.JE_ApplicationCodeInfo.ValueChanged -= JE_ApplicationCodeInfo_ValueChanged;
				declaration.US_EntryTypeInfo.ValueChanged -= US_EntryTypeInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				declaration.JE_TransportModeInfo.ValueChanged -= JE_TransportModeInfo_ValueChanged;
				declaration.US_MonthlyFilingInfo.ValueChanged -= US_MonthlyFilingInfo_ValueChanged;
				declaration.US_CargoReleaseTypeInfo.ValueChanged -= US_CargoReleaseTypeInfo_ValueChanged;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ChangePPQBox13Visibility();
			ChangePortOfCrossingAndInspectionVisibility();
			ShowOrHideEntryDateElectionCodeAndDate();
		}

		void ChangePPQBox13Visibility()
		{
			var visible = false;
			var declaration = (JobDeclaration)CurrentDataItem;

			if (declaration != null)
			{
				visible = !declaration.IsExport && !declaration.IsFTZAdmission && declaration.IsPPQForm368Box13Compatible;
			}
			PPQForm368Box13Button.Visible = visible;
		}

		void ChangePortOfCrossingAndInspectionVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			var isACECargoCertificationMode = declaration?.IsACECargoCertificationMode ?? false;
			AnticipatedPortOfCrossingCodeFindBox.Visible = !isACECargoCertificationMode;
			FSISInspectionCodeFindBox.Visible = isACECargoCertificationMode;
			PGAInspectionFirmsFindBox.Visible = isACECargoCertificationMode;
			InspectionPortFindBox.Visible = isACECargoCertificationMode;
			PGAInspectionDateEdit.Visible = isACECargoCertificationMode;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			JobDeclaration declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				declaration.US_BondTypeInfo.ValueChanged += US_BondTypeInfo_ValueChanged;
				declaration.US_BondType2Info.ValueChanged += US_BondTypeInfo_ValueChanged;
				declaration.US_FDACANTypeInfo.ValueChanged += US_FDACANTypeInfo_ValueChanged;
				declaration.JE_ApplicationCodeInfo.ValueChanged += JE_ApplicationCodeInfo_ValueChanged;
				declaration.US_EntryTypeInfo.ValueChanged += US_EntryTypeInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				declaration.JE_TransportModeInfo.ValueChanged += JE_TransportModeInfo_ValueChanged;
				declaration.US_MonthlyFilingInfo.ValueChanged += US_MonthlyFilingInfo_ValueChanged;
				declaration.US_CargoReleaseTypeInfo.ValueChanged += US_CargoReleaseTypeInfo_ValueChanged;

				ShowOrHideExportDetails();
				ShowOrHideBondAndCalculationCode();
				ShowOrHideDomesticCargoControl();
				US_FDACANTypeInfo_ValueChanged(this, EventArgs.Empty);
				ShowOrHideFTZControls(declaration.IsFTZAdmission, declaration.IsExport, declaration.IsPPQForm368Box13Compatible);
				ChangeACEControlVisibility();
				ShowOrHideTotalMPF();
			}
		}

		void US_CargoReleaseTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangePortOfCrossingAndInspectionVisibility();
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideExportDetails();
			ShowOrHideBondAndCalculationCode();

			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				ShowOrHideFTZControls(declaration.IsFTZAdmission, declaration.IsExport, declaration.IsPPQForm368Box13Compatible);
			}
			ChangeACEControlVisibility();
			ShowOrHideTotalMPF();
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeACEControlVisibility();
			ShowOrHideEBondControls();
			ChangePortOfCrossingAndInspectionVisibility();
		}

		void ChangeACEControlVisibility()
		{
			var isACE = JobDeclaration != null && JobDeclaration.IsACE;
			ACEBondGroupBox.Visible = isACE;
			ACCVDBondGroupBox.Visible = isACE;

			var isExport = JobDeclaration != null && JobDeclaration.IsExport;
			var isFTZAdmission = JobDeclaration != null && JobDeclaration.IsFTZAdmission;
			BondGroupBox.Visible = !isACE && !isFTZAdmission && !isExport;
			ExpressConsignmentDropEdit.Visible = isACE;
			GoodsFromFTZCodeFindBox.Visible = isACE;
		}

		void US_BondTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideBondAndCalculationCode();
		}

		void US_FDACANTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideFDACountryAndStateContols();
		}

		void US_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideBondAndCalculationCode();
			ShowOrHideDomesticCargoControl();
			ShowOrHideEntryDateElectionCodeAndDate();
		}

		void ShowOrHideEntryDateElectionCodeAndDate()
		{
			var isVisible = JobDeclaration != null && JobDeclaration.IsConsumptionFTZ;

			EntryDateElectionCodeDropEdit.Visible = isVisible;
			PresentationDateEdit.Visible = isVisible;
		}

		void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideTotalMPF();
		}

		void US_MonthlyFilingInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideTotalMPF();
		}

		void ShowOrHideTotalMPF()
		{
			var isConsolidatedMonthlyFiling = JobDeclaration != null && JobDeclaration.IsConsolidatedMonthlyFilingOverPipeline;
			PayableMPFCalcEdit.Visible = isConsolidatedMonthlyFiling;
			PayableMPFCalcEdit.BringToFront();

			EstEnteredValueCalcEdit.Visible = !isConsolidatedMonthlyFiling;
			EstEnteredValueCalcEdit.BringToFront();
		}

		void PPQForm368Box13Button_Click(object sender, EventArgs e)
		{
			IPPQForm368NoticeOfArrivalSupportable box13DataSupportable = JobDeclaration;
			if (box13DataSupportable != null)
			{
				PPQForm368NoticeOfArrivalData noticeOfArrivalData = new PPQForm368NoticeOfArrivalData(box13DataSupportable);
				PPQForm368NoticeOfArrivalDataForm noticeOfArrivalDataForm = new PPQForm368NoticeOfArrivalDataForm(noticeOfArrivalData);
				ZFormModaliser.Show(noticeOfArrivalDataForm, this.ParentForm);
			}
		}

		void RefreshBondButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show(RefreshBondDetailsWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				new BondDetailsDefaulter().Default(JobDeclaration, JobDeclaration.US_BondType);
			}
		}

		public const string RefreshBondDetailsWarning = "Are you sure you want to refresh the Bond details?";

		void SendEBondRequestButton_Click(object sender, EventArgs e)
		{
			if (JobDeclaration.HasChanges || Globals.Message.Show(SendEBondRequestWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				SenderController.SendMessages<EBondMessageSender>(ImportMessageSendingMessageType.EBondRequest, ImportMessageSendingMessageTypeAdditionalFilter.EntrySummary);
			}
		}

		MessageSenderController SenderController
		{
			get => senderController ?? (senderController = new MessageSenderController(JobDeclaration, new Lazy<ZForm>(() => ParentForm as ZForm), ShowMessageSendingAction));
		}
		MessageSenderController senderController;

		#region ShowImportMessageSendingActionForm

		bool ShowMessageSendingAction(ImportMessageSendingActionCollection actions)
		{
			var result = true;

			actions.IsCancelled = true;
			actions.RunPreSaveValidation();

			if (actions.HasErrors())
			{
				using (var msgBox = new ZErrorMessageBox(actions, "message", "send", "sent"))
				{
					ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					result = false;
				}
			}
			else if (((INotificationProvider)actions).HasNotifications(CargoWise.EntityFramework.NotificationType.MessageError) && !Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
			{
				Globals.Message.ShowInformation(Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight);
			}
			else
			{
				actions.IsCancelled = false;
			}

			return result;
		}

		#endregion

		public const string SendEBondRequestWarning = "Are you sure you want to send the eBond request?";

		void NAFTAStatButton_Click(object sender, EventArgs e)
		{
			StatDescriptionForm form = new StatDescriptionForm("NAFTA");
			ZFormModaliser.ShowDialogAndDispose(form, ParentForm as ZForm);
		}

		void ProtestStatButton_Click(object sender, EventArgs e)
		{
			StatDescriptionForm form = new StatDescriptionForm("Protest");
			ZFormModaliser.ShowDialogAndDispose(form, ParentForm as ZForm);
		}
	}
}
