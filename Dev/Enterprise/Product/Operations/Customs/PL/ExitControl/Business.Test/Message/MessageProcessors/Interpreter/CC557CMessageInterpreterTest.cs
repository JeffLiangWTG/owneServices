using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Testing;
using Moq;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC557CMessageInterpreterTest : MessageInterpreterTest<ICC557C>
{
	public void TestInterpret()
	{
		const string businessRejectionType = nameof(businessRejectionType);
		const string rejectionReason = nameof(rejectionReason);
		const string customsOfficeOfExitReferenceNumber = nameof(customsOfficeOfExitReferenceNumber);
		const string mrn = nameof(mrn);
		const string lrn = nameof(lrn);
		const string correlationIdentifier = nameof(correlationIdentifier);
		const string rejectionDateAndTime = nameof(rejectionDateAndTime);
		const string rejectionCode = nameof(rejectionCode);

		const string expectedInterpretation = GlobalHtmlStyle +
$"<h2>CC557 - Rejection from the customs office of exit</h2>" +
"<hr />" +
"<table>" +
	$"<caption><h3>CC{businessRejectionType}-{rejectionReason}</h3></caption>" +
	"<tbody>" +
		$"<tr><th>Customs Office of Exit</th><td>{customsOfficeOfExitReferenceNumber}</td></tr>" +
		$"<tr><th>LRN</th><td>{lrn}</td></tr>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>The message {businessRejectionType} received rejection [CC557C] from Customs</h3></caption>" +
	"<tbody>" +
		$"<tr><th>1. Rejected Message identification</th><td>{correlationIdentifier}</td></tr>" +
		$"<tr><th>2. Type of Business rejection</th><td>{businessRejectionType}</td></tr>" +
		$"<tr><th>3. Rejection date and time</th><td>{rejectionDateAndTime}</td></tr>" +
		$"<tr><th>4. Rejection Code</th><td>{rejectionCode}</td></tr>" +
		$"<tr><th>5. Rejection Reason</th><td>{rejectionReason}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />";

		var interpreter = new CC557CMessageInterpreter(Factory.New<CusExitReport>());
		var dataProvider = Mock.Of<ICC557C>(p =>
			p.LRN == lrn &&
			p.MRN == mrn &&
			p.CorrelationIdentifier == correlationIdentifier &&
			p.ExportOperation == Mock.Of<ICC557CExportOperation>(o =>
				o.BusinessRejectionType == businessRejectionType &&
				o.RejectionReason == rejectionReason &&
				o.RejectionDateAndTime == rejectionDateAndTime &&
				o.RejectionCode == rejectionCode) &&
			p.CustomsOfficeOfExitActual == Mock.Of<ICustomsOffice>(o =>
				o.ReferenceNumber == customsOfficeOfExitReferenceNumber));

		AssertInterpret(interpreter, dataProvider, expectedInterpretation);
	}

	public void TestInterpretWithFunctionalErrors()
	{
		const string businessRejectionType = nameof(businessRejectionType);
		const string rejectionReason = nameof(rejectionReason);
		const string customsOfficeOfExitReferenceNumber = nameof(customsOfficeOfExitReferenceNumber);
		const string mrn = nameof(mrn);
		const string lrn = nameof(lrn);
		const string correlationIdentifier = nameof(correlationIdentifier);
		const string rejectionDateAndTime = nameof(rejectionDateAndTime);
		const string rejectionCode = nameof(rejectionCode);
		const string errorPointer = nameof(errorPointer);
		const int errorCode = 0;
		const string errorReason = nameof(errorReason);
		const string originalAttributeValue = nameof(originalAttributeValue);

		const string expectedInterpretation = GlobalHtmlStyle +
$"<h2>CC557 - Rejection from the customs office of exit</h2>" +
"<hr />" +
"<table>" +
	$"<caption><h3>CC{businessRejectionType}-{rejectionReason}</h3></caption>" +
	"<tbody>" +
		$"<tr><th>Customs Office of Exit</th><td>{customsOfficeOfExitReferenceNumber}</td></tr>" +
		$"<tr><th>LRN</th><td>{lrn}</td></tr>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>The message {businessRejectionType} received rejection [CC557C] from Customs</h3></caption>" +
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
		"<tr><th>No.</th><th>Error Pointer</th><th>Error Code</th><th>Error Reason</th><th>Original Attribute Value</th></tr>" +
		$"<tr><td>1</td><td>{errorPointer}1</td><td>1</td><td>{errorReason}1</td><td>{originalAttributeValue}1</td></tr>" +
		$"<tr><td>2</td><td>{errorPointer}2</td><td>2</td><td>{errorReason}2</td><td>{originalAttributeValue}2</td></tr>" +
	"</tbody>" +
"</table>";

		var interpreter = new CC557CMessageInterpreter(Factory.New<CusExitReport>());
		var dataProvider = Mock.Of<ICC557C>(p =>
			p.LRN == lrn &&
			p.MRN == mrn &&
			p.CorrelationIdentifier == correlationIdentifier &&
			p.ExportOperation == Mock.Of<ICC557CExportOperation>(o =>
				o.BusinessRejectionType == businessRejectionType &&
				o.RejectionReason == rejectionReason &&
				o.RejectionDateAndTime == rejectionDateAndTime &&
				o.RejectionCode == rejectionCode) &&
			p.CustomsOfficeOfExitActual == Mock.Of<ICustomsOffice>(o =>
				o.ReferenceNumber == customsOfficeOfExitReferenceNumber) &&
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
