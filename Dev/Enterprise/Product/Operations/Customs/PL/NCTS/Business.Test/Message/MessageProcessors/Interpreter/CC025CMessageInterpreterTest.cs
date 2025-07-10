using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.NCTS.Business.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC025CMessageInterpreterTest : MessageInterpreterTest<IIE025>
{
	public void TestInterpret()
	{
		const string testMrn = "TST_MRN";
		const string testMessageSentOn = "TST_MRN";
		const string expectedInterpretation =
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>MRN</th><td>{testMrn}</td></tr>" +
		$"<tr><th>Message Sent On</th><td>{testMessageSentOn}</td></tr>" +
		"<tr><th>Release Date</th><td></td></tr>" +
		"<tr><th>Release Indicator</th><td></td></tr>" +
		"<tr><th>Customs Office Of Destination</th><td></td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />";

		var dataProviderMock = Mock.Of<IIE025>(x =>
			x.MRN == testMrn &&
			x.PreparationDateAndTime == testMessageSentOn);

		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var interpretation = interpreter.Interpret(dataProviderMock);

		AssertEquals(true, interpretation.EndsWith(expectedInterpretation));
	}

	public void TestHeader()
	{
		var transitOperationMock = new Mock<ICC025CTransitOperation>();
		var dataProvider = Mock.Of<IIE025>(x =>
			x.TransitOperation == transitOperationMock.Object);
		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);

		CombineAssertions("Header is displayed if release indicator equals 1, 2, 3, 4", () =>
		{
			foreach ((string testIndicator, string expectedHeader) in new[]
				{
					( "1", "IE025 - Full Release Of Goods From Transit Procedure" ),
					( "2", "IE025 - Partial Release Of Goods From Transit Procedure" ),
					( "3", "IE025 - Partial Release Of Goods From Transit Procedure" ),
					( "4", "IE025 - No Release Of Goods From Transit Procedure" ),
				})
			{
				transitOperationMock.Setup(x => x.ReleaseIndicator).Returns(testIndicator);
				AssertLineExists(interpreter, dataProvider, $"<h2>{expectedHeader}</h2><hr />");
			}
		});

		CombineAssertions("Header is not displayed for wrong release indicators", () =>
		{
			foreach ((string testIndicator, string description) in new[]
				{
					( null, "Release indicator is null" ),
					( string.Empty, "Release indicator is empty" ),
					( "5", "Release indicator is out of range" ),
				})
			{
				transitOperationMock.Setup(x => x.ReleaseIndicator).Returns(testIndicator);
				AssertLineExists(interpreter, dataProvider, "</h2><hr />", isExisted: false);
			}
		});
	}

	public void TestReleaseDate()
	{
		var testReleaseDate = DateTime.Now;
		var dataProviderMock = Mock.Of<IIE025>(x =>
			x.TransitOperation == Mock.Of<ICC025CTransitOperation>(t =>
				t.ReleaseDate == testReleaseDate));
		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		string interpretation = interpreter.Interpret(dataProviderMock);
		AssertEquals(expected: testReleaseDate.ToShortDateString(), actual: interpretation.GetTextBetween("<tr><th>Release Date</th><td>", "</td></tr>"));
	}

	public void TestReleaseIndicator() => CombineAssertions(() =>
	{
		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var transitOperationMock = new Mock<ICC025CTransitOperation>();
		var dataProviderMock = Mock.Of<IIE025>(x => x.TransitOperation == transitOperationMock.Object);

		const string testIndicator = "1";
		const string valueWithoutDescription = "9999999999";
		const string testIndicatorDescription = "Test Release Indicator Description";
		InitializeCodeAndDescriptionForCodeType(testIndicator, testIndicatorDescription, RefCusCodeListType.CL164);

		foreach ((string description, string testValue, string expected) in new[]
			{
				("Description for the value is defined in CL164", testIndicator, $"{testIndicator} - {testIndicatorDescription}"),
				("Description for the value is not defined in CL164", valueWithoutDescription, valueWithoutDescription),
				("Release indicator equals null", null, string.Empty),
			})
		{
			transitOperationMock.Setup(x => x.ReleaseIndicator).Returns(testValue);
			string interpretation = interpreter.Interpret(dataProviderMock);
			var actual = interpretation.GetTextBetween("<tr><th>Release Indicator</th><td>", "</td></tr>");
			AssertEquals(description , expected, actual);
		}

		void InitializeCodeAndDescriptionForCodeType(string code, string codeDescription, string codeType)
		{
			var helper = new UniversalReferenceTestDataHelper(nctsHeader.Factory);
			var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var plGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: eunGrouping);
			helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", eunGrouping.ZZZ_DataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(plGrouping.ZZZ_DataGrouping, codeType, code, codeDescription, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
			Factory.Save();
		}
	});

	public void TestCustomsOfficeOfDestination() => CombineAssertions(() =>
	{
		const string testCode = "PL1234";
		const string testCodeDescription = "Test Office Of Destination";
		const string testCodeWithoutDescription = "PL4321";
		Factory.CreateCustomOfficesForTest((code: testCode, description: testCodeDescription));
		Factory.Save();

		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);

		foreach ((string description, string testValue, string expected) in new[]
			{
				("Office is not specified", null,string.Empty),
				("Description is not defined", testCodeWithoutDescription, testCodeWithoutDescription),
				("Description is defined", testCode, $"{testCode} - {testCodeDescription}"),
			})
		{
			var dataProviderMock = Mock.Of<IIE025>(x => x.CustomsOfficeOfDestinationActual == testValue);
			string interpretation = interpreter.Interpret(dataProviderMock);
			var actual = interpretation.GetTextBetween("<tr><th>Customs Office Of Destination</th><td>", "</td></tr>");
			AssertEquals(description, expected, actual);
		}
	});

	public void TestDestinationTraderCreated() => CombineAssertions(() =>
	{
		const string testDestinationTrader = "DESTINATION_TRADER";
		var transitOperationMock = new Mock<ICC025CTransitOperation>();
		var dataProvider = Mock.Of<IIE025>(x =>
			x.TransitOperation == transitOperationMock.Object &&
			x.TraderAtDestination == testDestinationTrader);
		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);

		foreach ((string testIndicator, bool expected) in new[]
			{
				( null, false),
				( "1", true),
				( "2", false),
				( "3", false),
				( "4", true),
				( "5", false),
			})
		{
			transitOperationMock.Setup(x => x.ReleaseIndicator).Returns(testIndicator);
			AssertLineExists(interpreter, dataProvider, testDestinationTrader, isExisted: expected, description: $"For release indicator {testIndicator}");
		}
	});

	public void TestPartialRelease() => CombineAssertions(() =>
	{
		var houseConsignmentData = new[]
		{
			("HOUSE_CONS_1", new [] {
				("CONS_ITEM_11", 111),
				("CONS_ITEM_12", 121)
			}),
			("HOUSE_CONS_2", new [] {
				("CONS_ITEM_21", 211),
				("CONS_ITEM_22", 221)
			}),
		};
		var interpreter = new CC025CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var transitOperationMock = new Mock<ICC025CTransitOperation>();
		var dataProviderMock = Mock.Of<IIE025>(x =>
			x.TransitOperation == transitOperationMock.Object &&
			x.Consignment == houseConsignmentData
				.Select(houseData =>
					Mock.Of<ICC025CHouseConsignment>(houseConsignment =>
						houseConsignment.SequenceNumber == houseData.Item1 &&
						houseConsignment.ConsignmentItem == houseData.Item2
							.Select(itemData =>
								Mock.Of<ICC025CConsignmentItem>(consignmentItem =>
									consignmentItem.GoodsItemNumber == itemData.Item1 &&
									consignmentItem.Packaging == new[] { Mock.Of<IPackaging>(p => p.NumberOfPackages == itemData.Item2) }))
							.ToArray()))
				.ToArray());

		foreach ((var description, var testIndicator, var expectedIsVisible) in new[]
			{
				("Indicator is 1", "1", false),
				("Indicator is 2", "2", true),
				("Indicator is 3", "3", true),
				("Indicator is 4", "4", false),
				("Indicator is out of range", "5", false),
				("Indicator is not defined", null, false),
			})
		{
			transitOperationMock.Setup(x => x.ReleaseIndicator).Returns(testIndicator);
			string interpretation = interpreter.Interpret(dataProviderMock);

			AssertEquals($"{description}: Partial Release caption", expectedIsVisible, interpretation.Contains("Partial Release"));
			foreach ((var sequenceNum, var consignmentItems) in houseConsignmentData)
			{
				AssertEquals($"{description}: House Sequence Number  for {sequenceNum}", expectedIsVisible, interpretation.Contains(sequenceNum));
				foreach ((var itemNumber, var packaging) in consignmentItems)
				{
					AssertEquals($"{description}: Consignment item for [{sequenceNum}:{itemNumber}:{packaging}]",
						expectedIsVisible,
						interpretation.Contains($"<tr><th>{sequenceNum}</th><td>{itemNumber} = {packaging}</td>"));
				}
			}
		}
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
	}

	NctsHeader nctsHeader;
}
