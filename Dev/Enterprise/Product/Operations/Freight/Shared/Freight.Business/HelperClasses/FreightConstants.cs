using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for FreightConstants.
	/// </summary>
	public static class FreightConstants
	{
		public const string InnerPackType = "STD";
		public const string OuterPackType = "OUT";
		public const string DeliveryPackType = "DLV";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public const string AllContainers = "ALL CONTAINERS";
		public const int MinAvailableNeutrals = 10;

		public static string CargoWiseOnePortTransportXMLFile
		{
			get
			{
				return BrandingFactory.Instance.ProductName + (NoResString)" Port Transport XML File";
			}
		}

		public static string LocalCartageXmlEmailSubject
		{
			get { return Res.GetString("f1d8d366-90fe-4248-accf-bb4e4f7dfe26", "{0} Port Transport XML File", Core.Constants.ProductName); }
		}
		public static string LocalCartageNote
		{
			get { return Res.GetString("e416cfd3-331c-4801-9da5-1eb3cb9ceefa", "Port Transport Interface Notes"); }
		}

		public const string CartageJobExport = "CAR";
		public const string CartageStatusExport = "STA";

		public const string HarmonisedCodeRegexPattern = @"^[0-9]+[0-9\.]*$";

		public static class VICTPort
		{
			public const string AUMEL = "AUMEL";
		}

		public static class Classification
		{
			public static class Codes
			{
				public const string HarmonizedCode = "HSC";
				public const string ECICS = "CUS";
			}

			public static class Description
			{
				public static string HarmonizedCode
				{
					get { return Res.GetString("1f16c7b7-e294-460c-beae-4a976fd6ce3b", "Harmonized Code"); }
				}

				public static string ECICS
				{
					get { return Res.GetString("f0cf0868-5e94-42ec-a066-5eb705dbe45c", "ECICS"); }
				}
			}
		}

		public static class CoLoadStatus
		{
			public static string All
			{
				get { return Res.GetString("8eeea12f-9fcb-49dc-bcd0-270c385b6e62", "All"); }
			}
			public static string CoLoad
			{
				get { return Res.GetString("92df1c91-1ad6-4f1d-9d84-69bca9f517ad", "Co-Load"); }
			}
			public static string CoLoadMaster
			{
				get { return Res.GetString("119b864a-fc07-408c-a791-4339401cabc1", "Co-Load Master"); }
			}
			public static string CoLoadAndCoLoadMaster
			{
				get { return Res.GetString("c941a5d2-0b8c-4d74-b3b2-329dc65fa8f6", "Co-Load & Co-Load Master"); }
			}
			public static string NeitherCoLoadNorCoLoadMaster
			{
				get { return Res.GetString("e439fb56-1c91-46fb-9b09-dec7cd9d8737", "Neither Co-Load nor Co-Load Master"); }
			}
		}

		public static class LocalCartageBookingStatus
		{
			public static class Codes
			{
				public const string PreBookingAdvice = "PBA";
				public const string FirmBookingRequest = "FBR";
				public const string BookingModificationRequest = "BMR";
				public const string BookingCancellationRequest = "BCR";

				public const string BookingAccepted = "ACC";
				public const string BookingRejected = "REJ";

				public const string WorkCommenced = "WKC";
				public const string GoodsReceivedOnBoard = "GOB";
				public const string GoodsUnloadedDelivered = "GOD";
				public const string WorkCompleted = "WKD";
				public const string DemurrageEventAtWharf = "DAW";
				public const string DemurrageEventAtCFSDepot = "DAC";
				public const string DemurrageEventAtPickupPoint = "DAP";
				public const string SlotDateBooked = "SDB";
			}

			public static class Description
			{
				public static string PreBookingAdvice
				{
					get { return Res.GetString("f5744410-8eaf-4852-b713-5ca639ba7661", "Pre Booking Advice"); }
				}
				public static string FirmBookingRequest
				{
					get { return Res.GetString("79290868-85ac-405a-bb2a-42f550d72ce2", "Firm Booking Request"); }
				}
				public static string BookingModificationRequest
				{
					get { return Res.GetString("75396d13-8911-4959-92ab-569ffce9cc4d", "Booking Modification Request"); }
				}
				public static string BookingCancellationRequest
				{
					get { return Res.GetString("6de4a07b-9b23-4de4-8544-c0618d27037e", "Booking Cancellation Request"); }
				}

				public static string BookingAccepted
				{
					get { return Res.GetString("f570980d-b998-4cb0-9098-7879e72f0998", "Booking Request Accepted"); }
				}
				public static string BookingRejected
				{
					get { return Res.GetString("532980d9-a586-46fd-b13a-6b0c0b758a95", "Booking Request Rejected"); }
				}

				public static string WorkCommenced
				{
					get { return Res.GetString("cb3e54f6-c162-4803-9b50-d1dda49681bf", "Work commenced"); }
				}
				public static string GoodsReceivedOnBoard
				{
					get { return Res.GetString("0616d68f-a532-44fb-84ba-35c9a648f2b9", "Goods received on board"); }
				}
				public static string GoodsUnloadedDelivered
				{
					get { return Res.GetString("65683836-694c-4ed5-8505-493340fb9a8d", "Goods unloaded/delivered"); }
				}
				public static string WorkCompleted
				{
					get { return Res.GetString("397f7606-083b-45a8-88e9-5d088877989d", "Work Completed/Container De-hired"); }
				}
				public static string DemurrageEventAtWharf
				{
					get { return Res.GetString("40c48e78-83b9-4725-9fa6-a5e6c5bf6f7b", "Demurrage Event at Wharf"); }
				}
				public static string DemurrageEventAtCFSDepot
				{
					get { return Res.GetString("eedc0c7e-a9d1-45e0-8821-cb2bb99346c7", "Demurrage Event at CFS/depot"); }
				}
				public static string DemurrageEventAtPickupPoint
				{
					get { return Res.GetString("33f6f3ca-6f54-43b9-bc94-3e68048e9b99", "Demurrage Event at Pickup point"); }
				}
				public static string SlotDateBooked
				{
					get { return Res.GetString("00686ec7-f0da-4eb9-bd47-3335f71cd41b", "Slot Date Booked"); }
				}
			}
		}

		public static class ShippedOnBoardType
		{
			public const string Shipped = "SHP";
			public const string Clean = "CLN";
			public const string Laden = "LDN";
			public const string Received = "RFS";
		}

		public static class PrintOptionForCoLoads
		{
			public const string All = "ALL";
			public const string MastersOnly = "MAS";
			public const string SubHouseBillsOnly = "SUB";
		}

		public static class VesselDataProviders
		{
			public const string DBH = "DBH";
			public const string DAKOSY = "DAK";
			public const string OneStop = "1ST";
		}

		public static class VesselDataProviderNames
		{
			public static MultilingualString DBH { get { return ResString.GetMultilingualString("0FE07EB5-1705-4CEE-9410-9B8362E1ABD3", "DBH"); } }
			public static MultilingualString DAKOSY { get { return ResString.GetMultilingualString("33EA059C-573D-410C-954F-89B85540001E", "DAKOSY"); } }
			public static MultilingualString OneStop { get { return ResString.GetMultilingualString("769D99EF-C065-4D6C-A268-7ADF26D4A9EA", "1-STOP"); } }
		}

		public static class ASMShipmentMessages
		{
			public static string AssemblyMasterAsDirectMaster
			{
				get { return Res.GetString("8450c24c-3eb9-4f0b-a992-bb35e7f94278", "Assembly Master as Direct Master may breach customs law"); }
			}
		}

		#region Query Decider Filter List Codes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class NumberFilterTypes
		{
			public const string MostCommon = "Common";

			public const string ShipmentNumber = "Shipment #";
			public const string MasterBill = "Master Bill";
			public const string HouseBill = "House Bill";
			public const string ConsolNo = "Consol #";
			public const string ContainerNo = "Container #";
			public const string OrderNumber = "Order #";
			public const string InvoiceNumber = "Com. Invoice #";
			public const string CustomsEntryNumber = "Customs Entry #";
			public const string BookingRef = "Booking Ref #";
			public const string ShippingRef = "Shippers Ref #";
			public const string InterimReceiptNumber = "Interim Receipt #";
			public const string PackLineRef = "Pack Line Ref #";
			public const string CoLoadMasterNumber = "CoLoad Master #";
			public const string DocumentNumber = "Document #";
			public const string DirectMAWB = "DirectMAWB #";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class StatusFilterTypes
		{
			public const string None = "None";

			public const string ShipmentStatus = "Shipment";
			public const string InvoiceStatus = "Invoice";
			public const string CustomsStatus = "Customs";
			public const string CustomsMessageStatus = "Customs Message";
			public const string CustomsEntryStatus = "Customs Entry";
			public const string AirCargoStatus = "Air Cargo Message";
			public const string SeaCargoStatus = "Sea Cargo Message";
			public const string SeaCargoCMRStatus = "Sea Cargo Customs";
			public const string AirCargoCMRStatus = "Air Cargo Customs";
			public const string AirCargoCMRMessageStatus = "Air Cargo Message";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class OrgFilterTypes
		{
			public const string None = "None";
			public const string All = "All";

			public const string ConsignorConsignee = "Consignor / Consignee";
			public const string SendingRecvAgent = "Send. / Recv. Agent";
			public const string ConsolSendingRecvAgent = "Consol Send./Recv. Agent";
			public const string ShipmentSendingRecvAgent = "Shipment Send./Recv. Agent";
			public const string ExpCustoms = "Export Broker / Port Transport";
			public const string ImpCustoms = "Import Broker / Port Transport";
			public const string DeliveryAgent = "Delivery Agent";
			public const string TranshipAgent = "Tranship Agent";
			public const string Carrier = "Carrier";
			public const string DocumentOwner = "Doc Owner";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class PortFilterTypes
		{
			public const string None = "None";
			public const string All = "All";

			public const string LoadDischargeCode = "Load / Discharge";
			public const string EndPortsCode = "End Ports";
			public const string OriginDestinationCode = "Origin / Dest.";

			public const string LoadDischargeDesc = "Load / Discharge";
			public const string EndPortsDesc = "First Load / Last Discharge";
			public const string OriginDestinationDesc = "Shipment Origin / Dest.";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class CharterFilter
		{
			public const string All = "ALL";

			public const string CharterOnlyCode = "CHA";
			public const string NonCharterOnlyCode = "NCH";

			public const string CharterOnlyDesc = "Charter";
			public const string NonCharterOnlyDesc = "Non Charter";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class AddressFilterTypes
		{
			public const string CompanyName = "Company Name";
			public const string ConsigneeName = "Consignee Co.Name";
			public const string ConsignorName = "Consignor Co.Name";
		}

		#endregion

		#region Freight Spot Rate Types

		public enum SpotRateType
		{
			ShipmentSellRate,
			ShipmentCostRate,
			ShipmentGatewaySellRate,
			ContainerSellRate,
			ContainerCostRate,
			ContainerGatewaySellRate
		}

		#endregion

		public static class ServiceTask
		{
			public const string CargoIMPMessageSender = "CIS";
			public const string CargoIMPPhase2 = "CI2";
		}

		public static class PacklineOriginTransitWarehouseStatus
		{
			public static class Codes
			{
				public const string Unknown = "UNK";
				public const string Discrepencies = "DIS";
				public const string ShortShipped = "SHR";
				public const string Surplus = "SUR";
				public const string Confirmed = "CNF";
			}

			public static class Descriptions
			{
				public static MultilingualString Unknown { get { return ResString.GetMultilingualString("PacklineOriginTransitWarehouseStatus|Unknown", "Unknown"); } }
				public static MultilingualString Discrepencies { get { return ResString.GetMultilingualString("PacklineOriginTransitWarehouseStatus|Discrepencies", "Received with Discrepancies"); } }
				public static MultilingualString ShortShipped { get { return ResString.GetMultilingualString("PacklineOriginTransitWarehouseStatus|ShortShipped", "Short Shipped"); } }
				public static MultilingualString Surplus { get { return ResString.GetMultilingualString("PacklineOriginTransitWarehouseStatus|Surplus", "Surplus"); } }
				public static MultilingualString Confirmed { get { return ResString.GetMultilingualString("PacklineOriginTransitWarehouseStatus|Confirmed", "Confirmed"); } }
			}
		}

		public static class PacklineLastKnownTransitWarehouseStatus
		{
			public static class Codes
			{
				public const string Received = "RCV";
				public const string Dispatched = "DSP";
			}

			public static class Descriptions
			{
				public static MultilingualString Received { get { return ResString.GetMultilingualString("PacklineLastKnownTransitWarehouseStatus|Received", "Received"); } }
				public static MultilingualString Dispatched { get { return ResString.GetMultilingualString("PacklineLastKnownTransitWarehouseStatus|Dispatched", "Dispatched"); } }
			}
		}

		public static class PackLineHouseBillPaymentType
		{
			public static class Codes
			{
				public const string PaymentInCash = "A";
				public const string PaymentByCreditCard = "B";
				public const string PaymentByCheque = "C";
				public const string Other = "D";
				public const string ElectronicFundsTransfer = "H";
				public const string AccountHolderWithCarrier = "Y";
				public const string NotPrePaid = "Z";
			}

			public static class Descriptions
			{
				public static MultilingualString PaymentInCash { get { return ResString.GetMultilingualString("918b35fd-c02f-4486-897c-bf7cee2f9c39", "Payment in cash"); } }

				public static MultilingualString PaymentByCreditCard { get { return ResString.GetMultilingualString("885c9114-2984-4200-be74-0ff3211df0ca", "Payment by credit card"); } }

				public static MultilingualString PaymentByCheque { get { return ResString.GetMultilingualString("f5874e91-32e4-47a1-a430-81298faeb336", "Payment by cheque"); } }

				public static MultilingualString Other { get { return ResString.GetMultilingualString("864806c1-d85c-495e-87d9-86d3e9d3d6b3", "Other"); } }

				public static MultilingualString ElectronicFundsTransfer { get { return ResString.GetMultilingualString("7b05380b-ea54-45fd-94ef-aa80c2acd836", "Electronic funds transfer"); } }

				public static MultilingualString AccountHolderWithCarrier { get { return ResString.GetMultilingualString("3b5c0e11-656d-42a8-ae76-a3c330f2cda4", "Account holder with carrier"); } }

				public static MultilingualString NotPrePaid { get { return ResString.GetMultilingualString("9670b324-8341-4a68-b618-0d2dbad2b1b1", "Not pre-paid"); } }
			}
		}

		public static class CarrierBookingStatus
		{
			public static class Codes
			{
				public const string NotSent = "NSN";

				public static class BookingRequest
				{
					public const string Sent = "BSN";
					public const string Acknowledged = "BAC";
					public const string RejectedByInterchange = "BIR";
					public const string Confirmed = "BCF";
					public const string Rejected = "BRJ";
					public const string WithdrawalSent = "BWS";
					public const string WithdrawalRejected = "BWR";
					public const string WithdrawalAccepted = "BWA";
					public const string PendingProcessing = "BPP";
				}

				public static class ShippingInstruction
				{
					public const string Sent = "SSN";
					public const string Acknowledged = "SAC";
					public const string RejectedByInterchange = "SIR";
					public const string Confirmed = "SCF";
					public const string Rejected = "SRJ";
					public const string PendingProcessing = "SPP";
				}
			}

			public static class Description
			{
				public static string NotSent { get { return Res.GetString("55ADCAA1-EC3D-4114-B1C5-7CD211B78DF1", "Not Sent"); } }

				public static class BookingRequest
				{
					public static string Sent { get { return Res.GetString("E9D86380-F2CF-4E27-85C7-1F9323E3C1F9", "Booking Request Sent"); } }
					public static string Acknowledged { get { return Res.GetString("B4484511-906D-4F2C-9BF4-E9BD93C69DA7", "Booking Request Acknowledged"); } }
					public static string RejectedByInterchange { get { return Res.GetString("50DDFFE9-BC8F-40C4-A60E-87F9FC7D3392", "Booking Request Rejected by Interchange"); } }
					public static string Confirmed { get { return Res.GetString("6AC5D8C6-2B7B-41A7-BB3D-2A370EC554DD", "Booking Confirmed"); } }
					public static string Rejected { get { return Res.GetString("BE5EE0C9-3312-45BE-875E-15ABE194547D", "Booking Rejected"); } }
					public static string WithdrawalSent { get { return Res.GetString("FF8871C2-8E74-4791-ABA2-6FC9E76325AF", "Booking Withdrawal Sent"); } }
					public static string WithdrawalRejected { get { return Res.GetString("BCE41C6D-4159-4264-BABF-CC982270AF3C", "Booking Withdrawal Rejected"); } }
					public static string WithdrawalAccepted { get { return Res.GetString("4DD97947-5163-403F-AC59-20F24F3B81DD", "Booking Withdrawal Accepted"); } }
					public static string PendingProcessing { get { return Res.GetString("D1E5D2C0-9772-4DD0-BDF6-1BD2AF9F7397", "Booking Request Pending Processing"); } }
				}

				public static class ShippingInstruction
				{
					public static string Sent { get { return Res.GetString("AEF62099-75C3-49D2-BA48-51C8FC8C994D", "Shipping Instruction Sent"); } }
					public static string Acknowledged { get { return Res.GetString("55F8ED1D-BFEB-4165-800C-B89A44DD94BA", "Shipping Instruction Acknowledged"); } }
					public static string RejectedByInterchange { get { return Res.GetString("5162501B-2046-4D86-A445-482F258AD4CB", "Shipping Instruction Rejected by Interchange"); } }
					public static string Confirmed { get { return Res.GetString("FFF6F8FF-C0BA-41B9-8E58-64133C675B27", "Shipping Instruction Confirmed"); } }
					public static string Rejected { get { return Res.GetString("8B199765-2897-4366-913E-A56DD8ABA1D2", "Shipping Instruction Rejected"); } }
					public static string PendingProcessing { get { return Res.GetString("86A30117-DF11-4C6A-98D4-FC58BDFA088D", "Shipping Instruction Pending Processing"); } }
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constants")]
		public static class EventParameterDescriptions
		{
			public const string Departure = "Departure";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constants")]
		public static class LogSubscriber
		{
			public const string AgencyBookingUpdatedLogSubscriber = "Agency Booking Updated Log Subscriber";
		}

		public static class BillOfLadingBillStatus
		{
			public static class Codes
			{
				public const string OriginalBillReceived = "OBR";
				public const string OriginalBillTransferred = "OBT";
				public const string OriginalBillAmendmentInProgress = "OBA";
				public const string SwitchedToPaper = "STP";
				public const string Surrendered = "SUR";
				public const string SentForPublication = "OBS";
				public const string OriginalBillPublished = "OBP";
				public const string PublishingRejected = "OBF";
			}

			public static class Descriptions
			{
				public static MultilingualString OriginalBillReceived => ResString.GetMultilingualString("f3d2d56f-6d55-4c1c-a4fe-b7ed358fc4ad", "Original Bill Received");
				public static MultilingualString OriginalBillTransferred => ResString.GetMultilingualString("d5f64440-f8f2-4571-a62e-0018bb65fdc9", "Original Bill Transferred");
				public static MultilingualString OriginalBillAmendmentInProgress => ResString.GetMultilingualString("db6534a1-5a32-4ff2-ba9c-149d33dfa8ab", "Original Bill Amendment in Progress");
				public static MultilingualString SwitchedToPaper => ResString.GetMultilingualString("43960755-64b3-4a29-aa90-44e437c20df2", "Switched To Paper");
				public static MultilingualString Surrendered => ResString.GetMultilingualString("51c76dc7-d81f-41f9-b031-4a6f5206215e", "Surrendered");
				public static MultilingualString SentForPublication => ResString.GetMultilingualString("C415816E-7250-44B9-9D6E-91F39C139969", "Sent For Publication");
				public static MultilingualString OriginalBillPublished => ResString.GetMultilingualString("5F3C60F1-1E11-4B5D-9470-EF1A612481BB", "Original Bill Published");
				public static MultilingualString PublishingRejected => ResString.GetMultilingualString("C2E07F9B-324E-4DE9-8585-4DFDCC1DE51D", "Publishing Rejected");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "New Zealand Export Pre-Advice Status")]
		public static class NZExportPreAdviceStatus
		{
			public static class Codes
			{
				public const string NotSent = "Not Sent";
				public const string Sent = "Sent";
				public const string Accepted = "Accepted";
				public const string Rejected = "Rejected";
				public const string WithdrawalSent = "Withdrawal Sent";
				public const string Withdrawn = "Withdrawn";
			}
		}

		public static class TransitPackageStateUnitType
		{
			public const string Package = "PKG";
			public const string HandlingUnit = "HU";
			public const string AirULDContainer = "ULD";
			public const string SeaContainer = "CNT";
			public const string Overpack = "OVP";
			public const string PackLine = "PKL";
		}
	}
}
