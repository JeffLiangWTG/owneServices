using System.Collections.ObjectModel;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC182CMessageInterpreter))]
sealed class CC182CMessageInterpreterTest : MessageInterpreterTest<IIE182>
{
	public void TestInterpret()
	{
		const string testMrn = "TST_MRN";
		const string testMessageSentOn = "20/01/2002";
		const string testIncidentNotificationDateAndTime = "IncidentNotificationDateAndTime";
		const string testCustomsOfficeOfDepartureReferenceNumber = "CustomsOfficeOfDeparture";
		const string testCustomsOfficeOfIncidentRegistrationReferenceNumber = "CustomsOfficeOfIncidentRegistered";
		const string testIncidentLocationAndCoordinates =
@"1234<br />
Country Code: Test Country<br />
Latitude: Latitude<br />
Longitude: Longitude<br />
Street: 123 TestStreet<br />
Postcode: 123456<br />
City: Test City";

		const string testIncidentCode = "54321";
		const string testCountry = "Test Country";
		const string testUNLocode = "1234";
		const string testStreetAndNumber = "123 TestStreet";
		const string testPostcode = "123456";
		const string testCity = "Test City";
		const string testLatitude = "Latitude";
		const string testLongitude = "Longitude";
		const string testAdditionalTextInfo = "AdditionalTextInfo";

		var dataProvider = Mock.Of<IIE182>(p =>
			p.MRN == testMrn &&
			p.PreparationDateAndTime == testMessageSentOn &&
			p.IncidentNotificationDateAndTime == testIncidentNotificationDateAndTime &&
			p.CustomsOfficeOfDepartureReferenceNumber == testCustomsOfficeOfDepartureReferenceNumber &&
			p.CustomsOfficeOfIncidentRegistrationReferenceNumber == testCustomsOfficeOfIncidentRegistrationReferenceNumber &&
			p.ConsignmentIncidents == new Collection<ICC182CConsignmentIncident>(new [] { Mock.Of<ICC182CConsignmentIncident>(o =>
				o.Code == testIncidentCode &&
				o.Text == testAdditionalTextInfo &&
				o.Location == Mock.Of<ILocation>(l =>
					l.Country == testCountry &&
					l.UNLocode == testUNLocode &&
					l.Address == Mock.Of<ILocalAddress>(k =>
						k.StreetAndNumber == testStreetAndNumber &&
						k.Postcode == testPostcode &&
						k.City == testCity
					) &&
					l.GNSS == Mock.Of<IGNSS>(g =>
						g.Latitude == testLatitude &&
						g.Longitude == testLongitude
					))) }));

		var expectedInterpretation = $@"<style>table, th, td {{ border: 1px solid black; border-collapse: collapse; }} th, td {{ padding: 5px; text-align: left; }}</style>

<h2>IE182 - Incident Reported During Transit</h2>
<hr />

<table>
<tbody>
	<tr><th>MRN</th><td>TST_MRN</td></tr>
	<tr><th>Message Sent On</th><td>20/01/2002</td></tr>
	<tr><th>Incident Notification Date &amp; Time</th><td>IncidentNotificationDateAndTime</td></tr>
	<tr><th>Customs Office Of Departure</th><td>CustomsOfficeOfDeparture</td></tr>
	<tr><th>Customs Office Of Incident Registered</th><td>CustomsOfficeOfIncidentRegistered</td></tr>
</tbody>
</table>
<hr />
<table>
	<tbody>
		<tr><th>Sr.</th><th>Incident Location &amp; Co-ordinates</th><th>Incident Code</th><th>Additional Text Info</th></tr>
		<tr><td>1</td><td>{testIncidentLocationAndCoordinates}</td><td>54321</td><td>AdditionalTextInfo</td></tr>
	</tbody>
</table>
<hr />";

		var interpretation = interpreter.Interpret(dataProvider).ToString().ToSingleLineHtml();
		AssertEquals(expectedInterpretation.ToSingleLineHtml(), interpretation);
	}

	public void TestInterpret_EmptyAddressParams()
	{
		const string testMrn = "TST_MRN";
		const string testMessageSentOn = "20/01/2002";
		const string testIncidentNotificationDateAndTime = "IncidentNotificationDateAndTime";
		const string testCustomsOfficeOfDepartureReferenceNumber = "CustomsOfficeOfDeparture";
		const string testCustomsOfficeOfIncidentRegistrationReferenceNumber = "CustomsOfficeOfIncidentRegistered";

		const string testIncidentCode = "54321";
		const string testAdditionalTextInfo = "AdditionalTextInfo";

		var dataProvider = Mock.Of<IIE182>(p =>
			p.MRN == testMrn &&
			p.PreparationDateAndTime == testMessageSentOn &&
			p.IncidentNotificationDateAndTime == testIncidentNotificationDateAndTime &&
			p.CustomsOfficeOfDepartureReferenceNumber == testCustomsOfficeOfDepartureReferenceNumber &&
			p.CustomsOfficeOfIncidentRegistrationReferenceNumber == testCustomsOfficeOfIncidentRegistrationReferenceNumber &&
			p.ConsignmentIncidents == new Collection<ICC182CConsignmentIncident>(new [] { Mock.Of<ICC182CConsignmentIncident>(o =>
				o.Code == testIncidentCode &&
				o.Text == testAdditionalTextInfo &&
				o.Location == Mock.Of<ILocation>(l =>
					l.Country == string.Empty &&
					l.UNLocode == default &&
					l.Address == Mock.Of<ILocalAddress>(k =>
						k.StreetAndNumber == default &&
						k.Postcode == default &&
						k.City == default
					) &&
					l.GNSS == Mock.Of<IGNSS>(g =>
						g.Latitude == default &&
						g.Longitude == default
					))) }));

		var expectedInterpretation = @"<style>table, th, td { border: 1px solid black; border-collapse: collapse; } th, td { padding: 5px; text-align: left; }</style>

<h2>IE182 - Incident Reported During Transit</h2>
<hr />

<table>
<tbody>
	<tr><th>MRN</th><td>TST_MRN</td></tr>
	<tr><th>Message Sent On</th><td>20/01/2002</td></tr>
	<tr><th>Incident Notification Date &amp; Time</th><td>IncidentNotificationDateAndTime</td></tr>
	<tr><th>Customs Office Of Departure</th><td>CustomsOfficeOfDeparture</td></tr>
	<tr><th>Customs Office Of Incident Registered</th><td>CustomsOfficeOfIncidentRegistered</td></tr>
</tbody>
</table>
<hr />
<table>
	<tbody>
		<tr><th>Sr.</th><th>Incident Location &amp; Co-ordinates</th><th>Incident Code</th><th>Additional Text Info</th></tr>
		<tr><td>1</td><td></td><td>54321</td><td>AdditionalTextInfo</td></tr>
	</tbody>
</table>
<hr />";

		var interpretation = interpreter.Interpret(dataProvider).ToString().ToSingleLineHtml();
		AssertEquals(expectedInterpretation.ToSingleLineHtml(), interpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		interpreter = new CC182CMessageInterpreter(nctsHeader.MovementHeader);
	}

	NctsHeader nctsHeader;
	CC182CMessageInterpreter interpreter;
}
