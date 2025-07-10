using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.NCTS.Business.Testing.CC029TestHelper;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC029CMessageInterpreter))]
sealed class CC029CMessageInterpreterTest : MessageInterpreterTest<IIE029>
{
	public void TestInterpret()
	{
		const string expectedInterpretation = @"
<style>table, th, td { border: 1px solid black; border-collapse: collapse; } th, td { padding: 5px; text-align: left; }</style>

<h2>IE029 - Released For Transit</h2>
<hr />

<table><tbody>
	<tr><th>LRN</th><td>TST_LRN</td></tr>
	<tr><th>MRN</th><td>TST_MRN</td></tr>
	<tr><th>Message Sent On</th><td>TST_PreparationDateAndTime</td></tr>
	<tr><th>Customs Office Of Departure</th><td>PL2233 - Test Office Of Departure</td></tr>
	<tr><th>Customs Office Of Destination</th><td>PL4455 - Test Office Of Destination</td></tr>
	<tr><th>Declaration Type</th><td>TST_DeclarationType</td></tr>
	<tr><th>TIR Carnet Number</th><td>TST_TIRCarnetNumber</td></tr>
	<tr><th>Release Date</th><td>12-Feb-24 00:00:00</td></tr>
	<tr><th>Gross Weight</th><td>12</td></tr>
</tbody></table>
<hr />

<p><h3>Representative</h3>
	EORI: TST_Representative_EORI
</p>
<hr />

<p><h3>Holder of the Transit Procedure</h3>
	EORI: TST_HolderOfTheTransitProcedure_EORI<br />
	TIR Holder Identification Number: TST_TIRHolderIdentificationNumber<br />
	Name: TST_HolderOfTheTransitProcedure_Name<br />
	Street &amp; Address: TST_StreetAndNumber<br />
	Postcode: TST_PostCode<br />
	City: TST_City<br />
	Country: TST_CountryCode
</p>";

		var interpretation = GetInterpretation();
		AssertEquals(expectedInterpretation.ToSingleLineHtml(), interpretation);
	}

	public void TestOfficesNotFound()
	{
		var interpretation = GetInterpretation(mocks =>
		{
			mocks.IE029.Setup(m => m.CustomsOfficeOfDeparture).Returns($"{CountryCodes.Denmark}1122");
			mocks.IE029.Setup(m => m.CustomsOfficeOfDestinationDeclared).Returns($"{CountryCodes.Poland}9988");
		});

		CombineAssertions(() =>
		{
			AssertParamValue("Customs Office Of Departure", "DK1122");
			AssertParamValue("Customs Office Of Destination", "PL9988");
		});
		return;

		void AssertParamValue(string name, string expectedValue)
		{
			var actualValue = interpretation.GetTextBetween($"<tr><th>{name}</th><td>", "</td></tr>");
			AssertEquals(name, expectedValue, actualValue);
		}
	}

	public void TestEmpty_HolderOfTheTransitProcedure_Name() => CombineAssertions(() =>
	{
		AssertWithName("null organisation", testHolderOfTheTransitProcedureName: null, expectedInterpretationValue: string.Empty);

		NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "PC1", nctsHeader.Principal, traderName: "TST_ORG_NAME", suffix: string.Empty);
		AssertWithName("null value", testHolderOfTheTransitProcedureName: null, expectedInterpretationValue: "TST_ORG_NAME");
		AssertWithName("empty value", testHolderOfTheTransitProcedureName: string.Empty, expectedInterpretationValue: "TST_ORG_NAME");

		return;

		void AssertWithName(string testDescription, string testHolderOfTheTransitProcedureName, string expectedInterpretationValue)
		{
			var interpretation = GetInterpretation(mocks =>
			{
				mocks.HolderOfTheTransitProcedure.Setup(x => x.Name).Returns(testHolderOfTheTransitProcedureName);
			});

			var sequenceContent = interpretation.GetTextBetween("<p><h3>Holder of the Transit Procedure</h3>", "</p>");
			var actualValue = sequenceContent.GetTextBetween("Name: ", "<");
			AssertEquals(testDescription, expectedInterpretationValue, actualValue);
		}
	});

	public void TestNull_HolderOfTheTransitProcedure_Address() => CombineAssertions(() =>
	{
		var holderOfTheTransitProcedureInterpretation = GetInterpretation(mocks =>
		{
			mocks.HolderOfTheTransitProcedure.Setup(x => x.Address).Returns((IAddress)null);
		}).GetTextBetween("<p><h3>Holder of the Transit Procedure</h3>", "</p>");
		const string expectedNullAddressInterpretation = @"
	EORI: TST_HolderOfTheTransitProcedure_EORI<br />
	TIR Holder Identification Number: TST_TIRHolderIdentificationNumber<br />
	Name: TST_HolderOfTheTransitProcedure_Name<br />
	Street &amp; Address: <br />
	Postcode: <br />
	City: <br />
	Country:";
		AssertEquals("Null provider address, null principal address", expectedNullAddressInterpretation.ToSingleLineHtml(), holderOfTheTransitProcedureInterpretation);

		NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "PC1", nctsHeader.Principal, traderName: "TST_ORG_NAME", suffix: string.Empty, city: "TST_Principal_City", countryCode: CountryCodes.Denmark);
		nctsHeader.Principal.E2_AddressOverride = true;
		nctsHeader.Principal.Address1 = "TST_address1";
		nctsHeader.Principal.Address2 = "TST_address2";
		nctsHeader.Principal.Postcode = "P_Postcode";
		holderOfTheTransitProcedureInterpretation = GetInterpretation(mocks =>
		{
			mocks.HolderOfTheTransitProcedure.Setup(x => x.Address).Returns((IAddress)null);
		}).GetTextBetween("<p><h3>Holder of the Transit Procedure</h3>", "</p>");
		const string expectedNullAddressInterpretation_PrincipalValues = @"
	EORI: TST_HolderOfTheTransitProcedure_EORI<br />
	TIR Holder Identification Number: TST_TIRHolderIdentificationNumber<br />
	Name: TST_HolderOfTheTransitProcedure_Name<br />
	Street &amp; Address: TST_address1 TST_address2<br />
	Postcode: P_Postcode<br />
	City: TST_Principal_City<br />
	Country: DK";
		AssertEquals("Null provider address, principal values used", expectedNullAddressInterpretation_PrincipalValues.ToSingleLineHtml(), holderOfTheTransitProcedureInterpretation);
	});

	string GetInterpretation(Action<CC029Mocks> mocksModificationAction = null)
	{
		var mocks = SetupDataProviderMock(dataProviderMock);
		mocksModificationAction?.Invoke(mocks);

		var interpreter = new CC029CMessageInterpreter(nctsHeader.MovementHeader);
		return interpreter.Interpret(dataProviderMock.Object);
	}

	protected override void SetUp()
	{
		base.SetUp();

		Factory.CreateCustomOfficesForTest(
			(code: $"{CountryCodes.Poland}2233", description: "Test Office Of Departure"),
			(code: $"{CountryCodes.Poland}4455", description: "Test Office Of Destination"));
		Factory.Save();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Departure;
	}

	NctsHeader nctsHeader;
}
