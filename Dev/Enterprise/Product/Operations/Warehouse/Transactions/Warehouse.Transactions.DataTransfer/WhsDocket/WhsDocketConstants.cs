
namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	// NOTE WELL:  1.	ANY CHANGES TO THIS FORMAT MUST BE REFLECTED IN DOCUMENTATION AND SAMPLE FILES AS PUBLISHED ON OUR WEBSITE.
	//						Update this document and email to Kirsten who will review and publish on web.
	//						\\corporate.cargowise.com\globaldata\Eagle\data\eServices Team\PS Team\Standard Interfaces\Warehouse Order Data Import (3PL and Bonded)
	//						\\corporate.cargowise.com\globaldata\Eagle\data\eServices Team\PS Team\Standard Interfaces\Warehouse Order Sample File (3PL) (.txt)
	//
	//             2. FUNCTIONAL TESTING SHOULD INCLUDE THE SAMPLE FILE PUBLISHED ON OUR WEBSITE.
	//
	//             3.	WHEN ADDING NEW FIELDS, EITHER USE THE SPARE FIELDS OR ADD TO THE *END* OF THE DEFINITION.

	public static class WhsDocketConstants
	{
		public const string HeaderType = "WOH";
		public const string LineType = "WOL";
		public const string Version = "1.0";

		public static class HeaderRecord
		{
			public const int RecordLength = 112;

			public const int RecordType = 0;
			public const int Version = 1;
			public const int SenderEmail = 2;
			public const int OrderNumber = 3;
			public const int CustomerReference = 4;
			public const int OrderType = 6;
			public const int PickType = 7;
			public const int DateRequired = 8;
			public const int WarehouseCode = 12;
			public const int TotalBillToInvoiceAmount = 13;
			public const int TransportReference = 16;
			public const int TransportServiceLevel = 17;
			public const int ServiceLevel = 18;
			public const int TotalUnits = 19;

			public const int ClientCode = 33;
			public const int ClientContact = 34;
			public const int ClientName = 35;
			public const int ClientAddress1 = 36;
			public const int ClientAddress2 = 37;
			public const int ClientCity = 38;
			public const int ClientState = 39;
			public const int ClientPostCode = 40;
			public const int ClientISOCountryCode = 41;
			public const int ClientCountryName = 42;
			public const int ClientUNLOCO = 43;
			public const int ClientPhone = 44;
			public const int ClientEmailAddress = 45;

			public const int ConsigneeCode = 48;
			public const int ConsigneeContact = 49;
			public const int ConsigneeName = 50;
			public const int ConsigneeAddress1 = 51;
			public const int ConsigneeAddress2 = 52;
			public const int ConsigneeCity = 53;
			public const int ConsigneeState = 54;
			public const int ConsigneePostCode = 55;
			public const int ConsigneeISOCountryCode = 56;
			public const int ConsigneeCountryName = 57;
			public const int ConsigneeUNLOCO = 58;
			public const int ConsigneePhone = 59;
			public const int ConsigneeEmailAddress = 60;

			public const int GoodsBilledToCode = 63;
			public const int GoodsBilledToContact = 64;
			public const int GoodsBilledToName = 65;
			public const int GoodsBilledToAddress1 = 66;
			public const int GoodsBilledToAddress2 = 67;
			public const int GoodsBilledToCity = 68;
			public const int GoodsBilledToState = 69;
			public const int GoodsBilledToPostCode = 70;
			public const int GoodsBilledToISOCountryCode = 71;
			public const int GoodsBilledToCountryName = 72;
			public const int GoodsBilledToUNLOCO = 73;
			public const int GoodsBilledToPhone = 74;
			public const int GoodsBilledToEmailAddress = 75;

			public const int TransportCoCode = 78;
			public const int TransportCoContact = 79;
			public const int TransportCoName = 80;
			public const int TransportCoAddress1 = 81;
			public const int TransportCoAddress2 = 82;
			public const int TransportCoCity = 83;
			public const int TransportCoState = 84;
			public const int TransportCoPostCode = 85;
			public const int TransportCoISOCountryCode = 86;
			public const int TransportCoCountryName = 87;
			public const int TransportCoUNLOCO = 88;
			public const int TransportCoPhone = 89;
			public const int TransportCoEmailAddress = 90;

			public const int TransportBilledToCode = 93;
			public const int TransportBilledToContact = 94;
			public const int TransportBilledToName = 95;
			public const int TransportBilledToAddress1 = 96;
			public const int TransportBilledToAddress2 = 97;
			public const int TransportBilledToCity = 98;
			public const int TransportBilledToState = 99;
			public const int TransportBilledToPostCode = 100;
			public const int TransportBilledToISOCountryCode = 101;
			public const int TransportBilledToCountryName = 102;
			public const int TransportBilledToUNLOCO = 103;
			public const int TransportBilledToPhone = 104;
			public const int TransportBilledToEmailAddress = 105;

			public const int GoodsHandlingNotes = 108;
			public const int DGAdditionalHandlingNotes = 109;
			public const int Deliveryinstructions = 110;
			public const int ThirdPartyCarrierAccountNumber = 111;
		}

		public static class LineRecord
		{
			public const int RecordLength = 32;

			public const int ClientOrderLineNumber = 1;
			public const int ProductCode = 3;
			public const int ProductDescription = 4;
			public const int ConsigneeOrBuyerProductCode = 5;
			public const int ConsigneeOrBuyerProductDescription = 6;
			public const int QuantityFromClientOrder = 10;
			public const int QuantityActuallyOrdered = 11;
			public const int ProductUQ = 12;
			public const int RecommendedUnitPrice = 15;
			public const int UnitDiscount = 16;
			public const int UnitDiscountAmount = 17;
			public const int UnitPriceAfterDiscount = 18;
			public const int ExtendedPrice = 19;
			public const int PartAttribute1 = 22;
			public const int PartAttribute2 = 23;
			public const int PartAttribute3 = 24;
			public const int CustomAttribute1 = 25;
			public const int CustomAttribute2 = 26;
			public const int CustomAttribute3 = 27;
			public const int BondEntryNumber = 53;
			public const int BondEntryLineNumber = 54;
			public const int BondEntryDate = 55;
			public const int BondDeclarationRef = 56;
			public const int BondCountryOfOrigin = 57;
			public const int BondCustomsQuantity = 58;
			public const int BondCustomsQuantityUnit = 59;
			public const int BondWarehouseQuantity = 60;
			public const int BondWarehouseQuantityUnit = 61;
			public const int BondValueForDuty = 62;
			public const int BondTILVAmount = 63;
			public const int BondTILVCurrency = 64;
			public const int BondAddInfo = 65;
			public const int BondedEntryKey = 66;
			public const int ExpiryDate = 67;
			public const int PackingDate = 71;
			public const int LineComments = 72;
		}
	}
}
