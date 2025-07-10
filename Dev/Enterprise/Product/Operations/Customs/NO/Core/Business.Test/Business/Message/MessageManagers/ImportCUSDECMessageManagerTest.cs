using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECMessageManager))]
sealed class ImportCUSDECMessageManagerTest : EDIFACTMessageManagerTestCase
{
	public override void TestCanSendThisMessage()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "4";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		new LineMerger(declaration).DoMerge();

		var entryHeader = declaration.ActiveEntryHeaders[0];
		var testWrapper = new MessageSendingObject(entryHeader);
		testWrapper.MessageType = Common.Shared.MessageSubTypeCodes.Codes.Original;
		var manager = new ImportCUSDECMessageManagerForTest(testWrapper, notification);
		AssertEquals(expected: false, manager.CanSendThisMessage_Exposed(out ZString reason));
		AssertEquals("Job not yet saved, Please save before sending.", reason);

		Factory.Save();
		manager = new ImportCUSDECMessageManagerForTest(testWrapper, notification);
		AssertEquals(expected: true, manager.CanSendThisMessage_Exposed(out reason));
		AssertEquals(expected: string.Empty, reason);
	}

	public override void TestGetMessageBuilder()
	{
		var manager = ((ImportCUSDECMessageManagerForTest)messageManager);
		AssertType(typeof(CUSDECMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Create));
	}

	public override void TestMessageFriendlyName()
	{
		AssertEquals("CUSDEC", messageManager.MessageFriendlyName);
	}

	public void TestHeader()
	{
		var manager = (CUSDECMessageManager)messageManager;
		AssertType<CusEntryHeader>(manager.Header);
	}

	[TestDate(2024, 08, 10)]
	public override void TestPopulateMessages()
	{
		new RefCusTariffTestHelper(Factory).SetupGenericTariffData();

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 0.085m);
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_DeclarationReference = "B00001001";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MessageSubType = ShipmentTypeImport.Codes.ImportOfGoodsFromAllOtherNotCoveredByEu;
		declaration.JE_GoodsNumber = "123456789";
		declaration.JE_Position = "12345";
		declaration.JE_RL_NKOrigin = "NOBGO";
		declaration.JE_LocationOfGoods = ImportGoodsLocationCodeList.Codes.CustomsWarehouseA;
		declaration.JE_CustomsTransportMode = NOCustomsTransportTypeList.Codes.RAI;
		declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Denmark;
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
		declaration.JE_GS_NKCusAgent = "GOD";
		var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "11";
		instruction.CEI_PackageCount = 3;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
		invoice.JZ_IncoTermPlace = "OSLO";
		invoice.JZ_InvoiceAmount = 1000m;
		invoice.JZ_InvoiceDate = new (2023, 1, 20);
		invoice.JZ_InvoiceNumber = "4936218934";
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
		//invoiceLine.JI_CustomsUnitQty = UnitQuantityCodeList.Codes.Piece; // TODO: transformation to correct codes in future WI, for now test default PK
		invoiceLine.JI_GoodsMarks = "ADR";
		invoiceLine.JI_InvoiceQuantity = 1;
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_LinePrice = 1000m;
		invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.A;
		invoiceLine.JI_Procedure = "6021";
		invoiceLine.JI_ReducedCustomsFlag = "S";
		invoiceLine.JI_Tariff = "19059034";
		invoiceLine.JI_ValuationCode = "1";
		invoiceLine.JI_Weight = 110;
		invoiceLine.JI_NetWeight = 72;
		invoiceLine.JI_CustomsSecondUnitQty = "STK";
		invoiceLine.JI_CustomsSecondQuantity = 10;
		invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 100m);
		invoiceLine.ApportionedCharges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 1188m);
		invoiceLine.ApportionedCharges.AddNew(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, 1188m);
		var package = declaration.Packages.AddNew();
		invoiceLine.ToggleLinkageWithPackage(package, true);
		package.CW_MarksAndNos = "ME3DJELT5NV007653";
		package.CW_PackType = "VN";
		var container1 = declaration.CusContainers.AddNew();
		var container2 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "PVDU1130919";
		container2.CO_ContainerNumber = "PVDU1130666";
		invoice.AssignContainerToInvoiceLines("PVDU1130919");
		invoice.AssignContainerToInvoiceLines("PVDU1130666");
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		supplier.OH_FullName = "MyCorp";
		supplier.MainAddress.OA_Address1 = "Address line 1";
		supplier.MainAddress.OA_Address2 = "Address line 2";
		supplier.MainAddress.OA_PostCode = "28320";
		supplier.MainAddress.OA_City = "Madrid";
		supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

		var importer = Factory.NewWithValidTestData<OrgHeader>()
			.AsDeferredDutiesAccount("04049962")
			.AsMVARegistered("1212121212");
		importer.OH_Category = UniversalReferenceConstants.OrgHeaderType.Organization;
		declaration.JE_OH_Importer = importer.PK;
		var declarant = Factory.NewWithValidTestData<OrgHeader>()
			.AsMVARegistered("0101014444");
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = invoiceLine.JI_CEI;
		var entryLine1 = entryHeader.AllEntryLines.AddNew();
		entryLine1.InvoiceLines.Add(invoiceLine);
		entryLine1.CL_CustomsValue = 420.69m;
		invoiceLine.JI_CL = entryLine1.PK;

		var entryLineFee1 = entryLine1.Fees.AddNew("FA200", 27);
		entryLineFee1.CF_BaseValue = 10681;
		entryLineFee1.CF_Rate = 0.25m;
		var entryLineFee2 = entryLine1.Fees.AddNew("MG100", 50);
		entryLineFee2.CF_BaseValue = 20000;
		entryLineFee2.CF_Rate = 0.25m;
		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		var document1 = invoiceLine.SupportingDocuments.AddNew();
		document1.CSI_Code = "SER";
		document1.CSI_ReferenceNumber = "FR003790/0230";
		var document2 = invoiceLine.SupportingDocuments.AddNew();
		document2.CSI_Code = "USK";
		document2.CSI_ReferenceNumber = "23/R190181";
		invoiceLine.JI_Description = "PINK BISCUIT";
		entryHeader.CH_PaymentMethod = NOPaymentMethodCodeList.Codes.ImportersDeferred;
		entryHeader.CH_ToCustomsControllingUnit = "TollNO";
		entryHeader.CH_ReCalcReason = "varene gjenutføres i sin helhet (tekst relateringsårsak)";
		entryHeader.CH_ReCalcOrigDecl = "123456789";
		declaration.JE_CopyStatus = "FIN";
		entryLine1.CL_StatisticalValue = invoiceLine.JI_Calc_CIF_InLocalCurrency;
		AssertEquals("[PRE-CONDITION] StatisticalValue", 12941.18m, invoiceLine.JI_Calc_CIF_InLocalCurrency);
		var result = PopulateMessagesExposed(entryHeader);
		AssertEquals(1, result.Length);
		AssertMultilineASCIIEquals("MessageBody", PopulateMessagesOriginal, result[0].EM_MessageText.Replace("'", "'\r\n"));
	}

	public override void SetTestMode(bool testMode)
	{
		var manager = (ImportCUSDECMessageManagerForTest)messageManager;
		manager.ShouldSendMessagesInTestModeImpl = testMode;
	}

	protected override IEDIFACTMessageAttachee GetDataWrapper()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		new LineMerger(declaration).DoMerge();
		return declaration.ActiveEntryHeaders[0];
	}

	protected override EDIFACTMessageManager GetMessageManager()
	{
		return new ImportCUSDECMessageManagerForTest(new MessageSendingObject(dataWrapper as CusEntryHeader), new MessageNotificationCollector_ForTest());
	}

	EDIMessage[] PopulateMessagesExposed(CusEntryHeader entryHeader)
	{
		var testWrapper = new MessageSendingObject(entryHeader);
		var manager = new ImportCUSDECMessageManagerForTest(testWrapper, notification);
		return manager.PopulateMessages_Exposed();
	}

	new MessageNotificationCollector_ForTest notification => new MessageNotificationCollector_ForTest();

	const string PopulateMessagesOriginal =
		"""
		UNH+B00001001+CUSDEC:1:902:UN:NEP-I+<<JOB REFERENCE NUMBER PLACE HOLDER>>'
		BGM+929++137:20240810:102+ZZ'
		CST++IM11:104:1+01:112:NO1'
		RFF+CO:04049962'
		RFF+XC:123456789'
		RFF+LAR:12345'
		RFF+AEI:123456789'
		LOC+35:NO'
		LOC+14:A::NO1'
		TDT+11++20+:::::DK'
		GIS+1::NO1'
		NAD+IM+1212121212::NO1'
		NAD+SE++MyCorp:Address line 1:Address line 2:28320 Madrid ES'
		NAD+DT+0101014444::NO1'
		CTA++GOD'
		NAD+AA+M::NO1'
		NAD+EE+TollNO::NO1'
		TOD+++CIF+1::::OSLO'
		FTX+AHZ+++varene gjenutføres i sin helhet (tekst relateringsårsak)'
		MOA+4+39:1000:EUR+144:1176,47'
		CUX+2++0,085'
		UNS+D'
		DMS+4936218934+137:20230120:102+380'
		CST+1+19059034:122:NO1+6021:117:1+A:116:NO1+1:109:1+S:110:NO1'
		PAC+1++PK'
		PCI++ADR'
		PCI+++VT:ME3DJELT5NV007653'
		PCI+++AAQ:PVDU1130919'
		PCI+++AAQ:PVDU1130666'
		LOC+27:FR'
		MOA+14+123:12941,18+160:1188+40:420,69'
		MEA+AAF+G+KGM:110'
		MEA+AAF+N+KGM:72'
		MEA+AAF+ZZ+STK:10'
		TAX+1+161:27+FA:107:NO1+200:105:NO1+10681+0,25'
		TAX+1+161:50+MG:107:NO1+100:105:NO1+20000+0,25'
		DCR+811+SER:FR003790/0230'
		DCR+811+USK:23/R190181'
		GDS+2'
		FTX+AAA+++PINK BISCUIT'
		UNS+S'
		TAX+4+161:77'
		TAX+3+161:27+FA:107:NO1'
		TAX+3+161:50+MG:107:NO1'
		CNT+5:1+11:3'
		UNT+46+B00001001'
		""";
}

sealed class ImportCUSDECMessageManagerForTest : CUSDECMessageManager
{
	public ImportCUSDECMessageManagerForTest(MessageSendingObject sendingObject, IMessageNotificationCollector notification) : base(sendingObject, notification) { }

	public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode) => GetMessageBuilder(actionCode);

	public EDIMessage[] PopulateMessages_Exposed()
	{
		return PopulateMessage(MessageSubTypes.Create);
	}

	public bool CanSendThisMessage_Exposed(out ZString messageText)
	{
		return CanSendThisMessage(MessageSubTypes.Undefined, out messageText);
	}
}
