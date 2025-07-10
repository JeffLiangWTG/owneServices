using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	abstract class CommercialInvoiceWrapperTest : DocumentWrapperTest
	{
		public virtual void TestTransportation()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var declaration = invoice.JobDeclaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(wrapper.Transportation);
				AssertEquals(wrapper.Declaration.Transportation, wrapper.Transportation);
			});
		}

		public virtual void TestPortOfOriginName()
		{
			var originUNLOCO = Factory.New<RefUNLOCO>();
			originUNLOCO.RL_Code = "TWABC";
			originUNLOCO.Description = "origin name";
			originUNLOCO.RL_NameWithDiacriticals = "origin proper name";
			originUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var invoice = GenerateJobComInvoiceHeader();
			var declaration = invoice.JobDeclaration;
			declaration.JE_RL_NKOrigin = "TWABC";
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(wrapper.PortOfOriginName);
				AssertEquals(wrapper.Declaration.PortOfOriginName, wrapper.PortOfOriginName);
			});
		}

		public virtual void TestFinalDestinationName()
		{
			RefUNLOCO destinationUNLOCO = Factory.New<RefUNLOCO>();
			destinationUNLOCO.RL_Code = "TWABC";
			destinationUNLOCO.Description = "destination name";
			destinationUNLOCO.RL_NameWithDiacriticals = "destination proper name";
			destinationUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var invoice = GenerateJobComInvoiceHeader();
			var declaration = invoice.JobDeclaration;
			declaration.JE_RL_NKFinalDestination = "TWABC";
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(wrapper.FinalDestinationName);
				AssertEquals(wrapper.Declaration.FinalDestinationName, wrapper.FinalDestinationName);
			});
		}

		public void TestSellerJobDocAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = GenerateJobComInvoiceHeader();
			invoice.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			invoice.JobDeclaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader2.PK;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertSame(invoice.SupplierDocumentaryAddress, wrapper.SellerJobDocAddress.WrappedObject);

			invoice.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertSame(invoice.JobDeclaration.SupplierDocumentaryAddress, wrapper.SellerJobDocAddress.WrappedObject);
		}

		public void TestBuyerJobDocAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = GenerateJobComInvoiceHeader();
			invoice.BuyerDocumentaryAddress.OrganisationPK = orgHeader.PK;
			invoice.JobDeclaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader2.PK;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertSame(invoice.BuyerDocumentaryAddress, wrapper.BuyerJobDocAddress.WrappedObject);

			invoice.BuyerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertSame(invoice.JobDeclaration.ImporterDocumentaryAddress, wrapper.BuyerJobDocAddress.WrappedObject);
		}

		public void TestSellerAddressData()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var supplierOrganization = Factory.New<OrgHeader>();
			supplierOrganization.OH_Code = "org01";
			supplierOrganization.OH_RL_NKClosestPort = "TWKEL";
			supplierOrganization.Addresses.RemoveAndDeleteAll();

			var mainAddress = supplierOrganization.Addresses.MainAddress;
			mainAddress.Address1 = "Main Address1";
			mainAddress.Address2 = "Main Address2";
			mainAddress.City = "Main City";
			mainAddress.State = "Main State";
			mainAddress.Postcode = "123";
			mainAddress.Language = "EN-GB";

			var supplierOrganization2 = Factory.New<OrgHeader>();
			supplierOrganization2.OH_Code = "org02";
			supplierOrganization2.OH_RL_NKClosestPort = "TWKEL";
			supplierOrganization2.Addresses.RemoveAndDeleteAll();

			var mainAddress2 = supplierOrganization2.Addresses.MainAddress;
			mainAddress2.Address1 = "Main Address3";
			mainAddress2.Address2 = "Main Address4";
			mainAddress2.City = "Main City 2";
			mainAddress2.State = "Main State 2";
			mainAddress2.Postcode = "456";
			mainAddress2.Language = "EN-US";

			invoice.SupplierDocumentaryAddress.OrganisationPK = supplierOrganization.PK;
			invoice.JobDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplierOrganization2.PK;
			Factory.Save();

			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertType<ExportExporterDocumentaryAddressDetailsWrapper>(wrapper.SellerAddressData);
				AssertEquals("MAIN ADDRESS1 MAIN ADDRESS2 MAIN CITY 123 TAIWAN", wrapper.SellerAddressData.Address);
			});

			invoice.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertType<ExportExporterDocumentaryAddressDetailsWrapper>(wrapper.SellerAddressData);
				AssertEquals("MAIN ADDRESS3 MAIN ADDRESS4 MAIN CITY 2 456 TAIWAN", wrapper.SellerAddressData.Address);
			});
		}

		public void TestBuyerAddressData()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var importerOrganization = Factory.New<OrgHeader>();
			importerOrganization.OH_Code = "org01";
			importerOrganization.OH_RL_NKClosestPort = "TWKEL";
			importerOrganization.Addresses.RemoveAndDeleteAll();

			var mainAddress = importerOrganization.Addresses.MainAddress;
			mainAddress.Address1 = "Main Address1";
			mainAddress.Address2 = "Main Address2";
			mainAddress.City = "Main City";
			mainAddress.State = "Main State";
			mainAddress.Postcode = "123";
			mainAddress.Language = "EN-GB";

			var importerOrganization2 = Factory.New<OrgHeader>();
			importerOrganization2.OH_Code = "org02";
			importerOrganization2.OH_RL_NKClosestPort = "TWKEL";
			importerOrganization2.Addresses.RemoveAndDeleteAll();

			var mainAddress2 = importerOrganization2.Addresses.MainAddress;
			mainAddress2.Address1 = "Main Address3";
			mainAddress2.Address2 = "Main Address4";
			mainAddress2.City = "Main City 2";
			mainAddress2.State = "Main State 2";
			mainAddress2.Postcode = "456";
			mainAddress2.Language = "EN-US";

			invoice.BuyerDocumentaryAddress.OrganisationPK = importerOrganization.PK;
			invoice.JobDeclaration.ImporterDocumentaryAddress.OrganisationPK = importerOrganization2.PK;
			Factory.Save();

			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertType<ExportBuyerDocumentaryAddressDetailsWrapper>(wrapper.BuyerAddressData);
				AssertEquals("MAIN ADDRESS1 MAIN ADDRESS2 MAIN CITY 123 TAIWAN", wrapper.BuyerAddressData.Address);
			});

			invoice.BuyerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			CombineAssertions(() =>
			{
				AssertType<ExportBuyerDocumentaryAddressDetailsWrapper>(wrapper.BuyerAddressData);
				AssertEquals("MAIN ADDRESS3 MAIN ADDRESS4 MAIN CITY 2 456 TAIWAN", wrapper.BuyerAddressData.Address);
			});
		}

		public void TestDecimalPlaces()
		{
			var wrapper = CommercialInvoiceWrapper.New(null, Factory);
			AssertEquals(2m, wrapper.DecimalPlaces);
		}

		public void TestInvoiceNumber()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals("TEST INVOICENUMBER", wrapper.InvoiceNumber);
		}

		public void TestInvoiceDate()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals("2019-09-01", wrapper.InvoiceDate);
		}

		public void TestInvoiceCurrency()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals("TWD", wrapper.InvoiceCurrency);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			AssertEquals("USD", wrapper.InvoiceCurrency);
		}

		public void TestIncoTermCode()
		{
			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_IncoTerm = IncoTerms.FreeOnBoard;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals("FOB", wrapper.IncoTermCode);

			invoice.JZ_IncoTerm = IncoTerms.CostInsuranceAndFreight;
			AssertEquals("CIF", wrapper.IncoTermCode);

			invoice.JZ_IncoTerm = IncoTerms.FreeAlongsideShip;
			AssertEquals("FAS", wrapper.IncoTermCode);
		}

		public void TestIncoTermPlace()
		{
			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_IncoTermPlace = "XX TEST1";
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals("XX TEST1", wrapper.IncoTermPlace);

			invoice.JZ_IncoTermPlace = "aa TEST2 ";
			AssertEquals("AA TEST2", wrapper.IncoTermPlace);

			invoice.JZ_IncoTermPlace = ZString.Empty;
			AssertNullOrEmpty(wrapper.IncoTermPlace);
		}

		public void TestUnitPriceDecimalPlaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.AddInfoChild.TWL_DocumentaryUnitPrice = 1.1113m;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfoChild.TWL_DocumentaryUnitPrice = 2.58884m;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.AddInfoChild.TWL_DocumentaryUnitPrice = 5m;

			var wrapper = CommercialInvoiceWrapper.New(invoiceHeader, Factory);
			AssertEquals(5, wrapper.UnitPriceDecimalPlaces);
		}

		public void TestMarksAndNumbers()
		{
			var testMarksAndNumbers = new ZString('A', 600);
			var invoice = GenerateJobComInvoiceHeader();
			invoice.TW_MarksAndNumbers = testMarksAndNumbers;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(testMarksAndNumbers, wrapper.MarksAndNumbers);
		}

		public void TestDescription()
		{
			var testDescription = new ZString('D', 128);
			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_Description = testDescription;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(testDescription, wrapper.Description);
		}

		public void TestRemarks()
		{
			var testRemarks = new ZString('B', 600);
			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_Remarks = testRemarks;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(testRemarks, wrapper.Remarks);
		}

		public void TestInvoiceLineTotalAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceCurrExRate = 30m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;

			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10m;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 20m;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(30m, wrapper.InvoiceLineTotalAmount);

			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 30m;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(60m, wrapper.InvoiceLineTotalAmount);

			var charge = AddNewCharge(invoice, false, true, true);
			charge.J7_RX_NKCurrency = "TWD";
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(60m, wrapper.InvoiceLineTotalAmount);
		}

		[TestDate(2021, 02, 26)]
		public void TestAdditionChargesTotalAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30m, new ZDateTime(2021, 02, 26), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var additionCharges = AddNewCharge(invoice, false, true, true, amount: 3000m);
			additionCharges.J7_RX_NKCurrency = "TWD";
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(100m, wrapper.AdditionChargesTotalAmount);

			additionCharges = AddNewCharge(invoice, false, true, true, amount: 20m);
			additionCharges.J7_RX_NKCurrency = "USD";
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(120m, wrapper.AdditionChargesTotalAmount);
		}

		[TestDate(2021, 02, 26)]
		public void TestDeductionChargesTotalAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30m, new ZDateTime(2021, 02, 26), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var invoice = GenerateJobComInvoiceHeader();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var charge = AddNewCharge(invoice, false, false, true, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, amount: 3000m);
			charge.J7_RX_NKCurrency = "TWD";
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(100m, wrapper.DeductionChargesTotalAmount);

			charge = AddNewCharge(invoice, false, false, true, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, amount: 20m);
			charge.J7_RX_NKCurrency = "USD";
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(120m, wrapper.DeductionChargesTotalAmount);
		}

		public void TestSayTotalAmount()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 99999m;
			var additionCharge = AddNewCharge(invoice, false, true, true);
			var deductionCharge = AddNewCharge(invoice, false, false, true, chargeType: CustomsChargeTypeList.Codes.DeductionCharge);
			deductionCharge.J7_Amount = 50m;
			additionCharge.J7_Amount = 100m;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			var expectedAmount = 99999m + 100m - 50m;
			AssertEquals(expectedAmount, wrapper.SayTotalAmount);
		}

		public void TestSayTotalDescription()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1.01m;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("SAY TOTAL NEW TAIWAN DOLLAR ONE AND CENT ONE ONLY.", wrapper.SayTotalDescription);

			invoiceLine.JI_LinePrice = 1.02m;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("SAY TOTAL NEW TAIWAN DOLLAR ONE AND CENTS TWO ONLY.", wrapper.SayTotalDescription);

			invoiceLine.JI_LinePrice = 34567m;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("SAY TOTAL NEW TAIWAN DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY SEVEN ONLY.", wrapper.SayTotalDescription);

			invoiceLine.JI_LinePrice = 34567.02m;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("SAY TOTAL NEW TAIWAN DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY SEVEN AND CENTS TWO ONLY.", wrapper.SayTotalDescription);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("SAY TOTAL UNITED STATES DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY SEVEN AND CENTS TWO ONLY.", wrapper.SayTotalDescription);

			var additionCharge = AddNewCharge(invoice, false, true, true, amount: 1.01m);
			additionCharge.J7_RX_NKCurrency = "TWD";
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("SAY TOTAL UNITED STATES DOLLARS THIRTY FOUR THOUSAND FIVE HUNDRED AND SIXTY EIGHT AND CENTS FORTY ONE ONLY.", wrapper.SayTotalDescription);
		}

		public void TestInvoiceLinesGroupByQuantityRows()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_InvoiceQuantity = 2m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Bag;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals("2 BAG", wrapper.InvoiceLinesGroupByQuantity);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 30000m;
			invoiceLine.JI_InvoiceQuantity = 3m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Piece;

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 40000m;
			invoiceLine.JI_InvoiceQuantity = 4m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Pail;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(@"2 BAG
4 PAI
3 PCE", wrapper.InvoiceLinesGroupByQuantity);
		}

		public void TestInvoiceLinesGroupByQuantityDecimalPlaces()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.AddInfoChild.TWL_DocumentaryQty = 3.12m;
			invoiceLine1.AddInfoChild.TWL_DocumentaryUQ = Core.Constants.PkgUnit.Piece;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfoChild.TWL_DocumentaryQty = 4.345m;
			invoiceLine2.AddInfoChild.TWL_DocumentaryUQ = Core.Constants.PkgUnit.Pail;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(@"4.345 PAI
3.120 PCE", wrapper.InvoiceLinesGroupByQuantity);

			invoiceLine1.AddInfoChild.TWL_DocumentaryQty = 3.05m;
			invoiceLine1.AddInfoChild.TWL_DocumentaryUQ = Core.Constants.PkgUnit.Piece;

			invoiceLine2.AddInfoChild.TWL_DocumentaryQty = 4.05m;
			invoiceLine2.AddInfoChild.TWL_DocumentaryUQ = Core.Constants.PkgUnit.Piece;

			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(@"7.10 PCE", wrapper.InvoiceLinesGroupByQuantity);
		}

		public void TestInvoiceLinesGroupByQuantityStringListCount()
		{
			var invoice = GenerateJobComInvoiceHeader();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 500000m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Piece;
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(1m, wrapper.InvoiceLinesGroupByQuantityStringListCount);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 200000m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Piece;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(1m, wrapper.InvoiceLinesGroupByQuantityStringListCount);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 300000m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Piece;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(1m, wrapper.InvoiceLinesGroupByQuantityStringListCount);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 500000m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Bag;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(2m, wrapper.InvoiceLinesGroupByQuantityStringListCount);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 500000m;
			invoiceLine.JI_InvoiceUQ = Core.Constants.PkgUnit.Basket;
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(3m, wrapper.InvoiceLinesGroupByQuantityStringListCount);
		}

		public void TestInvoiceLinesSort()
		{
			var invoice = GenerateJobComInvoiceHeader();
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 20m;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10m;

			invoice.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrice);

			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);

			AssertEquals(20m, wrapper.InvoiceLines[0].LinePrice);
			AssertEquals(10m, wrapper.InvoiceLines[1].LinePrice);
		}

		public void TestAdditionChargesSort()
		{
			var invoice = GenerateJobComInvoiceHeader();
			AddNewCharge(invoice, false, true, true, amount: 20m);
			var additionCharge = AddNewCharge(invoice, false, true, true, chargeType: CustomsChargeTypeList.Codes.ForeignInlandFreight, amount: 0m);
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			invoice.Charges.Sort(InvoiceCharge.Schema.J7_Amount);
			AssertEquals(1, wrapper.AdditionCharges.Count);
			AssertEquals(20m, wrapper.AdditionCharges[0].Amount);

			additionCharge.J7_Amount = 10m;
			invoice.Charges.Sort(InvoiceCharge.Schema.J7_Amount);
			wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(2, wrapper.AdditionCharges.Count);
			AssertEquals(20m, wrapper.AdditionCharges[0].Amount);
			AssertEquals(10m, wrapper.AdditionCharges[1].Amount);
		}

		public void TestAdditionAndDeductionCharges()
		{
			var invoice = GenerateJobComInvoiceHeader();
			AddNewCharge(invoice, false, true, true, amount: 30m);
			AddNewCharge(invoice, false, false, true, chargeType: CustomsChargeTypeList.Codes.DeductionCharge, amount: 20m);

			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(2, wrapper.AdditionAndDeductionCharges.Count);
			AssertEquals(30m, wrapper.AdditionAndDeductionCharges[0].Amount);
			AssertEquals(20m, wrapper.AdditionAndDeductionCharges[1].Amount);
		}

		public void TestDeclaration()
		{
			var invoice = GenerateJobComInvoiceHeader();
			var wrapper = GetCommercialInvoiceWrapper(invoice, Factory);
			AssertEquals(invoice.JobDeclaration, wrapper.Declaration.DeclarationBO);
		}

		InvoiceCharge AddNewCharge(JobComInvoiceHeader invoice, ZBool isIncludedInITOT, ZBool calcIsIncludedInInvoiceAmount, ZBool isGSTApplicable, string chargeType = CustomsChargeTypeList.Codes.AdditionCharge, decimal amount = 200m, bool isDutiable = true)
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
			charge.J7_Calc_IsIncludedInInvoiceAmount = calcIsIncludedInInvoiceAmount;
			charge.J7_IsGSTApplicable = isGSTApplicable;
			charge.J7_Amount = amount;
			return charge;
		}

		JobComInvoiceHeader GenerateJobComInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_VesselName = "TEST NK Vessel";
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_CustomsOffice = "AA";
			declaration.JE_VesselArrivalReg = "212";
			declaration.JE_SLD = "X2";
			declaration.JE_VesselName = "VSCD";
			declaration.JE_MasterBill = "XXX123456";
			declaration.JE_HouseBill = "ABC85858";
			declaration.JE_ExportDate = new ZDateTime(2019, 09, 01);
			declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			declaration.JE_DefermentAccountNumber = "A3";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_RL_NKPortOfLoading = "TWKEL";
			declaration.JE_GoodsDescription = "Test GoodsDescription";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Test InvoiceNumber";
			invoice.JZ_InvoiceDate = new ZDateTime(2019, 09, 01);
			invoice.JZ_RX_NKInvoice_Currency = "TWD";
			invoice.JZ_InvoiceCurrExRate = 1m;
			invoice.JZ_RelatedIndicator = "N";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 88m;
			return invoice;
		}

		protected virtual CommercialInvoiceWrapper GetCommercialInvoiceWrapper(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factory)
		{
			return CommercialInvoiceWrapper.New(jobComInvoiceHeader, factory);
		}
	}
}
