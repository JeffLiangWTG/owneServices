using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC531CMessageInterpreter))]
sealed class CC531CMessageInterpreterTest : MessageInterpreterTest<ICC531C>
{
	public void TestInterpret()
	{
		const string testMrn = "TST_MRN";
		const string testCustomsOfficeOfExportReferenceNumber = "TEST_CustomsOfficeOfExportReferenceNumber";
		const string testTimerExpiryInformation = "TEST_TimerExpiryInformation";
		var testAcceptanceDate = new DateTime(2024, 12, 23);

		const string expectedInterpretation = GlobalHtmlStyle +
			"<h2>CC531C - Extended deadline for submitting a supplementary declaration</h2><hr />" +
			"<table>" +
				"<tbody>" +
				   $"<tr><th>Customs office of export sending the message</th><td>{testCustomsOfficeOfExportReferenceNumber}</td></tr>" +
				   $"<tr><th>MRN</th><td>{testMrn}</td></tr>" +
				   $"<tr><th>Lodgement of supplementary declaration start date</th><td>23/12/2024</td></tr>" +
				   $"<tr><th>Lodgement of supplementary declaration expiry date</th><td>23/12/2024</td></tr>" +
				   $"<tr><th>Timer expiry information</th><td>{testTimerExpiryInformation}</td></tr>" +
				"</tbody>" +
			"</table>";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC531CMessageInterpreter(entryHeader);

		string interpretation = interpreter.Interpret(Mock.Of<ICC531C>(x =>
			x.CustomsOfficeOfExportReferenceNumber == testCustomsOfficeOfExportReferenceNumber &&
			x.MRN == testMrn &&
			x.TimerExpiryForSupplementaryDeclaration.LodgementOfSupplementaryDeclarationStartDate == testAcceptanceDate &&
			x.TimerExpiryForSupplementaryDeclaration.LodgementOfSupplementaryDeclarationExpiryDate == testAcceptanceDate &&
			x.TimerExpiryForSupplementaryDeclaration.TimerExpiryInformation == testTimerExpiryInformation
			));

		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestCustomsOfficeOfExportReferenceNumber()
	{
		const string testCustomsOfficeOfExportReferenceNumber = "TEST_CustomsOfficeOfExportReferenceNumber";
		var dataProvider = Mock.Of<ICC531C>(o => o.CustomsOfficeOfExportReferenceNumber == testCustomsOfficeOfExportReferenceNumber);
		var expectedLine = $"<tr><th>Customs office of export sending the message</th><td>{testCustomsOfficeOfExportReferenceNumber}</td></tr>";
		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC531CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedLine);
	}

	public void TestMRN()
	{
		const string testMRN = "TEST_mrn";
		var dataProvider = Mock.Of<ICC531C>(o => o.MRN == testMRN);
		var expectedLine = $"<tr><th>MRN</th><td>{testMRN}</td></tr>";
		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC531CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedLine);
	}

	public void TestLodgementOfSupplementaryDeclarationStartDate()
	{
		var testDateTime = DateTime.Now;
		var dataProvider = Mock.Of<ICC531C>(x =>
			x.TimerExpiryForSupplementaryDeclaration == Mock.Of<ICC531CTimerExpiryForSupplementaryDeclaration>(o =>
				o.LodgementOfSupplementaryDeclarationStartDate == testDateTime));
		var expectedLine = $"<tr><th>Lodgement of supplementary declaration start date</th><td>{testDateTime.ToShortDateString()}</td></tr>";
		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC531CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedLine);
	}

	public void TestLodgementOfSupplementaryDeclarationExpiryDate()
	{
		var testDateTime = DateTime.Now;
		var dataProvider = Mock.Of<ICC531C>(x =>
			x.TimerExpiryForSupplementaryDeclaration == Mock.Of<ICC531CTimerExpiryForSupplementaryDeclaration>(o =>
				o.LodgementOfSupplementaryDeclarationExpiryDate == testDateTime));
		var expectedLine = $"<tr><th>Lodgement of supplementary declaration expiry date</th><td>{testDateTime.ToShortDateString()}</td></tr>";
		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC531CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedLine);
	}

	public void TestTimerExpiryInformation()
	{
		var testTimerExpiryInformation = "TST_TimerExpiryInformation";
		var dataProvider = Mock.Of<ICC531C>(x =>
			x.TimerExpiryForSupplementaryDeclaration == Mock.Of<ICC531CTimerExpiryForSupplementaryDeclaration>(o =>
				o.TimerExpiryInformation == testTimerExpiryInformation));
		var expectedLine = $"<tr><th>Timer expiry information</th><td>{testTimerExpiryInformation}</td></tr>";
		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC531CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedLine);
	}
}
