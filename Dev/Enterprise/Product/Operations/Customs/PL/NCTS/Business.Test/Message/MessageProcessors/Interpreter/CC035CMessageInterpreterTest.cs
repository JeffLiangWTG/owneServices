using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC035CMessageInterpreter))]
sealed class CC035CMessageInterpreterTest : MessageInterpreterTest<IIE035>
{
	public void TestInterpret()
	{
		const string testMrn = "TEST_MRN";
		const string customsOfficeOfDeparture = $"{CountryCodes.Poland}2233";
		const string guarantor = "GUARANTOR_EORI";
		const string transitHolder = "TRANSIT_HOLDER_EORI";

		const string expectedInterpretation = """
			<style>table, th, td { border: 1px solid black; border-collapse: collapse; } th, td { padding: 5px; text-align: left; }</style>

			<h2>IE035 - Recovery/Collection Procedure Started</h2>
			<hr />

			<table><tbody>
				<tr><th>MRN</th><td>TEST_MRN</td></tr>
				<tr><th>Message Sent On</th><td>2024-10-31T12:00:00</td></tr>
				<tr><th>Declaration Acceptance Date</th><td>30/10/2024 12:00:00 AM</td></tr>
				<tr><th>Customs Office Of Departure</th><td>PL2233 - Test Office Of Departure</td></tr>
				<tr><th>Recovery Notification Date</th><td>10/11/2024 12:00:00 AM</td></tr>
				<tr><th>Recovery Notification Text</th><td>RecoveryNotificationText</td></tr>
				<tr><th>Recovery Amount Claimed</th><td>1200.00 PLN</td></tr>
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

		Factory.CreateCustomOfficesForTest(
			(code: $"{CountryCodes.Poland}2233", description: "Test Office Of Departure"));

		Factory.Save();

		var ie035 = Mock.Of<IIE035>(m =>
			m.MRN == testMrn &&
			m.DeclarationAcceptanceDate == new DateTime(2024, 10, 30) &&
			m.PreparationDateAndTime == "2024-10-31T12:00:00" &&
			m.CustomsOfficeOfDeparture == customsOfficeOfDeparture &&
			m.RecoveryNotification == Mock.Of<IRecoveryNotification>(r =>
				r.RecoveryNotificationDate == new DateTime(2024, 11, 10) &&
				r.RecoveryNotificationText == "RecoveryNotificationText" &&
				r.AmountClaimed == 1200.00M &&
				r.Currency == "PLN") &&
			m.Guarantor == Mock.Of<IGuarantor>(g =>
				g.Name == "Guarantor Name" &&
				g.IdentificationNumber == guarantor &&
				g.Address == Mock.Of<IAddress>(a =>
					a.StreetAndNumber == "ul. Radosna 5" &&
					a.City == "Wroclaw" &&
					a.CountryCode == "PL" &&
					a.PostCode == "50-000")) &&
			m.HolderOfTheTransitProcedure == Mock.Of<IHolderOfTheTransitProcedure>(h =>
				h.Name == "Holder of Transit Procedure Name" &&
				h.IdentificationNumber == transitHolder &&
				h.TIRHolderIdentificationNumber == "TIR_ID" &&
				h.Address == Mock.Of<IAddress>(a =>
					a.StreetAndNumber == "ul. Jasna 10" &&
					a.City == "Warsaw" &&
					a.PostCode == "40-000" &&
					a.CountryCode == "PL"))
		);

		var interpreter = new CC035CMessageInterpreter(nctsHeader.MovementHeader);
		var interpretation = interpreter.Interpret(ie035);

		AssertEquals(expectedInterpretation.ToSingleLineHtml(), interpretation);
	}

	protected override void SetUp()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;
}
