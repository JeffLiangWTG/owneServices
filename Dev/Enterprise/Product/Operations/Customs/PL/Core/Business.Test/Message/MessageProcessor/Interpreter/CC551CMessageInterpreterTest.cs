using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC551CMessageInterpreter))]
sealed class CC551CMessageInterpreterTest : MessageInterpreterTest<ICC551C>
{
	public void TestInterpret()
	{
		const string customsOfficeOfExportReferenceNumber = "CustomsOfficeOfExportReferenceNumber";
		const string CustomsOfficeOfPresentationReferenceNumber = "CustomsOfficeOfPresentationReferenceNumber";
		const string otherThingsToReport = "OtherThingsToReport";
		const string mrn = "MRN";
		const string controlResultText = "ControlResultText";
		const string identificationNumber = "IdentificationNumber";
		const string name = "Name";
		const string streetAndNumber = "StreetAndNumber";

		const string expectedInterpretation = GlobalHtmlStyle +
"<h2>IE551 – Refusal to release the goods for export procedure</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
		"<tr><th>Message Sent On</th><td>01-Jan-24 00:00:00</td></tr>" +
		$"<tr><th>Customs Office of Export</th><td>{customsOfficeOfExportReferenceNumber}</td></tr>" +
		$"<tr><th>Customs Office of Presentation</th><td>{CustomsOfficeOfPresentationReferenceNumber}</td></tr>" +
		$"<tr><th>Refusal reason</th><td>{otherThingsToReport}</td></tr>" +
		"<tr><th>Date of control</th><td>29-Jan-24 00:00:00</td></tr>" +
		$"<tr><th>Additional Refusal Remark</th><td>{controlResultText}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	"<caption><h3>Exporter</h3></caption>" +
	"<tbody>" +
		$"<tr><th>EORI</th><td>{identificationNumber}</td></tr>" +
		$"<tr><th>Name</th><td>{name}</td></tr>" +
		$"<tr><th>Street &amp; Address</th><td>{streetAndNumber}</td></tr>" +
	"</tbody>" +
"</table>";

		var preparationDateAndTime = new DateTime(2024, 01, 01);
		var controlResultDate = new DateTime(2024, 01, 29);

		var interpreter = new CC551CMessageInterpreter(entryHeader);
		var customsOfficeOfExport = new Mock<ICustomsOffice>();
		var customsOfficeOfPresentation = new Mock<ICustomsOffice>();
		var controlResult = new Mock<ICC551CControlResult>();
		var exporter = new Mock<IExporter>();
		var address = new Mock<IAddress>();

		customsOfficeOfExport.Setup(x => x.ReferenceNumber).Returns(customsOfficeOfExportReferenceNumber);
		customsOfficeOfPresentation.Setup(x => x.ReferenceNumber).Returns(CustomsOfficeOfPresentationReferenceNumber);
		controlResult.Setup(x => x.Date).Returns(controlResultDate);
		controlResult.Setup(x => x.Text).Returns(controlResultText);
		exporter.Setup(x => x.IdentificationNumber).Returns(identificationNumber);
		exporter.Setup(x => x.Name).Returns(name);
		address.Setup(x => x.StreetAndNumber).Returns(streetAndNumber);

		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(preparationDateAndTime);
		dataProviderMock.Setup(x => x.CustomsOfficeOfExport).Returns(customsOfficeOfExport.Object);
		dataProviderMock.Setup(x => x.CustomsOfficeOfPresentation).Returns(customsOfficeOfPresentation.Object);
		dataProviderMock.Setup(x => x.ControlResult).Returns(controlResult.Object);
		dataProviderMock.Setup(x => x.Exporter).Returns(exporter.Object);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.OtherThingsToReport).Returns(otherThingsToReport);
		exporter.Setup(x => x.Address).Returns(address.Object);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedInterpretation);
	}

	public void TestName()
	{
		var interpreter = new CC551CMessageInterpreter(entryHeader);
		var preparationDateAndTime = new DateTime(2024, 01, 01);
		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(preparationDateAndTime);
		var jobDocAddress = declaration.ExporterDocAddress;
		var exporterHeader = Factory.New<OrgHeader>();
		var orgAddress = exporterHeader.Addresses.AddNew();
		jobDocAddress.OrganisationPK = exporterHeader.PK;
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		CombineAssertions(() =>
		{
			AssertLineExists(interpreter, dataProviderMock.Object, "<tr><th>Name</th><td></td></tr>", description: "The name is null");

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "TestCompany";
			AssertLineExists(interpreter, dataProviderMock.Object, "<tr><th>Name</th><td>TestCompany</td></tr>", description: "Use E2_CompanyName as Name");

			jobDocAddress.E2_AddressOverride = false;
			exporterHeader.OH_FullName = "exporterHeader";
			AssertLineExists(interpreter, dataProviderMock.Object, "<tr><th>Name</th><td>exporterHeader</td></tr>", description: "Use OH_FullName as Name");
		});
	}

	public void TestStreetAndNumber()
	{
		var interpreter = new CC551CMessageInterpreter(entryHeader);

		var preparationDateAndTime = new DateTime(2024, 01, 01);
		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(preparationDateAndTime);
		var jobDocAddress = declaration.ExporterDocAddress;
		var exporterHeader = Factory.New<OrgHeader>();
		var orgAddress = exporterHeader.Addresses.AddNew();
		jobDocAddress.OrganisationPK = exporterHeader.PK;
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		CombineAssertions(() =>
		{
			AssertLineExists(interpreter, dataProviderMock.Object, "<tr><th>Street &amp; Address</th><td></td></tr>", description: "The StreetAndNumber is null");

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Address1 = "Street";
			AssertLineExists(interpreter, dataProviderMock.Object, "<tr><th>Street &amp; Address</th><td>Street</td></tr>", description: "Use Address Street as StreetAndNumber");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
