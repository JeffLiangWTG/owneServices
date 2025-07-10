using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business
{
	public class TranshipmentRequestValidation : CusUnderbondValidation
	{
		public TranshipmentRequestValidation(TranshipmentRequest parent) : base(parent)
		{
		}

		new TranshipmentRequest Parent => (TranshipmentRequest)base.Parent;
		ITranshipmentRequestParent TranshipmentRequestParent => Parent.Parent;

		internal bool IsATranshipmentRequest => Parent.IsATranshipment && (!Parent.C4_ModeOfMovement.IsEmpty || !Parent.C4_TranshipModeOfMovement.IsEmpty);
		bool SeaTranshipment => Parent.IsATranshipment && Parent.IsSeaJob;
		bool AirTranshipment => Parent.IsATranshipment && Parent.IsAirJob;

		bool IsDTR => Parent.IsDTR;
		bool IsITR => Parent.IsITR;

		bool IsICR_Transhipment
		{
			get
			{
				var result = false;
				var requestParent = TranshipmentRequestParent;
				if (requestParent != null)
				{
					result = requestParent.IsTSWICRWriteOff && IsATranshipmentRequest;
				}

				return result;
			}
		}

		bool IsCRE_Transhipment
		{
			get
			{
				var result = false;
				var requestParent = TranshipmentRequestParent;
				if (requestParent != null)
				{
					result = requestParent.IsTSWCREWriteOff && IsATranshipmentRequest;
				}

				return result;
			}
		}

		protected override void CheckC4_ArrivalDate()
		{
			if (IsCRE_Transhipment)
			{
				base.CheckC4_ArrivalDate();
				if (SeaTranshipment || AirTranshipment)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_ArrivalDateInfo, "Arrival Date");
					var requestParent = TranshipmentRequestParent;
					if (requestParent != null && Parent.C4_ArrivalDate > requestParent.DepartureDate)
					{
						Parent.C4_ArrivalDateInfo.AddMessageError(ArrivalDateCannotBeAfterDepartureDate);
					}
				}
			}
		}

		protected override void CheckC4_TranshipDepartureDate()
		{
			if (IsICR_Transhipment && IsITR)
			{
				base.CheckC4_TranshipDepartureDate();
				if (SeaTranshipment && Parent.C4_TranshipModeOfMovement == TranshipmentRequestModeOfMovement.Codes.Sea)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_TranshipDepartureDateInfo, "Departure date");
					if (Parent.C4_TranshipDepartureDate.IsValid)
					{
						var requestParent = TranshipmentRequestParent;
						if (requestParent != null && Parent.C4_TranshipDepartureDate < requestParent.ArrivalDate)
						{
							Parent.C4_TranshipDepartureDateInfo.AddMessageError(DepartureDateCannotBeBeforeArrivalDate);
						}
					}
				}
			}
		}

		protected override void CheckC4_FlightNo()
		{
			if (IsCRE_Transhipment)
			{
				base.CheckC4_FlightNo();
				if (AirTranshipment)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_FlightNoInfo, "Flight number");
					if (!FlightsAndVesselsHelper.IsValidFlightOrVessel(Parent.Factory, Parent.C4_FlightNo))
					{
						Parent.C4_FlightNoInfo.AddMessageError(FlightNotOnCustomsSupportedList);
					}
				}
			}
		}

		protected override void CheckC4_TranshipBySeaVessel()
		{
			if (IsATranshipmentRequest && IsITR)
			{
				base.CheckC4_TranshipBySeaVessel();
				if (SeaTranshipment && Parent.C4_TranshipModeOfMovement == TranshipmentRequestModeOfMovement.Codes.Sea)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_TranshipBySeaVesselInfo, "Vessel");
					if (!FlightsAndVesselsHelper.IsValidFlightOrVessel(Parent.Factory, Parent.C4_TranshipBySeaVessel))
					{
						Parent.C4_TranshipBySeaVesselInfo.AddMessageError(VesselNotOnCustomsSupportedList);
					}

					if (!RefVessel.LookupVesselsByNameAndLloyds(Parent.C4_TranshipBySeaVessel, Parent.C4_TranshipBySeaLloydsIMONum, Parent.Factory).Any())
					{
						Parent.C4_TranshipBySeaVesselInfo.AddWarning("A Vessel with this Name and Lloyds Number cannot be found.");
					}
				}
			}
		}

		protected override void CheckC4_TranshipBySeaVoyage()
		{
			if (IsATranshipmentRequest && IsITR)
			{
				base.CheckC4_TranshipBySeaVoyage();
				if (SeaTranshipment && Parent.C4_TranshipModeOfMovement == TranshipmentRequestModeOfMovement.Codes.Sea)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.C4_TranshipBySeaVoyageInfo, "Voyage number");
				}
			}
		}

		protected override void CheckC4_MovementReason()
		{
			base.CheckC4_MovementReason();
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.C4_MovementReasonInfo);

			var cusHawb = parent.Parent as CusHAWB;
			if (cusHawb != null)
			{
				if (parent.C4_MovementReason == MovementReason.Codes.InternationalTranshipmentRequest && cusHawb.CS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
				{
					parent.C4_MovementReasonInfo.AddMessageError(Res.GetString("C3309B83-20F8-46A1-A0B0-CA262D3E3D1D", "The destination port entered would indicate this consignment is not an International Transhipment"));
				}
				else if (parent.C4_MovementReason == MovementReason.Codes.DomesticTranshipmentRequest && !cusHawb.CS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
				{
					parent.C4_MovementReasonInfo.AddMessageError(Res.GetString("C0B54006-D815-41C8-8DBB-936D55064F60", "The destination port entered would indicate this consignment is not a Domestic Transhipment"));
				}
			}

			var houseBill = parent.Parent as Express.CusSCAHouse;
			if (houseBill != null)
			{
				if (parent.C4_MovementReason == MovementReason.Codes.InternationalTranshipmentRequest && houseBill.CA_RL_NK_PortOfDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
				{
					parent.C4_MovementReasonInfo.AddMessageError(Res.GetString("C3309B83-20F8-46A1-A0B0-CA262D3E3D1D", "The destination port entered would indicate this consignment is not an International Transhipment"));
				}
				else if (parent.C4_MovementReason == MovementReason.Codes.DomesticTranshipmentRequest && !houseBill.CA_RL_NK_PortOfDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
				{
					parent.C4_MovementReasonInfo.AddMessageError(Res.GetString("C0B54006-D815-41C8-8DBB-936D55064F60", "The destination port entered would indicate this consignment is not a Domestic Transhipment"));
				}
			}
		}

		protected override void CheckC4_ModeOfMovement()
		{
			base.CheckC4_ModeOfMovement();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.C4_ModeOfMovementInfo);
			if (IsDTR || !parent.C4_TranshipModeOfMovement.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.C4_ModeOfMovementInfo);
			}
		}

		protected override void CheckC4_TranshipModeOfMovement()
		{
			base.CheckC4_TranshipModeOfMovement();
			ListValidation.MessageErrorIfInvalidCode(Parent.C4_TranshipModeOfMovementInfo);
		}

		protected override void CheckC4_OA_DestinationAddress()
		{
			if (IsICR_Transhipment)
			{
				base.CheckC4_OA_DestinationAddress();
				var orgToCheck = (OrgHeader)Parent.C4_OA_DestinationAddress_ZAddress.OrgHeader;
				if (orgToCheck == null)
				{
					if (SeaTranshipment)
					{
						if (Parent.C4_RL_NKTranshipDestPort.IsEmpty)
						{
							Parent.C4_OA_DestinationAddressInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, PremiseCodeRequiredForSea, Parent.C4_MovementReason));
						}
					}
					else
					{
						Parent.C4_OA_DestinationAddressInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, PremiseCodeRequiredForAir, Parent.C4_MovementReason));
					}
				}
				else
				{
					var premiseId = GetCustomsCode(orgToCheck, OrgCusCode.CodeTypes.ControlledPremisesID);
					if (premiseId.IsEmpty)
					{
						Parent.C4_OA_DestinationAddressInfo.AddMessageError(PremiseIDNotEnteredOnOrg);
					}

					premiseId = Parent.DestinationAddress.GetResponsiblePartyPremiseID() ;
					if (premiseId.IsEmpty)
					{
						Parent.C4_OA_DestinationAddressInfo.AddMessageError(TransitDestinationNotLinkedToCode);
					}

					if (!Parent.C4_RL_NKTranshipDestPort.IsEmpty)
					{
						Parent.C4_OA_DestinationAddressInfo.AddError(PremiseCodeOrTransitPortForSea);
					}

					if (Parent.C4_OA_DestinationAddress == Parent.C4_OA_OriginAddress)
					{
						Parent.C4_OA_DestinationAddressInfo.AddMessageError(DestinationAddressIsSameWithOriginAddress);
					}
				}

				ValidateC4_RL_NKTranshipDestPort();
			}
		}

		protected override void CheckC4_OA_OriginAddress()
		{
			if (IsICR_Transhipment)
			{
				base.CheckC4_OA_OriginAddress();
				if (Parent.C4_OA_OriginAddress.IsValid)
				{
					var premiseId = Parent.OriginAddress.GetResponsiblePartyPremiseID();
					if (premiseId.IsEmpty)
					{
						Parent.C4_OA_OriginAddressInfo.AddMessageError(OriginDestinationNotLinkedToCode);
					}
				}
				if (!Parent.C4_OA_OriginAddress.IsEmpty && Parent.C4_OA_DestinationAddress == Parent.C4_OA_OriginAddress)
				{
					Parent.C4_OA_OriginAddressInfo.AddMessageError(DestinationAddressIsSameWithOriginAddress);
				}
			}
		}

		protected override void CheckC4_RL_NKTranshipDestPort()
		{
			if (IsICR_Transhipment)
			{
				base.CheckC4_RL_NKTranshipDestPort();
				if (SeaTranshipment)
				{
					OrgHeader orgToCheck = (OrgHeader)Parent.C4_OA_DestinationAddress_ZAddress.OrgHeader;
					if (orgToCheck == null)
					{
						if (Parent.C4_RL_NKTranshipDestPort.IsEmpty)
						{
							Parent.C4_RL_NKTranshipDestPortInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, PremiseCodeRequiredForSea, Parent.C4_MovementReason));
						}
						else if (!Parent.C4_RL_NKTranshipDestPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
						{
							Parent.C4_RL_NKTranshipDestPortInfo.AddMessageError(TransitPortMustBeNZ);
						}
					}
					else if (!Parent.C4_RL_NKTranshipDestPort.IsEmpty)
					{
						Parent.C4_RL_NKTranshipDestPortInfo.AddError(PremiseCodeOrTransitPortForSea);
					}

					ValidateC4_OA_DestinationAddress();
				}
			}
		}

		ZString GetCustomsCode(OrgHeader organisation, params ZString[] codeTypes)
		{
			return organisation == null ? ZString.Empty : organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.NewZealand, codeTypes);
		}

		const string ArrivalDateCannotBeAfterDepartureDate = "Transhipment request Arrival Date cannot be after the declaration Departure Date";
		const string DepartureDateCannotBeBeforeArrivalDate = "Transhipment request Departure Date cannot be before the declaration Arrival Date";
		const string DestinationAddressIsSameWithOriginAddress = "Transhipment request Transit Destination cannot be same with Origin/Goods Location";
		const string FlightNotOnCustomsSupportedList = "Flight number of the import craft is not in the list of valid Flights supported by NZ Customs.";
		const string VesselNotOnCustomsSupportedList = "Vessel name of the import craft is not in the list of valid Vessel names supported by NZ Customs.";
		const string OriginDestinationNotLinkedToCode = "The address selected for the Origin/Goods Destination organisation does not have a Controlled Premise ID code (CCP) associated with it.\r\nPlease update the Organisation > Config grid to add the relevant CCP code for this address.";
		const string PremiseCodeRequiredForSea = "Transit Destination Premise code is mandatory for {0}.\r\nEnter an organisation with a Premise code issued by TSW or optionally, if destined to a New Zealand port, enter the Transit Dest. Port";
		const string PremiseCodeRequiredForAir = "Transit Destination Premise code is mandatory for {0}.\r\nEnter an organisation with a valid Premise code issued by TSW";
		const string PremiseIDNotEnteredOnOrg = "The Customs Premise ID (CCP) has not been entered on the organisation chosen for the Transit Destination.";
		const string TransitDestinationNotLinkedToCode = "The address selected for the Transit Destination organisation does not have a Controlled Premise ID code (CCP) associated with it.\r\nPlease update the Organisation > Config grid to add the relevant CCP code for this address.";
		const string TransitPortMustBeNZ = "Transit Destination Port must be a New Zealand port code.";
		const string PremiseCodeOrTransitPortForSea = "Only 1 of Transit Destination Premise code or Transit Destination Port can be entered.";
	}
}
