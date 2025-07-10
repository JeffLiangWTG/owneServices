using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC556CMessageInterpreter))]
sealed class CC556CMessageInterpreterTest : MessageInterpreterTest<ICC556C>
{
	public void TestInterpret()
	{
		const string businessRejectionType = "BusinessRejectionType";
		const string rejectionReason = "RejectionReason";
		const string customsOfficeOfExportReferenceNumber = "CustomsOfficeOfExportReferenceNumber";
		const string mrn = "MRN";
		const string lrn = "LRN";
		const string correlationIdentifier = "CorrelationIdentifier";
		const string rejectionDateAndTime = "RejectionDateAndTime";
		const string rejectionCode = "RejectionCode";

		const string expectedInterpretation = GlobalHtmlStyle +
$"<h2>CC556 - Rejection of the customs declaration or its amendment</h2>" +
"<hr />" +
"<table>" +
	$"<caption><h3>CC{businessRejectionType}-{rejectionReason}</h3></caption>" +
	"<tbody>" +
		$"<tr><th>Customs office of export rejecting the message</th><td>{customsOfficeOfExportReferenceNumber}</td></tr>" +
		$"<tr><th>LRN</th><td>{lrn}</td></tr>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>The message {businessRejectionType} received rejection [CC556C] from Customs</h3></caption>" +
	"<tbody>" +
		$"<tr><th>1. Rejected Message identification</th><td>{correlationIdentifier}</td></tr>" +
		$"<tr><th>2. Type of Business rejection</th><td>{businessRejectionType}</td></tr>" +
		$"<tr><th>3. Rejection date and time</th><td>{rejectionDateAndTime}</td></tr>" +
		$"<tr><th>4. Rejection Code</th><td>{rejectionCode}</td></tr>" +
		$"<tr><th>5. Rejection Reason</th><td>{rejectionReason}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC556CMessageInterpreter(entryHeader);
		var exportOperation = new Mock<ICC556CExportOperation>();

		var dataProvider = Mock.Of<ICC556C>(p =>
			p.CustomsOfficeOfExportReferenceNumber == customsOfficeOfExportReferenceNumber &&
			p.LRN == lrn &&
			p.MRN == mrn &&
			p.CorrelationIdentifier == correlationIdentifier &&
			p.ExportOperation == Mock.Of<ICC556CExportOperation>(o =>
				o.BusinessRejectionType == businessRejectionType &&
				o.RejectionReason == rejectionReason &&
				o.RejectionDateAndTime == rejectionDateAndTime &&
				o.RejectionCode == rejectionCode)
		);

		exportOperation.Setup(x => x.BusinessRejectionType).Returns(businessRejectionType);
		exportOperation.Setup(x => x.RejectionReason).Returns(rejectionReason);
		exportOperation.Setup(x => x.RejectionDateAndTime).Returns(rejectionDateAndTime);
		exportOperation.Setup(x => x.RejectionCode).Returns(rejectionCode);

		dataProviderMock.Setup(x => x.CustomsOfficeOfExportReferenceNumber).Returns(customsOfficeOfExportReferenceNumber);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperation.Object);

		AssertInterpret(interpreter, dataProvider, expectedInterpretation);
	}

	public void TestInterpretWithFunctionalErrors()
	{
		const string businessRejectionType = "BusinessRejectionType";
		const string rejectionReason = "RejectionReason";
		const string customsOfficeOfExportReferenceNumber = "CustomsOfficeOfExportReferenceNumber";
		const string mrn = "MRN";
		const string lrn = "LRN";
		const string correlationIdentifier = "CorrelationIdentifier";
		const string rejectionDateAndTime = "RejectionDateAndTime";
		const string rejectionCode = "RejectionCode";
		const string errorPointer = "ErrorPointer";
		const int errorCode = 0;
		const string errorReason = "ErrorReason";
		const string originalAttributeValue = "OriginalAttributeValue";

		const string expectedInterpretation = GlobalHtmlStyle +
$"<h2>CC556 - Rejection of the customs declaration or its amendment</h2>" +
"<hr />" +
"<table>" +
	$"<caption><h3>CC{businessRejectionType}-{rejectionReason}</h3></caption>" +
	"<tbody>" +
		$"<tr><th>Customs office of export rejecting the message</th><td>{customsOfficeOfExportReferenceNumber}</td></tr>" +
		$"<tr><th>LRN</th><td>{lrn}</td></tr>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>The message {businessRejectionType} received rejection [CC556C] from Customs</h3></caption>" +
	"<tbody>" +
		$"<tr><th>1. Rejected Message identification</th><td>{correlationIdentifier}</td></tr>" +
		$"<tr><th>2. Type of Business rejection</th><td>{businessRejectionType}</td></tr>" +
		$"<tr><th>3. Rejection date and time</th><td>{rejectionDateAndTime}</td></tr>" +
		$"<tr><th>4. Rejection Code</th><td>{rejectionCode}</td></tr>" +
		$"<tr><th>5. Rejection Reason</th><td>{rejectionReason}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	"<caption><h3>Functional errors</h3></caption>" +
	"<tbody>" +
		$"<tr><th>No.</th><th>Error Pointer</th><th>Error Code</th><th>Error Reason</th><th>Original Attribute Value</th></tr>" +
		$"<tr><td>1</td><td>{errorPointer}1</td><td>1</td><td>{errorReason}1</td><td>{originalAttributeValue}1</td></tr>" +
		$"<tr><td>2</td><td>{errorPointer}2</td><td>2</td><td>{errorReason}2</td><td>{originalAttributeValue}2</td></tr>" +
	"</tbody>" +
"</table>";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC556CMessageInterpreter(entryHeader);
		var dataProvider = Mock.Of<ICC556C>(p =>
			p.CustomsOfficeOfExportReferenceNumber == customsOfficeOfExportReferenceNumber &&
			p.LRN == lrn &&
			p.MRN == mrn &&
			p.CorrelationIdentifier == correlationIdentifier &&
			p.ExportOperation == Mock.Of<ICC556CExportOperation>(o =>
				o.BusinessRejectionType == businessRejectionType &&
				o.RejectionReason == rejectionReason &&
				o.RejectionDateAndTime == rejectionDateAndTime &&
				o.RejectionCode == rejectionCode) &&
			p.FunctionalErrors == new[]
			{
				Mock.Of<IFunctionalError>(f =>
					f.ErrorPointer == errorPointer + "1" &&
					f.ErrorCode == errorCode + 1 &&
					f.ErrorReason == errorReason + "1" &&
					f.OriginalAttributeValue == originalAttributeValue + "1"
				),
				Mock.Of<IFunctionalError>(f =>
					f.ErrorPointer == errorPointer + "2" &&
					f.ErrorCode == errorCode + 2 &&
					f.ErrorReason == errorReason + "2" &&
					f.OriginalAttributeValue == originalAttributeValue + "2"
				),
			}
		);

		AssertInterpret(interpreter, dataProvider, expectedInterpretation);
	}
}

