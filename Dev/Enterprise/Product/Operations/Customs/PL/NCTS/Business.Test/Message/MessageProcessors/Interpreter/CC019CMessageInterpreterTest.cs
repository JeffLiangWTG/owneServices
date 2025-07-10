using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC019CMessageInterpreter))]
sealed class CC019CMessageInterpreterTest : MessageInterpreterTest<IIE019>
{
	const string TestMrn = "TEST_MRN";
	const string CustomsOfficeOfDeparture = $"{Core.Constants.CountryCodes.Poland}2233";
	const string Guarantor = "GUARANTOR_EORI";
	const string TransitHolder = "TRANSIT_HOLDER_EORI";

	public void TestInterpret()
	{
		const string expectedInterpretation =
			"""
			<style>table, th, td { border: 1px solid black; border-collapse: collapse; } th, td { padding: 5px; text-align: left; }</style>

			<h2>IE019 - Major Discrepancies Observed During Checking At Destination</h2>
			<hr />

			<table><tbody>
				<tr><th>MRN</th><td>TEST_MRN</td></tr>
				<tr><th>Message Sent On</th><td>2024-10-31</td></tr>
				<tr><th>Discrepancies Notification Date</th><td>30-Oct-24 00:00:00</td></tr>
				<tr><th>Discrepancies Notification Text</th><td>Discrepancies Notification Text</td></tr>
				<tr><th>Customs Office Of Departure</th><td>PL2233 - Test Office Of Departure</td></tr>
			</tbody></table>
			<hr />

			<p><h3>Guarantor</h3>
				EORI: GUARANTOR_EORI<br />
				Name: Guarantor Name<br />
				Street &amp; Address: ul. Radosna 5<br />
				Postcode: 50-000<br />
				City: Wroclaw<br />
				Country: PL
			</p>
			<hr />

			<p><h3>Holder of the Transit Procedure</h3>
				EORI: TRANSIT_HOLDER_EORI<br />
				TIR Holder Identification Number: TIR_ID<br />
				Name: Holder of Transit Procedure Name<br />
				Street &amp; Address: ul. Jasna 10<br />
				Postcode: 40-000<br />
				City: Warsaw<br />
				Country: PL
			</p>
			""";

		Factory.CreateCustomOfficesForTest((code: CustomsOfficeOfDeparture, description: "Test Office Of Departure"));

		Factory.Save();

		var ie019 = Mock.Of<IIE019>(m =>
			m.MRN == TestMrn &&
			m.PreparationDateAndTime == "2024-10-31" &&
			m.TransitOperation == Mock.Of<ICC019CTransitOperation>(t =>
				t.DiscrepanciesNotificationDate == new DateTime(2024, 10, 30) &&
				t.DiscrepanciesNotificationText == "Discrepancies Notification Text") &&
			m.CustomsOfficeOfDeparture == CustomsOfficeOfDeparture &&
			m.Guarantor == Mock.Of<IGuarantor>(g =>
				g.IdentificationNumber == Guarantor &&
				g.Name == "Guarantor Name" &&
				g.Address == Mock.Of<IAddress>(ga =>
					ga.StreetAndNumber == "ul. Radosna 5" &&
					ga.City == "Wroclaw" &&
					ga.CountryCode == "PL" &&
					ga.PostCode == "50-000")) &&
			m.HolderOfTheTransitProcedure == Mock.Of<IHolderOfTheTransitProcedure>(h =>
				h.IdentificationNumber == TransitHolder &&
				h.Name == "Holder of Transit Procedure Name" &&
				h.TIRHolderIdentificationNumber == "TIR_ID" &&
				h.Address == Mock.Of<IAddress>(ha =>
					ha.StreetAndNumber == "ul. Jasna 10" &&
					ha.City == "Warsaw" &&
					ha.CountryCode == "PL" &&
					ha.PostCode == "40-000")));

		var interpreter = new CC019CMessageInterpreter(nctsHeader.ArrivalMovementHeader);
		var interpretation = interpreter.Interpret(ie019);

		AssertEquals(expectedInterpretation.ToSingleLineHtml(), interpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
	}

	NctsHeader nctsHeader;
}

