using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class AESTIRMessageBlockBuilder
	{
		public AESTIRMessageBlockBuilder(IAESTIRMessageAttachee attachee)
		{
			this.attachee = attachee;
		}

		public IEnumerable<MessageBlock> Build(UpdateActionCode action)
		{
			ClearCachedValues();
			AddSC1(action);

			if (!cancelShipment)
			{
				AddSC2();
				AddTransportationDetails();
				AddUSPPI();
				AddForwardingAgent();
				AddUltimateConsignee();
				AddIntermediateConsignee();
				AddCommodityLineItems();
			}

			return AESMessageBlocks;
		}

		List<MessageBlock> AESMessageBlocks
		{
			get { return fAESMessageBlocks ?? (fAESMessageBlocks = new List<MessageBlock>()); }
		}
		List<MessageBlock> fAESMessageBlocks;

		void ClearCachedValues()
		{
			fAESMessageBlocks = null;
		}

		readonly IAESTIRMessageAttachee attachee;
		bool cancelShipment;

		void AddSC1(UpdateActionCode action)
		{
			var aesCommShipSC1XP = new AESCommShipSC1XP();
			aesCommShipSC1XP.RelatedCompanyIndicator = attachee.RelatedCompanyIndicator;
			aesCommShipSC1XP.ModeOfTransportationCodeMOT = attachee.ModeOfTransportationCodeMOT;
			aesCommShipSC1XP.CountryOfUltimateDestinationCode = attachee.CountryOfUltimateDestinationCode;
			aesCommShipSC1XP.USStateOfOriginCode = attachee.USStateOfOriginCode;
			aesCommShipSC1XP.CarrierIDSCACIATA = attachee.CarrierIDSCACIATA;
			aesCommShipSC1XP.ShipmentReferenceNumber = attachee.ShipmentReferenceNumber;
			aesCommShipSC1XP.ShipmentFilingActionRequestIndicator = UpdateActionCodeConverter.ConvertToAESString(action);
			aesCommShipSC1XP.ConveyanceNameCarrierName = attachee.ConveyanceNameCarrierName;
			aesCommShipSC1XP.FilingOptionIndicator = attachee.FilingOptionIndicator;
			aesCommShipSC1XP.AEIFilingType = attachee.AEIFilingType;
			aesCommShipSC1XP.PortOfUnladingCode = attachee.PortOfUnladingCode;
			aesCommShipSC1XP.PortOfExportationCode = attachee.PortOfExportationCode;
			aesCommShipSC1XP.EstimatedDateOfExport = attachee.EstimatedDateOfExport;
			aesCommShipSC1XP.HazardousMaterialIndicatorHAZMAT = attachee.HazardousMaterialIndicatorHAZMAT;
			AESMessageBlocks.Add(aesCommShipSC1XP);

			cancelShipment = aesCommShipSC1XP.ShipmentFilingActionRequestIndicator == "X";
		}

		void AddSC2()
		{
			AESCommShipSC2XP aesCommShipSC2XP = new AESCommShipSC2XP();
			aesCommShipSC2XP.InbondCode = attachee.InbondCode;
			if (aesCommShipSC2XP.InbondCode.IsEmpty)
			{
				aesCommShipSC2XP.InbondCode = InbondTypeList.Codes.MerchandiseNOTShippedInbond;
			}

			aesCommShipSC2XP.EntryNumber = attachee.EntryNumber;
			aesCommShipSC2XP.ForeignTradeZoneIdentifier = attachee.ForeignTradeZoneIdentifier;
			aesCommShipSC2XP.RoutedExportTransactionIndicator = attachee.RoutedExportTransactionIndicator;
			aesCommShipSC2XP.OriginalITN = attachee.OriginalITN;
			AESMessageBlocks.Add(aesCommShipSC2XP);
		}

		void AddSC3(IAESTIRTransportationDetail transportationDetail)
		{
			AESCommShipSC3XP aesCommShipSC3XP = new AESCommShipSC3XP();
			aesCommShipSC3XP.EquipmentNumber = transportationDetail.EquipmentNumber;
			aesCommShipSC3XP.SealNumber = transportationDetail.SealNumber;
			aesCommShipSC3XP.TransportationReferenceNumber = FormattedTransportationReference(transportationDetail.TransportationReferenceNumber);
			AESMessageBlocks.Add(aesCommShipSC3XP);
		}

		ZString FormattedTransportationReference(ZString refNo)
		{
			if (attachee.ModeOfTransportationCodeMOT == TransportModeCodes.Codes.AirNonContainer || attachee.ModeOfTransportationCodeMOT == TransportModeCodes.Codes.AirContainer)
			{
				if (!refNo.IsEmpty && refNo.IndexOf('-') == -1)
				{
					refNo = refNo.SubstringSafe(0, 3) + "-" + refNo.SubstringSafe(4);
				}
			}

			return refNo;
		}

		void AddTransportationDetails()
		{
			IEnumerable<IAESTIRTransportationDetail> transportationDetails = attachee.TransportationDetails;
			if (transportationDetails != null)
			{
				foreach (IAESTIRTransportationDetail transportationDetail in transportationDetails)
				{
					if (transportationDetail.EquipmentNumber.IsEmpty && transportationDetail.SealNumber.IsEmpty && transportationDetail.TransportationReferenceNumber.IsEmpty)
					{
						// don't build SC3 segment without at least 1 of the conditional values present.
					}
					else
					{
						AddSC3(transportationDetail);
					}
				}
			}
		}

		void GenerateAESParty(IAESTIRParty party, ZString partyType, ZString uSPPIIRSNumber, ZString uSPPIIRSIDType, ZString toBeSoldEnRouteIndicator, ZString ultimateConsigneeType)
		{
			if (party != null)
			{
				var aesCommShipN01XP = new AESCommShipN01XP();
				aesCommShipN01XP.PartyType = partyType;
				aesCommShipN01XP.PartyID = party.PartyID;
				aesCommShipN01XP.PartyIDType = party.PartyIDType;
				aesCommShipN01XP.PartyName = party.PartyName;
				aesCommShipN01XP.ContactFirstName = party.ContactFirstName;
				aesCommShipN01XP.ContactLastName = party.ContactLastName;
				aesCommShipN01XP.ToBeSoldEnRouteIndicator = toBeSoldEnRouteIndicator;
				AESMessageBlocks.Add(aesCommShipN01XP);

				var aesCommShipN02XP = new AESCommShipN02XP();
				aesCommShipN02XP.AddressLine1 = party.AddressLine1;
				aesCommShipN02XP.AddressLine2 = party.AddressLine2;
				aesCommShipN02XP.ContactPhoneNumber = (aesCommShipN01XP.PartyType == AESTIRPartyTypeList.Codes.USPPI || aesCommShipN01XP.PartyType == AESTIRPartyTypeList.Codes.ForwardingAgent) ? party.ContactPhoneNumber.Right(10) : party.ContactPhoneNumber;
				AESMessageBlocks.Add(aesCommShipN02XP);

				var aesCommShipN03XP = new AESCommShipN03XP();
				aesCommShipN03XP.City = party.City;
				aesCommShipN03XP.StateCode = party.StateCode;
				aesCommShipN03XP.CountryCode = party.CountryCode;

				aesCommShipN03XP.PostalCode = party.PostalCode;
				aesCommShipN03XP.USPPIIRSNumber = uSPPIIRSNumber.SubstringSafe(0, 9);
				aesCommShipN03XP.USPPIIRSIDType = uSPPIIRSIDType;
				aesCommShipN03XP.UltimateConsigneeType = ultimateConsigneeType;
				AESMessageBlocks.Add(aesCommShipN03XP);
			}
		}

		void AddUSPPI()
		{
			GenerateAESParty(attachee.USPPI, AESTIRPartyTypeList.Codes.USPPI, attachee.USPPIIRSNumber, attachee.USPPIIRSIDType, ZString.Empty, ZString.Empty);
		}

		void AddForwardingAgent()
		{
			GenerateAESParty(attachee.ForwardingAgent, AESTIRPartyTypeList.Codes.ForwardingAgent, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		}

		void AddUltimateConsignee()
		{
			if (attachee.IsSoldEnRoute)
			{
				var aesCommShipN01XP = new AESCommShipN01XP();
				aesCommShipN01XP.PartyType = AESTIRPartyTypeList.Codes.UltimateConsignee;
				aesCommShipN01XP.PartyName = AESConstants.SoldEnRouteName;
				aesCommShipN01XP.ToBeSoldEnRouteIndicator = "Y";
				AESMessageBlocks.Add(aesCommShipN01XP);

				var aesCommShipN02XP = new AESCommShipN02XP();
				AESMessageBlocks.Add(aesCommShipN02XP);

				var aesCommShipN03XP = new AESCommShipN03XP();
				aesCommShipN03XP.City = attachee.CityOfFirstPortOfCall;
				aesCommShipN03XP.CountryCode = attachee.CountryOfFirstPortOfCall;
				aesCommShipN03XP.UltimateConsigneeType = UltimateConsigneeTypeList.Codes.Other;
				AESMessageBlocks.Add(aesCommShipN03XP);
			}
			else
			{
				GenerateAESParty(attachee.UltimateConsignee, AESTIRPartyTypeList.Codes.UltimateConsignee, ZString.Empty, ZString.Empty, "N", attachee.UltimateConsigneeType);
			}
		}

		void AddIntermediateConsignee()
		{
			GenerateAESParty(attachee.IntermediateConsignee, AESTIRPartyTypeList.Codes.IntermediateConsignee, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		}

		void AddCommodityLineItems()
		{
			IEnumerable<IAESTIRCommodityLineItem> commodityLineItems = attachee.CommodityLineItems;
			if (commodityLineItems != null)
			{
				foreach (IAESTIRCommodityLineItem lineItem in commodityLineItems)
				{
					var hasPGABlocksGenerated = false;
					var aesCommShipCL1XP = new AESCommShipCL1XP();
					aesCommShipCL1XP.ExportInformationCode = lineItem.ExportInformationCode;
					aesCommShipCL1XP.LineNumber = lineItem.LineNumber;
					aesCommShipCL1XP.CommodityDescription = lineItem.CommodityDescription.Left(45);
					aesCommShipCL1XP.LicenseValue = lineItem.LicenseValue;
					aesCommShipCL1XP.LineItemFilingActionRequestIndicator = "A";
					aesCommShipCL1XP.LicenseCodeLicenseExemptionCode = lineItem.LicenseCodeLicenseExemptionCode;
					aesCommShipCL1XP.ForeignDomesticOriginIndicator = lineItem.ForeignDomesticOriginIndicator;
					AESMessageBlocks.Add(aesCommShipCL1XP);

					var aesCommShipCL2XP = new AESCommShipCL2XP();
					if (lineItem.ExportInformationCode == ExportInformationCodeList.Codes.HH)
					{
						aesCommShipCL2XP.ScheduleBHTSNumber = ZString.Empty;
						aesCommShipCL2XP.UnitOfMeasure1 = ZString.Empty;
						aesCommShipCL2XP.Quantity1 = 0;
						aesCommShipCL2XP.UnitOfMeasure2 = ZString.Empty;
						aesCommShipCL2XP.Quantity2 = 0;
					}
					else
					{
						aesCommShipCL2XP.ScheduleBHTSNumber = lineItem.ScheduleBHTSNumber;
						aesCommShipCL2XP.UnitOfMeasure1 = lineItem.UnitOfMeasure1;
						aesCommShipCL2XP.Quantity1 = lineItem.Quantity1;
						aesCommShipCL2XP.UnitOfMeasure2 = lineItem.UnitOfMeasure2;
						aesCommShipCL2XP.Quantity2 = lineItem.Quantity2;
					}

					aesCommShipCL2XP.ValueOfGoods = lineItem.ValueOfGoods;
					aesCommShipCL2XP.ShippingWeight = lineItem.ShippingWeight;
					aesCommShipCL2XP.ExportControlClassificationNumberECCN = lineItem.ExportControlClassificationNumberECCN;
					aesCommShipCL2XP.ExportLicenseNumberCFRCitationAuthorizationSymbolKCPACM = lineItem.ExportLicenseNumberCFRCitationAuthorizationSymbolKCP;
					AESMessageBlocks.Add(aesCommShipCL2XP);

					if (!lineItem.DDTCEligiblePartyCertificationIndicator.IsEmpty ||
						!lineItem.DDTCITARExemptionNumber.IsEmpty ||
						!lineItem.DDTCQuantity.IsEmpty ||
						!lineItem.DDTCRegistrationNumber.IsEmpty ||
						!lineItem.DDTCSignificantMilitaryEquipmentSMEIndicator.IsEmpty ||
						!lineItem.DDTCUnitOfMeasureCode.IsEmpty ||
						!lineItem.DDTCUSMLCategoryCode.IsEmpty ||
						!lineItem.DDTCCommodityJurisdictionNumber.IsEmpty)
					{
						var aesCommShipODTXP = new AESCommShipODTXP();
						aesCommShipODTXP.DDTCITARExemptionNumber = lineItem.DDTCITARExemptionNumber;
						aesCommShipODTXP.DDTCRegistrationNumber = lineItem.DDTCRegistrationNumber;
						aesCommShipODTXP.DDTCSignificantMilitaryEquipmentSMEIndicator = lineItem.DDTCSignificantMilitaryEquipmentSMEIndicator;
						aesCommShipODTXP.DDTCEligiblePartyCertificationIndicator = lineItem.DDTCEligiblePartyCertificationIndicator;
						aesCommShipODTXP.DDTCUSMLCategoryCode = lineItem.DDTCUSMLCategoryCode;
						aesCommShipODTXP.DDTCUnitOfMeasureCode = lineItem.DDTCUnitOfMeasureCode;
						aesCommShipODTXP.DDTCQuantity = lineItem.DDTCQuantity;
						aesCommShipODTXP.DDTCCategoryXXIDeterminationNumber = lineItem.DDTCCommodityJurisdictionNumber;
						AESMessageBlocks.Add(aesCommShipODTXP);
					}

					IEnumerable<IAESTIRUsedVehicle> usedVehicles = lineItem.UsedVehicles;
					if (usedVehicles != null)
					{
						foreach (IAESTIRUsedVehicle usedVehicle in usedVehicles)
						{
							var aesCommShipEV1XP = new AESCommShipEV1XP();
							aesCommShipEV1XP.VehicleIdentificationNumberVINProductID = usedVehicle.VehicleIdentificationNumberVINProductID;
							aesCommShipEV1XP.VehicleIDQualifier = usedVehicle.VehicleIDQualifier;
							aesCommShipEV1XP.VehicleTitleNumber = usedVehicle.VehicleTitleNumber;
							aesCommShipEV1XP.VehicleTitleStateCode = usedVehicle.VehicleTitleStateCode;
							AESMessageBlocks.Add(aesCommShipEV1XP);
						}
					}

					foreach (MessageBlock pgaMessageBlock in ExportPGABlocksCreator.BuildPGABlocks(lineItem))
					{
						AESMessageBlocks.Add(pgaMessageBlock);
						hasPGABlocksGenerated = true;
					}

					aesCommShipCL1XP.PGALicenseRequiredIndicator = hasPGABlocksGenerated ? "Y" : string.Empty;
				}
			}
		}
	}
}
