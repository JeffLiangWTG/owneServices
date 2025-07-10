namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	#region SuppressResourceStringsCheckRegion

	static class DocDataConstants
	{
		public static class RefCusCodeListType
		{
			public const string Code_ECICS = "ECICS";
		}

		public static class Notes
		{
			public const string FreightPayableAt = "Freight Payable At";
			public const string PaymentInstructionRemark = "Payment Instruction Remark";
			public const string ChargesFreighted = "ChargesFreighted";
			public const string CertificateOfOriginNotes = "Certificate of Origin Notes";
		}

		public static class AdditionalReferences
		{
			public static class Codes
			{
				public const string BillOfLading = "BOL";
				public const string ShipperReference = "SHP";
				public const string FreightForwarderReference = "FFW";
				public const string CarrierContractNumber = "CON";
				public const string CarrierQuoteNumber = "CQN";
				public const string ContractNamedAccount = "NAC";
				public const string QuotationNumber = "QUO";
				public const string CarrierBookingPrefix = "SLD";
				public const string CarrierBookingReference = "BKG";
				public const string LetterOfCreditNumber = "LCR";
				public const string ShippingOrderNumber = "SLD";
				public const string SendingPartyAPPlusCode = "APP";
				public const string CTOAPPlusCode = "CTO";
				public const string DOSReferenceNumber = "DOS";
				public const string DeclarationType = "DCT";
				public const string DeclarantsSIRETNumber = "SIR";
				public const string CargoControlAndTransitRUCNumber = "RUC";
				public const string AcidNumber = "ACI";
				public const string EHubInterchangeReference = "HIR";
				public const string Tariff = "TAR";
				public const string EHubInterchangeID = "HID";
			}

			public static class Descriptions
			{
				public const string BillOfLading = "Bill Of Lading Number";
				public const string ShipperReference = "Shipper Reference";
				public const string FreightForwarderReference = "Freight Forwarder Reference";
				public const string CarrierContractNumber = "Carrier Contract Number";
				public const string CarrierQuoteNumber = "Carrier Quote Number";
				public const string ContractNamedAccount = "Contract Named Account";
				public const string QuotationNumber = "Quotation Number";
				public const string CarrierBookingPrefix = "Shipping Order/Shi Lian Dan";
				public const string CarrierBookingReference = "Carrier Booking Reference";
				public const string LetterOfCreditNumber = "Letter Of Credit Number";
				public const string ShippingOrderNumber = "Shipping Order/Shi Lian Dan";
				public const string SendingPartyAPPlusCode = "Sending Party APPlus Code";
				public const string CTOAPPlusCode = "CTO APPlus Code";
				public const string DOSReferenceNumber = "DOS Reference Number";
				public const string DeclarationType = "Declaration Type";
				public const string DeclarantsSIRETNumber = "Declarants SIRET Number";
				public const string CargoControlAndTransitRUCNumber = "Referência Única da Carga";
				public const string AcidNumber = "ACID Number";
				public const string EHubInterchangeReference = "eHub Interchange Reference";
				public const string Tariff = "Tariff";
			}
		}

		public static class Charges
		{
			public static class Categories
			{
				public static class Codes
				{
					public const string Freight = "FRT";
					public const string DestinationHaulage = "DHC";
					public const string DestinationPort = "DPC";
					public const string OriginHaulage = "OHC";
					public const string OriginPort = "OPC";
				}

				public static class Descriptions
				{
					public const string Freight = "Freight";
					public const string FreightCharges = "Freight Charges";
					public const string DestinationHaulage = "Destination Haulage";
					public const string DestinationPort = "Destination Port";
					public const string DestinationPortCharge = "Destination Port Charge";
					public const string OriginHaulage = "Origin Haulage";
					public const string OriginPort = "Origin Port";
					public const string OriginPortCharge = "Origin Port Charge";
				}
			}

			public static class Codes
			{
				public const string Prepaid = "PPD";
				public const string Collect = "CCX";
				public const string Free = "FRE";
				public const string PayableElsewhere = "ELS";
				public const string FirstLinePrepaidSecondLineCollect = "1PC";
			}

			public static class Descriptions
			{
				public const string Prepaid = "Prepaid";
				public const string Collect = "Collect";
				public const string Free = "Free";
				public const string PayableElsewhere = "Payable Elsewhere";
				public const string FirstLinePrepaidSecondLineCollect = "1st Line Prepaid 2nd Line Collect";
			}
		}

		public static class BillOfLadingTypes
		{
			public static class Codes
			{
				public const string Prepaid = "FPP";
				public const string Collect = "FCL";
				public const string AsAgreed = "FAA";
				public const string ReceivedForShipment = "RFS";
				public const string LadenOnBoard = "LOB";
				public const string OnBoardRail = "OBR";
				public const string LadenOnBoardVessel = "LBV";
				public const string OnBoardVessel = "OBV";
				public const string LadenOnBoardNamedVessel = "LNV";
				public const string ShipperLoadAndCount = "SLC";
				public const string ShipperLoadStowageAndCount = "LSC";
				public const string NoShipperExportDeclarationRequired = "NSD";
			}

			public static class Descriptions
			{
				public const string Prepaid = "Freight Prepaid";
				public const string Collect = "Freight Collect";
				public const string AsAgreed = "Freight As Agreed";
				public const string ReceivedForShipment = "Received for Shipment";
				public const string LadenOnBoard = "Laden on Board";
				public const string OnBoardRail = "On Board Rail";
				public const string LadenOnBoardVessel = "Laden on Board Vessel";
				public const string OnBoardVessel = "On Board Vessel";
				public const string LadenOnBoardNamedVessel = "Laden on Board Named Vessel";
				public const string ShipperLoadAndCount = "Shipper's Load and Count";
				public const string ShipperLoadStowageAndCount = "Shipper's Load, Stowage and Count";
				public const string NoShipperExportDeclarationRequired = "No Shipper's Export Declaration Required";
			}
		}

		public static class ActionPurpuses
		{
			public static class Codes
			{
				public const string AsPerPayload = "APP";
				public const string Amendment = "AMD";
			}

			public static class Descriptions
			{
				public const string AsPerPayload = "As Per Payload";
				public const string Amendment = "Amendment";
			}
		}

		public static class DataSources
		{
			public const string Booking = "Booking";
			public const string ForwardingConsol = "ForwardingConsol";
			public const string ForwardingShipment = "ForwardingShipment";
		}

		public static class ChargeCategoryCodes
		{
			public const string Freight = "FRT";
		}

		public static class ChargeCategoryDescriptions
		{
			public const string Freight = "Freight";
		}

		public static class ChargeCodes
		{
			public const string Prepaid = "PPD";
			public const string Collect = "CCX";
			public const string Free = "FRE";
			public const string PayableElsewhere = "ELS";
			public const string FirstLinePrepaidSecondLineCollect = "1PC";
		}

		public static class ChargeDescriptions
		{
			public const string Prepaid = "Prepaid";
			public const string Collect = "Collect";
			public const string Free = "Free";
			public const string PayableElsewhere = "Payable Elsewhere";
			public const string FirstLinePrepaidSecondLineCollect = "1st Line Prepaid 2nd Line Collect";
		}

		public static class BillOfLadingTypeCodes
		{
			public const string Prepaid = "FPP";
			public const string Collect = "FCL";
		}

		public static class BillOfLadingTypeDescriptions
		{
			public const string Prepaid = "Freight Prepaid";
			public const string Collect = "Freight Collect";
		}

		public static class CarrierSCACCodes
		{
			public const string One = "ONEY";
		}

		public static class WoodenPackageProcessTypes
		{
			public const string NotApplicable = "Not Applicable";
			public const string TreatedAndCertified = "Treated and Certified";
			public const string NotTreatedAndNotCertified = "Not-Treated and Not-Certified";
			public const string Processed = "Processed";
		}

		public static class NoteTypes
		{
			public const string USCanadaManifestSelfFilerID = "USCanadaManifestSelfFilerID";
			public const string WoodenPackageProcessType = "WoodenPackageProcessType";
			public const string OtherBillClauses = "OtherBillClauses";
			public const string ChargesFreighted = "ChargesFreighted";
		}

		public static class AddinfoTypes
		{
			public const string CarrierBookingOffice = "CarrierBookingOffice";
			public const string CommercialTransportOffer = "OTP";
			public const string PhysicalTransportAdvice = "ATP";
			public const string FormVersion = "FormVersion";
			public const string PortServiceCode = "PortServiceCode";
			public const string PortServiceReference = "PortServiceReference";
			public const string IsCoLoad = "IsCoLoad";
			public const string ProductCode = "ProductCode";
			public const string CommodityCode = "CommodityCode";
			public const string ALPOUserID = "AlpoUserId";
			public const string ALPOReference = "AlpoReference";
			public const string OperationalPort = "OperationalPort";
			public const string FreightPayableAt = "FreightPayableAt";
			public const string CarrierBookingReference = "CarrierBookingReference";
			public const string GroupingMethod = "GroupingMethod";
			public const string OriginCriterion = nameof(OriginCriterion);
			public const string InvoiceNumber = nameof(InvoiceNumber);
			public const string InvoiceDate = nameof(InvoiceDate);
			public const string InvoiceAmount = nameof(InvoiceAmount);
			public const string InvoiceCurrency = nameof(InvoiceCurrency);
			public const string CertificateOfOriginId = nameof(CertificateOfOriginId);
			public const string DateOfIssue = nameof(DateOfIssue);
			public const string IssuingBody = nameof(IssuingBody);
			public const string Fta = nameof(Fta);
			public const string SignatureUsed = nameof(SignatureUsed);
			public const string DateSigned = nameof(DateSigned);
			public const string ExporterClientCode = nameof(ExporterClientCode);
			public const string SelfDeclaration = nameof(SelfDeclaration);
			public const string BackToBackCertificateOfOrigin = nameof(BackToBackCertificateOfOrigin);
			public const string SubjectToThirdPartyInvoice = nameof(SubjectToThirdPartyInvoice);
			public const string IssuedRetroactively = nameof(IssuedRetroactively);
			public const string DeMinimis = nameof(DeMinimis);
			public const string Accumulation = nameof(Accumulation);
			public const string PreferentialTreatmentGiven = nameof(PreferentialTreatmentGiven);
			public const string FOBCurrency = nameof(FOBCurrency);
			public const string FOBAmount = nameof(FOBAmount);
			public const string VerboseLogging = nameof(VerboseLogging);
			public const string SubmitToCustomsAuthority = nameof(SubmitToCustomsAuthority);
			public const string ThirdPartyInvoiceIssuer = nameof(ThirdPartyInvoiceIssuer);
			public const string Exhibition = nameof(Exhibition);
			public const string ExhibitionDetail = nameof(ExhibitionDetail);
			public const string ExporterReference = nameof(ExporterReference);
			public const string MarksAndNumbers = nameof(MarksAndNumbers);
			public const string DeclarationCompletedBy = nameof(DeclarationCompletedBy);
			public const string ExportDocumentNumber = nameof(ExportDocumentNumber);
		}

		public static class TaxStatus
		{
			public const string Prepaid = "PPD";
			public const string Collect = "CLT";
		}

		public static class LPDStatus
		{
			public static class Codes
			{
				public const string Provisional = "PRO";
				public const string Finalized = "VAL";
				public const string FinalizedNoPacksWeight = "VAO";
				public const string FinalizedNoPacksOnly = "VAC";
			}

			public static class Descriptions
			{
				public const string Provisional = "Provisional";
				public const string Finalized = "Finalized";
				public const string FinalizedNoPacksWeight = "Finalized (no. packs + weight)";
				public const string FinalizedNoPacksOnly = "Finalized (no. packs only)";
			}
		}

		public static class ShippingLineMessagingRequirement
		{
			public static class Types
			{
				public const string ContractNumberMandatory = Core.Constants.ShippingLineMessagingRequirement.Code.ContractNumberMandatory;
				public const string NamedAccountMandatory = Core.Constants.ShippingLineMessagingRequirement.Code.NamedAccountMandatory;
				public const string DGNetWeightMandatory = Core.Constants.ShippingLineMessagingRequirement.Code.DGNetWeightMandatory;
				public const string AcceptEitherAirflowOrHumidity = Core.Constants.ShippingLineMessagingRequirement.Code.AcceptEitherAirflowOrHumidity;
				public const string DimensionsMandatoryForOOG = Core.Constants.ShippingLineMessagingRequirement.Code.DimensionsMandatoryForOOG;
				public const string HarmonisedCode = Core.Constants.ShippingLineMessagingRequirement.Code.HarmonisedCode;
				public const string BillOfLadingProvider = Core.Constants.ShippingLineMessagingRequirement.Code.BillOfLadingProvider;
				public const string AttachFormAsPDFInMessage = Core.Constants.ShippingLineMessagingRequirement.Code.AttachFormAsPDFInMessage;
				public const string SealNumberMandatory = Core.Constants.ShippingLineMessagingRequirement.Code.SealNumberMandatory;
				public const string IntegrationViaEmailToCarrierLocalOffice = Core.Constants.ShippingLineMessagingRequirement.Code.IntegrationViaEmailToCarrierLocalOffice;
			}

			public static class ValidationMessages
			{
				public const string ContractNumberMandatory = "This carrier requires Carrier Contract Number or Carrier Quote Number.";
				public const string NamedAccountMandatory = "This carrier requires Contract Named Account.";
				public const string DGNetWeightMandatory = "This carrier requires Net Weight for each Dangerous Goods.";
				public const string AcceptEitherAirflowOrHumidity = "This carrier requires temperature-controlled container to include either Airflow or Humidity, not both.";
				public const string DimensionsMandatoryForOOG = "This carrier requires Dimensions for out of gauge cargo for Open Top or Flat Rack containers.";
				public const string DimensionsMandatoryForOOGPackline = "This carrier requires Dimensions for out of gauge cargo.\r\nPlease enter dimensions against Shipment > Packline.";
				public const string DimensionsMandatoryForOOGContainer = "The carrier requires container Overhang details for Out Of Gauge cargo,\r\nplease enter Overhang details in Consol > Containers > Measures tab.";
				public const string SealNumberMandatoryForExists = "Seal Number is mandatory for this carrier.";
				public const string SealNumberMandatoryForLength = "Seal number cannot exceed 15 characters as per carrier requirement.";
				public const string CarrierLinkShippingLine = "This Carrier Organisation is not linked to a Shipping Line record. Please link this Carrier to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.";
				public const string RecipientLinkShippingLine = "This Co-Load With Organisation is not linked to a Shipping Line record. Please link this Co-Loader to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.";
			}

			public static class ContainerTypes
			{
				public const char OpenTop = 'U';
				public const char FlatRack = 'P';
			}

			public static class AddInfoKey
			{
				public const string BillOfLadingProvider = "eBLDocumentationProvider";
			}
		}

		public static class Department
		{
			public const string FIATA = "FIATA";
		}

		public static class CertificationParameterNames
		{
			public const string CertificateOfOrigin = "Certificate Of Origin";
			public const string Certify = "Certify";
			public const string Originals = "Originals";
			public const string Copies = "Copies";
			public const string Legalized = "Legalized";
			public const string DocumentType = "DocumentType";
			public const string IndemnityTermsAccepted = "IndemnityTermsAccepted";
			public const string IndemnityTermsVersion = "IndemnityTermsVersion";
		}

		public static class CertificateAddressOptions
		{
			public const string Unknown = "UNKNOWN";
		}

		public static class ICS2DeclarantEORI
		{
			public static class AddInfoKey
			{
				public const string ICS2DeclarantEORI = "ICS2DeclarantEORI";
				public const string ICS2FilingType = "ICS2FilingType";
			}
			public static class AddInfoValue
			{
				public const string Declarant = "Declarant";
				public const string Carrier = "Carrier";
			}
		}

		public static class DocOrgCusCodeDirection
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Both = "BTH";
		}

		public static class ILCargoIdentifierType
		{
			public const string SeaDealImport = "11";
			public const string LandDealImport = "20";
			public const string AirBillOfLoadingImport = "1";
			public const string MasterBillPartSeparator = "-";
		}

		public static class ILCargoTypes
		{
			public const string Containerized = "1";
			public const string Bulk = "3";
			public const string Liquid = "4";
		}

		public static class ILProcessType
		{
			public const string Import = "1";
			public const string Export = "3";
		}

		public static class XmlNamespaces
		{
			public const string EHBL = "/eHBL/1";
			public const string BLData = "/BLData/1";
		}

		public static class ILMessageEventParameter
		{
			public const string Department = "Customs";
			public const string DeliveryOrderDocumentName = "Delivery Order";
			public const string GatePassMovementDocumentName = "Gatepass Movement";
		}
	}

	#endregion
}
