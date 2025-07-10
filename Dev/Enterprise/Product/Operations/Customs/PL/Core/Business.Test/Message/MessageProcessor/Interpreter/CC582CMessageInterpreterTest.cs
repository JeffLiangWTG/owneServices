using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC582CMessageInterpreter))]
sealed class CC582CMessageInterpreterTest : MessageInterpreterTest<ICC582C>
{
	public void TestInterpret()
	{
		const string customsOfficeOfExport = "CustomsOfficeOfExport";
		const string mrn = "MRN";
		var requestOnNonExitedExportDate = new DateTime(2024, 01, 01);
		var limitForResponseDate = new DateTime(2024, 01, 29);

		const string expectedInterpretation = GlobalHtmlStyle +
"<h2>CC582C - Query on not exited goods (not finished export operations)</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>Customs Office of Export</th><td>{customsOfficeOfExport}</td></tr>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
		"<tr><th>Request on non-exited export date</th><td>01-Jan-24 00:00:00</td></tr>" +
		"<tr><th>Limit for response date</th><td>29-Jan-24 00:00:00</td></tr>" +
	"</tbody>" +
"</table>";

		var interpreter = new CC582CMessageInterpreter(Factory.New<CusEntryHeader>());
		var exportOperation = Mock.Of<ICC582CExportOperation>(x =>
		x.RequestOnNonExitedExportDate == requestOnNonExitedExportDate &&
		x.LimitForResponseDate == limitForResponseDate);

		dataProviderMock.Setup(x => x.CustomsOfficeOfExportReferenceNumber).Returns(customsOfficeOfExport);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperation);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedInterpretation);
	}
}
