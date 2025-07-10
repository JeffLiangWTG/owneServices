using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class AddInfoCUSDECValidation : AddInfoJobDeclarationValidation
	{
		public AddInfoCUSDECValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckSG_SupplyIndicator()
		{
			base.CheckSG_SupplyIndicator();

			if (Parent.SG_SupplyIndicator.IsEmpty && Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
			{
				foreach (JobComInvoiceLine line in Declaration.InvoiceLines)
				{
					if (line.SG_LastSellingPrice > 0)
					{
						Parent.SG_SupplyIndicatorInfo.AddMessageError(SupplyIndicatorRequired);
						break;
					}
				}
			}
		}
		internal const string SupplyIndicatorRequired = "For imported goods, if Last Selling Price has been entered on a line, the Supply Indicator must be 'Y'. LSP and not the CIF value, is used for computation of GST";

		protected override void CheckSG_OutwardVesselName()
		{
			base.CheckSG_OutwardVesselName();
			if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardVesselNameInfo, "Outward Vessel");
			}
		}

		protected override void CheckSG_OutwardVoyageFlightNo()
		{
			base.CheckSG_OutwardVoyageFlightNo();

			if (Parent.Declaration.IsTradeNet4Point1)
			{
				if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardVoyageFlightNoInfo, "Outward Voyage");
				}
				else if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
				{
					if (Parent.SG_OutwardVoyageFlightNo.IsEmpty && Parent.SG_OutwardFolio.IsEmpty)
					{
						ZString errorMsg = "You have not entered an Outward Flight Number, (and/or Aircraft Registration Number for chartered flight).";
						Parent.SG_OutwardVoyageFlightNoInfo.AddMessageError(errorMsg);
					}
				}
				else if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_3_Road)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardVoyageFlightNoInfo, "Vehicle Licence Registration Number");
				}
			}
			else
			{
				if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA || Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardVoyageFlightNoInfo, "Outward Voyage/Flight No.");
				}
			}
		}

		protected override void CheckSG_OutwardFolio()
		{
			base.CheckSG_OutwardFolio();

			if (Parent.Declaration.IsTradeNet4Point1)
			{
				if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
				{
					if (Parent.SG_OutwardFolio.IsEmpty && Parent.SG_OutwardVoyageFlightNo.IsEmpty)
					{
						Parent.SG_OutwardFolioInfo.AddMessageError("For chartered flights, enter the Aircraft Registration number.");
					}

					ValidateSG_OutwardVoyageFlightNo();
				}
			}
		}

		protected override void CheckSG_US_NKInwardVesselBerth()
		{
			if (Parent.Declaration.IsTradeNet4Point1)
			{
				//vessel berth no longer used
			}
			else
			{
				base.CheckSG_US_NKInwardVesselBerth();
				if (Declaration.JE_TransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_US_NKInwardVesselBerthInfo, "Inward Vessel Berth");
				}
			}
		}

		protected override void CheckSG_US_NKOutwardVesselBerth()
		{
			if (Parent.Declaration.IsTradeNet4Point1)
			{
				//vessel berth no longer used
			}
			else
			{
				base.CheckSG_US_NKOutwardVesselBerth();
				if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_US_NKOutwardVesselBerthInfo, "Outward Vessel Berth");
				}
			}
		}

		protected override void CheckSG_RL_NKNextPortOfCall()
		{
			base.CheckSG_RL_NKNextPortOfCall();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_RL_NKNextPortOfCallInfo, Declaration.Lookups.SGLocoList);
			if (!Parent.SG_OutwardTransportMode.IsEmpty && Parent.Declaration.IsSeaStore)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RL_NKNextPortOfCallInfo, "Next Port of Call");
			}
		}

		protected override void CheckSG_RL_NKFinalPortOfCall()
		{
			base.CheckSG_RL_NKFinalPortOfCall();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_RL_NKFinalPortOfCallInfo, Declaration.Lookups.SGLocoList);
			if (!Parent.SG_OutwardTransportMode.IsEmpty && Parent.Declaration.IsSeaStore && Parent.Declaration.HasLiquorOrTobacco)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RL_NKFinalPortOfCallInfo, "Final Port of Call");
			}
		}

		protected override void CheckSG_US_NKPlaceOfCargoRelease()
		{
			base.CheckSG_US_NKPlaceOfCargoRelease();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_US_NKPlaceOfCargoReleaseInfo, "Place of Release");
			Parent.Declaration.Validation.ValidateJE_OH_Consignee();
		}

		protected override void CheckSG_US_NKPlaceOfReceipt()
		{
			base.CheckSG_US_NKPlaceOfReceipt();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_US_NKPlaceOfReceiptInfo, "Place of Receipt");

			Declaration.Validation.ValidateJE_OH_Importer();
			ValidateSG_PreviousPermitNo();
		}

		protected override void CheckSG_US_NKPlaceOfStorage()
		{
			base.CheckSG_US_NKPlaceOfStorage();
			Parent.Declaration.Validation.ValidateJE_OH_Consignee();
		}

		protected override void CheckSG_PreviousPermitNo()
		{
			base.CheckSG_PreviousPermitNo();
			var placeOfReceipt = Declaration.PlaceOfReceipt;
			if (Parent.SG_PreviousPermitNo.IsEmpty && placeOfReceipt.IsShortPayment())
			{
				Parent.SG_PreviousPermitNoInfo.AddMessageError("A Previous Permit Number is required when Place of Receipt indicates a Short Payment");
			}
		}

		protected override void CheckSG_NoOfCrew()
		{
			base.CheckSG_NoOfCrew();
			CompareValidation.CheckNumberNotNegative(Parent.SG_NoOfCrewInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_NoOfCrewInfo, 500);

			if (Parent.Declaration.IsTradenet4 && Parent.SG_IsSeaStore && Parent.SG_NoOfCrew == 0)
			{
				Parent.SG_NoOfCrewInfo.AddMessageError("It is mandatory to specify the Number of Crew when a declaration is Sea Stores");
			}
		}

		protected override void CheckSG_VoyageDuration()
		{
			base.CheckSG_VoyageDuration();
			CompareValidation.CheckNumberNotNegative(Parent.SG_VoyageDurationInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_VoyageDurationInfo, 500);

			if (Parent.Declaration.IsTradenet4 && Parent.SG_IsSeaStore && Parent.SG_VoyageDuration == 0)
			{
				Parent.SG_VoyageDurationInfo.AddMessageError("It is mandatory to specify the Voyage Duration when a declaration is Sea Stores");
			}
		}

		protected override void CheckSG_IsSeaStore()
		{
			if (Parent.Declaration.IsTradenet4)
			{
				base.CheckSG_IsSeaStore();
				Parent.Declaration.Validation.ValidateJE_OH_Consignee();
			}
		}
	}
}
