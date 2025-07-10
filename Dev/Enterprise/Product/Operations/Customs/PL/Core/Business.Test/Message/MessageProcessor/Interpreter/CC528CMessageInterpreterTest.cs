using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC528CMessageInterpreter))]
sealed class CC528CMessageInterpreterTest : MessageInterpreterTest<ICC528C>
{
	public void TestInterpret()
	{
		const string testIdentifier = "TST_IDENTIFIER";
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		var acceptanceDate = new DateTime(2000, 05, 30);

		const string expectedInterpretation = GlobalHtmlStyle +
"<h2>CC528C - Export MRN Allocation</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>Linked by message Identification</th><td>{testIdentifier}</td></tr>" +
		$"<tr><th>LRN</th><td>{testLrn}</td></tr>" +
		$"<tr><th>MRN</th><td>{testMrn}</td></tr>" +
		"<tr><th>Declaration is accepted on</th><td>30/05/2000</td></tr>" +
		$"<tr><th>Customs Office of Export</th><td></td></tr>" +
	"</tbody>" +
"</table>";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC528CMessageInterpreter(entryHeader);

		var dataProvider = Mock.Of<ICC528C>(x =>
			x.CorrelationIdentifier == testIdentifier &&
			x.LRN == testLrn &&
			x.MRN == testMrn &&
			x.DeclarationAcceptanceDate == acceptanceDate);

		AssertInterpret(interpreter, dataProvider, expectedInterpretation);
	}

	public void TestInterpretCustomsOfficeOfExport()
	{
		const string testCustomsOfficeReferenceNumber = "CUS_OFFICE_REF_NUM";
		var dataProvider = Mock.Of<ICC528C>(x =>
			x.CustomsOfficeOfExport == Mock.Of<ICustomsOffice>(o =>
				o.ReferenceNumber == testCustomsOfficeReferenceNumber));

		var entryHeader = Factory.New<CusEntryHeader>();
		string interpretation = new CC528CMessageInterpreter(entryHeader).Interpret(dataProvider);

		AssertEquals(testCustomsOfficeReferenceNumber, interpretation.GetTextBetween("<tr><th>Customs Office of Export</th><td>", "</td></tr>"));
	}
}
