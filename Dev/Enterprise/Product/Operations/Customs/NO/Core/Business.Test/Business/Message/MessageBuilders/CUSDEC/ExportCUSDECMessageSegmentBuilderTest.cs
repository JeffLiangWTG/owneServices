using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Messages.CUSDEC;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestDate(2024, 1, 1)]
[TestedType(typeof(CUSDECMessageSegmentBuilder))]
sealed class ExportCUSDECMessageSegmentBuilderTest : TestCaseWithFactory
{
	public void TestPopulateCUSDECMessage()
	{
		var (_, testInput) = GetTestInput();
		AssertGeneratedMessageBody(testInput);
	}

	public void TestPopulateCUSDECMessageBGMMessageType()
	{
		const int index = 1;
		AssertStartsWith("[PRE-CONDITION] original segment", "BGM+", expectedMessageLines[index]);
		var (mocks, testInput) = GetTestInput();
		var testInputMock = mocks.TestInputMock;
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration, "9");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.ManualDeclaration, "ZZ");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.PreliminaryDeclaration, "14");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.FinalDeclaration, "2");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.Correction, "4");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.PostDeclaration, "18");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.RefundDeclaration, "19");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.StatisticalRecalculatedDeclaration, "43");
		AssertGeneratedMessageBodyMessageType(MessageSendingMessageTypes.Codes.CollectiveCustomClearance, "42");
		return;

		void AssertGeneratedMessageBodyMessageType(string messageType, string expectedValue) => CombineAssertions($"Message type: {messageType}", () =>
		{
			testInputMock.SetupGet(x => x.MessageType).Returns(messageType);
			expectedMessageLines[index] = $"BGM+007++137:20240101:102+{expectedValue}'";
			AssertGeneratedMessageBody(testInput);
		});
	}

	public void TestPopulateCUSDECMessage_WithEmptyOrInvalidMessageTypeInDataProvider() => CombineAssertions(() =>
	{
		var (mocks, testInput) = GetTestInput();
		var testInputMock = mocks.TestInputMock;
		SetMessageTypeAndAssert(ZString.Empty);
		SetMessageTypeAndAssert("XY");

		void SetMessageTypeAndAssert(ZString messageType)
		{
			testInputMock.Setup(p => p.MessageType).Returns(messageType);
			var generatedMessage = string.Empty;

			AssertNoExceptionThrown($"When MessageType is '{messageType}'", () => generatedMessage = GenerateMessageBody(testInput));
			Assert($"When MessageType is '{messageType}', MessageFunctionCoded value is empty and is not included in the segment", generatedMessage.Contains("BGM+007++137:20240101:102'"));
		}
	});

	public void TestPopulateCUSDECMessageNADPaymentMethod()
	{
		const int index = 18;
		AssertStartsWith("[PRE-CONDITION] original segment", "NAD+AA+", expectedMessageLines[index]);
		var (mocks, testInput) = GetTestInput();
		var testInputMock = mocks.TestInputMock;
		AssertGeneratedMessageBodyPaymentMethod(NOPaymentMethodCodeList.Codes.Cash, "K");
		AssertGeneratedMessageBodyPaymentMethod(NOPaymentMethodCodeList.Codes.ForwardersDayCredit, "D");
		AssertGeneratedMessageBodyPaymentMethod(NOPaymentMethodCodeList.Codes.ImportersDeferred, "M");
		AssertGeneratedMessageBodyPaymentMethod(NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable, " ");
		return;

		void AssertGeneratedMessageBodyPaymentMethod(string paymentMethod, string expectedValue) => CombineAssertions($"Payment method: {paymentMethod}", () =>
		{
			testInputMock.SetupGet(x => x.PaymentMethod).Returns(paymentMethod);
			expectedMessageLines[index] = $"NAD+AA+{expectedValue}::NO1'";
			AssertGeneratedMessageBody(testInput);
		});
	}

	public void TestPopulateCUSDECMessageGISContainerMode()
	{
		const int index = 12;
		AssertStartsWith("[PRE-CONDITION] original segment", "GIS+", expectedMessageLines[index]);
		var (mocks, testInput) = GetTestInput();
		var testInputMock = mocks.TestInputMock;
		AssertGeneratedMessageBodyContainerMode(Core.Constants.ContainerModes.FCL, "1");
		AssertGeneratedMessageBodyContainerMode(Core.Constants.ContainerModes.LCL, "1");
		AssertGeneratedMessageBodyContainerMode(Core.Constants.ContainerModes.ULD, "1");
		AssertGeneratedMessageBodyContainerMode(Core.Constants.ContainerModes.Containerised, "1");
		AssertGeneratedMessageBodyContainerMode(Core.Constants.ContainerModes.NonContainerised, "0");
		return;

		void AssertGeneratedMessageBodyContainerMode(string containerMode, string expectedValue) => CombineAssertions($"Container mode: {containerMode}", () =>
		{
			testInputMock.SetupGet(x => x.ContainerMode).Returns(containerMode);
			expectedMessageLines[index] = $"GIS+{expectedValue}::NO1'";
			AssertGeneratedMessageBody(testInput);
		});
	}

	public void TestPopulateCUSDECMessageMOAStatisticValueIsMandatory()
	{
		const int index = 32;
		AssertStartsWith("[PRE-CONDITION] original segment", "MOA+14+", expectedMessageLines[index]);
		var (mocks, testInput) = GetTestInput();
		var itemDetailsMock = mocks.ItemDetailsMock;
		itemDetailsMock.SetupGet(x => x.StatisticValue).Returns("0");
		expectedMessageLines[index] = "MOA+14+123:0'";
		AssertGeneratedMessageBody(testInput);
	}

	void AssertGeneratedMessageBody(ICUSDECMessageDataProvider testInput)
	{
		var expected = string.Join("\n", expectedMessageLines);
		var result = GenerateMessageBody(testInput).Replace("'", "'\n");
		AssertMultilineASCIIEquals(expected, result);
	}

	readonly struct TestMocks(
		Mock<ICUSDECMessageDataProvider> testInputMock,
		Mock<ICUSDECMessageDataProvider.IItemDetails> itemDetailsMock)
	{
		public Mock<ICUSDECMessageDataProvider> TestInputMock { get; } = testInputMock;
		public Mock<ICUSDECMessageDataProvider.IItemDetails> ItemDetailsMock { get; } = itemDetailsMock;
	}

	static (TestMocks mocks, ICUSDECMessageDataProvider testInput) GetTestInput()
	{
		var testInputMock = new Mock<ICUSDECMessageDataProvider>();
		// UNH
		testInputMock.SetupGet(x => x.DeclarationReferenceNumber).Returns("01011498465KLL");
		testInputMock.SetupGet(x => x.AssociationAssignedCode).Returns("NEP");
		testInputMock.SetupGet(x => x.LocalReferenceNumber).Returns("NO1234");
		// BGM
		testInputMock.SetupGet(x => x.DocumentMessageName).Returns(DocumentMessageNameCodedList.GetFromString("007"));
		testInputMock.SetupGet(x => x.MessageType).Returns("FU");
		// CST
		testInputMock.SetupGet(x => x.DeclarationType).Returns("RCD");
		testInputMock.SetupGet(x => x.TransactionNature).Returns("CASH");
		// RFF
		testInputMock.SetupGet(x => x.ControlNumber).Returns("04049962");
		testInputMock.SetupGet(x => x.GoodsNumber).Returns("SAMPLE-123");
		testInputMock.SetupGet(x => x.GoodsNumberPosition).Returns("12");
		testInputMock.SetupGet(x => x.RelatedDeclaration).Returns("123456789");
		// LOC
		testInputMock.SetupGet(x => x.GoodsDestination).Returns("SE");
		testInputMock.SetupGet(x => x.GoodsOrigin).Returns("NO");
		testInputMock.SetupGet(x => x.CustomsOfficeOfExit).Returns("0209");
		testInputMock.SetupGet(x => x.LocationOfGoods).Returns("NOSAM");
		// TDT
		testInputMock.SetupGet(x => x.TransportMode).Returns("30");
		testInputMock.SetupGet(x => x.TransportNationality).Returns("RO");
		// GIS
		testInputMock.SetupGet(x => x.ContainerMode).Returns("CNT");
		// NAD
		testInputMock.SetupGet(x => x.ExporterCustomsRegNo).Returns("911705907");
		testInputMock.SetupGet(x => x.ImporterCustomsRegNo).Returns("981940490");
		testInputMock.SetupGet(x => x.SenderAddressType).Returns(PartyQualifierList.GetFromString("CN"));
		testInputMock.SetupGet(x => x.SenderFullName).Returns("NEXANS NETWORK SOLUTIONS");
		testInputMock.SetupGet(x => x.SenderAddress1).Returns("LOGISTICKA 173");
		testInputMock.SetupGet(x => x.SenderAddress2).Returns("");
		testInputMock.SetupGet(x => x.SenderAddress3).Returns("35002 CHEB 2");
		testInputMock.SetupGet(x => x.DeclarantCustomsRegNo).Returns("935596440");
		testInputMock.SetupGet(x => x.PaymentMethod).Returns("M");
		testInputMock.SetupGet(x => x.CustomsControllingUnit).Returns("441001");
		// CTA
		testInputMock.SetupGet(x => x.InitialsOfDeclarant).Returns("KLL");
		// TOD
		testInputMock.SetupGet(x => x.IncoTerm).Returns("DAP");
		testInputMock.SetupGet(x => x.IncoTermPlace).Returns("0613 OSLO");
		// FTX
		testInputMock.SetupGet(x => x.ReExportReason).Returns("SOME GOOD REASON FOR RE-EXPORT");
		// MOA
		testInputMock.SetupGet(x => x.CommercialInvoiceAmount).Returns("9851,24");
		testInputMock.SetupGet(x => x.CommercialInvoiceCurrency).Returns("EUR");
		testInputMock.SetupGet(x => x.FreightAmountNOK).Returns("18276");
		// CUX
		testInputMock.SetupGet(x => x.CurrencyExchangeRate).Returns("10,75");
		// DMS
		var documentMessageSummaryMock = new Mock<ICUSDECMessageDataProvider.IDocumentMessageSummary>();
		testInputMock.SetupGet(x => x.DocumentMessageSummaryCollection).Returns(new[] { documentMessageSummaryMock.Object });
		documentMessageSummaryMock.SetupGet(x => x.InvoiceNumber).Returns("4936218934");
		documentMessageSummaryMock.SetupGet(x => x.InvoiceDate).Returns("20230120");
		// CST
		var itemDetailsMock = new Mock<ICUSDECMessageDataProvider.IItemDetails>();
		documentMessageSummaryMock.SetupGet(x => x.ItemDetailsCollection).Returns(new [] { itemDetailsMock.Object });
		itemDetailsMock.SetupGet(x => x.DeclarationLineNumber).Returns("1");
		itemDetailsMock.SetupGet(x => x.TariffNumber).Returns("19059034");
		itemDetailsMock.SetupGet(x => x.ProcedureCode).Returns("6021");
		itemDetailsMock.SetupGet(x => x.PreferenceCode).Returns("A");
		itemDetailsMock.SetupGet(x => x.ValuationMethod).Returns("1");
		// PAC
		itemDetailsMock.SetupGet(x => x.NumberOfPackages).Returns("1");
		itemDetailsMock.SetupGet(x => x.PackageType).Returns("PK");
		// PCI
		itemDetailsMock.SetupGet(x => x.GeneralGoodsMarks).Returns("ADR");
		itemDetailsMock.SetupGet(x => x.ContainerNumbers).Returns(new[] { (ZString)"PVDU1130919", (ZString)"PVDU1130666" });
		// LOC
		itemDetailsMock.SetupGet(x => x.RegionOfOrigin).Returns("31");
		// MOA
		itemDetailsMock.SetupGet(x => x.AdjustedValue).Returns("0");
		itemDetailsMock.SetupGet(x => x.StatisticValue).Returns("16188");
		itemDetailsMock.SetupGet(x => x.AddedValueDueToProcessingAbroad).Returns("0");
		// MEA
		itemDetailsMock.SetupGet(x => x.GrossWeight).Returns("110");
		itemDetailsMock.SetupGet(x => x.NetWeight).Returns("72");
		itemDetailsMock.SetupGet(x => x.CustomsQtyAmount).Returns("10");
		itemDetailsMock.SetupGet(x => x.CustomsQtyUnitOfMeasurement).Returns("STK");
		// TAX
		var itemDetailsFeeMock1 = new Mock<ICUSDECMessageDataProvider.IItemDetailsFee>();
		var itemDetailsFeeMock2 = new Mock<ICUSDECMessageDataProvider.IItemDetailsFee>();
		itemDetailsMock.SetupGet(x => x.ItemDetailsFeeCollection).Returns(new [] { itemDetailsFeeMock1.Object, itemDetailsFeeMock2.Object });
		itemDetailsFeeMock1.SetupGet(x => x.FeeAmount).Returns("27");
		itemDetailsFeeMock1.SetupGet(x => x.FeeType).Returns("FA");
		itemDetailsFeeMock1.SetupGet(x => x.FeeTypeSequence).Returns("200");
		itemDetailsFeeMock1.SetupGet(x => x.FeeBaseValueForCalculation).Returns("10681");
		itemDetailsFeeMock1.SetupGet(x => x.FeeRate).Returns("0,25");
		itemDetailsFeeMock2.SetupGet(x => x.FeeAmount).Returns("50");
		itemDetailsFeeMock2.SetupGet(x => x.FeeType).Returns("MG");
		itemDetailsFeeMock2.SetupGet(x => x.FeeTypeSequence).Returns("100");
		itemDetailsFeeMock2.SetupGet(x => x.FeeBaseValueForCalculation).Returns("20000");
		itemDetailsFeeMock2.SetupGet(x => x.FeeRate).Returns("0,25");
		// DCR
		var itemDetailsSupportingDocs1 = new Mock<ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments>();
		var itemDetailsSupportingDocs2 = new Mock<ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments>();
		itemDetailsMock.SetupGet(x => x.ItemDetailsSupportingDocumentsCollection).Returns(new[] { itemDetailsSupportingDocs1.Object, itemDetailsSupportingDocs2.Object });
		itemDetailsSupportingDocs1.SetupGet(x => x.DocumentCode).Returns("SER");
		itemDetailsSupportingDocs1.SetupGet(x => x.DocumentNumberOrText).Returns("FR003790/0230");
		itemDetailsSupportingDocs2.SetupGet(x => x.DocumentCode).Returns("USK");
		itemDetailsSupportingDocs2.SetupGet(x => x.DocumentNumberOrText).Returns("23/R190181");
		// FTX
		itemDetailsMock.SetupGet(x => x.GoodsDescriptions).Returns(["PINK BISCUIT", "BROWN COOKIE"]);
		// TAX
		var itemTotalFee1 = new Mock<ICUSDECMessageDataProvider.ITotalFeeLines>();
		var itemTotalFee2 = new Mock<ICUSDECMessageDataProvider.ITotalFeeLines>();
		testInputMock.SetupGet(x => x.TotalFeeCollection).Returns(new[] { itemTotalFee1.Object, itemTotalFee2.Object });
		itemTotalFee1.SetupGet(x => x.Amount).Returns(118);
		itemTotalFee1.SetupGet(x => x.FeeCode).Returns("FA");
		itemTotalFee2.SetupGet(x => x.Amount).Returns(2770);
		itemTotalFee2.SetupGet(x => x.FeeCode).Returns("RT");
		testInputMock.SetupGet(x => x.TotalFeeAmount).Returns("2888");
		// CNT
		testInputMock.SetupGet(x => x.TotalNoOfItemLines).Returns("3");
		testInputMock.SetupGet(x => x.TotalNoOfPackages).Returns("3");

		return (new (testInputMock, itemDetailsMock), testInputMock.Object);
	}

	static string GenerateMessageBody(ICUSDECMessageDataProvider dataProvider)
	{
		var result = new CUSDECMessage();
		CUSDECMessageSegmentBuilder.PopulateCUSDECMessage(result, dataProvider);
		return result.ToString(new Edifact.UNOACharacterSet());
	}

	readonly string[] expectedMessageLines =
	[
		"UNH+01011498465KLL+CUSDEC:1:902:UN:NEP+NO1234'",
		"BGM+007++137:20240101:102+9'",
		"CST++RCD:104:1+CASH:112:NO1'",
		"RFF+CO:04049962'",
		"RFF+XC:SAMPLE-123'",
		"RFF+LAR:12'",
		"RFF+AEI:123456789'",
		"LOC+36:SE'",
		"LOC+35:NO'",
		"LOC+42:0209::NO1'",
		"LOC+14:NOSAM::NO1'",
		"TDT+11++30+:::::RO'",
		"GIS+1::NO1'",
		"NAD+EX+911705907::NO1'",
		"NAD+IM+981940490::NO1'",
		"NAD+CN++NEXANS NETWORK SOLUTIONS:LOGISTICKA 173::35002 CHEB 2'",
		"NAD+DT+935596440::NO1'",
		"CTA++KLL'",
		"NAD+AA+M::NO1'",
		"NAD+EE+441001::NO1'",
		"TOD+++DAP+1::::0613 OSLO'",
		"FTX+AHZ+++SOME GOOD REASON FOR RE-EXPORT'",
		"MOA+4+39:9851,24:EUR+144:18276'",
		"CUX+2++10,75'",
		"UNS+D'",
		"DMS+4936218934+137:20230120:102+380'",
		"CST+1+19059034:122:NO1+6021:117:1+A:116:NO1+1:109:1'",
		"PAC+1++PK'",
		"PCI++ADR'",
		"PCI+++AAQ:PVDU1130919'",
		"PCI+++AAQ:PVDU1130666'",
		"LOC+106:31'",
		"MOA+14+123:16188'",
		"MEA+AAF+G+KGM:110'",
		"MEA+AAF+N+KGM:72'",
		"MEA+AAF+ZZ+STK:10'",
		"TAX+1+161:27+FA:107:NO1+200:105:NO1+10681+0,25'",
		"TAX+1+161:50+MG:107:NO1+100:105:NO1+20000+0,25'",
		"DCR+811+SER:FR003790/0230'",
		"DCR+811+USK:23/R190181'",
		"GDS+2'",
		"FTX+AAA+++PINK BISCUIT:BROWN COOKIE'",
		"UNS+S'",
		"TAX+4+161:2888'",
		"TAX+3+161:118+FA:107:NO1'",
		"TAX+3+161:2770+RT:107:NO1'",
		"CNT+5:3+11:3'",
		"UNT+48+01011498465KLL'"
	];
}
