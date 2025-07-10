using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADH))]
sealed class NODocSADHTest : DocBaseWrapperTest
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => NODocSADH.New(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("When entryHeader.Declaration is null",
			() => NODocSADH.New(Factory.New<CusEntryHeader>(), Factory));
	}

	public void TestInvoices()
	{
		AssertType<NODocSADHInvoiceCollection>(CreateNewNoSADHWrapper().Invoices);
	}

	public void TestLines()
	{
		var lines = CreateNewNoSADHWrapper().Lines;
		AssertNotNull(lines);
		AssertType<NODocSADHLineCollection>(lines);
	}

	#region SADH Page 1

	public void TestBoxAContent()
	{
		AssertEquals("[Pre-Condition] BoxA Content", "LINJEDEKLARERT", CreateNewNoSADHWrapper().BoxAContent);
		entryHeader.CH_EntryReleaseDate = new ZDateTime(2023, 4, 11);

		AssertEquals("BoxA Content", @"LINJEDEKLARERT
20230411", CreateNewNoSADHWrapper().BoxAContent);

		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = "CER";
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = "123334328647308372342";
		cusEntryNumber.CE_IssueDate = new ZDateTime(2020, 01, 01);

		AssertEquals("BoxA Content", @"LINJEDEKLARERT
20230411
123334
7308372342", CreateNewNoSADHWrapper().BoxAContent);
	}

	public void TestBox1DeclarationType()
	{
		AssertEquals("[Pre-Condition] Declaration Type", ZString.Empty, CreateNewNoSADHWrapper().Box1DeclarationType);

		declaration.JE_MessageSubType = "EU";
		AssertEquals("Declaration Type", "EU", CreateNewNoSADHWrapper().Box1DeclarationType);
	}

	public void TestBox1EntryStyle()
	{
		AssertEquals("[Pre-Condition] Entry Style", ZString.Empty, CreateNewNoSADHWrapper().Box1EntryStyle);

		entryInstruction.CEI_Style = "4";
		AssertEquals("Entry Style", "4", CreateNewNoSADHWrapper().Box1EntryStyle);
	}

	public void TestBox2Supplier()
	{
		var supplier = Factory.New<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Supplier = supplier.PK;
		var supplierAddress = supplier.Addresses.AddNew();
		supplierAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		supplierAddress.OA_Address1 = "address1";
		supplierAddress.OA_Address2 = "address2";
		supplierAddress.OA_City = "city";
		supplierAddress.OA_State = "state";
		supplierAddress.OA_PostCode = "111";
		supplierAddress.OA_RL_NKRelatedPortCode = "COBOG";

		declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var wrapper = NODocSADH.New(entryHeader, Factory);

		AssertContains("ADDRESS1", wrapper.Box2Supplier.ToString());
		AssertContains("ADDRESS2", wrapper.Box2Supplier.ToString());
		AssertContains("CITY", wrapper.Box2Supplier.ToString());
		AssertContains("111", wrapper.Box2Supplier.ToString());
		AssertContains("STATE", wrapper.Box2Supplier.ToString());
		AssertContains("COLOMBIA", wrapper.Box2Supplier.ToString());
	}

	public void TestShowSupplierOrgNumber()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("ShowSupplierOrganisationNumber for Import", expected: false, CreateNewNoSADHWrapper().ShowSupplierOrganisationNumber);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("ShowSupplierOrganisationNumber for Export", expected: true, CreateNewNoSADHWrapper().ShowSupplierOrganisationNumber);
	}

	public void TestBox6TotalNoOfUnits()
	{
		AssertEquals("[Pre-Condition] Total No Of Units", 0, CreateNewNoSADHWrapper().Box6TotalNoOfUnits);

		declaration.JE_TotalNoOfPacks = 14;
		AssertEquals("Total No Of Units", 14, CreateNewNoSADHWrapper().Box6TotalNoOfUnits);
	}

	public void TestBox8Importer()
	{
		var importer = Factory.New<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = importer.PK;
		var importerAddress = importer.Addresses.AddNew();
		importerAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		importerAddress.OA_Address1 = "address1";
		importerAddress.OA_Address2 = "address2";
		importerAddress.OA_City = "city";
		importerAddress.OA_State = "state";
		importerAddress.OA_PostCode = "111";
		importerAddress.OA_RL_NKRelatedPortCode = "COBOG";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var wrapper = NODocSADH.New(entryHeader, Factory);

		AssertContains("ADDRESS1", wrapper.Box8Importer.ToString());
		AssertContains("ADDRESS2", wrapper.Box8Importer.ToString());
		AssertContains("CITY", wrapper.Box8Importer.ToString());
		AssertContains("111", wrapper.Box8Importer.ToString());
		AssertContains("STATE", wrapper.Box8Importer.ToString());
		AssertContains("COLOMBIA", wrapper.Box8Importer.ToString());
	}

	public void TestBox14Declarant()
	{
		var declarant = CreateOrgHeaderWithAddress();
		declaration.JE_OA_DeclarantAddress = declarant.Address.PK;
		var box14Declarant = CreateNewNoSADHWrapper().Box14Declarant;
		AssertNotNull("Declarant", box14Declarant);

		var declarantAddress = declarant.Address;
		CombineAssertions(() =>
		{
			AssertEquals("CompanyName", declarant.Header.OH_FullName, box14Declarant.CompanyName);
			AssertEquals("PhoneNumber", declarantAddress.OA_Phone_Formatted, box14Declarant.Phone);
		});
	}

	public void TestBox5NoOfEntries()
	{
		AssertEquals("[Pre-Condition] No of entries (one merged line)", 1, CreateNewNoSADHWrapper().Box5NoOfEntries);

		entryHeader.MergedLines.AddNew();
		AssertEquals("No of entries (when merged line count is 2)", 2, CreateNewNoSADHWrapper().Box5NoOfEntries);

		entryHeader.AllEntryLines.AddNew();
		AssertEquals("No of entries (when merged line count is 2 and all entry line count is 3)", 2, CreateNewNoSADHWrapper().Box5NoOfEntries);

		entryHeader.MergedLines.AddNew();
		AssertEquals("No of entries (when merged line count is 3 and all entry line count is 4)", 3, CreateNewNoSADHWrapper().Box5NoOfEntries);
	}

	public void TestBox7ReferenceNumber()
	{
		AssertEquals("[Pre-Condition] ReferenceNumber", ZString.Empty, CreateNewNoSADHWrapper().Box7ReferenceNumber);
		declaration.JE_DeclarationReference = "REF12323423423422132";
		AssertEquals("ReferenceNumber", "REF12323423423422132", CreateNewNoSADHWrapper().Box7ReferenceNumber);
	}

	public void TestBox8ImporterOrganisationNumber()
	{
		AssertEquals("[Pre-Condition] Importer Orgnization Number", ZString.Empty, CreateNewNoSADHWrapper().Box8ImporterOrganisationNumber);

		var importer = CreateOrgHeaderWithAddress();
		declaration.JE_OH_Importer = importer.Header.PK;
		var orgHeader = importer.Header;
		orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeader.CustomsCodes.AddNew("SSN", "12047812346");
		AssertEquals("Importer Orgnisation Number when SSN and Category=Private Person", "120478*****", CreateNewNoSADHWrapper().Box8ImporterOrganisationNumber);

		orgHeader.OH_Category = OrgConstants.Category.Business;
		orgHeader.CustomsCodes.RemoveAndDeleteAll();
		orgHeader.CustomsCodes.AddNew("MVA", "12343333");
		AssertEquals("Importer Orgnisation Number when MVA and Category=Business", "12343333", CreateNewNoSADHWrapper().Box8ImporterOrganisationNumber);

		orgHeader.CustomsCodes.RemoveAndDeleteAll();
		orgHeader.CustomsCodes.AddNew("ORG", "12343333");
		AssertEquals("Importer Orgnisation Number when ORG, MVA and Category=Business", "12343333", CreateNewNoSADHWrapper().Box8ImporterOrganisationNumber);
	}

	public void TestBox12TotalFreightCost()
	{
		AssertEquals("[Pre-Condition] Total Freight Cost", 0, CreateNewNoSADHWrapper().Box12TotalFreightCost);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.UnitedStates, 0.2m, ZDateTime.Today, Constants.ExchangeRateTypes.Code.CustomsRate);
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;

		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine.PK;

		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		invoice.JZ_InvoiceAmount = 1000m;
		invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10, Core.Constants.CurrencyCodes.UnitedStates);

		invoiceLine.JI_LinePrice = 500m;
		invoiceLine2.JI_LinePrice = 250m;
		invoiceLine3.JI_LinePrice = 250m;
		Factory.Save();
		declaration.DoMerge();

		AssertEquals("Total Freight Cost", 50, NODocSADH.New(declaration.CustomsEntryHeaders[0], Factory).Box12TotalFreightCost);
	}

	public void TestBox15PortOfOrigin()
	{
		AssertEquals("[Pre-Condition] Port of Origin", ZString.Empty, CreateNewNoSADHWrapper().Box15PortOfOrigin);

		declaration.JE_GoodsOrigin = "FR";
		AssertEquals("Port of Origin", "FR", CreateNewNoSADHWrapper().Box15PortOfOrigin);
	}

	public void TestBox19ContainerMode()
	{
		AssertEquals("[Pre-Condition] Container Mode", "0", CreateNewNoSADHWrapper().Box19ContainerMode);

		declaration.JE_ContainerMode = "CNT";
		AssertEquals("Container Mode when JE_ContainerMode=CNT", "1", CreateNewNoSADHWrapper().Box19ContainerMode);

		declaration.JE_ContainerMode = "XXX";
		AssertEquals("Container Mode when JE_ContainerMode=XXX", "0", CreateNewNoSADHWrapper().Box19ContainerMode);
	}

	public void TestBox20IncoTerm()
	{
		AssertEquals("[Pre-Condition] IncoTerm", ZString.Empty, CreateNewNoSADHWrapper().Box20IncoTerm);

		invoice.JZ_IncoTerm = "EXW";
		AssertEquals("IncoTerm", "EXW", CreateNewNoSADHWrapper().Box20IncoTerm);
	}

	public void TestBox20IncoTermPlace()
	{
		AssertEquals("[Pre-Condition] IncoTerm Place", ZString.Empty, CreateNewNoSADHWrapper().Box20IncoTermPlace);

		invoice.JZ_IncoTerm = "EXW";
		invoice.JZ_IncoTermPlace = "PLACE1";

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_IncoTerm = "EXW";
		invoice2.JZ_IncoTermPlace = "PLACE2";

		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;

		AssertEquals("IncoTerm Place", "PLACE1", CreateNewNoSADHWrapper().Box20IncoTermPlace);
	}

	public void TestBox21TransportNationality()
	{
		AssertEquals("[Pre-Condition] Transport Nationality", ZString.Empty, CreateNewNoSADHWrapper().Box21TransportNationality);

		declaration.JE_RN_NKTransportNationality = "SE";
		AssertEquals("Transport Nationality", "SE", CreateNewNoSADHWrapper().Box21TransportNationality);
	}

	public void TestBox21TransportId()
	{
		AssertEquals("[Pre-Condition] Transport Id", ZString.Empty, CreateNewNoSADHWrapper().Box21TransportId);

		declaration.JE_VesselName = "B12B3";
		AssertEquals("Transport Id", "B12B3", CreateNewNoSADHWrapper().Box21TransportId);
	}

	public void TestBox22Currency()
	{
		invoice.JZ_RX_NKInvoice_Currency = "EUR";

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.InvoiceLines.AddNew().JI_CL = entryLine.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "USD";
		AssertEquals("When Currency across the lines is different", Core.Constants.CurrencyCodes.Norway, CreateNewNoSADHWrapper().Box22Currency);

		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		AssertEquals("When Currencies are same", "EUR", CreateNewNoSADHWrapper().Box22Currency);
	}

	public void TestBox22InvTotalAmount()
	{
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invoice.JZ_InvoiceAmount = 100.00m;
		CurrencyConverterTestHelper.SetExchangeRate(Factory, "EUR", 0.6m, ZDateTime.Today, "CUS");
		CurrencyConverterTestHelper.SetExchangeRate(Factory, "USD", 0.8m, ZDateTime.Today, "CUS");

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.InvoiceLines.AddNew().JI_CL = entryLine.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "USD";
		invoice2.JZ_InvoiceAmount = 200.00m;
		AssertEquals("When Currency across the lines is different", "416.67", CreateNewNoSADHWrapper().Box22InvTotalAmount);

		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		AssertEquals("When Currencies are same", "300.00", CreateNewNoSADHWrapper().Box22InvTotalAmount);
	}

	[TestDate(2025, 3, 1)]
	public void TestBox23ExchangeRate()
	{
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;

		var strategy = CurrencyConverterTestHelper.StrategyIfExchangeRateAlreadyExists.SplitTimePeriod;
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.UnitedKingdom, 1.23m, ZDateTime.Today.AddDays(-10), "CUS", strategy);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.UnitedKingdom, 1.45m, ZDateTime.Today, "CUS", strategy);
		var currency = CurrencyConverterTestHelper.GetCurrency(Factory, Constants.CurrencyCodes.UnitedKingdom);
		AssertEquals("[PRE-CONDITION] Exchange rate at today", 1.45m, currency.GetCustomsRate(ZDateTime.Today));
		AssertEquals("[PRE-CONDITION] Exchange rate at 10 days ago", 1.23m, currency.GetCustomsRate(ZDateTime.Today.AddDays(-10)));

		CombineAssertions("Exchange rate", () =>
		{
			var wrapper = CreateNewNoSADHWrapper();
			AssertEquals("When no exchange rate has been provided, empty string is expected", "1.000000", wrapper.Box23ExchangeRate);

			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedKingdom;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoice.JZ_InvoiceCurrExRate = 1.2m;
			AssertEquals("When exchange rate has been provided, it must be shown in the wrapper", "1.200000", wrapper.Box23ExchangeRate);

			invoice.IsJZ_InvoiceCurrExRateUserEnterable = false;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-10);
			AssertEquals("When requested process date is set, use this date for exchange rate", "1.230000", wrapper.Box23ExchangeRate);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertEquals("When requested process date is NOT set, use current date for exchange rate", "1.450000", wrapper.Box23ExchangeRate);
		});
	}

	public void TestBox24TransactionNature()
	{
		invoice.JZ_ValuationCode = "12";
		invoiceLine.JI_CL = entryLine.PK;

		var wrapper = CreateNewNoSADHWrapper();
		AssertEquals("12", wrapper.Box24TransactionNature);
	}

	public void TestBox25TransportMode()
	{
		declaration.JE_TransportMode = "Air";
		var wrapper = CreateNewNoSADHWrapper();
		AssertEquals("Transport Mode", "Air", wrapper.Box25TransportMode);
	}

	public void Box28FinancialInformation()
	{
		var wrapper = CreateNewNoSADHWrapper();
		AssertEquals("Financial Information", "SE FAKTURAOVERSIKT", wrapper.Box28FinancialInformation);
	}

	public void TestBox30GoodsLocation() => CombineAssertions(() =>
	{
		var wrapper = CreateNewNoSADHWrapper();
		AssertEquals("Empty goodslocation", ZString.Empty, wrapper.Box30LocationOfGoods);
		declaration.JE_LocationOfGoods = "A";
		AssertEquals("Valid location with description", "Customs Warehouse A - A", wrapper.Box30LocationOfGoods);
		declaration.JE_LocationOfGoods = "Z";
		AssertEquals("Invalid location, no description", "Z", wrapper.Box30LocationOfGoods);
	});

	public void TestBox37ProcedureCode()
	{
		entryInstruction.CEI_Procedure = "5000";
		var wrapper = CreateNewNoSADHWrapper();
		CombineAssertions(() =>
		{
			AssertEquals("No procedure on invoiceline, use CEI_Procedure", "5000", wrapper.Box37ProcedureCode);

			invoiceLine.JI_Procedure = "5010";
			AssertEquals("Procedure on invoiceline, use JI_Procedure", "5010", wrapper.Box37ProcedureCode);

			var entryHeaderWithoutInvoices = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var wrapperWithoutInvoices = NODocSADH.New(entryHeaderWithoutInvoices, Factory);
			AssertNoExceptionThrown("When no invoice exists", () => AssertEquals(ZString.Empty, wrapperWithoutInvoices.Box37ProcedureCode));
		});
	}

	public void TestBox45Adjustments()
	{
		var wrapper = CreateNewNoSADHWrapper();
		AssertEquals("[PRE-CONDITION] No adjustments", ZString.Empty, wrapper.Box45Adjustments);
		var entryLine2 = entryHeader.MergedLines.AddNew();

		entryLine.CL_CustomsValue = 100;
		entryLine2.CL_CustomsValue = 100;

		CombineAssertions(() =>
		{
			entryLine.CL_InvoiceAmount = 12.34m;
			entryLine2.CL_InvoiceAmount = 12.34m;
			AssertEquals("Summarized adjustments to put on first page should be 176", "176", wrapper.Box45Adjustments);

			entryLine.CL_InvoiceAmount = 4.56m;
			entryLine2.CL_InvoiceAmount = 4.56m;
			AssertEquals("Summarized adjustments to put on first page should be 190", "190", wrapper.Box45Adjustments);

			entryLine.CL_InvoiceAmount = 100m;
			entryLine2.CL_InvoiceAmount = 100m;
			AssertEquals("Summarized adjustments to put on first page should be <empty>", ZString.Empty, wrapper.Box45Adjustments);
		});
	}

	public void TestBox46StatisticalValue()
	{
		var wrapper = CreateNewNoSADHWrapper();
		AssertEquals("[PRE-CONDITION] No Statistical Values", ZString.Empty, wrapper.Box46StatisticalValue);
		var entryLine2 = entryHeader.MergedLines.AddNew();

		CombineAssertions(() =>
		{
			entryLine.CL_StatisticalValue = 12.34m;
			entryLine2.CL_StatisticalValue = 12.34m;
			AssertEquals("Summarized adjustments to put on first page should be 24", "24", wrapper.Box46StatisticalValue);

			entryLine.CL_StatisticalValue = 4.56m;
			entryLine2.CL_StatisticalValue = 4.56m;
			AssertEquals("Summarized adjustments to put on first page should be 10", "10", wrapper.Box46StatisticalValue);

			entryLine.CL_StatisticalValue = 0m;
			entryLine2.CL_StatisticalValue = 0m;
			AssertEquals("Summarized adjustments to put on first page should be <empty>", ZString.Empty, wrapper.Box46StatisticalValue);
		});
	}

	public void TestBox47Taxes()
	{
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
		invoiceLine2.JI_CL = entryLine2.PK;

		AddNewEntryLineFee("GA200", entryLine, 1m);
		AddNewEntryLineFee("TL1", entryLine, 2m);
		AddNewEntryLineFee("MV1", entryLine, 4m);
		AddNewEntryLineFee("GA400", entryLine2, 8m);
		AddNewEntryLineFee("RT100", entryLine2, 16m);
		AddNewEntryLineFee("TL1", entryLine2, 32m);
		AddNewEntryLineFee("MV1", entryLine2, 64m);

		var feeTypes = CreateNewNoSADHWrapper().Box47Taxes;
		var expected = new[] { "TL", "RT", "GA", "MV" };

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInExactOrder("Duties types should be correct sorted", expected, feeTypes.Cast<NODocSADHLineTax>().Select(x => x.Type).ToArray());
			AssertEquals("Total TL", 34, feeTypes[0].ChargeAmountTotal);
			AssertEquals("Total RT", 16, feeTypes[1].ChargeAmountTotal);
			AssertEquals("Total GA", 9, feeTypes[2].ChargeAmountTotal);
			AssertEquals("Total MV", 68, feeTypes[3].ChargeAmountTotal);
			AssertEquals("Total for all charges", 127, CreateNewNoSADHWrapper().Box47TaxesTotalSum);
		});

		void AddNewEntryLineFee(ZString rateCode, CusEntryLine entryline, ZDecimal amount)
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeType = rateCode;
			fee.CF_ChargeAmount = amount;
			entryLine.Fees.Add(fee);
		}
	}

	public void TestBox48DeferredPayment()
	{
		declaration.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
		AssertEquals(declaration.CountryCode, Core.Constants.CountryCodes.Norway);

		OrgHeader importer = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;

		CombineAssertions(() =>
		{
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1", Core.Constants.CountryCodes.Norway);
			entryHeader.CH_PaymentMethod = NOPaymentMethodCodeList.Codes.ImportersDeferred;
			var wrapper = CreateNewNoSADHWrapper();
			AssertEquals("1", wrapper.Box48DeferredPayment);

			entryHeader.CH_PaymentMethod = NOPaymentMethodCodeList.Codes.Cash;
			AssertEquals("Kontant", wrapper.Box48DeferredPayment);

			entryHeader.CH_PaymentMethod = NOPaymentMethodCodeList.Codes.ForwardersDayCredit;
			AssertEquals("Dagsoppgjør", wrapper.Box48DeferredPayment);

			entryHeader.CH_PaymentMethod = NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable;
			AssertEquals(ZString.Empty, wrapper.Box48DeferredPayment);
		});
	}

	public void TestBox49EntryNumber()
	{
		declaration.JE_GoodsNumber = "202401321022001";
		var wrapper = CreateNewNoSADHWrapper();

		CombineAssertions(() =>
		{
			AssertEquals("202401321022001", wrapper.Box49EntryNumber);

			declaration.JE_Position = "001";
			AssertEquals("202401321022001/001", wrapper.Box49EntryNumber);

			entryInstruction.CEI_SubPosition = "123";
			AssertEquals("202401321022001/001/123", wrapper.Box49EntryNumber);

			declaration.JE_Position = ZString.Empty;
			AssertEquals("202401321022001/123", wrapper.Box49EntryNumber);
		});
	}

	public void TestBox54Place()
	{
		var wrapper = NODocSADH.New(entryHeader, Factory);
		AssertEquals("Box54Place must be empty", ZString.Empty, wrapper.Box54Date);

		var address = Factory.New<OrgAddress>();
		address.City = "Drammen";
		declaration.JE_OA_DeclarantAddress = address.PK;
		wrapper = NODocSADH.New(entryHeader, Factory);

		var place54 = wrapper.Box54Place;
		CombineAssertions(() =>
		{
			AssertEquals("Box54Place is correct", "Drammen", place54);
			AssertEquals("Cached Box54Place", wrapper.Box54Place, place54);
		});
	}

	[TestDate(1995, 02, 16)]
	public void TestBox54Date() => CombineAssertions(() =>
	{
		var wrapper = NODocSADH.New(entryHeader, Factory);
		AssertEquals("Box54Date must be empty", ZString.Empty, wrapper.Box54Date);

		var yesterday = ZDate.Today.AddDays(-1);
		entryHeader.SetEntryReleaseNumber("NO123", yesterday);
		wrapper = NODocSADH.New(entryHeader, Factory);
		AssertEquals("Box54Date must be equal to CE_IssueDate", yesterday.ToString(), wrapper.Box54Date);
	});

	public void TestBox54Details_Box54SignatoryNameAndPosition()
	{
		var broker = CreateBroker("F_N");
		declaration.JE_GS_NKCusAgent = broker.GS_Code;
		var wrapper = NODocSADH.New(entryHeader, Factory);

		CombineAssertions(() =>
		{
			AssertEquals("When Title is Not Empty", broker.GS_FullName + " (Headkicker)", wrapper.Box54SignatoryNameAndPosition);

			broker.GS_Title = "";
			AssertEquals("When Title is Empty", broker.GS_FullName, wrapper.Box54SignatoryNameAndPosition);
		});
	}

	public void TestBox54Details_Box54SignatoryEmailDetails()
	{
		var broker = CreateBroker("F_N");
		declaration.JE_GS_NKCusAgent = broker.GS_Code;

		var wrapper = NODocSADH.New(entryHeader, Factory);
		AssertEquals("Box54SignatoryContactDetails", "sample@wisetechglobal.com", wrapper.Box54SignatoryEmailDetails);
	}

	public void TestBox54Details_Box54NameOfDeclarantAndRepresentative()
	{
		var broker = CreateBroker("F_N");
		SetUpCusAgent(broker.GS_Code);

		var wrapper = NODocSADH.New(entryHeader, Factory);
		AssertEquals("Box54NameOfDeclarantAndRepresentative", "Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);
	}

	#endregion

	#region SADH Page 2

	public void TestDeclarationReference()
	{
		AssertEquals("[PRE-CONDITION] DeclarationReference", ZString.Empty, CreateNewNoSADHWrapper().DeclarationReference);
		declaration.JE_DeclarationReference = "REF11111222223333344";
		AssertEquals("DeclarationReference", "REF11111222223333344", CreateNewNoSADHWrapper().DeclarationReference);
	}

	public void TestEntryReleaseDate()
	{
		AssertEquals("[PRE-CONDITION] EntryReleaseDate", ZString.Empty, CreateNewNoSADHWrapper().EntryReleaseDate);
		entryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;
		AssertEquals("EntryReleaseDate", "19710918", CreateNewNoSADHWrapper().EntryReleaseDate);
	}

	public void TestEntryReleaseNumberFirst6Chars()
	{
		AssertEquals("[PRE-CONDITION] CountryCode", Core.Constants.CountryCodes.Norway, entryHeader.CountryCode);
		AssertEquals("[PRE-CONDITION] EntryReleaseNumberFirst6Chars", ZString.Empty, CreateNewNoSADHWrapper().EntryReleaseNumberFirst6Chars);
		var entryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Norway.CustomsEntryReleaseNumber, entryHeader.CountryCode);
		entryNumber.CE_EntryNum = "4430406666666666";
		AssertEquals("EntryReleaseNumberFirst6Chars", "443040", CreateNewNoSADHWrapper().EntryReleaseNumberFirst6Chars);
	}

	public void TestEntryReleaseNumberLast10Chars()
	{
		AssertEquals("[PRE-CONDITION] CountryCode", Core.Constants.CountryCodes.Norway, entryHeader.CountryCode);
		AssertEquals("[PRE-CONDITION] EntryReleaseNumberLast10Chars", ZString.Empty, CreateNewNoSADHWrapper().EntryReleaseNumberLast10Chars);
		var entryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Norway.CustomsEntryReleaseNumber, entryHeader.CountryCode);
		entryNumber.CE_EntryNum = "6666662400025555";
		AssertEquals("EntryReleaseNumberLast10Chars", "2400025555", CreateNewNoSADHWrapper().EntryReleaseNumberLast10Chars);
	}

	public void TestOwnerReference()
	{
		AssertEquals("[PRE-CONDITION] OwnerReference", ZString.Empty, CreateNewNoSADHWrapper().OwnerReference);
		declaration.JE_OwnerRef = "PO:24010789";
		AssertEquals("OwnerReference", "PO:24010789", CreateNewNoSADHWrapper().OwnerReference);
	}

	#endregion

	protected override DocBaseWrapper GetNewDocumentWrapper() => NODocSADH.New(entryHeader, Factory);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	NODocSADH CreateNewNoSADHWrapper() => GetNewDocumentWrapper() as NODocSADH;

	(OrgHeader Header, OrgAddress Address) CreateOrgHeaderWithAddress()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.OH_FullName = "OrgHeader Name";

		var address = supplier.Addresses.AddNew();
		address.OA_Address1 = "Street A";
		address.OA_Address2 = "Number1";
		address.Postcode = "1234";
		address.OA_Phone = "012343333";
		return (supplier, address);
	}

	void SetUpCusAgent(string cusAgent = "")
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_GS_NKCusAgent = cusAgent;
		declaration.JE_DeclarantType = "SEL";
		declaration.Branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
		declaration.Branch.GB_RL_NKHomePort = "GBFXT";
		declaration.Branch.GB_BranchName = "WAHOO WAHOONIES";
		declaration.Branch.GB_Phone = "0420 019 999";
		declaration.Branch.GB_Email = "sample@wisetechglobal.com";
		declaration.Branch.Company.GC_Name = "Some Big Company Ltd";
	}

	GlbStaff CreateBroker(ZString staffCode)
	{
		var broker = Factory.New<GlbStaff>();
		broker.GS_Code = staffCode;
		broker.GS_FullName = "Frederika Nerkus";
		broker.GS_WorkPhone = "02 5555 9999";
		broker.GS_EmailAddress = "sample@wisetechglobal.com";
		broker.GS_Title = "Headkicker";
		broker.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;

		return broker;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine;
	BaseJobComInvoiceLine invoiceLine;
	BaseJobComInvoiceHeader invoice;
}
