using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ZADeclarationUserControl : Customs.GUI.BaseCustomsDeclarationUserControl
	{
		ZDateEdit issuedDateDateEdit;
		ZCodeFindBox issuedAtCodeFindBox;
		ZDateEdit houseBillIssuedDateDateEdit;
		ZCodeFindBox uZ_CargoCarrierCodeFindBox;
		internal ZDropEdit paidByDropEdit;
		ZStmNotePopupButton marksAndNumbersStmNotePopupButton;
		protected ZArchitecture.ZTextBox marksAndNumbersZTextbox;
		internal ZArchitecture.ZTextBox transitManifestTextBox;
		ZArchitecture.ZTextBox agentsReferenceTextBox;
		internal ZCodeFindBox masterCargoCarrierCodeFindBox;
		ZDropEdit jE_CustomsOfficeDropEdit;
		internal ZCodeFindBox jE_LocationOfGoodsCodeFindBox;
		ZArchitecture.ZTextBox rulesOfOriginCertificateTextBox;
		ZCodeFindBox jE_GoodsOriginCodeFindBox;
		ZArchitecture.ZTextBox agentCodeTextBox;
		ZDropEdit rooTypeDropEdit;
		public RadioCallSignCodeFindBox UZ_RadioCallSignTextBox;
		internal ZCodeFindBox uZ_CarrierCodeFindBox;
		internal ZArchitecture.ZTextBox uZ_Trailer1TextBox;
		internal ZArchitecture.ZTextBox uZ_Trailer2TextBox;
		internal ZArchitecture.ZTextBox jE_MasterBillForAirBoundNonIATATextBox;
		internal ZCheckBox nonIATAFormatCheckBox;
		internal ZCodeFindBox zCodeFindBoxVesselAgent;
		ZDropEdit removalTransportCodeDropEdit;

		public ZADeclarationUserControl()
		{
			InitializeComponent();
			if (ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
			{
				AddDefaultDetailsGroupBoxAndMoveControls();
			}
#if DEBUG
			TypeDescriptor.AddAttributes(jE_CustomsOfficeDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		protected void MarksAndNumbersStmNotePopupButton_NoteHasChangesChanged()
		{
			JobDeclaration.JE_MarksAndNumbersShortInfo.RefreshBinding();
			JobDeclaration.Validation.ValidateJE_MarksAndNumbersShort();
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			UZ_RadioCallSignTextBox.Visible = JobDeclaration.IsSea;
			uZ_CarrierCodeFindBox.Visible = JobDeclaration.IsSea;
			if (TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(JobDeclaration))
			{
				masterCargoCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 110, true);
				zCodeFindBoxVesselAgent.Visible = !JobDeclaration.IsExport;
			}
			else
			{
				masterCargoCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 110, true);
				zCodeFindBoxVesselAgent.Visible = false;
			}

			masterCargoCarrierCodeFindBox.Visible = JobDeclaration.IsSea && (!JobDeclaration.IsExport && !JobDeclaration.IsExWarehouse);

			JE_MessageSubTypeBoundDropDownEdit.Visible = JobDeclaration.IsExport;
			transitManifestTextBox.Visible = JobDeclaration.IsRoad || JobDeclaration.IsRail || JobDeclaration.IsPost || JobDeclaration.JE_TransportMode == Core.Constants.TransportModes.FixedTransportInstallations;
			if (transitManifestTextBox.Visible)
			{
				transitManifestTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
			paidByDropEdit.Visible = JobDeclaration.IsDeclarationIntegrated;
			rooTypeDropEdit.Visible = JobDeclaration.IsExport;
			nonIATAFormatCheckBox.Visible = IsJE_MasterBillForAirBoundTextBoxVisible;
			IsNonIATAFormatAirWayBillInfo_ValueChanged(this, null);

			if (JobDeclaration.IsExport)
			{
				rulesOfOriginCertificateTextBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(297);
			}
			else
			{
				rulesOfOriginCertificateTextBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			}
			ShowOrHideVoyageFlightVehicleControls();
			ShowOrHideExBondControls();

			if (JobDeclaration.JE_TransportMode.IsEmpty)
			{
				JE_TransportModeBoundDropDownEdit.DescriptionBox.Text = JobDeclaration.Lookups.TransportTypeList.GetDescriptionFromCode(ZString.Empty);
			}
		}

		void ShowOrHideVoyageFlightVehicleControls()
		{
			if (JobDeclaration.IsRoad)
			{
				ControlDpiScalingHelper.SetTop(this.JE_VoyageFlightNoBoundTextBox, 64, true);
				uZ_Trailer2TextBox.Visible = uZ_Trailer1TextBox.Visible = true;
			}
			else
			{
				uZ_Trailer2TextBox.Visible = uZ_Trailer1TextBox.Visible = false;
				ControlDpiScalingHelper.SetTop(this.JE_VoyageFlightNoBoundTextBox, 88, true);
			}
		}

		void ShowOrHideExBondControls()
		{
			if (JobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.ExBond)
			{
				SupplierOrganisationControl.Visible = false;
				jE_LocationOfGoodsCodeFindBox.Visible = false;
				PortOfLoadingFindBox.Visible = false;
				JE_ExportDateBoundDateEdit.Visible = false;
				PortOfDischargeFindBox.Visible = false;

				if (JobDeclaration.IsExBondAndAutomaticDeferred)
				{
					JE_DateOfArrivalBoundDateEdit.Visible = true;
					this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 132, true);
				}
				else
				{
					JE_DateOfArrivalBoundDateEdit.Visible = false;
					this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 183, true);
				}
			}
			else
			{
				SupplierOrganisationControl.Visible = true;
				jE_LocationOfGoodsCodeFindBox.Visible = true;
				PortOfLoadingFindBox.Visible = true;
				JE_ExportDateBoundDateEdit.Visible = true;
				PortOfDischargeFindBox.Visible = true;
				JE_DateOfArrivalBoundDateEdit.Visible = true;
				JE_DateOfArrivalBoundDateEdit.Visible = true;
				this.JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 183, true);
			}
		}

		protected override void SetContainerCountAndNoOfPieces()
		{
			JE_ContainerCountCalcEdit.Visible = false;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
		}

		protected override bool OwnerReferenceVisible
		{
			get { return JobDeclaration.IsImport || JobDeclaration.IsExport; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			IsNonIATAFormatAirWayBillInfo_ValueChanged(this, null);
			JobDeclaration.JE_IsNonIATAFormatAirWayBillInfo.ValueChanged += IsNonIATAFormatAirWayBillInfo_ValueChanged;
		}

		void IsNonIATAFormatAirWayBillInfo_ValueChanged(object sender, EventArgs e)
		{
			JE_MasterBillForAirBoundTextBox.Visible = nonIATAFormatCheckBox.Visible && !JobDeclaration.JE_IsNonIATAFormatAirWayBill;
			jE_MasterBillForAirBoundNonIATATextBox.Visible = nonIATAFormatCheckBox.Visible && JobDeclaration.JE_IsNonIATAFormatAirWayBill;
		}

		void AddDefaultDetailsGroupBoxAndMoveControls()
		{
			SuspendLayout();
			ShipmentDetailsGroupBox.SuspendLayout();
			ShipmentDetailsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;

			var y = 14;
			var lastTop = HouseBillParcelPostTextEdit.Top;
			foreach (var control in new[] { (Control)HouseBillParcelPostTextEdit, houseBillIssuedDateDateEdit, uZ_CargoCarrierCodeFindBox, OriginFindBox, JE_ExportDateBoundDateEdit2, FinalDestinationFindBox,
				JE_DateOfArrivalBoundDateEdit2, WeightzCalcDropEdit, VolumeCalcDropEdit, TotalNoOfPacksCalcDropEdit, marksAndNumbersZTextbox, marksAndNumbersStmNotePopupButton,
				ScreenButton, ScreeningStatusDropEdit })
			{
				if (Math.Abs(control.Top - lastTop) > 2)
				{
					y += 22;
				}
				lastTop = control.Top;
				control.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(y);
			}
			ShipmentDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(y + 30);

			var defaultDetailsGroupBox = new ZGroupBox();
			Controls.Add(defaultDetailsGroupBox);
			defaultDetailsGroupBox.SuspendLayout();
			defaultDetailsGroupBox.CaptionResourceString = Res.GetData("D81A7617-C0D5-4FBE-AD20-17AEC0666119", "Default Details");
			defaultDetailsGroupBox.Left = ShipmentDetailsGroupBox.Left;
			defaultDetailsGroupBox.Name = "DefaultDetailsGroupBox";
			defaultDetailsGroupBox.TabStop = false;
			defaultDetailsGroupBox.Top = ShipmentDetailsGroupBox.Top + ShipmentDetailsGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
			defaultDetailsGroupBox.Width = ShipmentDetailsGroupBox.Width;

			var exchangeRateDateControl = new ZDateEdit();
			defaultDetailsGroupBox.Controls.Add(exchangeRateDateControl);
			exchangeRateDateControl.AllowDrop = true;
			exchangeRateDateControl.AutoCompleteMonthThreshold = 1;
			BindingSource.SetBindingMember(exchangeRateDateControl, JobDeclaration.Schema.JE_ValuationDate);
			exchangeRateDateControl.CaptionResourceString = Res.GetData("58CD206E-7092-48E7-8E15-B7D81C759FE6", "Exchange Rate Date");
			exchangeRateDateControl.Location = ControlDpiScalingHelper.NewScaledPoint(120, 14);
			exchangeRateDateControl.Name = "ExchangeRateDateControl";
			exchangeRateDateControl.TabIndex = 0;

			y = 14;
			lastTop = 0;
			var tabIndex = 1;
			foreach (var control in new[] { (Control)jE_GoodsOriginCodeFindBox, rooTypeDropEdit, rulesOfOriginCertificateTextBox, GoodsDescriptionTextBox, OwnersReferenceTextBox, IncoTermDropEdit, IncoTermExplainButton, agentsReferenceTextBox })
			{
				if (Math.Abs(control.Top - lastTop) > 2)
				{
					y += 22;
				}

				ShipmentDetailsGroupBox.Controls.Remove(control);
				defaultDetailsGroupBox.Controls.Add(control);

				lastTop = control.Top;
				control.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(y);
				control.TabIndex = tabIndex++;
			}
			defaultDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(y + 30);

			ShipmentDetailsGroupBox.ResumeLayout(false);
			ShipmentDetailsGroupBox.PerformLayout();
			defaultDetailsGroupBox.ResumeLayout(false);
			defaultDetailsGroupBox.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.JE_IsNonIATAFormatAirWayBillInfo.ValueChanged -= IsNonIATAFormatAirWayBillInfo_ValueChanged;
			}
			base.Dispose(isNotFinalizing);
		}
	}
}

