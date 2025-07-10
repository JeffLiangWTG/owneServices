namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders.Testing
{
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Moq;

	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	class MAFValidatorTest : RequestMessageTestCase
	{
		public void TestValidateAllWithInvalidFile()
		{
			var messagingRequest = GetSeaValidData();

			var mafMessaging = new MAFMessagingBO(plugInSupport.Object);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_EDocsUniqueID = ZGuid.Invalid;

			AssertNoExceptionThrown("Exception was thrown, when it shouldn't have been.", () => new MAFValidator(messagingRequest).GetErrorMessage());
		}

		public void TestEmptyHouseBillRowIsAnError()
		{
			var messageData = GetSeaValidData_V3();
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new[] { ZString.Empty });
			AssertMultilineASCIIEquals("dataValidator.Errors", "House Bill 1: You cannot have a Blank House Bill on a packing row.", new MAFValidator(messageData).GetErrorMessage());

			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new ZString[] { "SDFHJK32490823" });
			AssertMultilineASCIIEquals("dataValidator.Errors", "", new MAFValidator(messageData).GetErrorMessage());
			plugInSupport.VerifyAll();
		}

		public void TestEmptyMasterBillRowIsAnError()
		{
			var messageData = GetSeaValidData_V3();

			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new[] { ZString.Empty });
			AssertMultilineASCIIEquals("dataValidator.Errors", "Master Bill 1: You cannot have a Blank Master Bill on a packing row.", new MAFValidator(messageData).GetErrorMessage());

			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new ZString[] { "FDSJHK3242307" });
			AssertMultilineASCIIEquals("dataValidator.Errors", "", new MAFValidator(messageData).GetErrorMessage());
			plugInSupport.VerifyAll();
		}

		public void TestDataValidationForECIWriteOff()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			new TestDataBuilder(declaration).PopulateDeclarationThatPassesValidation();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			var requestData = TestDataBuilder.GetMAFMessaging(declaration);
			var dataValidator = new MAFValidator(requestData);

			AssertMultilineASCIIEquals("dataValidator.Errors", "", dataValidator.GetErrorMessage());
		}

		public void TestOnlyLinesWithStatUnitsRequireAQty()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var dataBuilder = new TestDataBuilder(Factory.New<JobDeclaration>());
			dataBuilder.PopulateDeclarationForAirWithTwoInvoiceLines();
			var declaration = dataBuilder.declaration;

			var requestData = TestDataBuilder.GetMAFMessaging(declaration);
			var dataValidator = new MAFValidator(requestData);
			AssertMultilineASCIIEquals("dataValidator.Errors", "", dataValidator.GetErrorMessage());

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CustomsUnitQty = "";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1].JI_CustomsUnitQty = "LTR";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1].JI_CustomsQuantity = 0;

			dataValidator = new MAFValidator(requestData);
			AssertMultilineASCIIEquals("dataValidator.Errors", "Merged Line 2 - You must have at least one Measurement for each commodity line.", new ZStringBuilder(dataValidator.GetErrorMessage()).ToStringWithNewLineBetweenAppends());

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1].JI_CustomsUnitQty = "LTR";
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[1].JI_CustomsQuantity = 85;

			dataValidator = new MAFValidator(requestData);
			AssertMultilineASCIIEquals("dataValidator.Errors", "", dataValidator.GetErrorMessage());
		}

		public void TestMinimalDataValidation()
		{
			var messageData = GetSeaValidData();

			var dataValidator = new MAFValidator(messageData);
			AssertMultilineASCIIEquals("dataValidator.Errors", "", dataValidator.GetErrorMessage());
		}

		public void TestEmptyFlightDateIsNotAllowed()
		{
			var messagingRequest = GetSeaValidData_V2();
			plugInSupport.Setup(m => m.FlightNumber).Returns((ZString)"QF253");
			plugInSupport.Setup(m => m.FlightArrivalDate).Returns(ZDateTime.Empty);

			var dataValidator = new MAFValidator(messagingRequest);
			AssertContains("Error list should error when there is a flight number but no date of arrival.", "You must have an Actual Time of Arrival.", dataValidator.GetErrorMessage());
			plugInSupport.VerifyAll();
		}

		public void TestValidateCommodities()
		{
			var messageData = GetSeaValidData_V3();
			plugInSupport.Setup(m => m.Commodities).Returns(System.Array.Empty<IMAFCommodity>());
			AssertMultilineASCIIEquals("dataValidator.Errors", "You must have at least one Commodity (Goods Type or Merged Line).", new MAFValidator(messageData).GetErrorMessage());

			var measurement1 = GetGoodsMeasurementMock(0, MeasurementUQList.Codes.kilograms).Object;
			var measurement2 = GetGoodsMeasurementMock(1, MeasurementUQList.Codes.kilograms).Object;

			var commodity1 = GetCommodityMock(1, GoodsTypeList.Codes.StoredProducts, "Commodity1", true, System.Array.Empty<ZString>(), new[] { measurement1 });
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodity1.Object });
			AssertMultilineASCIIEquals("dataValidator.Errors", string.Empty, new MAFValidator(messageData).GetErrorMessage());

			commodity1 = GetCommodityMock(1, GoodsTypeList.Codes.StoredProducts, "Commodity1", true, System.Array.Empty<ZString>(), new[] { measurement2 });
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodity1.Object });
			AssertMultilineASCIIEquals("dataValidator.Errors", string.Empty, new MAFValidator(messageData).GetErrorMessage());

			var commodity2 = GetCommodityMock(2, GoodsTypeList.Codes.StoredProducts, "Commodity2", true, System.Array.Empty<ZString>(), new[] { measurement1 });
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodity1.Object, commodity2.Object });
			AssertMultilineASCIIEquals("dataValidator.Errors", string.Empty, new MAFValidator(messageData).GetErrorMessage());

			commodity2 = GetCommodityMock(2, GoodsTypeList.Codes.StoredProducts, "Commodity2", true, System.Array.Empty<ZString>(), new[] { measurement2 });
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodity1.Object, commodity2.Object });
			AssertMultilineASCIIEquals("dataValidator.Errors", string.Empty, new MAFValidator(messageData).GetErrorMessage());

			commodity1 = GetCommodityMock(1, GoodsTypeList.Codes.StoredProducts, "Commodity1", true, System.Array.Empty<ZString>(), new[] { measurement1 });
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodity1.Object, commodity2.Object });
			AssertMultilineASCIIEquals("dataValidator.Errors", string.Empty, new MAFValidator(messageData).GetErrorMessage());
			plugInSupport.VerifyAll();
		}

		public void TestEmptyMessageDataDoesntPassValidation()
		{
			var dataValidator = new MAFValidator(GetEmptyData());

			const string expectedErrors = @"
You must have a Company Name in your Company Registration.
You must have a Container Number on All Containers.
You must have a Goods Description.
You must have a MPI Account Holder if your Payment Method is 'Account'.
You must have a MPI Account Number if your Payment Method is 'Account'.
You must have a MPI Consignment Type.
You must have a MPI Container Type for Container No [].
You must have a MPI Total Quantity Unit.
You must have a MPI Total Quantity.
You must have a POP3 Email Address in the Registry.
You must have a Port of Destination.
You must have a Port of Discharge.
You must have a Port of Origin.
You must have a Supplier when sending an eBACCa/IPI.
You must have an Importer when sending an eBACCa/IPI.
You must have at least one Master Bill or House Bill number.
You must setup Brokerage Details to send an eBACCa/IPI.
You must supply either a Flight number for Air shipments or a Vessel Name for Sea shipments.
Merged Line 1 - You must have a Goods Description.
Merged Line 1 - You must have a MPI Goods Type.
Merged Line 1 - You must have a setting for 'Is New'.
Merged Line 1 - You must have at least one Measurement for each commodity line.
Merged Line 2 - You must have a Goods Description.
Merged Line 2 - You must have a MPI Goods Type.
Merged Line 2 - You must have a setting for 'Is New'.
Merged Line 2 - You must have at least one Measurement for each commodity line.";
			AssertMultilineASCIIEquals("dataValidator.Errors", expectedErrors.Trim(), dataValidator.GetErrorMessage());
		}

		public void TestEmptyDeclarationDoesntPassValidation()
		{
			var declaration = Factory.New<JobDeclaration>();

			var requestData = TestDataBuilder.GetMAFMessaging(declaration);
			var dataValidator = new MAFValidator(requestData);

			AssertEquals("dataValidator.HasErrors", true, !dataValidator.GetErrorMessage().IsEmpty);
		}
	}

	#region RequestMessageTestCase

	class RequestMessageTestCase : TestCaseWithFactory
	{
		protected IMAFMessagingRequest GetSeaValidData()
		{
			var master = Factory.New<JobDeclaration>();
			plugInSupport = new Mock<IMAFPlugInSupport>();

			//IHaveNZAddInfo
			plugInSupport.Setup(m => m.AddInfo).Returns(new NZAddInfo(master));

			//IDocManagerSupport
			plugInSupport.Setup(m => m.DocManagerInfo).Returns(master.DocManagerInfo);

			//IMAFMessagingFallback
			var importerMock = GetOrganisationMock(
				"654321C", "ACME Importers", "12 Summer St", ZString.Empty,
				"Auckland, New Zealand", ZString.Empty, "NZ",
				"Person Importer", "a.importer@acme.co.nz", "09 321 1234", "09 123 1234");
			importerMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			importerMock.Setup(m => m.PostalCode).Returns(ZString.Empty);
			importerMock.Setup(m => m.ContactFax).Returns("09 321 1234");
			importerMock.Setup(m => m.ContactPhone).Returns("09 123 1234");

			plugInSupport.Setup(m => m.Importer).Returns(importerMock.Object);

			var exporterMock = GetOrganisationMock(
				"123456F", "Fun Exports", "1 Kangaroo Drive", ZString.Empty,
				"Tauranga, Australia", ZString.Empty, "AU",
				"Person Exporter", string.Empty, string.Empty, ".");
			exporterMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			exporterMock.Setup(m => m.PostalCode).Returns(ZString.Empty);
			exporterMock.Setup(m => m.ContactFax).Returns(string.Empty);
			exporterMock.Setup(m => m.ContactPhone).Returns(".");

			plugInSupport.Setup(m => m.Exporter).Returns(exporterMock.Object);
			plugInSupport.Setup(m => m.ProcessingOffice).Returns((ZString)MAFProcessingOfficeList.Codes.Auckland);
			plugInSupport.Setup(m => m.ConsignmentType).Returns((ZString)ConsignmentTypeList.Codes.PrivateCargo);
			plugInSupport.Setup(m => m.CargoType).Returns((ZString)CargoTypeList.Codes.Lcl);
			plugInSupport.Setup(m => m.MeasurementUQ).Returns((ZString)MeasurementUQList.Codes.cartons);
			plugInSupport.Setup(m => m.MeasurementValue).Returns((ZInt)100);

			//IMAFMessagingSource
			plugInSupport.Setup(m => m.Master).Returns(master);

			var brokerMock = GetOrganisationMock(
				"00231456A", "A Broker New Zealand Ltd", "11 Green Rd", ZString.Empty,
				"NZ 1010", ZString.Empty, "NZ",
				"Person Broker", "Broker@shipping.co.nz", string.Empty, string.Empty);
			plugInSupport.Setup(m => m.Broker).Returns(brokerMock.Object);
			plugInSupport.Setup(m => m.OriginCountry).Returns((ZString)"AU");
			plugInSupport.Setup(m => m.DischargePorts).Returns(new ZString[] { "NZAKL" });
			plugInSupport.Setup(m => m.Destinations).Returns(new ZString[] { "NZBLU" });
			plugInSupport.Setup(m => m.FlightNumber).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.ShipName).Returns((ZString)"DIRECT BANANA");
			plugInSupport.Setup(m => m.VoyageNumber).Returns((ZString)"123");
			plugInSupport.Setup(m => m.ShippingCompany).Returns((ZString)"Mersk Shipping Company");
			plugInSupport.Setup(m => m.VoyageArrivalDate).Returns(ZDateTime.Empty);
			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new ZString[] { "3463654" });
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new ZString[] { "11111111" });

			var containers =
				new[]
					{
						GetContainerMock("121212121222", ContainerTypeList.Codes.OpenTop40ft).Object,
						GetContainerMock("272727272722", ContainerTypeList.Codes.General20ft).Object
					};
			plugInSupport.Setup(m => m.Containers).Returns(containers);
			plugInSupport.Setup(m => m.ConsignmentDescription).Returns((ZString)"shoes");

			var goodsMeasurements =
				new[]
					{
						GetGoodsMeasurementMock(1000, MeasurementUQList.Codes.kilograms).Object,
						GetGoodsMeasurementMock(3, MeasurementUQList.Codes.metricTonnes).Object
					};
			var commodityMock = GetCommodityMock(1, GoodsTypeList.Codes.Vehicles, "shoes", true, new ZString[] { "6403991949C" }, goodsMeasurements);
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodityMock.Object });
			plugInSupport.Setup(m => m.CustomsEntryNumber).Returns((ZInt)0);
			plugInSupport.Setup(m => m.IsECIWriteoff).Returns(false);
			plugInSupport.Setup(m => m.TransitionalFacility).Returns(GetOrganisationMock(".", "Smiths Tranistional", "23 Smith Street", ZString.Empty,
																			   "Auckland, New Zealand", ZString.Empty, "NZ",
																			   string.Empty, string.Empty, string.Empty, string.Empty).Object);

			GlbCompany.CurrentCompany.GC_Name = "A Sender";
			Env.Registry.MailboxEmailAddress = "sender.email@domain.co.nz";
			var mafMessaging = new MAFMessagingBO(plugInSupport.Object);
			mafMessaging.ZX_ConsignmentNumber = "11111111";
			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Other;

			var fileNameWithPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Enterprise.Customs.NZ.Business.Test\MAFeBACCa\MessageBuilder\Sample.pdf";
			var fileNameWithoutPath = Path.GetFileName(fileNameWithPath);
			var eDoc = master.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF Sea File Content %EOF\n"), fileNameWithoutPath, DocumentTypeList.Codes.ExporterDeclaration);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_FileName = Path.GetFileName(fileNameWithPath);
			file.ZF_DocumentType = eDoc.DocType;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			return mafMessaging;
		}

		protected IMAFMessagingRequest GetSeaValidData_V2()
		{
			var master = Factory.New<JobDeclaration>();
			plugInSupport = new Mock<IMAFPlugInSupport>();

			//IHaveNZAddInfo
			plugInSupport.Setup(m => m.AddInfo).Returns(new NZAddInfo(master));

			//IDocManagerSupport
			plugInSupport.Setup(m => m.DocManagerInfo).Returns(master.DocManagerInfo);

			//IMAFMessagingFallback
			var importerMock = GetOrganisationMock(
				"654321C", "ACME Importers", "12 Summer St", ZString.Empty,
				"Auckland, New Zealand", ZString.Empty, "NZ",
				"Person Importer", "a.importer@acme.co.nz", "09 321 1234", "09 123 1234");
			plugInSupport.Setup(m => m.Importer).Returns(importerMock.Object);

			var exporterMock = GetOrganisationMock(
				"123456F", "Fun Exports", "1 Kangaroo Drive", ZString.Empty,
				"Tauranga, Australia", ZString.Empty, "AU",
				"Person Exporter", string.Empty, string.Empty, ".");
			plugInSupport.Setup(m => m.Exporter).Returns(exporterMock.Object);
			plugInSupport.Setup(m => m.ProcessingOffice).Returns((ZString)MAFProcessingOfficeList.Codes.Auckland);
			plugInSupport.Setup(m => m.ConsignmentType).Returns((ZString)ConsignmentTypeList.Codes.PrivateCargo);
			plugInSupport.Setup(m => m.CargoType).Returns((ZString)CargoTypeList.Codes.Lcl);
			plugInSupport.Setup(m => m.MeasurementUQ).Returns((ZString)MeasurementUQList.Codes.cartons);
			plugInSupport.Setup(m => m.MeasurementValue).Returns((ZInt)100);

			//IMAFMessagingSource
			plugInSupport.Setup(m => m.Master).Returns(master);

			var brokerMock = GetOrganisationMock(
				"00231456A", "A Broker New Zealand Ltd", "11 Green Rd", ZString.Empty,
				"NZ 1010", ZString.Empty, "NZ",
				"Person Broker", "Broker@shipping.co.nz", string.Empty, string.Empty);
			plugInSupport.Setup(m => m.OriginCountry).Returns((ZString)"AU");
			plugInSupport.Setup(m => m.DischargePorts).Returns(new ZString[] { "NZAKL" });
			plugInSupport.Setup(m => m.Destinations).Returns(new ZString[] { "NZBLU" });
			plugInSupport.Setup(m => m.FlightNumber).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.ShipName).Returns((ZString)"DIRECT BANANA");
			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new ZString[] { "3463654" });
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new ZString[] { "11111111" });

			var containers =
				new[]
					{
						GetContainerMock("121212121222", ContainerTypeList.Codes.OpenTop40ft).Object,
						GetContainerMock("272727272722", ContainerTypeList.Codes.General20ft).Object
					};
			plugInSupport.Setup(m => m.Containers).Returns(containers);
			plugInSupport.Setup(m => m.ConsignmentDescription).Returns((ZString)"shoes");

			var goodsMeasurements =
				new[]
					{
						GetGoodsMeasurementMock(1000, MeasurementUQList.Codes.kilograms).Object,
						GetGoodsMeasurementMock(3, MeasurementUQList.Codes.metricTonnes).Object
					};
			var commodityMock = GetCommodityMock(1, GoodsTypeList.Codes.Vehicles, "shoes", true, new ZString[] { "6403991949C" }, goodsMeasurements);
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodityMock.Object });

			GlbCompany.CurrentCompany.GC_Name = "A Sender";
			Env.Registry.MailboxEmailAddress = "sender.email@domain.co.nz";
			var mafMessaging = new MAFMessagingBO(plugInSupport.Object);
			mafMessaging.ZX_ConsignmentNumber = "11111111";
			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Other;

			var fileNameWithPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Enterprise.Customs.NZ.Business.Test\MAFeBACCa\MessageBuilder\Sample.pdf";
			var fileNameWithoutPath = Path.GetFileName(fileNameWithPath);
			var eDoc = master.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF Sea File Content %EOF\n"), fileNameWithoutPath, DocumentTypeList.Codes.ExporterDeclaration);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_FileName = Path.GetFileName(fileNameWithPath);
			file.ZF_DocumentType = eDoc.DocType;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			return mafMessaging;
		}

		protected IMAFMessagingRequest GetSeaValidData_V3()
		{
			var master = Factory.New<JobDeclaration>();
			plugInSupport = new Mock<IMAFPlugInSupport>();
			//IHaveNZAddInfo
			plugInSupport.Setup(m => m.AddInfo).Returns(new NZAddInfo(master));
			//IDocManagerSupport
			plugInSupport.Setup(m => m.DocManagerInfo).Returns(master.DocManagerInfo);
			//IMAFMessagingFallback
			var importerMock = GetOrganisationMock(
				"654321C", "ACME Importers", "12 Summer St", ZString.Empty,
				"Auckland, New Zealand", ZString.Empty, "NZ",
				"Person Importer", "a.importer@acme.co.nz", "09 321 1234", "09 123 1234");
			plugInSupport.Setup(m => m.Importer).Returns(importerMock.Object);
			var exporterMock = GetOrganisationMock(
				"123456F", "Fun Exports", "1 Kangaroo Drive", ZString.Empty,
				"Tauranga, Australia", ZString.Empty, "AU",
				"Person Exporter", string.Empty, string.Empty, ".");
			plugInSupport.Setup(m => m.Exporter).Returns(exporterMock.Object);
			plugInSupport.Setup(m => m.ProcessingOffice).Returns((ZString)MAFProcessingOfficeList.Codes.Auckland);
			plugInSupport.Setup(m => m.ConsignmentType).Returns((ZString)ConsignmentTypeList.Codes.PrivateCargo);
			plugInSupport.Setup(m => m.CargoType).Returns((ZString)CargoTypeList.Codes.Lcl);
			plugInSupport.Setup(m => m.MeasurementUQ).Returns((ZString)MeasurementUQList.Codes.cartons);
			plugInSupport.Setup(m => m.MeasurementValue).Returns((ZInt)100);
			//IMAFMessagingSource
			plugInSupport.Setup(m => m.Master).Returns(master);
			var brokerMock = GetOrganisationMock(
				"00231456A", "A Broker New Zealand Ltd", "11 Green Rd", ZString.Empty,
				"NZ 1010", ZString.Empty, "NZ",
				"Person Broker", "Broker@shipping.co.nz", string.Empty, string.Empty);
			plugInSupport.Setup(m => m.Broker).Returns(brokerMock.Object);
			plugInSupport.Setup(m => m.OriginCountry).Returns((ZString)"AU");
			plugInSupport.Setup(m => m.DischargePorts).Returns(new ZString[] { "NZAKL" });
			plugInSupport.Setup(m => m.Destinations).Returns(new ZString[] { "NZBLU" });
			plugInSupport.Setup(m => m.FlightNumber).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.ShipName).Returns((ZString)"DIRECT BANANA");
			plugInSupport.Setup(m => m.VoyageNumber).Returns((ZString)"123");
			plugInSupport.Setup(m => m.ShippingCompany).Returns((ZString)"Mersk Shipping Company");
			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new ZString[] { "3463654" });
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new ZString[] { "11111111" });

			var containers =
				new[]
					{
						GetContainerMock("121212121222", ContainerTypeList.Codes.OpenTop40ft).Object,
						GetContainerMock("272727272722", ContainerTypeList.Codes.General20ft).Object
					};
			plugInSupport.Setup(m => m.Containers).Returns(containers);
			plugInSupport.Setup(m => m.ConsignmentDescription).Returns((ZString)"shoes");

			var goodsMeasurements =
				new[]
					{
						GetGoodsMeasurementMock(1000, MeasurementUQList.Codes.kilograms).Object,
						GetGoodsMeasurementMock(3, MeasurementUQList.Codes.metricTonnes).Object
					};
			var commodityMock = GetCommodityMock(1, GoodsTypeList.Codes.Vehicles, "shoes", true, new ZString[] { "6403991949C" }, goodsMeasurements);
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodityMock.Object });

			var organisationMock = new Mock<IMAFOrganisation>();
			organisationMock.Setup(m => m.OrganisationName).Returns("Smiths Tranistional");
			organisationMock.Setup(m => m.AddressLine1).Returns("23 Smith Street");
			organisationMock.Setup(m => m.City).Returns("Auckland, New Zealand");
			organisationMock.Setup(m => m.Country).Returns("NZ");

			plugInSupport.Setup(m => m.TransitionalFacility).Returns(organisationMock.Object);

			GlbCompany.CurrentCompany.GC_Name = "A Sender";
			Env.Registry.MailboxEmailAddress = "sender.email@domain.co.nz";
			var mafMessaging = new MAFMessagingBO(plugInSupport.Object);
			mafMessaging.ZX_ConsignmentNumber = "11111111";
			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Other;

			var fileNameWithPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Enterprise.Customs.NZ.Business.Test\MAFeBACCa\MessageBuilder\Sample.pdf";
			var fileNameWithoutPath = Path.GetFileName(fileNameWithPath);
			var eDoc = master.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF Sea File Content %EOF\n"), fileNameWithoutPath, DocumentTypeList.Codes.ExporterDeclaration);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_FileName = Path.GetFileName(fileNameWithPath);
			file.ZF_DocumentType = eDoc.DocType;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			return mafMessaging;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected IMAFMessagingRequest GetAirValidData()
		{
			var master = Factory.New<JobDeclaration>();
			GlbCompany.CurrentCompany.GC_Name = "EDI DEMONSTRATION SYSTEM NZ";
			Env.Registry.MailboxEmailAddress = "enterprise_bjg@enterprisedevelopment.cargowise.com";
			plugInSupport = new Mock<IMAFPlugInSupport>();

			//IHaveNZAddInfo
			plugInSupport.Setup(m => m.AddInfo).Returns(new NZAddInfo(master));

			//IDocManagerSupport
			plugInSupport.Setup(m => m.DocManagerInfo).Returns(master.DocManagerInfo);

			//IMAFMessagingFallback
			var importerMock = GetOrganisationMock(
				"00782903F", "ADULT BOOKS LTD", "TEST CODE FOR NZ CUSTOMS", "LOCATED IN NZAKL",
				"AUCKLAND", "0000", "NZ",
				"Test Importer", "test.importer@adultbooks.co.nz", ZString.Empty, ZString.Empty);
			importerMock.Setup(m => m.AddressLine2).Returns("LOCATED IN NZAKL");
			importerMock.Setup(m => m.PostalCode).Returns("0000");
			importerMock.Setup(m => m.ContactFax).Returns(ZString.Empty);
			importerMock.Setup(m => m.ContactPhone).Returns(ZString.Empty);

			plugInSupport.Setup(m => m.Importer).Returns(importerMock.Object);

			var exporterMock = GetOrganisationMock(
				"00710841Y", "TEST SUPPLIER AU", "TEST CODE FOR NZ CUSTOMS", "LOCATED IN AUSYD",
				"SYDNEY", "2015", "AU",
				"Major Exporter", "major.exporter@tessupsyd.com.au", ZString.Empty, ZString.Empty);
			exporterMock.Setup(m => m.AddressLine2).Returns("LOCATED IN AUSYD");
			exporterMock.Setup(m => m.PostalCode).Returns("2015");
			exporterMock.Setup(m => m.ContactFax).Returns(ZString.Empty);
			exporterMock.Setup(m => m.ContactPhone).Returns(ZString.Empty);

			plugInSupport.Setup(m => m.Exporter).Returns(exporterMock.Object);

			plugInSupport.Setup(m => m.ProcessingOffice).Returns((ZString)MAFProcessingOfficeList.Codes.Auckland);
			plugInSupport.Setup(m => m.ConsignmentType).Returns((ZString)ConsignmentTypeList.Codes.PrivateCargo);
			plugInSupport.Setup(m => m.MeasurementUQ).Returns((ZString)MeasurementUQList.Codes.bunch);
			plugInSupport.Setup(m => m.MeasurementValue).Returns((ZInt)12);

			//IMAFMessagingSource
			plugInSupport.Setup(m => m.Master).Returns(master);

			var brokerMock = GetOrganisationMock(
				"00009917B", "EDI DEMONSTRATION SYSTEM NZ", "Unit 3a 72 O'Riordan Street", ZString.Empty,
				"Alexandria, NSW", "2015", "AU",
				"Ben Govett", "ben.govett@cargowise.com", ZString.Empty, ZString.Empty);
			brokerMock.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			brokerMock.Setup(m => m.PostalCode).Returns("2015");
			brokerMock.Setup(m => m.ContactFax).Returns(ZString.Empty);
			brokerMock.Setup(m => m.ContactPhone).Returns(ZString.Empty);

			plugInSupport.Setup(m => m.Broker).Returns(brokerMock.Object);

			plugInSupport.Setup(m => m.OriginCountry).Returns((ZString)"AU");
			plugInSupport.Setup(m => m.DischargePorts).Returns(new ZString[] { "NZAKL" });

			plugInSupport.Setup(m => m.Destinations).Returns(new ZString[] { "NZAKL" });

			plugInSupport.Setup(m => m.FlightNumber).Returns((ZString)"QF253");
			plugInSupport.Setup(m => m.FlightArrivalDate).Returns(new ZDateTime(2009, 2, 2));

			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new ZString[] { "081-11111111" });
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new ZString[] { "HB01010101" });

			plugInSupport.Setup(m => m.Containers).Returns(System.Array.Empty<IMAFContainer>());

			plugInSupport.Setup(m => m.ConsignmentDescription).Returns((ZString)"UNQUALIFIED GROMMETS");

			var goodsMeasurements =
				new[]
					{
						GetGoodsMeasurementMock(1, MeasurementUQList.Codes.cage).Object,
						GetGoodsMeasurementMock(2, MeasurementUQList.Codes.sacks).Object
					};
			var commodityMock = GetCommodityMock(1, GoodsTypeList.Codes.Miscellaneous, "GROMMETS THAT CAN'T SURF", false, new ZString[] { "4016930000F" }, goodsMeasurements);
			plugInSupport.Setup(m => m.Commodities).Returns(new[] { commodityMock.Object });

			plugInSupport.Setup(m => m.CustomsEntryNumber).Returns((ZInt)72334537);
			plugInSupport.Setup(m => m.IsECIWriteoff).Returns(false);

			var accountDetails = GetAccountDetailsMock("AB123", "ADULT BOOKS LTD");
			plugInSupport.Setup(m => m.AccountDetails).Returns(accountDetails.Object);

			var transitionalFacility = GetOrganisationMock("698", "A Hartrodt NZ Ltd", "Unit 5 / 197 Montgomerie Road", ZString.Empty,
														   "Airport Oaks, Auckland", ZString.Empty, "NZ",
														   string.Empty, string.Empty, string.Empty, string.Empty);
			plugInSupport.Setup(m => m.TransitionalFacility).Returns(transitionalFacility.Object);

			var mafMessaging = new MAFMessagingBO(plugInSupport.Object);
			mafMessaging.ZX_ReceiptNumber = "A123B456C7";
			mafMessaging.ZX_Comments = "I HOPE THIS TEST WORKS.";

			var fileNameWithPath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Enterprise.Customs.NZ.Business.Test\MAFeBACCa\MessageBuilder\Sample.pdf";
			var fileNameWithoutPath = Path.GetFileName(fileNameWithPath);
			var eDoc = master.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF Air File Content %EOF\n"), fileNameWithoutPath, DocumentTypeList.Codes.ExporterDeclaration);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_FileName = Path.GetFileName(fileNameWithPath);
			file.ZF_DocumentType = eDoc.DocType;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			return mafMessaging;
		}

		protected IMAFMessagingRequest GetEmptyData()
		{
			var master = Factory.New<JobDeclaration>();
			plugInSupport = new Mock<IMAFPlugInSupport>();

			//IHaveNZAddInfo
			plugInSupport.Setup(m => m.AddInfo).Returns(new NZAddInfo(master));

			//IDocManagerSupport
			plugInSupport.Setup(m => m.DocManagerInfo).Returns(master.DocManagerInfo);

			//IMAFMessagingFallback
			plugInSupport.Setup(m => m.ProcessingOffice).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.ConsignmentType).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.CargoType).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.MeasurementUQ).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.MeasurementValue).Returns(ZInt.Zero);

			plugInSupport.Setup(m => m.AccountDetails).Returns(GetAccountDetailsMock(ZString.Empty, ZString.Empty).Object);

			//IMAFMessagingSource
			plugInSupport.Setup(m => m.Master).Returns(master);

			plugInSupport.Setup(m => m.OriginCountry).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.DischargePorts).Returns(System.Array.Empty<ZString>());

			plugInSupport.Setup(m => m.Destinations).Returns(System.Array.Empty<ZString>());

			plugInSupport.Setup(m => m.FlightNumber).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.ShipName).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.VoyageNumber).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.ShippingCompany).Returns(ZString.Empty);

			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(System.Array.Empty<ZString>());
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(System.Array.Empty<ZString>());
			plugInSupport.Setup(m => m.Containers).Returns(new[] { GetContainerMock(ZString.Empty, ZString.Empty).Object });

			plugInSupport.Setup(m => m.ConsignmentDescription).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.Commodities).Returns(new[] {
															GetCommodityMock(1, ZString.Empty, ZString.Empty, null, System.Array.Empty<ZString>(), System.Array.Empty<IMAFMeasurement>()).Object,
															GetCommodityMock(2, ZString.Empty, ZString.Empty, null, System.Array.Empty<ZString>(), System.Array.Empty<IMAFMeasurement>()).Object });

			plugInSupport.Setup(m => m.CustomsEntryNumber).Returns(ZInt.Zero);
			plugInSupport.Setup(m => m.IsECIWriteoff).Returns(false);

			GlbCompany.CurrentCompany.GC_Name = ZString.Empty;
			Env.Registry.MailboxEmailAddress = string.Empty;
			var mafMessaging = new MAFMessagingBO(plugInSupport.Object);
			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			mafMessaging.Files.AddNew();
			return mafMessaging;
		}

		protected Mock<IMAFOrganisation> GetOrganisationMock(ZString orgCode, ZString orgName, ZString address1, ZString address2, ZString city, ZString postCode,
																	ZString country, ZString contact, ZString email, ZString fax, ZString phone)
		{
			var organisationMock = new Mock<IMAFOrganisation>();
			organisationMock.Setup(m => m.OrganisationCode).Returns(orgCode);
			organisationMock.Setup(m => m.OrganisationName).Returns(orgName);
			organisationMock.Setup(m => m.AddressLine1).Returns(address1);
			organisationMock.Setup(m => m.City).Returns(city);
			organisationMock.Setup(m => m.Country).Returns(country);
			organisationMock.Setup(m => m.ContactName).Returns(contact);
			organisationMock.Setup(m => m.ContactEmail).Returns(email);
			return organisationMock;
		}

		protected Mock<IMAFCommodity> GetCommodityMock(ZInt mergedLines, ZString goodsType, ZString goodsDescription, bool? isNew,
															  IEnumerable<ZString> tariffCodes, IEnumerable<IMAFMeasurement> goodsMeasurements)
		{
			var commodityMock = new Mock<IMAFCommodity>();
			commodityMock.Setup(m => m.MergedLineNumber).Returns(mergedLines);
			commodityMock.Setup(m => m.GoodsType).Returns(goodsType);
			commodityMock.Setup(m => m.GoodsDescription).Returns(goodsDescription);
			commodityMock.Setup(m => m.IsNew).Returns(isNew);
			commodityMock.Setup(m => m.TariffCodes).Returns(tariffCodes);
			commodityMock.Setup(m => m.GoodsMeasurements).Returns(goodsMeasurements);
			return commodityMock;
		}

		protected Mock<IMAFMeasurement> GetGoodsMeasurementMock(ZDecimal value, ZString unit)
		{
			var goodsMeasurementMock = new Mock<IMAFMeasurement>();
			goodsMeasurementMock.Setup(m => m.MeasurementUQ).Returns(unit);
			goodsMeasurementMock.Setup(m => m.MeasurementValue).Returns(value);
			return goodsMeasurementMock;
		}

		protected Mock<IMAFContainer> GetContainerMock(string number, string type)
		{
			var containerMock = new Mock<IMAFContainer>();
			containerMock.Setup(m => m.ContainerNumber).Returns((ZString)number);
			containerMock.Setup(m => m.ContainerType).Returns((ZString)type);
			return containerMock;
		}

		protected Mock<IMAFAccountDetails> GetAccountDetailsMock(ZString accountNumber, ZString accountHolderName)
		{
			var accountDetails = new Mock<IMAFAccountDetails>();
			accountDetails.Setup(m => m.AccountNumber).Returns(accountNumber);
			accountDetails.Setup(m => m.AccountHolderName).Returns(accountHolderName);
			return accountDetails;
		}

		protected Mock<IMAFPlugInSupport> plugInSupport;
	}

	#endregion

}
