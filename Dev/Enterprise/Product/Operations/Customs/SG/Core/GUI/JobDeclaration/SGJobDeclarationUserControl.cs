using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class SGJobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public SGJobDeclarationUserControl()
		{
			InitializeComponent();
		}

		public override BaseJobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set
			{
				Unhook(Declaration);
				base.JobDeclaration = value;
				Hook(Declaration);
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Unhook(Declaration);
			}
			base.Dispose(isNotFinalizing);
		}

		void Hook(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.SG_OutwardTransportModeInfo.ValueChanged += SG_OutwardTransportModeInfo_ValueChanged;
			}
		}

		void Unhook(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.SG_OutwardTransportModeInfo.ValueChanged -= SG_OutwardTransportModeInfo_ValueChanged;
			}
		}

		protected override void SetContainerCountAndNoOfPieces()
		{
			JE_ContainerCountCalcEdit.Visible = true;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
		}

		JobDeclaration Declaration => (JobDeclaration)JobDeclaration;

		void SG_OutwardTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetOutwardTransportVisibility();
		}

		#region Visibility

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetOutwardTransportVisibility();
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			SetInwardTransportVisibility();
			SetOutwardTransportVisibility();
		}

		protected override bool IsVoyageFlightNoVisible => !Declaration.IsOutwardTransportOnly && (JobDeclaration.IsAir || JobDeclaration.IsSea || (JobDeclaration.IsRoad && Declaration.IsTradeNet4Point1));

		void SetInwardTransportVisibility()
		{
			if (Declaration.IsOutwardTransportOnly)
			{
				JE_MasterBillForAirBoundTextBox.Visible = false;
				IsInwardHandCarriedCheckBox.Visible = false;
				FolioNumberTextBox.Visible = false;
				JE_MasterBillForSeaBoundTextBox.Visible = false;
				VesselFindBox.Visible = false;
				InwardBerthFindBox.Visible = false;
				InwardHousebillTextBox.Visible = false;
				InwardHousebillTextBox.Visible = false;
			}
			else
			{
				JE_MasterBillForAirBoundTextBox.Visible = Declaration.IsAir;
				IsInwardHandCarriedCheckBox.Visible = Declaration.IsAir;
				FolioNumberTextBox.Visible = Declaration.IsAir && Declaration.IsTradeNet4Point1;
				JE_MasterBillForSeaBoundTextBox.Visible = Declaration.IsSea;
				VesselFindBox.Visible = Declaration.IsSea;
				InwardBerthFindBox.Visible = Declaration.IsSea && !Declaration.IsTradeNet4Point1;
				InwardHousebillTextBox.Visible = !Declaration.JE_TransportMode.IsEmpty;
				InwardHousebillTextBox.Visible = !Declaration.JE_TransportMode.IsEmpty;
			}
		}

		void SetOutwardTransportVisibility()
		{
			if (Declaration != null)
			{
				OutwardMasterBillForAirControl.Visible = Declaration.IsOutwardTransportModeAir;
				IsOutwardHandCarriedCheckBox.Visible = Declaration.IsOutwardTransportModeAir;
				CharterRegistrationTextBox.Visible = Declaration.IsOutwardTransportModeAir && Declaration.IsTradeNet4Point1;
				OutwardOceanBillTextBox.Visible = Declaration.IsOutwardTransportModeSea;
				OutwardVesselCodeFindBox.Visible = Declaration.IsOutwardTransportModeSea;
				OutwardBerthCodeFindBox.Visible = Declaration.IsOutwardTransportModeSea && !Declaration.IsTradeNet4Point1;
				OutwardVesselNRTCalcEdit.Visible = Declaration.IsOutwardTransportModeSea;
				OutwardVesselNationalityFindBox.Visible = Declaration.IsOutwardTransportModeSea;
				OutwardVesselTypeDropDownEdit.Visible = Declaration.IsOutwardTransportModeSea;

				if (Declaration.IsOutwardTransportModeAir)
				{
					OutwardMasterBillForAirControl.GetExtension<ILabelCaptionRenderer>().Caption = "Master Bill";
					OutwardVoyageFlightTextBox.Visible = true;
					OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Flight/Rego.";
				}
				else if (Declaration.IsOutwardTransportModeSea)
				{
					OutwardOceanBillTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Ocean Bill";
					OutwardVoyageFlightTextBox.Visible = true;
					OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("A5F44C5F-5332-4627-BC7C-B05CC9AF4B83", "Voyage");
				}
				else if (Declaration.IsOutwardTransportModeRoad && Declaration.IsTradeNet4Point1)
				{
					OutwardVoyageFlightTextBox.Visible = true;
					OutwardVoyageFlightTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Registration";
				}
				else
				{
					OutwardVoyageFlightTextBox.Visible = false;
				}

				OutwardHouseBillTextBox.Visible = !Declaration.SG_OutwardTransportMode.IsEmpty;
			}
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible => true;

		#endregion
	}
}
