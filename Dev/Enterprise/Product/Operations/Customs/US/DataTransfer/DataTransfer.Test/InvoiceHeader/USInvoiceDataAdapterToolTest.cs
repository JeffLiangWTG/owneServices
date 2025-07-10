using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class USInvoiceDataAdapterToolTest
	{
		public USInvoiceDataAdapterToolTest(IValueObjectDataAdapter invoiceDataAdapter, GetInvoiceHeaderDelegate getInvoiceHeader, GetInvoiceHeaderDelegate getUSInvoiceHeaderWithTestData, BusinessObjectFactory factory)
		{
			this.invoiceDataAdapter = invoiceDataAdapter;
			this.getInvoiceHeader = getInvoiceHeader;
			this.getUSInvoiceHeaderWithTestData = getUSInvoiceHeaderWithTestData;
			this.factory = factory;
		}

		internal delegate BaseJobComInvoiceHeader GetInvoiceHeaderDelegate();
		readonly GetInvoiceHeaderDelegate getInvoiceHeader;
		readonly GetInvoiceHeaderDelegate getUSInvoiceHeaderWithTestData;
		readonly IValueObjectDataAdapter invoiceDataAdapter;
		readonly BusinessObjectFactory factory;

		public void TestSomePropertiesMaxLengthExceeded()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.CountryPayload.USInvoice.CountryOfExport = "SOMECOUNTRY";
			xmlInvoiceHeader.CountryPayload.USInvoice.CountryOfOrigin = "ANOTHERCOUNTRY";
			Xsd.InvoiceLine xmlLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			xmlLine.CountryPayload.USInvoiceLine.CountryOfExport = "SOMECOUNTRY";
			xmlLine.CountryPayload.USInvoiceLine.CountryOfOrigin = "SOMECOUNTRY";
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.InvoiceLines.AddNew();
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(factory, notify);
			USInvoiceDataAdapter invoiceDataAdapter = new USInvoiceDataAdapter(header.JobDeclaration);
			invoiceDataAdapter.ImportFromValueObject(header, xmlInvoiceHeader, context);
			TestCaseWithFactory.AssertEquals("SO", header.US_UC_NKCountryOfExport);
			TestCaseWithFactory.AssertEquals("AN", header.US_UC_NKCountryOfOrigin);
			TestCaseWithFactory.AssertEquals("SO", line.US_UC_NKCountryOfExport);
			TestCaseWithFactory.AssertEquals("SO", line.US_UC_NKCountryOfOrigin);
		}

		public void TestExportImportForJZ_OH_Buyer()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)getInvoiceHeader();
			OrgHeader buyer = factory.New<OrgHeader>();
			buyer.FillWithValidTestData();
			buyer.OH_FullName = "CargoWise pty ltd";
			invoiceHeader.JZ_OH_Buyer = buyer.PK;
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TestCaseWithFactory.Assert(xmlInvoiceHeader.Consignee.IsSpecified);
			TestCaseWithFactory.AssertEquals(buyer.OH_FullName, xmlInvoiceHeader.Consignee.OrganisationDetails.Name);
			xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.Consignee = new Xsd.Organisation();
			xmlInvoiceHeader.Consignee.EDICode = "TEST";
			xmlInvoiceHeader.Consignee.OrganisationDetails = new Xsd.OrganisationDetail();
			xmlInvoiceHeader.Consignee.OrganisationDetails.Name = "CargoWise pty ltd";
			xmlInvoiceHeader.Consignee.OwnerCode = "OWNER";
			invoiceHeader = (JobComInvoiceHeader)getInvoiceHeader();
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectImportContext(factory, new NotificationBuffer()));
			TestCaseWithFactory.AssertEquals("Consignee is imported", "CargoWise pty ltd", invoiceHeader.Importer.OH_FullName);
		}

		public void TestImportUSInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)getInvoiceHeader();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.USInvoice uSXMLInvoice = new Xsd.USInvoice();
			xmlInvoiceHeader.ExchangeRate = 0.32m;
			xmlInvoiceHeader.ExchangeRateSpecified = true;
			uSXMLInvoice.CountryOfExport = "AD";
			uSXMLInvoice.CountryOfOrigin = "IT";
			uSXMLInvoice.DateOfExport = new ZDateTime(1998, 3, 4).Date;
			uSXMLInvoice.DateOfExportFromCO = new ZDateTime(1998, 3, 4).Date;
			uSXMLInvoice.DestinationState = "AL";
			uSXMLInvoice.InvoiceType = "IN";
			uSXMLInvoice.PaymentTerms.Code = "01";
			uSXMLInvoice.PaymentTerms.Desc = "Indiv";
			uSXMLInvoice.TermsOfDelivery.Indicator = "Y";
			uSXMLInvoice.TermsOfDelivery.Location = "A";
			uSXMLInvoice.TermsOfDelivery.Qualifier = "A";
			uSXMLInvoice.TransactionRelated = true;
			uSXMLInvoice.TransactionRelatedSpecified = true;
			uSXMLInvoice.ValueForDiscount = 100m;
			uSXMLInvoice.ValueForDiscountSpecified = true;
			uSXMLInvoice.ValueForForeignTax = 120m;
			uSXMLInvoice.ValueForForeignTaxSpecified = true;
			uSXMLInvoice.ContactName = "William";
			uSXMLInvoice.ContactPhoneNo = "7218900298";
			Xsd.TypeNumber doc1 = uSXMLInvoice.RelatedDocuments.AddNew();
			doc1.Number = "M001MB2";
			doc1.Type = RelatedDocumentIdentifierList.Codes.AirWaybillNumber;
			Xsd.TypeNumber doc2 = uSXMLInvoice.RelatedDocuments.AddNew();
			doc2.Number = "H001HB2";
			doc2.Type = RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber;
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(factory, notify);
			ValueObjectExportContext exportContext = new ValueObjectExportContext(notify);
			uSXMLInvoice.Organisations.Buyer = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.BuyingAgent = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.Exporter = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.Seller = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.SellingAgent = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.Invoicer.Item = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			((Xsd.Organisation)uSXMLInvoice.Organisations.Invoicer.Item).OrganisationDetails.Addresses[0].Sequence = 1;
			((Xsd.Organisation)uSXMLInvoice.Organisations.Invoicer.Item).OrganisationDetails.Addresses[1].Sequence = 1;
			((Xsd.Organisation)uSXMLInvoice.Organisations.Invoicer.Item).OrganisationDetails.Addresses[2].Sequence = 1;
			uSXMLInvoice.Organisations.Manufacturer.Item = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			xmlInvoiceHeader.CountryPayload.USInvoice = uSXMLInvoice;
			USInvoiceDataAdapter invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			AssertImportUSInvoiceHeaderResults(invoiceHeader);
			NotSpecifiedValuesDoesNotUpdateExistingValuesForInvoice(invoiceHeader, xmlInvoiceHeader, invoiceDataAdapter, context);
		}

		public void TestImportUSExportSpecificInvoiceData()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(factory, notify);
			ValueObjectExportContext exportContext = new ValueObjectExportContext(notify);
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			var contact = TestOrganization.Contacts.AddNew();
			contact.OC_ContactName = "SUPPLIER CONTACT";
			contact.OC_Phone = "400600";
			xmlInvoiceHeader.Consignor = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			Xsd.USInvoice uSXMLInvoice = new Xsd.USInvoice();
			xmlInvoiceHeader.ExchangeRate = 0.32m;
			xmlInvoiceHeader.ExchangeRateSpecified = true;
			uSXMLInvoice.FTZ = "FFTZ";
			uSXMLInvoice.InBondType = Xsd.USInvoiceInBondType.Item36;
			uSXMLInvoice.StateOfOrigin = "CO";
			uSXMLInvoice.ImpEntryNo = "2002";
			uSXMLInvoice.Hazardous = Xsd.TrueFalse.@false;
			uSXMLInvoice.RoutedTran = "N";
			uSXMLInvoice.TransactionRelated = true;
			uSXMLInvoice.TransactionRelatedSpecified = true;
			uSXMLInvoice.Organisations.UltimateConsignee.Organisation = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.UltimateConsignee.ContactDetails.Name = "ULT CONSIGNEE CONTACT";
			uSXMLInvoice.Organisations.UltimateConsignee.ContactDetails.Phone = "12456";
			uSXMLInvoice.Organisations.IntermediateConsignee.Organisation = OrganisationDataAdapter.ExportToValueObject(TestOrganization, exportContext);
			uSXMLInvoice.Organisations.IntermediateConsignee.ContactDetails.Name = "INTERM CONSIGNEE CONTACT";
			uSXMLInvoice.Organisations.IntermediateConsignee.ContactDetails.Phone = "800200";
			uSXMLInvoice.Organisations.SupplierContactDetails.Name = "SUPPLIER CONTACT";
			uSXMLInvoice.Organisations.SupplierContactDetails.Phone = "400600";
			var xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew().CountryPayload.USInvoiceLine;
			xmlInvoiceLine.ExportLicense.ECCN = "GYGTY";
			xmlInvoiceLine.ExportLicense.Number = "NLR";
			xmlInvoiceLine.ExportLicense.Type = "C33";
			xmlInvoiceLine.ExportCode = "DD";
			xmlInvoiceLine.DDTCDetails.ITARExemptionNumber = "123.16B1";
			xmlInvoiceLine.DDTCDetails.MilitaryEquipmentIndicator = Xsd.TrueFalse.@true;
			xmlInvoiceLine.DDTCDetails.PartyCertificationIndicator = Xsd.TrueFalse.@true;
			xmlInvoiceLine.DDTCDetails.Quantity.Value = 1332m;
			xmlInvoiceLine.DDTCDetails.Quantity.DimensionType = "BOX";
			xmlInvoiceLine.DDTCDetails.RegistrationNumber = "NUMBER";
			xmlInvoiceLine.DDTCDetails.USMLCategoryCode = "13";
			xmlInvoiceLine.OriginIndicator = Xsd.USInvoiceLineOriginIndicator.D;
			xmlInvoiceLine.VehicleDetails.UsedVehicle = true;
			xmlInvoiceLine.VehicleDetails.ID = "IF42";
			xmlInvoiceLine.VehicleDetails.IDType = "V";
			xmlInvoiceLine.VehicleDetails.TitleNumber = "TITLE";
			xmlInvoiceLine.VehicleDetails.TitleState = "AR";
			xmlInvoiceHeader.CountryPayload.USInvoice = uSXMLInvoice;
			USInvoiceDataAdapter invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			JobComInvoiceLine line = invoiceHeader.InvoiceLines[0];
			TestCaseWithFactory.AssertEquals("FFTZ", invoiceHeader.US_ForeignTradeZone);
			TestCaseWithFactory.AssertEquals(InbondTypeList.Codes.IEWarehouseWithdrawal, invoiceHeader.US_InbondType);
			TestCaseWithFactory.AssertEquals("CO", invoiceHeader.US_StateOfOrigin);
			TestCaseWithFactory.AssertEquals("2002", invoiceHeader.US_ImportEntryNo);
			TestCaseWithFactory.AssertEquals(YesNoDefaultList.Codes.No, invoiceHeader.US_HazardousCargo);
			TestCaseWithFactory.AssertEquals(YesNoDefaultList.Codes.No, invoiceHeader.US_RoutedTransaction);
			TestCaseWithFactory.AssertEquals(YesNoDefaultList.Codes.Yes, invoiceHeader.US_TransactionsRelated);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.JZ_OA_IntermediateConsigneeAddress);
			TestCaseWithFactory.AssertEquals("INTERM CONSIGNEE CONTACT", invoiceHeader.IntermediateConsigneeDocAddress.E2_Contact);
			TestCaseWithFactory.AssertEquals("800200", invoiceHeader.IntermediateConsigneeDocAddress.E2_Phone_Formatted);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.JZ_OA_BuyerAddress);
			TestCaseWithFactory.AssertEquals("ULT CONSIGNEE CONTACT", invoiceHeader.UltimateConsigneeDocAddress.E2_Contact);
			TestCaseWithFactory.AssertEquals("12456", invoiceHeader.UltimateConsigneeDocAddress.E2_Phone_Formatted);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.JZ_OA_SupplierAddress);
			TestCaseWithFactory.AssertEquals("SUPPLIER CONTACT", invoiceHeader.USPPIDocAddress.E2_Contact);
			TestCaseWithFactory.AssertEquals("400600", invoiceHeader.USPPIDocAddress.E2_Phone_Formatted);
			TestCaseWithFactory.AssertEquals("GYGTY", line.US_ECCN);
			TestCaseWithFactory.AssertEquals("NLR", line.US_LicenseNo);
			TestCaseWithFactory.AssertEquals("C33", line.US_LicenseType);
			TestCaseWithFactory.AssertEquals("DD", line.US_ExportCode);
			TestCaseWithFactory.AssertEquals("123.16B1", line.US_DDTCITARExemptionNo);
			TestCaseWithFactory.AssertEquals("Y", line.US_DDTCMilitaryEquipmentIndicator);
			TestCaseWithFactory.AssertEquals("Y", line.US_DDTCPartyCertificationIndicator);
			TestCaseWithFactory.AssertEquals(1332m, line.US_DDTCQuantity);
			TestCaseWithFactory.AssertEquals("BOX", line.US_DDTCUnit);
			TestCaseWithFactory.AssertEquals("NUMBER", line.US_DDTCRegistrationNo);
			TestCaseWithFactory.AssertEquals("13", line.US_DDTCUSMLCategoryCode);
			TestCaseWithFactory.AssertEquals("D", line.US_AESOriginIndicator);
			TestCaseWithFactory.AssertEquals(true, line.US_IsUsedVehicle);
			TestCaseWithFactory.AssertEquals("IF42", line.US_VehicleID);
			TestCaseWithFactory.AssertEquals("V", line.US_VehicleIDType);
			TestCaseWithFactory.AssertEquals("TITLE", line.US_VehicleTitleNo);
			TestCaseWithFactory.AssertEquals("AR", line.US_VehicleTitleState);
		}

		public void TestImportUSExportSpecificInvoiceData_InBondShouldBeBlankIfNotSpecified()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(factory, notify);
			ValueObjectExportContext exportContext = new ValueObjectExportContext(notify);
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			USInvoiceDataAdapter invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, new Xsd.InvoiceHeader(), context);
			TestCaseWithFactory.AssertEquals("", invoiceHeader.US_InbondType);
		}

		public void TestExportUSInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)getInvoiceHeader();
			invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoiceHeader.JZ_InvoiceCurrExRate = 0.86m;
			invoiceHeader.US_UC_NKCountryOfExport = "AD";
			invoiceHeader.US_UC_NKCountryOfOrigin = "IT";
			invoiceHeader.US_DateOfExport = new ZDateTime(1998, 3, 4);
			invoiceHeader.US_DateOfExportFromCountryOfOrigin = new ZDateTime(1998, 3, 4);
			invoiceHeader.US_DestinationState = "AL";
			invoiceHeader.US_InvoiceType = "IN";
			invoiceHeader.US_PaymentTerms = "01";
			invoiceHeader.US_PaymentTermsDesc = "Indiv";
			invoiceHeader.US_TermsOfDeliveryLocation = "A";
			invoiceHeader.US_TermsOfDeliveryLocationIndicator = "Y";
			invoiceHeader.US_TermsOfDeliveryLocationQualifier = "A";
			invoiceHeader.US_TransactionsRelated = "Y";
			invoiceHeader.US_ValueForDiscount = 100m;
			invoiceHeader.US_ValueForForeignTax = 120m;
			invoiceHeader.BuyerOrgPK = TestOrganization.PK;
			invoiceHeader.JZ_OH_BuyerAgent = TestOrganization.PK;
			invoiceHeader.ExporterOrgPK = TestOrganization.PK;
			invoiceHeader.JZ_OA_SellerAddress = TestOrganization.MainAddress.PK;
			invoiceHeader.JZ_OH_SellingAgent = TestOrganization.PK;
			invoiceHeader.US_FDAContactName = "DENZEL";
			invoiceHeader.US_TransactionsRelated = ZString.Empty;
			invoiceHeader.RelatedDocuments.AddNew(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, "1111");
			invoiceHeader.RelatedDocuments.AddNew(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber, "22222");
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			USInvoiceDataAdapter invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			invoiceDataAdapter.ExportToValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			TestCaseWithFactory.AssertEquals(0.86m, xmlInvoiceHeader.ExchangeRate);
			TestCaseWithFactory.AssertEquals(true, xmlInvoiceHeader.ExchangeRateSpecified);
			Xsd.USInvoice uSXMLInvoice = xmlInvoiceHeader.CountryPayload.USInvoice;
			TestCaseWithFactory.AssertEquals("AD", uSXMLInvoice.CountryOfExport);
			TestCaseWithFactory.AssertEquals("IT", uSXMLInvoice.CountryOfOrigin);
			TestCaseWithFactory.AssertEquals(new ZDateTime(1998, 3, 4), uSXMLInvoice.DateOfExport);
			TestCaseWithFactory.AssertEquals(new ZDateTime(1998, 3, 4), uSXMLInvoice.DateOfExportFromCO);
			TestCaseWithFactory.AssertEquals("AL", uSXMLInvoice.DestinationState);
			TestCaseWithFactory.AssertEquals("IN", uSXMLInvoice.InvoiceType);
			TestCaseWithFactory.AssertEquals("01", uSXMLInvoice.PaymentTerms.Code);
			TestCaseWithFactory.AssertEquals("Indiv", uSXMLInvoice.PaymentTerms.Desc);
			TestCaseWithFactory.AssertEquals("Y", uSXMLInvoice.TermsOfDelivery.Indicator);
			TestCaseWithFactory.AssertEquals("A", uSXMLInvoice.TermsOfDelivery.Location);
			TestCaseWithFactory.AssertEquals("A", uSXMLInvoice.TermsOfDelivery.Qualifier);
			TestCaseWithFactory.AssertEquals(false, uSXMLInvoice.TransactionRelatedSpecified);
			TestCaseWithFactory.AssertEquals(100m, uSXMLInvoice.ValueForDiscount);
			TestCaseWithFactory.AssertEquals(120m, uSXMLInvoice.ValueForForeignTax);
			TestCaseWithFactory.AssertEquals("Test Org", uSXMLInvoice.Organisations.Buyer.OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals("Test Org", uSXMLInvoice.Organisations.BuyingAgent.OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals("Test Org", uSXMLInvoice.Organisations.Exporter.OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals("Test Org", uSXMLInvoice.Organisations.Seller.OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals("Test Org", uSXMLInvoice.Organisations.SellingAgent.OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals("DENZEL", uSXMLInvoice.ContactName);
			TestCaseWithFactory.AssertEquals("1111", uSXMLInvoice.RelatedDocuments[0].Number);
			TestCaseWithFactory.AssertEquals(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, uSXMLInvoice.RelatedDocuments[0].Type);
			TestCaseWithFactory.AssertEquals("22222", uSXMLInvoice.RelatedDocuments[1].Number);
			TestCaseWithFactory.AssertEquals(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber, uSXMLInvoice.RelatedDocuments[1].Type);
			invoiceHeader.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			invoiceDataAdapter.ExportToValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			uSXMLInvoice = xmlInvoiceHeader.CountryPayload.USInvoice;
			TestCaseWithFactory.AssertEquals(true, uSXMLInvoice.TransactionRelatedSpecified);
			TestCaseWithFactory.AssertEquals(true, uSXMLInvoice.TransactionRelated);
		}

		public void TestExportUSExportInvoice()
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_ForeignTradeZone = "FFTZ";
			invoiceHeader.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			invoiceHeader.US_StateOfOrigin = "AL";
			invoiceHeader.US_ImportEntryNo = "4564242342";
			invoiceHeader.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			invoiceHeader.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			invoiceHeader.JZ_OA_ConsigneeAddress = TestOrganization.MainAddress.PK;
			invoiceHeader.JZ_OH_Consignee = TestOrganization.PK;
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.US_ECCN = "GYGTY";
			line.US_LicenseNo = "133";
			line.US_LicenseType = "C33";
			line.US_ExportCode = "DD";
			line.US_DDTCITARExemptionNo = "123.16B1";
			line.US_DDTCMilitaryEquipmentIndicator = "Y";
			line.US_DDTCPartyCertificationIndicator = "Y";
			line.US_DDTCQuantity = 1332;
			line.US_DDTCUnit = "BOX";
			line.US_DDTCRegistrationNo = "NUMBER";
			line.US_DDTCUSMLCategoryCode = "13";
			line.US_AESOriginIndicator = "D";
			line.US_IsUsedVehicle = true;
			line.US_VehicleID = "IF42";
			line.US_VehicleIDType = "V";
			line.US_VehicleTitleNo = "TITLE";
			line.US_VehicleTitleState = "AR";
			var xmlInvoiceHeader = new Xsd.InvoiceHeader();
			var invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			invoiceDataAdapter.ExportToValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.USInvoice uSXMLInvoice = xmlInvoiceHeader.CountryPayload.USInvoice;
			TestCaseWithFactory.AssertEquals("FFTZ", uSXMLInvoice.FTZ);
			TestCaseWithFactory.AssertEquals(Xsd.USInvoiceInBondType.Item67, uSXMLInvoice.InBondType);
			TestCaseWithFactory.AssertEquals("AL", uSXMLInvoice.StateOfOrigin);
			TestCaseWithFactory.AssertEquals("4564242342", uSXMLInvoice.ImpEntryNo);
			TestCaseWithFactory.AssertEquals("Y", uSXMLInvoice.Hazardous);
			TestCaseWithFactory.AssertEquals("Y", uSXMLInvoice.RoutedTran);
			TestCaseWithFactory.AssertNotNull(uSXMLInvoice.Organisations.IntermediateConsignee);
			TestCaseWithFactory.AssertNotNull(uSXMLInvoice.Organisations.UltimateConsignee);
			var xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines[0].CountryPayload.USInvoiceLine;
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.ExportLicense.ECCN, "GYGTY");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.ExportLicense.Number, "NLR");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.ExportLicense.Type, "C33");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.ExportCode, "DD");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.ITARExemptionNumber, "123.16B1");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.MilitaryEquipmentIndicator, Xsd.TrueFalse.@true);
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.PartyCertificationIndicator, Xsd.TrueFalse.@true);
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.Quantity.Value, 1332m);
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.Quantity.DimensionType, "BOX");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.RegistrationNumber, "NUMBER");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.DDTCDetails.USMLCategoryCode, "13");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.OriginIndicator, Xsd.USInvoiceLineOriginIndicator.D);
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.VehicleDetails.UsedVehicle, true);
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.VehicleDetails.ID, "IF42");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.VehicleDetails.IDType, "V");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.VehicleDetails.TitleNumber, "TITLE");
			TestCaseWithFactory.AssertEquals(xmlInvoiceLine.VehicleDetails.TitleState, "AR");
		}

		public void TestImportUSInvoiceLines()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = ExportedXmlInvoiceHeader();
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)getInvoiceHeader();
			USInvoiceDataAdapter invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(factory, notify);
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			AssertInvoiceLineDetails(invoiceLine);
			NotSpecifiedValuesDoesNotUpdateExistingValuesForInvoiceLine(invoiceHeader, xmlInvoiceHeader, invoiceDataAdapter, context);
		}

		public void TestImportUSInvoiceLines_ImportNetWeightSetsSecondQtyIfItsEmpty()
		{
			var xmlInvoiceHeader = ExportedXmlInvoiceHeader();
			var invoiceHeader = (JobComInvoiceHeader)getInvoiceHeader();
			xmlInvoiceHeader.InvoiceLines[0].NetWeight.DimensionType = "T";
			xmlInvoiceHeader.InvoiceLines[0].NetWeight.Value = 1m;
			xmlInvoiceHeader.InvoiceLines[0].LineClassification.TariffCode.Value = "0306230020";
			xmlInvoiceHeader.InvoiceLines[0].CustomsInvoiceQty.DimensionType = "KG";
			xmlInvoiceHeader.InvoiceLines[0].CustomsInvoiceQty.Value = 99m;
			var invoiceDataAdapter = new USInvoiceDataAdapter(invoiceHeader.JobDeclaration);
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(factory, notify);
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			var invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			TestCaseWithFactory.AssertEquals(99m, invoiceLine.JI_CustomsQuantity);
			TestCaseWithFactory.AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
			xmlInvoiceHeader.InvoiceLines[0].CustomsInvoiceQty.DimensionType = "";
			xmlInvoiceHeader.InvoiceLines[0].CustomsInvoiceQty.Value = 0m;
			xmlInvoiceHeader.InvoiceLines[0].InvoiceQty.Value = 10;
			xmlInvoiceHeader.InvoiceLines[0].InvoiceQty.DimensionType = "KG";
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			TestCaseWithFactory.AssertEquals(10m, invoiceLine.JI_CustomsQuantity);
			TestCaseWithFactory.AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
		}

		public void TestExportUSInvoiceLines()
		{
			var xmlInvoiceHeader = ExportedXmlInvoiceHeader();
			TestCaseWithFactory.AssertEquals(161m, xmlInvoiceHeader.InvoiceLines[0].NetWeight.Value);
			TestCaseWithFactory.AssertEquals("KJ", xmlInvoiceHeader.InvoiceLines[0].NetWeight.DimensionType);
			var uSXMLInvoiceLine = xmlInvoiceHeader.InvoiceLines[0].CountryPayload.USInvoiceLine;
			TestCaseWithFactory.AssertEquals(USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff, xmlInvoiceHeader.InvoiceLines[0].LineClassification.TariffCode.Value);
			TestCaseWithFactory.AssertEquals("MACH PTS OTH,U/IN PREP OF", xmlInvoiceHeader.InvoiceLines[0].LineClassification.TariffCode.Description);
			TestCaseWithFactory.AssertEquals(TariffTypeList.Codes.HTS, xmlInvoiceHeader.InvoiceLines[0].LineClassification.TariffCode.Type);
			TestCaseWithFactory.AssertEquals(135m, uSXMLInvoiceLine.USOrOriginalGoodsValue);
			TestCaseWithFactory.AssertEquals(168m, uSXMLInvoiceLine.USOrOriginalValueInInvCurr);
			TestCaseWithFactory.AssertEquals(true, uSXMLInvoiceLine.AntiDumping.Bonded);
			TestCaseWithFactory.AssertEquals("A234", uSXMLInvoiceLine.AntiDumping.CaseNo);
			TestCaseWithFactory.AssertEquals("1", uSXMLInvoiceLine.AntiDumping.DepositRateIndicator);
			TestCaseWithFactory.AssertEquals(148m, uSXMLInvoiceLine.AntiDumping.DepositValue);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.AntiDumping.IsSpecified);
			TestCaseWithFactory.AssertEquals("A", uSXMLInvoiceLine.ArticleNo.ID1);
			TestCaseWithFactory.AssertEquals("B", uSXMLInvoiceLine.ArticleNo.ID2);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.ArticleNo.IsSpecified);
			TestCaseWithFactory.AssertEquals(11m, uSXMLInvoiceLine.BasisUnit.BasisUnit);
			TestCaseWithFactory.AssertEquals(132m, uSXMLInvoiceLine.BasisUnit.Price);
			TestCaseWithFactory.AssertEquals(true, uSXMLInvoiceLine.Countervailing.Bonded);
			TestCaseWithFactory.AssertEquals("C497", uSXMLInvoiceLine.Countervailing.CaseNo);
			TestCaseWithFactory.AssertEquals("1", uSXMLInvoiceLine.Countervailing.DepositRateIndicator);
			TestCaseWithFactory.AssertEquals(46m, uSXMLInvoiceLine.Countervailing.DepositValue);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.Countervailing.IsSpecified);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.OverriddenTaxRateSpecified);
			TestCaseWithFactory.AssertEquals(0.99m, uSXMLInvoiceLine.OverriddenTaxRate);
			TestCaseWithFactory.AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, USInvoiceLineTaxCodeMappings.Instance.GetEnterpriseCode(uSXMLInvoiceLine.TaxCode, "", null));
			TestCaseWithFactory.AssertEquals("AD", uSXMLInvoiceLine.CountryOfExport);
			TestCaseWithFactory.AssertEquals("IT", uSXMLInvoiceLine.CountryOfOrigin);
			TestCaseWithFactory.AssertEquals(new ZDateTime(2007, 3, 4), uSXMLInvoiceLine.DateOfExportFromCO);
			TestCaseWithFactory.AssertEquals("AL", uSXMLInvoiceLine.DestinationState);
			TestCaseWithFactory.AssertEquals(false, uSXMLInvoiceLine.TransactionRelated);
			TestCaseWithFactory.AssertEquals("3", uSXMLInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonCode);
			TestCaseWithFactory.AssertEquals("fffd", uSXMLInvoiceLine.DispatchedInvoiceQty.QtyDiffReasonDesc);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.DispatchedInvoiceQty.IsSpecified);
			TestCaseWithFactory.AssertEquals(ZDateTime.Empty, uSXMLInvoiceLine.FTZ.PrivilegedStatusDate); // empty because declaration not PrivilegedForeign
			TestCaseWithFactory.AssertEquals("D", uSXMLInvoiceLine.FTZ.ZoneStatus);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.FTZ.IsSpecified);
			TestCaseWithFactory.AssertEquals("Haz Mat", uSXMLInvoiceLine.HazardousMaterial.ClassificationDesc);
			TestCaseWithFactory.AssertEquals("Descr", uSXMLInvoiceLine.HazardousMaterial.Desc);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.HazardousMaterial.IsSpecified);
			TestCaseWithFactory.AssertEquals(1250m, uSXMLInvoiceLine.ManifestQty);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.ManifestQtySpecified);
			TestCaseWithFactory.AssertEquals("Test Org", ((Xsd.Organisation)uSXMLInvoiceLine.Manufacturer.Item).OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals(false, uSXMLInvoiceLine.NAFTANet);
			TestCaseWithFactory.AssertEquals(1.2m, uSXMLInvoiceLine.PercentageOfActiveIngredient);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.PercentageOfActiveIngredientSpecified);
			TestCaseWithFactory.AssertEquals("125", uSXMLInvoiceLine.PermitAndLicence.AgricultureLicNo);
			TestCaseWithFactory.AssertEquals("cert", uSXMLInvoiceLine.PermitAndLicence.CAExportCertificate);
			TestCaseWithFactory.AssertEquals("126", uSXMLInvoiceLine.PermitAndLicence.CBTPACertificate);
			TestCaseWithFactory.AssertEquals("127", uSXMLInvoiceLine.PermitAndLicence.MiscPermitNo);
			TestCaseWithFactory.AssertEquals("1", uSXMLInvoiceLine.PermitAndLicence.SWPMIndicator);
			TestCaseWithFactory.AssertEquals("131", uSXMLInvoiceLine.PermitAndLicence.WoolLicenceNo);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.PermitAndLicence.IsSpecified);
			TestCaseWithFactory.AssertEquals("165", uSXMLInvoiceLine.PIRPRuling.Number);
			TestCaseWithFactory.AssertEquals("R", uSXMLInvoiceLine.PIRPRuling.Type);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.PIRPRuling.IsSpecified);
			TestCaseWithFactory.AssertEquals("A", uSXMLInvoiceLine.SecondarySPI);
			TestCaseWithFactory.AssertEquals(560m, uSXMLInvoiceLine.SecondQty.Value);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.SecondQty.IsSpecified);
			TestCaseWithFactory.AssertEquals("NO", uSXMLInvoiceLine.SecondQty.DimensionType);
			TestCaseWithFactory.AssertEquals("1", uSXMLInvoiceLine.SelectedRateType);
			TestCaseWithFactory.AssertEquals(154.1m, uSXMLInvoiceLine.SoftwoodLumber.ExportCharge);
			TestCaseWithFactory.AssertEquals(300m, uSXMLInvoiceLine.SoftwoodLumber.ExportPrice);
			TestCaseWithFactory.AssertEquals(true, uSXMLInvoiceLine.SoftwoodLumber.ImporterDec);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.SoftwoodLumber.IsSpecified);
			TestCaseWithFactory.AssertEquals("Z", uSXMLInvoiceLine.SPI);
			TestCaseWithFactory.AssertEquals("882", uSXMLInvoiceLine.Textile.CategoryNo);
			TestCaseWithFactory.AssertEquals("221", uSXMLInvoiceLine.Textile.VisaNo);
			TestCaseWithFactory.AssertEquals(46m, uSXMLInvoiceLine.Textile.VisaQty);
			TestCaseWithFactory.AssertEquals("KG", uSXMLInvoiceLine.Textile.VisaUQ);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.Textile.IsSpecified);
			TestCaseWithFactory.AssertEquals("NO", uSXMLInvoiceLine.ThirdQty.DimensionType);
			TestCaseWithFactory.AssertEquals(16.8m, uSXMLInvoiceLine.ThirdQty.Value);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.ThirdQty.IsSpecified);
			TestCaseWithFactory.AssertEquals("I", uSXMLInvoiceLine.TSCA.Indicator);
			TestCaseWithFactory.AssertEquals("Name", uSXMLInvoiceLine.TSCA.Name);
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.TSCA.IsSpecified);
			TestCaseWithFactory.AssertEquals(59m, uSXMLInvoiceLine.DispatchedInvoiceQty.Quantity.Value);
			TestCaseWithFactory.AssertEquals("HY", uSXMLInvoiceLine.DispatchedInvoiceQty.Quantity.DimensionType);
			TestCaseWithFactory.AssertEquals("C111", uSXMLInvoiceLine.RegistrationNumbers[0].Number);
			TestCaseWithFactory.AssertEquals(RegoNumberCodeList.Codes.ChassisNumber, uSXMLInvoiceLine.RegistrationNumbers[0].Type);
			var usNotification = new NotificationBuffer();
			var usContext = new ValueObjectImportContext(factory, usNotification);
			TestCaseWithFactory.AssertEquals(OGAIndicatorList.Codes.Declared, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(uSXMLInvoiceLine.OGAIndicators.DOTIndicator.ToString(), "", usContext));
			TestCaseWithFactory.AssertEquals(OGAIndicatorList.Codes.Declared, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(uSXMLInvoiceLine.OGAIndicators.FCCIndicator.ToString(), "", usContext));
			TestCaseWithFactory.AssertEquals(OGAIndicatorList.Codes.Declared, OGAIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(uSXMLInvoiceLine.OGAIndicators.FDAIndicator.ToString(), "", usContext));
			TestCaseWithFactory.Assert(uSXMLInvoiceLine.OGAIndicators.IsSpecified);
			TestCaseWithFactory.AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, uSXMLInvoiceLine.Fees[0].Code);
			TestCaseWithFactory.AssertEquals(200m, uSXMLInvoiceLine.Fees[0].FeeAmount);
			TestCaseWithFactory.AssertEquals(Xsd.TrueFalse.@true, uSXMLInvoiceLine.Fees[0].Overidden);
			OGADataTransferToolTest.AssertXmlDotDetails(uSXMLInvoiceLine.DOTs[0]);
			OGADataTransferToolTest.AssertXmlFCCDetails(uSXMLInvoiceLine.FCCs[0]);
			OGADataTransferToolTest.AssertXmlFDADetails(uSXMLInvoiceLine.FDAs[0]);
			OGADataTransferToolTest.AssertXmlLaceyActDetails(uSXMLInvoiceLine.LaceyActDetails[0]);
			TestCaseWithFactory.AssertEquals(600, uSXMLInvoiceLine.LaceyActDetails[0].Value);
			TestCaseWithFactory.AssertEquals("Ultimate Consignee", ((Xsd.Organisation)uSXMLInvoiceLine.UltimateConsignee.Item).OrganisationDetails.Name);
			TestCaseWithFactory.AssertEquals("Sup Tariff", "9817005000", uSXMLInvoiceLine.Supplementary.Tariff);
			TestCaseWithFactory.AssertEquals("Sup Qty1", 12m, uSXMLInvoiceLine.Supplementary.Quantity1.Value);
			TestCaseWithFactory.AssertEquals("Sup Qty1 UQ", "CAR", uSXMLInvoiceLine.Supplementary.Quantity1.DimensionType);
			TestCaseWithFactory.AssertEquals("Sup Qty2", 14m, uSXMLInvoiceLine.Supplementary.Quantity2.Value);
			TestCaseWithFactory.AssertEquals("Sup Qty2 UQ", "KG", uSXMLInvoiceLine.Supplementary.Quantity2.DimensionType);
			TestCaseWithFactory.AssertEquals("Sup Qty3", 15m, uSXMLInvoiceLine.Supplementary.Quantity3.Value);
			TestCaseWithFactory.AssertEquals("Sup Qty3 UQ", "T", uSXMLInvoiceLine.Supplementary.Quantity3.DimensionType);
		}

		void AssertImportUSInvoiceHeaderResults(JobComInvoiceHeader invoiceHeader)
		{
			TestCaseWithFactory.AssertEquals("AD", invoiceHeader.US_UC_NKCountryOfExport);
			TestCaseWithFactory.AssertEquals("IT", invoiceHeader.US_UC_NKCountryOfOrigin);
			TestCaseWithFactory.AssertEquals(new ZDateTime(1998, 3, 4), invoiceHeader.US_DateOfExport);
			TestCaseWithFactory.AssertEquals(new ZDateTime(1998, 3, 4), invoiceHeader.US_DateOfExportFromCountryOfOrigin);
			TestCaseWithFactory.AssertEquals("AL", invoiceHeader.US_DestinationState);
			TestCaseWithFactory.AssertEquals("IN", invoiceHeader.US_InvoiceType);
			TestCaseWithFactory.AssertEquals("01", invoiceHeader.US_PaymentTerms);
			TestCaseWithFactory.AssertEquals("Indiv", invoiceHeader.US_PaymentTermsDesc);
			TestCaseWithFactory.AssertEquals("A", invoiceHeader.US_TermsOfDeliveryLocation);
			TestCaseWithFactory.AssertEquals("Y", invoiceHeader.US_TermsOfDeliveryLocationIndicator);
			TestCaseWithFactory.AssertEquals("A", invoiceHeader.US_TermsOfDeliveryLocationQualifier);
			TestCaseWithFactory.AssertEquals("Y", invoiceHeader.US_TransactionsRelated);
			TestCaseWithFactory.AssertEquals(100m, invoiceHeader.US_ValueForDiscount);
			TestCaseWithFactory.AssertEquals(120m, invoiceHeader.US_ValueForForeignTax);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.BuyerOrgPK);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.JZ_OH_BuyerAgent);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.ExporterOrgPK);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.JZ_OA_SellerAddress);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, invoiceHeader.JZ_OH_SellingAgent);
			TestCaseWithFactory.AssertNotEquals(address2, invoiceHeader.JZ_OA_InvoicerDocAddress);
			TestCaseWithFactory.AssertNotEquals(address2, invoiceHeader.JZ_OA_ManufacturerAddress);
			TestCaseWithFactory.AssertEquals(true, invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable);
			TestCaseWithFactory.AssertEquals(0.32m, invoiceHeader.JZ_InvoiceCurrExRate);
			TestCaseWithFactory.AssertEquals(2, invoiceHeader.RelatedDocuments.Count);
			var relatedDoc1 = invoiceHeader.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.AirWaybillNumber);
			TestCaseWithFactory.AssertNotNull(relatedDoc1);
			TestCaseWithFactory.AssertEquals("M001MB2", relatedDoc1.CY_Data);
			var relatedDoc2 = invoiceHeader.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber);
			TestCaseWithFactory.AssertNotNull(relatedDoc2);
			TestCaseWithFactory.AssertEquals("H001HB2", relatedDoc2.CY_Data);
			TestCaseWithFactory.AssertNotEquals(ZString.Empty, invoiceHeader.US_FDAContactName);
		}

		void NotSpecifiedValuesDoesNotUpdateExistingValuesForInvoice(JobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, USInvoiceDataAdapter invoiceDataAdapter, ValueObjectImportContext context)
		{
			invoiceHeader.RelatedDocuments.RemoveAndDeleteAll();
			xmlInvoiceHeader.CountryPayload.USInvoice.CountryOfExport = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.CountryOfOrigin = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.DateOfExport = ZDate.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.DateOfExportFromCO = ZDate.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.DestinationState = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.InvoiceType = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.PaymentTerms.Code = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.PaymentTerms.Desc = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.TermsOfDelivery.Indicator = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.TermsOfDelivery.Location = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.TermsOfDelivery.Qualifier = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.TransactionRelated = false;
			xmlInvoiceHeader.CountryPayload.USInvoice.TransactionRelatedSpecified = false;
			xmlInvoiceHeader.CountryPayload.USInvoice.ValueForDiscount = ZDecimal.Zero;
			xmlInvoiceHeader.CountryPayload.USInvoice.ValueForDiscountSpecified = false;
			xmlInvoiceHeader.CountryPayload.USInvoice.ValueForForeignTax = ZDecimal.Zero;
			xmlInvoiceHeader.CountryPayload.USInvoice.ValueForForeignTaxSpecified = false;
			xmlInvoiceHeader.CountryPayload.USInvoice.ContactName = ZString.Empty;
			xmlInvoiceHeader.CountryPayload.USInvoice.ContactPhoneNo = ZString.Empty;
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			AssertImportUSInvoiceHeaderResults(invoiceHeader);
		}

		void AssertInvoiceLineDetails(JobComInvoiceLine invoiceLine)
		{
			TestCaseWithFactory.AssertEquals(135m, invoiceLine.US_98GoodsValue);
			TestCaseWithFactory.AssertEquals(168m, invoiceLine.US_98ValueInvCurr);
			TestCaseWithFactory.AssertEquals(true, invoiceLine.US_IsBondedADD);
			TestCaseWithFactory.AssertEquals("A234", invoiceLine.US_ADDCaseNo);
			TestCaseWithFactory.AssertEquals("1", invoiceLine.US_ADDDepositRateIndicator);
			TestCaseWithFactory.AssertEquals(148m, invoiceLine.US_ADDDepositValue);
			TestCaseWithFactory.AssertEquals("A", invoiceLine.US_ArticleNoA);
			TestCaseWithFactory.AssertEquals("B", invoiceLine.US_ArticleNoB);
			TestCaseWithFactory.AssertEquals(true, invoiceLine.US_IsBondedCVD);
			TestCaseWithFactory.AssertEquals("C497", invoiceLine.US_CVDCaseNo);
			TestCaseWithFactory.AssertEquals("1", invoiceLine.US_CVDDepositRateIndicator);
			TestCaseWithFactory.AssertEquals(46m, invoiceLine.US_CVDDepositValue);
			TestCaseWithFactory.AssertEquals("AD", invoiceLine.US_UC_NKCountryOfExport);
			TestCaseWithFactory.AssertEquals("IT", invoiceLine.US_UC_NKCountryOfOrigin);
			TestCaseWithFactory.AssertEquals(new ZDateTime(2007, 3, 4), invoiceLine.US_DateOfExportFromCountryOfOrigin);
			TestCaseWithFactory.AssertEquals("AL", invoiceLine.US_DestinationState);
			TestCaseWithFactory.AssertEquals("N", invoiceLine.US_TransactionsRelated);
			TestCaseWithFactory.AssertEquals(ZDateTime.Empty, invoiceLine.US_PrivilegedStatusDate);
			TestCaseWithFactory.AssertEquals("D", invoiceLine.US_ZoneStatus);
			TestCaseWithFactory.AssertEquals("Haz Mat", invoiceLine.US_HazMatClassDesc);
			TestCaseWithFactory.AssertEquals("Descr", invoiceLine.US_HazMatDesc);
			TestCaseWithFactory.AssertEquals(1250, invoiceLine.US_ManifestQty);
			TestCaseWithFactory.AssertEquals(false, invoiceLine.US_IsNAFTANet);
			TestCaseWithFactory.AssertEquals("125", invoiceLine.US_AgricultureLicNo);
			TestCaseWithFactory.AssertEquals("cotton", invoiceLine.US_CottonCertificateNo);
			TestCaseWithFactory.AssertEquals("cert", invoiceLine.US_CAExportCertificate);
			TestCaseWithFactory.AssertEquals("126", invoiceLine.US_CBTPACertificateNo);
			TestCaseWithFactory.AssertEquals("127", invoiceLine.US_MiscPermitNo);
			TestCaseWithFactory.AssertEquals("1", invoiceLine.US_SWPMIndicator);
			TestCaseWithFactory.AssertEquals("131", invoiceLine.US_WoolLicenceNo);
			TestCaseWithFactory.AssertEquals("165", invoiceLine.US_PIRPRulingNo);
			TestCaseWithFactory.AssertEquals("R", invoiceLine.US_PIRPRulingType);
			TestCaseWithFactory.AssertEquals("A", invoiceLine.US_SecondarySPI);
			TestCaseWithFactory.AssertEquals(560m, invoiceLine.JI_CustomsSecondQuantity);
			TestCaseWithFactory.AssertEquals("NO", invoiceLine.JI_CustomsSecondUnitQty);
			TestCaseWithFactory.AssertEquals("1", invoiceLine.US_SelectedRateType);
			TestCaseWithFactory.AssertEquals(154.1m, invoiceLine.US_LumberExportCharges);
			TestCaseWithFactory.AssertEquals(300m, invoiceLine.US_LumberExportPrice);
			TestCaseWithFactory.AssertEquals("Y", invoiceLine.US_LumberImporterDeclaration);
			TestCaseWithFactory.AssertEquals("Z", invoiceLine.US_SPI);
			TestCaseWithFactory.AssertEquals("882", invoiceLine.US_TextileCategoryNo);
			TestCaseWithFactory.AssertEquals("221", invoiceLine.US_VisaNo);
			TestCaseWithFactory.AssertEquals(46m, invoiceLine.US_VisaQty);
			TestCaseWithFactory.AssertEquals("KG", invoiceLine.US_VisaUQ);
			TestCaseWithFactory.AssertEquals("NO", invoiceLine.JI_CustomsThirdUnitQty);
			TestCaseWithFactory.AssertEquals(16.8m, invoiceLine.JI_CustomsThirdQuantity);
			TestCaseWithFactory.AssertEquals("I", invoiceLine.US_TSCAIndicator);
			TestCaseWithFactory.AssertEquals("Name", invoiceLine.US_TSCAName);
			TestCaseWithFactory.AssertEquals(2, invoiceLine.DOTs.Count);
			TestCaseWithFactory.AssertEquals(1, invoiceLine.FCCs.Count);
			TestCaseWithFactory.AssertEquals(1, invoiceLine.FDAs.Count);
			TestCaseWithFactory.AssertEquals(1, invoiceLine.LaceyActLines.Count);
			TestCaseWithFactory.AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, invoiceLine.US_TaxCode);
			TestCaseWithFactory.AssertEquals(TaxApplyList.Codes.Override, invoiceLine.US_TaxApply);
			TestCaseWithFactory.AssertEquals(0.99m, invoiceLine.US_TaxRate);
			TestCaseWithFactory.AssertEquals("9817005000", invoiceLine.US_SupTariff);
			TestCaseWithFactory.AssertEquals(12m, invoiceLine.US_SupQty1);
			TestCaseWithFactory.AssertEquals("CAR", invoiceLine.US_SupUQ1);
			TestCaseWithFactory.AssertEquals(14m, invoiceLine.US_SupQty2);
			TestCaseWithFactory.AssertEquals("KG", invoiceLine.US_SupUQ2);
			TestCaseWithFactory.AssertEquals(15m, invoiceLine.US_SupQty3);
			TestCaseWithFactory.AssertEquals("T", invoiceLine.US_SupUQ3);
			TestCaseWithFactory.AssertEquals(TariffTypeList.Codes.HTS, invoiceLine.US_TariffType);
			AssertDotDetails(invoiceLine.DOTs[0]);
			AssertFCCDetails(invoiceLine.FCCs[0]);
			var testPK = TestOrganization.Addresses[2].PK;
			AssertFDADetails(invoiceLine.FDAs[0]);
			OGADataTransferToolTest.AssertLaceyActDetails(invoiceLine.LaceyActLines[0]);
			TestCaseWithFactory.AssertEquals(testPK, invoiceLine.JI_OA_ManufacturerAddress);
			TestCaseWithFactory.AssertEquals(161m, invoiceLine.JI_NetWeight);
			TestCaseWithFactory.AssertEquals("KJ", invoiceLine.JI_NetWeightUQ);
			TestCaseWithFactory.AssertEquals(200m, invoiceLine.FeeCusCodes[0].CY_FeeAmount);
			TestCaseWithFactory.AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.FeeCusCodes[0].CY_Code);
			TestCaseWithFactory.AssertEquals(true, invoiceLine.FeeCusCodes[0].CY_IsOverridden);
			TestCaseWithFactory.AssertEquals("D", invoiceLine.US_DOTIndicator);
			TestCaseWithFactory.AssertEquals("D", invoiceLine.US_FCCIndicator);
			TestCaseWithFactory.AssertEquals("D", invoiceLine.US_FDAIndicator);
			TestCaseWithFactory.AssertEquals(ultimateConsignee.MainAddress.PK, invoiceLine.JI_OA_ConsigneeAddress);
		}

		static void AssertDotDetails(DOT dOT)
		{
			TestCaseWithFactory.AssertEquals("01", dOT.US_DOTBoxNo);
			TestCaseWithFactory.AssertEquals("2", dOT.US_DOTClarCode);
			TestCaseWithFactory.AssertEquals("comm descr", dOT.US_DOTCommercialDesc);
			TestCaseWithFactory.AssertEquals("AD", dOT.US_DOTCountryOfOrigin);
			TestCaseWithFactory.AssertEquals(true, dOT.US_DOTImpSubstStatement);
			TestCaseWithFactory.AssertEquals("456", dOT.US_DOTPassport);
			TestCaseWithFactory.AssertEquals(false, dOT.US_DOTPriorApproval);
			TestCaseWithFactory.AssertEquals("891", dOT.US_DOTBondSuretyCode);
			TestCaseWithFactory.AssertEquals("Brand", dOT.US_DOTTireBrandName);
			TestCaseWithFactory.AssertEquals("ID", dOT.US_DOTTireID);
			TestCaseWithFactory.AssertEquals("1234", dOT.DOTVINs[0].US_DOTVEN);
			TestCaseWithFactory.AssertEquals("make", dOT.DOTVINs[0].US_DOTMake);
			TestCaseWithFactory.AssertEquals("model", dOT.DOTVINs[0].US_DOTModel);
			TestCaseWithFactory.AssertEquals("4573", dOT.DOTVINs[0].US_DOTRINo);
			TestCaseWithFactory.AssertEquals("10", dOT.DOTVINs[0].US_DOTVIN);
			TestCaseWithFactory.AssertEquals(98, dOT.DOTVINs[0].US_DOTYear);
		}

		static void AssertFCCDetails(FCC fCC)
		{
			TestCaseWithFactory.AssertEquals("comm descr", fCC.US_FCCCommercialDesc);
			TestCaseWithFactory.AssertEquals("21", fCC.US_FCCID);
			TestCaseWithFactory.AssertEquals("No", fCC.US_FCCImpCondNo);
			TestCaseWithFactory.AssertEquals(false, fCC.US_FCCImpCondNoQtyAppr);
			TestCaseWithFactory.AssertEquals("Model", fCC.US_FCCModel);
			TestCaseWithFactory.AssertEquals(151m, fCC.US_FCCQty);
			TestCaseWithFactory.AssertEquals("Trade Name", fCC.US_FCCTradeName);
			TestCaseWithFactory.AssertEquals(true, fCC.US_FCCWithhold);
		}

		static void AssertFDADetails(FDA fDA)
		{
			TestCaseWithFactory.AssertEquals("CCN", fDA.AffirmationCodes[0].CY_Code);
			TestCaseWithFactory.AssertEquals("CN", fDA.AffirmationCodes[0].CY_Data);
			TestCaseWithFactory.AssertEquals("Trade Brand Name", fDA.US_TradeBrandName);
			TestCaseWithFactory.AssertEquals("A", fDA.US_FDACargoStorageCode);
			TestCaseWithFactory.AssertEquals("MALE HORSES, PUREBRED BREE", fDA.US_FDACommercialDesc);
			TestCaseWithFactory.AssertEquals("IT", fDA.US_UC_NKFDAProduction);
			TestCaseWithFactory.AssertEquals(FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals, fDA.US_DimUQ);
			TestCaseWithFactory.AssertEquals(10m, fDA.US_ContainerDim1);
			TestCaseWithFactory.AssertEquals(20m, fDA.US_ContainerDim2);
			TestCaseWithFactory.AssertEquals(30m, fDA.US_ContainerDim3);
			TestCaseWithFactory.AssertEquals("C", fDA.US_FDAContainerDimType);
			TestCaseWithFactory.AssertEquals("12AAB01", fDA.US_FDAProductCode);
			TestCaseWithFactory.AssertEquals(130m, fDA.US_FDAQty1);
			TestCaseWithFactory.AssertEquals("BOL", fDA.US_FDAMeasure1);
			TestCaseWithFactory.AssertEquals(131m, fDA.US_FDAQty2);
			TestCaseWithFactory.AssertEquals("AE", fDA.US_FDAMeasure2);
			TestCaseWithFactory.AssertEquals(132m, fDA.US_FDAQty3);
			TestCaseWithFactory.AssertEquals("AM", fDA.US_FDAMeasure3);
			TestCaseWithFactory.AssertEquals(133m, fDA.US_FDAQty4);
			TestCaseWithFactory.AssertEquals("AP", fDA.US_FDAMeasure4);
			TestCaseWithFactory.AssertEquals(135m, fDA.US_FDAQty5);
			TestCaseWithFactory.AssertEquals("BG", fDA.US_FDAMeasure5);
			TestCaseWithFactory.AssertEquals(136m, fDA.US_FDAQty6);
			TestCaseWithFactory.AssertEquals("BF", fDA.US_FDAMeasure6);
			TestCaseWithFactory.AssertNotEquals(ZGuid.Empty, fDA.US_FDAManufacturerAddress);
			TestCaseWithFactory.AssertEquals("food reg", fDA.US_PFR);
			TestCaseWithFactory.AssertEquals(FDAPriorNoticeExemptCodeList.Codes.M, fDA.US_FME);
			TestCaseWithFactory.AssertEquals(ProducerFirmTypeList.Codes.G, fDA.US_PFT);
			TestCaseWithFactory.AssertEquals("GT", fDA.US_CSH);
			TestCaseWithFactory.AssertEquals(OwnerFirmTypeList.Codes.Carrier, fDA.US_OFT);
			TestCaseWithFactory.AssertEquals("7845", fDA.US_SFR);
			TestCaseWithFactory.AssertEquals("Test Org", fDA.FDAFEIAddress.EffectiveCompanyNameTruncated);
			var supplier = fDA.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Test Shipper"));
			TestCaseWithFactory.AssertEquals(supplier.MainAddress.PK, fDA.US_FDAShipperAddress);
		}

		void NotSpecifiedValuesDoesNotUpdateExistingValuesForInvoiceLine(JobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, USInvoiceDataAdapter invoiceDataAdapter, ValueObjectImportContext context)
		{
			invoiceHeader.JobComInvoiceLines[0].FDAs.RemoveAndDeleteAll();
			invoiceHeader.JobComInvoiceLines[0].FCCs.RemoveAndDeleteAll();
			invoiceHeader.JobComInvoiceLines[0].DOTs.RemoveAndDeleteAll();
			invoiceHeader.JobComInvoiceLines[0].LaceyActLines.RemoveAndDeleteAll();
			var xmlUSLine = xmlInvoiceHeader.InvoiceLines[0].CountryPayload.USInvoiceLine;
			xmlUSLine.AntiDumping.CaseNo = ZString.Empty;
			xmlUSLine.Countervailing.CaseNo = ZString.Empty;
			xmlUSLine.ArticleNo.ID1 = ZString.Empty;
			xmlUSLine.ArticleNo.ID2 = ZString.Empty;
			xmlUSLine.BasisUnit.BasisUnit = 11m;
			xmlUSLine.BasisUnitSpecified = false;
			xmlUSLine.BasisUnit.Price = ZDecimal.Zero;
			xmlUSLine.BasisUnit.PriceSpecified = false;
			xmlUSLine.CountryOfExport = ZString.Empty;
			xmlUSLine.CountryOfOrigin = ZString.Empty;
			xmlUSLine.DateOfExportFromCO = ZDate.Empty;
			xmlUSLine.DestinationState = ZString.Empty;
			xmlUSLine.TransactionRelated = false;
			xmlUSLine.DispatchedInvoiceQty.QtyDiffReasonCode = ZString.Empty;
			xmlUSLine.DispatchedInvoiceQty.QtyDiffReasonDesc = ZString.Empty;
			xmlUSLine.DispatchedInvoiceQty.Quantity.Value = ZDecimal.Zero;
			xmlUSLine.FirstSaleIndicator = false;
			xmlUSLine.FirstSaleIndicatorSpecified = false;
			xmlUSLine.FTZ.PrivilegedStatusDate = ZDate.Empty;
			xmlUSLine.FTZ.ZoneStatus = ZString.Empty;
			xmlUSLine.HazardousMaterial.ClassificationDesc = ZString.Empty;
			xmlUSLine.HazardousMaterial.Desc = ZString.Empty;
			xmlUSLine.ManifestQty = ZDecimal.Zero;
			xmlUSLine.ManifestQtySpecified = false;
			xmlUSLine.NAFTANet = false;
			xmlUSLine.NAFTANetSpecified = false;
			xmlUSLine.PercentageOfActiveIngredient = ZDecimal.Zero;
			xmlUSLine.PercentageOfActiveIngredientSpecified = false;
			xmlUSLine.PermitAndLicence.AgricultureLicNo = ZString.Empty;
			xmlUSLine.PermitAndLicence.CAExportCertificate = ZString.Empty;
			xmlUSLine.PermitAndLicence.CBTPACertificate = ZString.Empty;
			xmlUSLine.PermitAndLicence.CottonCertificateNo = ZString.Empty;
			xmlUSLine.PermitAndLicence.CottonFeeExemptIndicator = ZString.Empty;
			xmlUSLine.PermitAndLicence.MiscPermitNo = ZString.Empty;
			xmlUSLine.PermitAndLicence.SWPMIndicator = ZString.Empty;
			xmlUSLine.PermitAndLicence.WoolLicenceNo = ZString.Empty;
			xmlUSLine.PIRPRuling.Number = ZString.Empty;
			xmlUSLine.PIRPRuling.Type = ZString.Empty;
			xmlUSLine.SecondarySPI = ZString.Empty;
			xmlUSLine.SecondQty.Value = ZDecimal.Zero;
			xmlUSLine.SelectedRateType = ZString.Empty;
			xmlUSLine.SoftwoodLumber.ExportCharge = ZDecimal.Zero;
			xmlUSLine.SoftwoodLumber.ExportChargeSpecified = false;
			xmlUSLine.SoftwoodLumber.ExportPrice = ZDecimal.Zero;
			xmlUSLine.SoftwoodLumber.ExportPriceSpecified = false;
			xmlUSLine.SoftwoodLumber.ImporterDec = false;
			xmlUSLine.SoftwoodLumber.ImporterDecSpecified = false;
			xmlUSLine.SPI = ZString.Empty;
			xmlUSLine.Textile.CategoryNo = ZString.Empty;
			xmlUSLine.Textile.VisaNo = ZString.Empty;
			xmlUSLine.Textile.VisaQty = ZDecimal.Zero;
			xmlUSLine.Textile.VisaQtySpecified = false;
			xmlUSLine.Textile.VisaUQ = ZString.Empty;
			xmlUSLine.ThirdQty.Value = ZDecimal.Zero;
			xmlUSLine.TSCA.Indicator = ZString.Empty;
			xmlUSLine.TSCA.Name = ZString.Empty;
			xmlUSLine.USOrOriginalGoodsValue = ZDecimal.Zero;
			xmlUSLine.USOrOriginalGoodsValueSpecified = false;
			xmlUSLine.TaxCode = Xsd.USInvoiceLineTaxCode.Item016;
			xmlUSLine.OverriddenTaxRate = 0.99m;
			invoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);
			AssertInvoiceLineDetails(invoiceHeader.JobComInvoiceLines[0]);
		}

		void GetTestInvoiceLineWithUSSpeccificData(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_SupTariff = "9817005000";
			invoiceLine.US_SupUQ1 = "CAR";
			invoiceLine.US_SupQty1 = 12m;
			invoiceLine.US_SupUQ2 = "KG";
			invoiceLine.US_SupQty2 = 14m;
			invoiceLine.US_SupUQ3 = "T";
			invoiceLine.US_SupQty3 = 15m;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff;
			invoiceLine.JI_OA_ManufacturerAddress = TestOrganization.Addresses[2].PK;
			invoiceLine.US_98GoodsValue = 135m;
			invoiceLine.US_98ValueInvCurr = 168m;
			invoiceLine.US_IsBondedADD = true;
			invoiceLine.US_ADDCaseNo = "A234";
			invoiceLine.US_ADDDepositRateIndicator = "1";
			invoiceLine.US_ADDDepositValue = 148m;
			invoiceLine.US_ArticleNoA = "A";
			invoiceLine.US_ArticleNoB = "B";
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AIILine aiiLine = invoiceLine.FirstAIILine;
			aiiLine.US_UnitBasis = 11;
			aiiLine.US_UnitPrice = 132m;
			invoiceLine.US_IsBondedCVD = true;
			invoiceLine.US_CVDCaseNo = "C497";
			invoiceLine.US_CVDDepositRateIndicator = "1";
			invoiceLine.US_CVDDepositValue = 46m;
			invoiceLine.US_UC_NKCountryOfExport = "AD";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_DateOfExportFromCountryOfOrigin = new ZDateTime(2007, 3, 4);
			invoiceLine.US_DestinationState = "AL";
			invoiceLine.US_TransactionsRelated = "N";
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 0.99m;
			aiiLine.US_QtyDiffRsnCode = "3";
			aiiLine.US_QtyDiffRsn = "fffd";
			aiiLine.US_InvQtyDisp = 59m;
			aiiLine.US_InvUQDisp = "HY";
			aiiLine.US_PercActvIngr = 1.2m;
			DOT dot = invoiceLine.DOTs.AddNew();
			DOT dot1 = invoiceLine.DOTs.AddNew();
			OGADataTransferToolTest.GetDotsDetails(dot, dot1);
			FCC fcc = invoiceLine.FCCs.AddNew();
			OGADataTransferToolTest.GetFCCDetails(fcc);
			FDA fda = invoiceLine.FDAs.AddNew();
			OGADataTransferToolTest.GetFDADetails(fda, TestOrganization.Addresses[2].PK, TestOrganization.PK);
			PGA pga = invoiceLine.LaceyActLines.AddNew();
			OGADataTransferToolTest.GetLaceyActDetals(pga);
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2007, 3, 4);
			invoiceLine.US_ZoneStatus = "D";
			invoiceLine.US_HazMatClassDesc = "Haz Mat";
			invoiceLine.US_HazMatDesc = "Descr";
			invoiceLine.US_ManifestQty = 1250;
			invoiceLine.US_IsNAFTANet = false;
			invoiceLine.US_AgricultureLicNo = "125";
			invoiceLine.US_CottonCertificateNo = "cotton";
			invoiceLine.US_CAExportCertificate = "cert";
			invoiceLine.US_CBTPACertificateNo = "126";
			invoiceLine.US_MiscPermitNo = "127";
			invoiceLine.US_SWPMIndicator = "1";
			invoiceLine.US_WoolLicenceNo = "131";
			invoiceLine.US_PIRPRulingNo = "165";
			invoiceLine.US_PIRPRulingType = "R";
			invoiceLine.US_SecondarySPI = "A";
			invoiceLine.JI_CustomsSecondUnitQty = "NO";
			invoiceLine.JI_CustomsSecondQuantity = 560;
			invoiceLine.US_SelectedRateType = "1";
			invoiceLine.US_LumberExportCharges = 154.1m;
			invoiceLine.US_LumberExportPrice = 300m;
			invoiceLine.US_LumberImporterDeclaration = "Y";
			invoiceLine.US_SPI = "Z";
			invoiceLine.US_TextileCategoryNo = "882";
			invoiceLine.US_VisaNo = "221";
			invoiceLine.US_VisaQty = 46m;
			invoiceLine.US_VisaUQ = "KG";
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 16.8m;
			invoiceLine.US_TSCAIndicator = "I";
			invoiceLine.US_TSCAName = "Name";
			invoiceLine.JI_NetWeight = 161m;
			invoiceLine.JI_NetWeightUQ = "KJ";
			aiiLine.RegoNumbers.AddNew(RegoNumberCodeList.Codes.ChassisNumber, "C111");
			FeeCusCodeData fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_FeeAmount = 200m;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_FDAIndicator = "D";
			invoiceLine.US_FCCIndicator = "D";
			invoiceLine.US_DOTIndicator = "D";
			ultimateConsignee = factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			factory.Save();
		}

		OrgHeader ultimateConsignee;
		void GetPopulatedXmlInvoiceHeader(Xsd.InvoiceHeader xmlInvoiceHeader, JobComInvoiceHeader invoiceHeader)
		{
			invoiceDataAdapter.ExportToValueObject(invoiceHeader, xmlInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
		}

		Xsd.InvoiceHeader ExportedXmlInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)getUSInvoiceHeaderWithTestData();
			invoiceHeader.JobComInvoiceLines.RemoveAndDeleteAll();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			GetTestInvoiceLineWithUSSpeccificData(invoiceLine);
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			GetPopulatedXmlInvoiceHeader(xmlInvoiceHeader, invoiceHeader);
			return xmlInvoiceHeader;
		}

		OrganisationValueObjectDataAdapter organisationDataAdapter;
		OrganisationValueObjectDataAdapter OrganisationDataAdapter => organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter());

		OrgHeader testOrganization;
		OrgAddress address2;
		OrgHeader TestOrganization
		{
			get
			{
				if (testOrganization == null)
				{
					testOrganization = factory.New<OrgHeader>();
					testOrganization.FillWithValidTestData();
					testOrganization.OH_FullName = "Test Org";
					var countryData = factory.New<OrgCountryData>();
					countryData.OV_OH_OrgHeader = testOrganization.PK;
					countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
					var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
					addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
					var address1 = TestOrganization.Addresses.AddNew();
					address1.OA_Address1 = "Test 1";
					address2 = TestOrganization.Addresses.AddNew();
					address2.OA_Address1 = "Test 2";
					var contact = testOrganization.Contacts.AddNew();
					contact.OC_ContactName = "Test Contact";
				}

				return testOrganization;
			}
		}
	}
}
