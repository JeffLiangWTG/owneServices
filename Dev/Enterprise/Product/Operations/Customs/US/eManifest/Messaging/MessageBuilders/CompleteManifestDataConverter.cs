using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact.D08A.Elements;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal static class CompleteManifestDataConverter
	{
		#region Conversions

		#region Identification Code

		internal static string GetIdentificationCodeFromCrewACEIdType(BusinessObjectFactory factory, string crewACEIdType)
		{
			return GetCrewACEIdTypesDictionary(factory).GetValue(crewACEIdType, key => key);
		}

		internal static ZString GetCrewACEIdTypeFromIdentificationCode(BusinessObjectFactory factory, string idCode)
		{
			return GetCrewACEIdTypesDictionary(factory).GetKey(idCode);
		}

		static Dictionary<string, string> GetCrewACEIdTypesDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.CrewACEIdTypesDictionary",
				() => new Dictionary<string, string>
				{
					{ CrewACEIdTypes.Codes.Id, IdentificationCodes.ACEId },
					{ CrewACEIdTypes.Codes.ProximityCardId, IdentificationCodes.ACEProximityCardId }
				});
		}

		internal static string GetIdentificationCodePartyIdType(BusinessObjectFactory factory, string idType)
		{
			return GetPartyIdTypesDictionary(factory).GetValue(idType, key => string.Empty);
		}

		internal static ZString GetPartyIdTypeFromIdentificationCode(BusinessObjectFactory factory, string idCode)
		{
			return GetPartyIdTypesDictionary(factory).GetKey(idCode);
		}

		static Dictionary<string, string> GetPartyIdTypesDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.PartyIdTypesDictionary",
				() => new Dictionary<string, string>
				{
					{ PartyIdTypes.Codes.ACE, IdentificationCodes.ACEId },
					{ PartyIdTypes.Codes.FAST, IdentificationCodes.FAST },
					{ PartyIdTypes.Codes.FilerCode, IdentificationCodes.FilerCode },
					{ PartyIdTypes.Codes.FIRMS, IdentificationCodes.FIRMS },
					{ PartyIdTypes.Codes.SocialSecurityNumber, IdentificationCodes.SSNOrEIN },
					{ PartyIdTypes.Codes.EmployerIdentificationNumber, IdentificationCodes.SSNOrEIN },
					{ PartyIdTypes.Codes.CustomsAssignedNumber, IdentificationCodes.CustomsAssignedNumber },
					{ PartyIdTypes.Codes.DUNS, IdentificationCodes.DUNS },
					{ PartyIdTypes.Codes.SCAC, IdentificationCodes.SCAC }
				});
		}

		internal static string GetIdentificationCodeFromLocationCodeType(BusinessObjectFactory factory, string locationCodeType)
		{
			return GetLocationCodeTypesDictionary(factory).GetValue(locationCodeType, key => key);
		}

		internal static ZString GetLocationCodeTypeFromIdentificationCode(BusinessObjectFactory factory, string idCode)
		{
			return GetLocationCodeTypesDictionary(factory).GetKey(idCode);
		}

		static Dictionary<string, string> GetLocationCodeTypesDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.LocationCodeTypesDictionary",
				() => new Dictionary<string, string>
				{
					{ PortCodeTypes.Codes.ScheduleD,            IdentificationCodes.ScheduleD },
					{ PortCodeTypes.Codes.ScheduleK,            IdentificationCodes.ScheduleK },
					{ PortCodeTypes.Codes.InlandScheduleK,  IdentificationCodes.InlandScheduleK },
					{ PortCodeTypes.Codes.IATA,             IdentificationCodes.IATA },
					{ PortCodeTypes.Codes.LocationName,     IdentificationCodes.FreeFormText },
				});
		}

		#endregion

		#region Party Qualifier

		internal static PartyFunctionCodeQualifierList GetCrewPartyQualifier(BusinessObjectFactory factory, string crewType)
		{
			return GetCrewDictionary(factory).GetValue(crewType, key => PartyFunctionCodeQualifierList.GetFromString(crewType));
		}

		internal static ZString GetCrewTypeFromQualifier(BusinessObjectFactory factory, PartyFunctionCodeQualifierList qualifier)
		{
			return GetCrewDictionary(factory).GetKey(qualifier);
		}

		static Dictionary<string, PartyFunctionCodeQualifierList> GetCrewDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.CrewDictionary",
				() => new Dictionary<string, PartyFunctionCodeQualifierList>
				{
					{ CrewTypes.Codes.Passenger, PartyFunctionCodeQualifierList.Passenger },
					{ CrewTypes.Codes.CrewMember, PartyFunctionCodeQualifierList.CrewMember },
					{ CrewTypes.Codes.ResponsibleParty, PartyFunctionCodeQualifierList.ResponsibleParty },
				});
		}

		internal static PartyFunctionCodeQualifierList GetPartyQualifier(BusinessObjectFactory factory, string partyType)
		{
			return GetPartiesDictionary(factory).GetValue(partyType, key => PartyFunctionCodeQualifierList.GetFromString(partyType));
		}

		internal static ZString GetPartyTypeFromQualifier(BusinessObjectFactory factory, PartyFunctionCodeQualifierList qualifier)
		{
			return GetPartiesDictionary(factory).GetKey(qualifier);
		}

		internal static bool IsPartyQualifier(PartyFunctionCodeQualifierList qualifier)
		{
			var carrierCodes = new[]
			{
				PartyFunctionCodeQualifierList.GetFromString("OCG"),
				PartyFunctionCodeQualifierList.GoodsCustodian,
				PartyFunctionCodeQualifierList.ConnectingCarrier
			};
			return !carrierCodes.Contains(qualifier);
		}

		static Dictionary<string, PartyFunctionCodeQualifierList> GetPartiesDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.PartiesDictionary",
				() => new Dictionary<string, PartyFunctionCodeQualifierList>
				{
					{ PartyTypes.Codes.AirportAuthority,                                PartyFunctionCodeQualifierList.GetFromString("AEB") },
					{ PartyTypes.Codes.PortAuthority,                               PartyFunctionCodeQualifierList.GetFromString("AEE") },
					{ PartyTypes.Codes.AuthorizedOfficial,                          PartyFunctionCodeQualifierList.AuthorizedOfficial },
					{ PartyTypes.Codes.FinancialInstitution,                            PartyFunctionCodeQualifierList.FinancialInstitution },
					{ PartyTypes.Codes.BeneficialOwner,                             PartyFunctionCodeQualifierList.GetFromString("BNO") },
					{ PartyTypes.Codes.BrokerOrSalesOffice,                         PartyFunctionCodeQualifierList.BrokerOrSalesOffice },
					{ PartyTypes.Codes.BillAndShipTo,                               PartyFunctionCodeQualifierList.BillAndShipTo },
					{ PartyTypes.Codes.PartyToBeBilledForOtherThanFreightBillTo,        PartyFunctionCodeQualifierList.PartyToBeBilledForOtherThanFreightBillTo },
					{ PartyTypes.Codes.ServiceBureau,                               PartyFunctionCodeQualifierList.ServiceBureau },
					{ PartyTypes.Codes.Buyer,                                       PartyFunctionCodeQualifierList.Buyer },
					{ PartyTypes.Codes.InCareOfPartyNo1,                                PartyFunctionCodeQualifierList.InCareOfPartyNo1 },
					{ PartyTypes.Codes.InCareOfPartyNo2,                                PartyFunctionCodeQualifierList.InCareOfPartyNo2 },
					{ PartyTypes.Codes.Carrier,                                     PartyFunctionCodeQualifierList.Carrier },
					{ PartyTypes.Codes.CustomsBroker,                               PartyFunctionCodeQualifierList.CustomsBroker },
					{ PartyTypes.Codes.ConsigneeToReceiveLargeParcelsAndFreight,        PartyFunctionCodeQualifierList.GetFromString("CEL") },
					{ PartyTypes.Codes.CarnetIssuer,                                    PartyFunctionCodeQualifierList.GetFromString("CGI") },
					{ PartyTypes.Codes.ContainerLocationParty,                      PartyFunctionCodeQualifierList.ContainerLocationParty },
					{ PartyTypes.Codes.Consignee,                                   PartyFunctionCodeQualifierList.Consignee },
					{ PartyTypes.Codes.Dispatcher,                                  PartyFunctionCodeQualifierList.Dispatcher },
					{ PartyTypes.Codes.PartyToReceiveCertificateOfCompliance,       PartyFunctionCodeQualifierList.PartyToReceiveCertificateOfCompliance },
					{ PartyTypes.Codes.CopyReportTo,                                    PartyFunctionCodeQualifierList.CopyReportTo },
					{ PartyTypes.Codes.Consolidator,                                    PartyFunctionCodeQualifierList.Consolidator },
					{ PartyTypes.Codes.ContainerReturnCompany,                      PartyFunctionCodeQualifierList.ContainerReturnCompany },
					{ PartyTypes.Codes.Consignor,                                   PartyFunctionCodeQualifierList.Consignor },
					{ PartyTypes.Codes.DistributorBranch,                           PartyFunctionCodeQualifierList.DistributorBranch },
					{ PartyTypes.Codes.DestinationCarrier,                          PartyFunctionCodeQualifierList.GetFromString("DCA") },
					{ PartyTypes.Codes.Distiller,                                   PartyFunctionCodeQualifierList.GetFromString("DIS") },
					{ PartyTypes.Codes.Division,                                        PartyFunctionCodeQualifierList.GetFromString("DIV") },
					{ PartyTypes.Codes.DestinationMailFacility,                     PartyFunctionCodeQualifierList.GetFromString("DMF") },
					{ PartyTypes.Codes.DocumentRecipient,                           PartyFunctionCodeQualifierList.DocumentRecipient },
					{ PartyTypes.Codes.DeliveryParty,                               PartyFunctionCodeQualifierList.DeliveryParty },
					{ PartyTypes.Codes.Distributor,                                 PartyFunctionCodeQualifierList.Distributor },
					{ PartyTypes.Codes.DownstreamParty,                             PartyFunctionCodeQualifierList.GetFromString("DSP") },
					{ PartyTypes.Codes.DestinationTerminal,                         PartyFunctionCodeQualifierList.GetFromString("DTE") },
					{ PartyTypes.Codes.Exhibitor,                                   PartyFunctionCodeQualifierList.GetFromString("EHB") },
					{ PartyTypes.Codes.Exporter,                                        PartyFunctionCodeQualifierList.Exporter },
					{ PartyTypes.Codes.OperatorCommunicationChannel,                    PartyFunctionCodeQualifierList.OperatorCommunicationChannel },
					{ PartyTypes.Codes.FinalScheduledDestination,                   PartyFunctionCodeQualifierList.GetFromString("FSD") },
					{ PartyTypes.Codes.FreightForwarder,                                PartyFunctionCodeQualifierList.FreightForwarder },
					{ PartyTypes.Codes.RoadCarrier,                                 PartyFunctionCodeQualifierList.RoadCarrier },
					{ PartyTypes.Codes.Warehouse,                                   PartyFunctionCodeQualifierList.GetFromString("GG") },
					{ PartyTypes.Codes.PartyWhichDeliversConsignmentsToTheTerminal, PartyFunctionCodeQualifierList.PartyWhichDeliversConsignmentsToTheTerminal },
					{ PartyTypes.Codes.PartyWhichPicksUpConsignmentsFromTheTerminal,    PartyFunctionCodeQualifierList.PartyWhichPicksUpConsignmentsFromTheTerminal },
					{ PartyTypes.Codes.CorporateOffice,                             PartyFunctionCodeQualifierList.GetFromString("HDQ") },
					{ PartyTypes.Codes.ShippingLineService,                         PartyFunctionCodeQualifierList.ShippingLineService },
					{ PartyTypes.Codes.DesignatedHazardousWasteFacility,                PartyFunctionCodeQualifierList.GetFromString("HWF") },
					{ PartyTypes.Codes.TransporterOfHazardousWaste,                 PartyFunctionCodeQualifierList.GetFromString("HWT") },
					{ PartyTypes.Codes.IntermediateConsignee,                       PartyFunctionCodeQualifierList.IntermediateConsignee },
					{ PartyTypes.Codes.InternationalFreightForwarder,               PartyFunctionCodeQualifierList.GetFromString("IFF") },
					{ PartyTypes.Codes.IntermediateCarrier,                         PartyFunctionCodeQualifierList.GetFromString("IK") },
					{ PartyTypes.Codes.Importer,                                        PartyFunctionCodeQualifierList.Importer },
					{ PartyTypes.Codes.InterestedParty,                             PartyFunctionCodeQualifierList.GetFromString("IPT") },
					{ PartyTypes.Codes.Invoice,                                     PartyFunctionCodeQualifierList.Invoicee },
					{ PartyTypes.Codes.AuthorizedEntity,                                PartyFunctionCodeQualifierList.GetFromString("J3") },
					{ PartyTypes.Codes.PowerOfAttorney,                             PartyFunctionCodeQualifierList.GetFromString("J6") },
					{ PartyTypes.Codes.LocationOfLoadExchangeExport,                    PartyFunctionCodeQualifierList.GetFromString("LLE") },
					{ PartyTypes.Codes.ManufacturerOfGoods,                         PartyFunctionCodeQualifierList.ManufacturerOfGoods },
					{ PartyTypes.Codes.PlanningScheduleMaterialReleaseIssuer,       PartyFunctionCodeQualifierList.PlanningScheduleMaterialReleaseIssuer },
					{ PartyTypes.Codes.ReleaseDrayman,                              PartyFunctionCodeQualifierList.GetFromString("MO") },
					{ PartyTypes.Codes.ManufacturingUnit,                           PartyFunctionCodeQualifierList.ManufacturingUnit },
					{ PartyTypes.Codes.NotifyParty,                                 PartyFunctionCodeQualifierList.NotifyParty },
					{ PartyTypes.Codes.OrderedBy,                                   PartyFunctionCodeQualifierList.OrderedBy },
					{ PartyTypes.Codes.OwnerOfProperty,                             PartyFunctionCodeQualifierList.OwnerOfProperty },
					{ PartyTypes.Codes.ThirdParty,                                  PartyFunctionCodeQualifierList.ThirdParty },
					{ PartyTypes.Codes.OrderOfTheShipperParty,                      PartyFunctionCodeQualifierList.OrderOfTheShipperParty },
					{ PartyTypes.Codes.OperatorOfPropertyOrEquipment,               PartyFunctionCodeQualifierList.OperatorOfPropertyOrEquipment },
					{ PartyTypes.Codes.Shipper,                                     PartyFunctionCodeQualifierList.OriginalShipper },
					{ PartyTypes.Codes.OwnerOfMeansOfTransport,                     PartyFunctionCodeQualifierList.TransportMeansOwner },
					{ PartyTypes.Codes.Payee,                                       PartyFunctionCodeQualifierList.Payee },
					{ PartyTypes.Codes.PartyToReceiveFreightBill,                   PartyFunctionCodeQualifierList.PartyToReceiveFreightBill },
					{ PartyTypes.Codes.PartyToReceiveCorrespondence,                    PartyFunctionCodeQualifierList.PartyToReceiveCorrespondence },
					{ PartyTypes.Codes.PartyToReceivePaperMemoOfInvoice,                PartyFunctionCodeQualifierList.PartyToReceivePaperMemoOfInvoice },
					{ PartyTypes.Codes.PartyToReceiveShippingNotice,                    PartyFunctionCodeQualifierList.PartyToReceiveShippingNotice },
					{ PartyTypes.Codes.Payer,                                       PartyFunctionCodeQualifierList.Payer },
					{ PartyTypes.Codes.PierName,                                        PartyFunctionCodeQualifierList.GetFromString("PRN") },
					{ PartyTypes.Codes.PartyAtPickupLocation,                       PartyFunctionCodeQualifierList.GetFromString("PU") },
					{ PartyTypes.Codes.PickupAddress,                               PartyFunctionCodeQualifierList.GetFromString("PUA") },
					{ PartyTypes.Codes.ResaleDealer,                                    PartyFunctionCodeQualifierList.GetFromString("RD") },
					{ PartyTypes.Codes.DestinationIntermodalRamp,                   PartyFunctionCodeQualifierList.GetFromString("RDI") },
					{ PartyTypes.Codes.OriginalIntermodalRamp,                      PartyFunctionCodeQualifierList.GetFromString("RO") },
					{ PartyTypes.Codes.SoldToAndShipTo,                             PartyFunctionCodeQualifierList.GetFromString("SD") },
					{ PartyTypes.Codes.Seller,                                      PartyFunctionCodeQualifierList.Seller },
					{ PartyTypes.Codes.ShipFrom,                                        PartyFunctionCodeQualifierList.ShipFrom },
					{ PartyTypes.Codes.PartyToReceiveShippingManifest,              PartyFunctionCodeQualifierList.GetFromString("SM") },
					{ PartyTypes.Codes.SoldToIfDifferentThanBillTo,                 PartyFunctionCodeQualifierList.SoldToIfDifferentThanBillTo },
					{ PartyTypes.Codes.PartyFillingShippersOrder,                   PartyFunctionCodeQualifierList.GetFromString("SP") },
					{ PartyTypes.Codes.SellerAgentRepresentative,                   PartyFunctionCodeQualifierList.SellerAgent },
					{ PartyTypes.Codes.ShipTo,                                      PartyFunctionCodeQualifierList.ShipTo },
					{ PartyTypes.Codes.Supplier,                                        PartyFunctionCodeQualifierList.Supplier },
					{ PartyTypes.Codes.Surety,                                      PartyFunctionCodeQualifierList.Surety },
					{ PartyTypes.Codes.TerminalLocation,                                PartyFunctionCodeQualifierList.GetFromString("T3") },
					{ PartyTypes.Codes.TransferPoint,                               PartyFunctionCodeQualifierList.GetFromString("T4") },
					{ PartyTypes.Codes.Attorney,                                        PartyFunctionCodeQualifierList.Attorney },
					{ PartyTypes.Codes.TerminalOperator,                                PartyFunctionCodeQualifierList.TerminalOperator },
					{ PartyTypes.Codes.Terminal,                                        PartyFunctionCodeQualifierList.GetFromString("TRM") },
					{ PartyTypes.Codes.TransferTo,                                  PartyFunctionCodeQualifierList.TransferTo },
					{ PartyTypes.Codes.UltimateConsignee,                           PartyFunctionCodeQualifierList.UltimateConsignee },
					{ PartyTypes.Codes.UltimateCustomer,                                PartyFunctionCodeQualifierList.UltimateCustomer },
					{ PartyTypes.Codes.AffiliatedCompany,                           PartyFunctionCodeQualifierList.AffiliatedCompany },
					{ PartyTypes.Codes.Subsidiary,                                  PartyFunctionCodeQualifierList.Subsidiary },
					{ PartyTypes.Codes.JointOwner,                                  PartyFunctionCodeQualifierList.JointOwner },
					{ PartyTypes.Codes.JointVenture,                                    PartyFunctionCodeQualifierList.JointVenture },
					{ PartyTypes.Codes.OtherRelatedParty,                           PartyFunctionCodeQualifierList.OtherRelatedParty },
					{ PartyTypes.Codes.PartyToReceiveOrderToSupply,                 PartyFunctionCodeQualifierList.PartyToReceiveOrderToSupply },
					{ PartyTypes.Codes.DoingBusinessAs,                             PartyFunctionCodeQualifierList.DoingBusinessAs },
					{ PartyTypes.Codes.PartyToReceiveStatus,                            PartyFunctionCodeQualifierList.GetFromString("Z1") },
					{ PartyTypes.Codes.BreakBulkPoint,                              PartyFunctionCodeQualifierList.GetFromString("ZF") },
				});
		}

		#endregion

		#region Entry Type Code

		internal static string GetEntryTypeCode(BusinessObjectFactory factory, ZString shipmentType, ZString inbondType, ZBool isFDA)
		{
			switch (shipmentType)
			{
				case ShipmentTypes.Codes.LowValue:
					return isFDA ? EntryTypeCodes.LowValueEntriesWithFDA : EntryTypeCodes.LowValueEntriesWithoutFDA;
				case ShipmentTypes.Codes.Inbond:
					return GetEntryTypeCodesDictionary(factory).GetKey(inbondType);
				default:
					return GetEntryTypeCodesDictionary(factory).GetKey(shipmentType, string.Empty);
			}
		}

		internal static ZString GetEntryTypeFromCode(BusinessObjectFactory factory, string code)
		{
			return GetEntryTypeCodesDictionary(factory).GetValue(code, key => key);
		}

		static Dictionary<string, string> GetEntryTypeCodesDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.EntryTypeCodesDictionary",
				() => new Dictionary<string, string>
				{
					{ EntryTypeCodes.LowValueEntriesWithFDA,             ShipmentTypes.Codes.LowValue },
					{ EntryTypeCodes.LowValueEntriesWithoutFDA,          ShipmentTypes.Codes.LowValue },
					{ EntryTypeCodes.GoodsAstray,                   ShipmentTypes.Codes.GoodsAstray },
					{ EntryTypeCodes.UnaccompaniedArticles,         ShipmentTypes.Codes.UnaccompaniedArticles },
					{ EntryTypeCodes.FreeOfDuty,                        ShipmentTypes.Codes.FreeOfDuty },
					{ EntryTypeCodes.ReturnedGoods,                 ShipmentTypes.Codes.ReturnedGoods },
					{ EntryTypeCodes.ImmediateTransportation,       InbondTypes.Codes.ImmediateTransportation },
					{ EntryTypeCodes.ImmediateExportation,          InbondTypes.Codes.ImmediateExportation },
					{ EntryTypeCodes.TransportationAndExportation,  InbondTypes.Codes.TransportationAndExportation },
				});
		}

		#endregion

		#region Travel Document Type Code

		internal static DocumentNameCodeList GetTravelDocumentCodeFromType(BusinessObjectFactory factory, string travelDocType)
		{
			return GetTravelDocumentTypeCodeDictionary(factory).GetValue(travelDocType, key => DocumentNameCodeList.GetFromString(travelDocType));
		}

		internal static ZString GetTravelDocumentTypeFromCode(BusinessObjectFactory factory, DocumentNameCodeList travelDocCode)
		{
			return GetTravelDocumentTypeCodeDictionary(factory).GetKey(travelDocCode);
		}

		static Dictionary<string, DocumentNameCodeList> GetTravelDocumentTypeCodeDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.TravelDocumentTypeCodeDictionary",
				() => new Dictionary<string, DocumentNameCodeList>
				{
					{ TravelDocumentTypes.Codes.Passport,                   DocumentNameCodeList.Passport },
					{ TravelDocumentTypes.Codes.DrivingLicenseNational,     DocumentNameCodeList.DrivingLicenceNational },
					{ TravelDocumentTypes.Codes.CommercialDriversLicense,   DocumentNameCodeList.GetFromString("5K") },
					{ TravelDocumentTypes.Codes.EnhancedDriversLicense,     DocumentNameCodeList.GetFromString("6W") },
					{ TravelDocumentTypes.Codes.VisaImmigrant,              DocumentNameCodeList.GetFromString("989") },
					{ TravelDocumentTypes.Codes.MilitaryIdDocument,         DocumentNameCodeList.GetFromString("AAG") },
					{ TravelDocumentTypes.Codes.PermanentResidentCard2,     DocumentNameCodeList.GetFromString("ACU") },
					{ TravelDocumentTypes.Codes.USPassportCard,             DocumentNameCodeList.GetFromString("AEF") },
					{ TravelDocumentTypes.Codes.NexusCard,                  DocumentNameCodeList.GetFromString("AEW") },
					{ TravelDocumentTypes.Codes.USAlienRegistrationCard1,   DocumentNameCodeList.GetFromString("AGR") },
					{ TravelDocumentTypes.Codes.PermanentResidentCard1,     DocumentNameCodeList.GetFromString("AGS") },
					{ TravelDocumentTypes.Codes.VisaNonImmigrant,           DocumentNameCodeList.GetFromString("AGT") },
					{ TravelDocumentTypes.Codes.USAlienRegistrationCard2,   DocumentNameCodeList.GetFromString("ALR") },
					{ TravelDocumentTypes.Codes.SentriCard,                 DocumentNameCodeList.GetFromString("ALV") },
					{ TravelDocumentTypes.Codes.USMerchantMarinerDocument,  DocumentNameCodeList.GetFromString("ALX") },
					{ TravelDocumentTypes.Codes.NativeAmericanIndian,       DocumentNameCodeList.GetFromString("ALY") },
					{ TravelDocumentTypes.Codes.BirthCertificate,           DocumentNameCodeList.GetFromString("BCN") },
					{ TravelDocumentTypes.Codes.LaserVisaBorderCrossingCard,    DocumentNameCodeList.GetFromString("BCP") },
					{ TravelDocumentTypes.Codes.CitizenshipDocumentNumber,  DocumentNameCodeList.GetFromString("CDN") },
					{ TravelDocumentTypes.Codes.CertificateOfNaturalization,    DocumentNameCodeList.GetFromString("CON") },
					{ TravelDocumentTypes.Codes.OtherTravelDocument,            DocumentNameCodeList.GetFromString("OTD") },
					{ TravelDocumentTypes.Codes.DHSReEntryPermit,           DocumentNameCodeList.GetFromString("REP") },
					{ TravelDocumentTypes.Codes.DHSRefugeeTravelDocument,   DocumentNameCodeList.GetFromString("RTP") },
				});
		}

		internal static ReferenceCodeQualifierList GetTravelDocumentQualifierFromType(BusinessObjectFactory factory, string travelDocType)
		{
			return GetTravelDocumentTypeQualifierDictionary(factory).GetValue(travelDocType, key => ReferenceCodeQualifierList.GetFromString(travelDocType));
		}

		internal static ZString GetTravelDocumentTypeFromQualifier(BusinessObjectFactory factory, ReferenceCodeQualifierList qualifier)
		{
			return GetTravelDocumentTypeQualifierDictionary(factory).GetKey(qualifier);
		}

		static Dictionary<string, ReferenceCodeQualifierList> GetTravelDocumentTypeQualifierDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.TravelDocumentTypeQualifierDictionary",
				() => new Dictionary<string, ReferenceCodeQualifierList>
				{
					{ TravelDocumentTypes.Codes.Passport,                   ReferenceCodeQualifierList.PassportNumber },
					{ TravelDocumentTypes.Codes.HazmatEndorsement,          ReferenceCodeQualifierList.DangerousGoodsTransportLicenceNumber },
					{ TravelDocumentTypes.Codes.DrivingLicenseNational,     ReferenceCodeQualifierList.StateOrProvinceAssignedEntityIdentification },
					{ TravelDocumentTypes.Codes.CommercialDriversLicense,   ReferenceCodeQualifierList.MutuallyDefinedReferenceNumber },
					{ TravelDocumentTypes.Codes.EnhancedDriversLicense,     ReferenceCodeQualifierList.RelatedDocumentNumber },
					{ TravelDocumentTypes.Codes.VisaImmigrant,              ReferenceCodeQualifierList.PartyReference },
					{ TravelDocumentTypes.Codes.MilitaryIdDocument,         ReferenceCodeQualifierList.ApplicantsReference },
					{ TravelDocumentTypes.Codes.PermanentResidentCard2,     ReferenceCodeQualifierList.ExcessTransportationNumber },
					{ TravelDocumentTypes.Codes.USPassportCard,             ReferenceCodeQualifierList.PersonalIdentityCardNumber },
					{ TravelDocumentTypes.Codes.NexusCard,                  ReferenceCodeQualifierList.DocumentIdentifier },
					{ TravelDocumentTypes.Codes.USAlienRegistrationCard1,   ReferenceCodeQualifierList.ForeignResidentIdentificationNumber },
					{ TravelDocumentTypes.Codes.PermanentResidentCard1,     ReferenceCodeQualifierList.FileIdentificationNumber },
					{ TravelDocumentTypes.Codes.VisaNonImmigrant,           ReferenceCodeQualifierList.GovernmentReferenceNumber },
					{ TravelDocumentTypes.Codes.USAlienRegistrationCard2,   ReferenceCodeQualifierList.SalesForecastNumber },
					{ TravelDocumentTypes.Codes.SentriCard,                 ReferenceCodeQualifierList.RegisteredCapitalReference },
					{ TravelDocumentTypes.Codes.USMerchantMarinerDocument,  ReferenceCodeQualifierList.PersonRegistrationNumber },
					{ TravelDocumentTypes.Codes.NativeAmericanIndian,       ReferenceCodeQualifierList.SituationNumber },
					{ TravelDocumentTypes.Codes.LaserVisaBorderCrossingCard,    ReferenceCodeQualifierList.CustomerReferenceNumber },
					{ TravelDocumentTypes.Codes.CitizenshipDocumentNumber,  ReferenceCodeQualifierList.GetFromString("CDN") },
					{ TravelDocumentTypes.Codes.CertificateOfNaturalization,    ReferenceCodeQualifierList.GetFromString("CON") },
					{ TravelDocumentTypes.Codes.OtherTravelDocument,            ReferenceCodeQualifierList.GetFromString("OTD") },
					{ TravelDocumentTypes.Codes.DHSReEntryPermit,           ReferenceCodeQualifierList.GetFromString("REP") },
					{ TravelDocumentTypes.Codes.DHSRefugeeTravelDocument,   ReferenceCodeQualifierList.GetFromString("RTP") },
					{ TravelDocumentTypes.Codes.BirthCertificate,           ReferenceCodeQualifierList.GetFromString("BCN") },
				});
		}

		#endregion

		#region GetMethodOfTransportation

		internal static string GetMethodOfTransportationQualifier(BusinessObjectFactory factory, string transportMode)
		{
			return GetMethodOfTransportationDictionary(factory).GetValue(transportMode, key => key);
		}

		internal static ZString GetMethodOfTransportationFromQualifier(BusinessObjectFactory factory, string qualifier)
		{
			return GetMethodOfTransportationDictionary(factory).GetKey(qualifier);
		}

		static Dictionary<string, string> GetMethodOfTransportationDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(
				"US.eManifest.MethodOfTransportationDictionary",
				() => new Dictionary<string, string> { { TransportModes.Codes.Road, TransportTypes.Road } });
		}

		#endregion

		#endregion

		#region Constants

		#region Identification Codes

		internal class IdentificationCodes
		{
			internal const string ACEId = "109";
			internal const string ACEProximityCardId = "8";
			internal const string ConveyanceId = "172";
			internal const string Country = "162";
			internal const string CountrySubEntity = "163";
			internal const string RegionGeographicLocation = "229";
			internal const string StateOfDriverLicense = "84";
			internal const string CustomsAssignedNumber = "160";
			internal const string DOTNumber = "274";
			internal const string DUNS = "46";
			internal const string EquipmentNumber = "172";
			internal const string FAST = "274";
			internal const string FilerCode = "275";
			internal const string FIRMS = "276";
			internal const string FreeFormText = "ZZZ";
			internal const string IATA = "145";
			internal const string InlandScheduleK = "277";
			internal const string LicensePlate = "215";
			internal const string SCAC = "172";
			internal const string ScheduleD = "77";
			internal const string ScheduleK = "78";
			internal const string SSNOrEIN = "167";
			internal const string TransponderId = "8";
			internal const string VIN = "146";
			internal const string C4Code = "117";
			internal const string HarmonizedTariffCode = "122";
		}

		#endregion

		#region Entry Type Codes

		internal static class EntryTypeCodes
		{
			internal const string LowValueEntriesWithoutFDA = "13";
			internal const string LowValueEntriesWithFDA = "35";
			internal const string GoodsAstray = "18";
			internal const string PAPS = "34";
			internal const string ImmediateTransportation = "61";
			internal const string TransportationAndExportation = "62";
			internal const string ImmediateExportation = "63";
			internal const string FreeOfDuty = "83";
			internal const string ReturnedGoods = "84";
			internal const string UnaccompaniedArticles = "85";
			internal const string MexicanPedimento = "MO";
			internal const string ShipmentIdentifier = "SI";
		}

		#endregion

		#region TransportModes

		internal static class TransportTypes
		{
			internal const string Road = "03";
		}

		#endregion

		#endregion

		#region Extensions

		internal static ZString ToStringDelimited(this IEnumerable<ZString> strings, string delimiter)
		{
			return new ZStringBuilder(strings).ToStringWithDelimiterBetweenAppends(delimiter);
		}

		internal static ZString ToStringDelimited(this IEnumerable<string> strings, string delimiter)
		{
			return new ZStringBuilder(strings).ToStringWithDelimiterBetweenAppends(delimiter);
		}

		internal static string GetKey<T>(this Dictionary<string, T> dictionary, T value, string fallback = null) where T : class
		{
			return value == null ? string.Empty : (from pair in dictionary where pair.Value.ToString() == value.ToString() select pair.Key).FirstOrDefault() ?? fallback ?? value.ToString();
		}

		internal static T GetValue<T>(this Dictionary<string, T> dictionary, string key, Func<string, T> fallback)
		{
			T result;
			if (string.IsNullOrEmpty(key) || !dictionary.TryGetValue(key, out result))
			{
				result = fallback(key);
			}

			return result;
		}

		#endregion
	}
}
