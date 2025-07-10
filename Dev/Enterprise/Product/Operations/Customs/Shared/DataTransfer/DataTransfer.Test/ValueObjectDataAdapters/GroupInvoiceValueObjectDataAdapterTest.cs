using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(GroupInvoiceValueObjectDataAdapter))]
	sealed class GroupInvoiceValueObjectDataAdapterTest : ValueObjectDataAdapterTest<BaseJobComInvoiceGroupHeader, Xsd.InvoiceHeader>
	{
		public void TestFromXmlValueObjectCore()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.InvoiceNumber = "All Invoices";

			BaseJobDeclaration bizObj = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader invoiceHeader = bizObj.JobComInvoiceGroupHeaders[0];
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			groupInvoiceXmlDataAdapter.TestFromXmlValueObjectCore(invoiceHeader, xmlInvoiceHeader, context);

			AssertEquals("All Invoices", invoiceHeader.JZ_InvoiceNumber);

			Factory.Save();
			Assert("Factory.Save should not throw exception", true);
		}

		public void TestNewBusinessObject()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			xmlInvoiceHeader.IsGroupInvoice = Xsd.TrueFalse.@true;
			AssertEquals(typeof(BaseJobComInvoiceGroupHeader), groupInvoiceXmlDataAdapter.TestNewBusinessObject(Factory, xmlInvoiceHeader).GetType());
		}

		public void TestExportToValueObjectGroupInvoice()
		{
			BaseJobDeclaration bizObj = Factory.NewWithValidTestData<BaseJobDeclaration>();
			bizObj.JE_MessageType = "IMP";

			BaseJobComInvoiceGroupHeader groupInvoiceHeader = bizObj.JobComInvoiceGroupHeaders[0];

			Xsd.InvoiceHeader xmlGroupInvoiceHeader = new Xsd.InvoiceHeader();
			groupInvoiceXmlDataAdapter.TestToXmlValueObject(groupInvoiceHeader, xmlGroupInvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("All Invoices", xmlGroupInvoiceHeader.InvoiceNumber);
		}

		public void TestAddImportEvent()
		{
			Xsd.InvoiceHeader xsdInvoice = new Xsd.InvoiceHeader();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BaseJobComInvoiceGroupHeader importedInvoice = groupInvoiceXmlDataAdapter.CreateOrUpdateFromValueObject(xsdInvoice, context);
			StmALog[] dataImportEvents = importedInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to new group invoice", 1, dataImportEvents.Length);

			Factory.Save();

			importedInvoice = groupInvoiceXmlDataAdapter.CreateOrUpdateFromValueObject(xsdInvoice, context);
			dataImportEvents = importedInvoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to NEW group invoice", 1, dataImportEvents.Length);
		}

		public void TestAddExportEvent()
		{
			BaseJobDeclaration bizObj = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader invoice = bizObj.JobComInvoiceGroupHeaders[0];

			StmALog[] dataExportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to group invoice", 0, dataExportEvents.Length);

			groupInvoiceXmlDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to group invoice", 1, dataExportEvents.Length);

			groupInvoiceXmlDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to group invoice", 2, dataExportEvents.Length);
		}

		protected override string ExpectedRootCollectionElementName => null;

		protected override string ExpectedRootElementName => "InvoiceHeader";

		protected override bool IsExportToCollectionSupported => false;

		protected override ValueObjectDataAdapter<BaseJobComInvoiceGroupHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter()
		{
			return new GroupInvoiceValueObjectDataAdapter(Factory.New<BaseJobDeclaration>());
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyGroupInvoiceHeader(), TestFileHelper.GetPathForTesting("EmptyGroupInvoice.xml"), ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetGroupInvoiceHeaderWithTestData(), TestFileHelper.GetPathForTesting("PopulatedGroupInvoice.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
						"CountryPayload",
						"Incoterm",
						"ExchangeRate",
						"Consignor/OrganisationDetails",
						"InvoiceLines",
						"AddCustomsDetails",
						"Weight",
						"Volume",
						"RelatedGroupInvoiceNumber",
						"Consignor",
						"ValuationDate",
						"InvoiceAmount",
						"PackingDetails",//only applicable to non-group invoices
						"Packages",//covered by a separate test
						"Consignee",
						"StandAloneInvoiceDirection",

					"Routings/PortOfLoading/Port/Country", //covered by another xml data adapter
					"Routings/PortOfLoading/Port/City",
					"Routings/PortOfLoading/Port/Value",
					"Routings/PortOfLoading/EstimatedDateTime",
					"Routings/PortOfLoading/ActualDateTime",
					"Routings/PortOfDischarge/Port/Country",
					"Routings/PortOfDischarge/Port/City",
					"Routings/PortOfDischarge/Port/Value",
					"Routings/PortOfDischarge/EstimatedDateTime",
					"Routings/PortOfDischarge/ActualDateTime",
					"Routings/TransportType",
					"Routings/Item/ETD",
					"Routings/Item/ETA",
					"Routings/Item/ATD",
					"Routings/Item/ATA",
					"Routings/Item/LoadPortETA",
					"Routings/Item/LoadPortATA",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Name",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Location/Country",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Location/City",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Location/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/CompanyName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Location/Country",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Location/City",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Location/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/AddressType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/AddressLine1",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/AddressLine2",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/AddressCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/CityOrSuburb",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/StateOrProvince",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/PostCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Email",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Name",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Salutation",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/NotifyMode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/JobTitle",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Phone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/PhoneExtension",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Fax",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Mobile",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/HomePhone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Pager",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/OtherPhone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/AttachmentType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/WebAccessEnable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/WebContractSignDate",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Birthday",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/EmailAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Contacts/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/OrgWebURLs/URL",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/OrgWebURLs/Description",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/OrgWebURLs/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/WebAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/RegistrationNumbers/Number",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/RegistrationNumbers/AddressCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/EDITransmissionDetails/Address",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/OrganisationTypes/Status",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/EDICodeMappings/Relationship",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/EDICodeMappings/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/EDICodeMappings/ForeignCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BrandNames/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/AccountGroup",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/CreditLimit",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/CreditApproved",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/AddressCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrganisationTypes/Status",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/ActivityCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Type",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Number",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/SuretyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Amount",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Effective",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Expiry",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/FiledPort",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/IsActiveClient",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/AllowMultiCurrencyPayment",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/CompanyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/ExternalCreditorCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/ExternalDebtorCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/ExternalCreditorCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/ExternalDebtorCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/ExternalDebtorCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/ExternalCreditorCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/ExternalDebtorCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/ExternalCreditorCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/AccountGroup",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/CreditLimit",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/PaymentTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/PaymentDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/AddressCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrganisationTypes/Status",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AllowMultiCurrencyPayment",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/ActivityCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Type",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Number",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/SuretyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Amount",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Effective",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Expiry",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/FiledPort",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/IsActiveClient",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/CompanyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/ActivityCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/Type",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/Number",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/SuretyCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/Amount",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/Effective",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/Expiry",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/BondDetails/FiledPort",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/IsActiveClient",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/OwnerCode",
					"Routings/Item/DepartureCTO/Organisation/Notes/CustomNoteTypeName",
					"Routings/Item/DepartureCTO/Organisation/Notes/NoteData",
					"Routings/Item/DepartureCTO/Organisation/Notes/NoteCreatedDateTime",
					"Routings/Item/DepartureCTO/Organisation/EDICode",
					"Routings/Item/DepartureCTO/Organisation/OwnerCode",
					"Routings/Item/DepartureBerth",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Name",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Location/Country",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Location/City",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Location/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/CompanyName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Location/Country",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Location/City",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Location/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/AddressCapabilities/IsMainAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/AddressCapabilities/AddressType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/AddressType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/AddressLine1",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/AddressLine2",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/AddressCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/CityOrSuburb",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/StateOrProvince",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/PostCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/TelephoneNumbers/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Email",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Name",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Salutation",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/NotifyMode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/JobTitle",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Phone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/PhoneExtension",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Fax",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Mobile",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/HomePhone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Pager",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/OtherPhone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/AttachmentType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/WebAccessEnable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/WebContractSignDate",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Birthday",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/EmailAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Contacts/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/OrgWebURLs/URL",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/OrgWebURLs/Description",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/OrgWebURLs/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/WebAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/RegistrationNumbers/CountryOfRegistration",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/RegistrationNumbers/Number",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/RegistrationNumbers/AddressCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/EDITransmissionDetails/Address",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/OrganisationTypes/Status",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/EDICodeMappings/Relationship",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/EDICodeMappings/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/EDICodeMappings/ForeignCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BrandNames/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/DefaultCurrency",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/AccountGroup",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/CreditLimit",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/UseSettlementGroupCreditLimit",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/CreditApproved",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/CreditOnHold",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/GSTIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/WithholdingTaxIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Name",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Country",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/City",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Location/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Email",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Addresses/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Name",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/WebAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/RegistrationNumbers/AddressCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OrganisationTypes/Status",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/DefaultCurrency",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/AccountGroup",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CreditLimit",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/GSTIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/WithholdingTaxIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/PaymentDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/AccountsPayables/CompanyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/ActivityCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Type",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Number",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/SuretyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Amount",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Effective",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/Expiry",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/BondDetails/FiledPort",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/IsActiveClient",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/OwnerCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/AllowMultiCurrencyPayment",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/CompanyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/DefaultCurrency",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/AccountGroup",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/CreditLimit",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/GSTIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/WithholdingTaxIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/PaymentTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/PaymentDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Name",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Country",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/City",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Location/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CompanyName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Country",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/City",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Location/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/IsMainAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCapabilities/AddressType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine1",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressLine2",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/AddressCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/CityOrSuburb",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/StateOrProvince",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/PostCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/TelephoneNumbers/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Email",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Addresses/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Name",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Salutation",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/NotifyMode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/JobTitle",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Phone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/PhoneExtension",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Fax",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Mobile",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/HomePhone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Pager",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/OtherPhone",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/AttachmentType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebAccessEnable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/WebContractSignDate",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Birthday",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/EmailAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Contacts/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/URL",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Description",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrgWebURLs/Sequence",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/WebAddress",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/Language",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/CountryOfRegistration",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/Number",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/RegistrationNumbers/AddressCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDITransmissionDetails/Address",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OrganisationTypes/Status",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/Relationship",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICodeMappings/ForeignCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BrandNames/Value",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/DefaultCurrency",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AccountGroup",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditLimit",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/UseSettlementGroupCreditLimit",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditApproved",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CreditOnHold",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/GSTIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/WithholdingTaxIsApplicable",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/StandardInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceTerms",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/SettlementDetails/DisbursementInvoiceDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/AllowMultiCurrencyPayment",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/AccountsReceivables/CompanyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/ActivityCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Type",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Number",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/SuretyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Amount",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Effective",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/Expiry",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/BondDetails/FiledPort",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/IsActiveClient",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/OwnerCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/CompanyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/ActivityCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/Type",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/Number",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/SuretyCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/Amount",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/Effective",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/Expiry",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/BondDetails/FiledPort",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/IsActiveClient",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/OwnerCode",
					"Routings/Item/ArrivalCTO/Organisation/Notes/CustomNoteTypeName",
					"Routings/Item/ArrivalCTO/Organisation/Notes/NoteData",
					"Routings/Item/ArrivalCTO/Organisation/Notes/NoteCreatedDateTime",
					"Routings/Item/ArrivalCTO/Organisation/EDICode",
					"Routings/Item/ArrivalCTO/Organisation/OwnerCode",
					"Routings/Item/ArrivalBerth",
					"Routings/Item/DocCutOffDate",
					"Routings/Item/IsTranshipment",
					"Routings/Item/IsPublished",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"Routings/Item/DepartureCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsReceivables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/AccountsPayables/SettlementDetails/SettlementGroup/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/SubmitterFirmType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FirstName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/LastName",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/PhoneNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FaxNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/Email",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/ProducerFirmType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/FDA/FoodFacilityRegistrationExemption",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/FileTheirOwnRecon",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/Issue",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Reconciliation/NAFTA",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PayersUnitNo",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/StatementPrintDateWorkingDays",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/DoNotAutoGenerateStmDayChangeRequest",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/PaymentType",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/TaxDeferredInd",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/ImporterOfRecordDetails/Purchased",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/ProductCode",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute1",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute2",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/EntryDocumentPrinting/CustomAttribute3",
					"Routings/Item/ArrivalCTO/Organisation/OrganisationDetails/CountrySpecificDetails/USOrganisationSpecificDetails/Misc/BIRDDefaultBranch",
					"Routings/LegOrderNumber",
					"References/Type",
					"References/Value"
				};
			}
		}

		protected override BaseJobComInvoiceGroupHeader NewBusinessObject()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			return jobDec.JobComInvoiceGroupHeaders[0];
		}

		protected override void SetUp()
		{
			base.SetUp();
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			groupInvoiceXmlDataAdapter = new GroupInvoiceXmlDataAdapterTestClass(jobDec);
		}

		BaseJobComInvoiceGroupHeader GetEmptyGroupInvoiceHeader()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.MakeNonPersistent();

			BaseJobComInvoiceGroupHeader invoiceHeader = jobDec.JobComInvoiceGroupHeaders[0];

			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);

			return invoiceHeader;
		}

		BaseJobComInvoiceGroupHeader GetGroupInvoiceHeaderWithTestData()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();

			BaseJobComInvoiceGroupHeader invoiceHeader = jobDec.JobComInvoiceGroupHeaders[0];

			invoiceHeader.JZ_GroupInvoice = true;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);

			BaseGroupInvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_Amount = 32;
			invoiceCharge.J7_ChargeType = "Cha";
			invoiceCharge.J7_IsDutiable = true;
			invoiceCharge.J7_IsGSTApplicable = true;
			invoiceCharge.J7_IsIncludedInITOT = true;
			invoiceCharge.J7_RX_NKCurrency = "AUD";

			return invoiceHeader;
		}

		sealed class GroupInvoiceXmlDataAdapterTestClass : GroupInvoiceValueObjectDataAdapter
		{
			public GroupInvoiceXmlDataAdapterTestClass(BaseJobDeclaration jobDec) : base(jobDec)
			{
			}

			public BaseJobComInvoiceGroupHeader TestNewBusinessObject(BusinessObjectFactory factory, Xsd.InvoiceHeader value)
			{
				NotificationBuffer notification = new NotificationBuffer();
				ValueObjectImportContext context = new ValueObjectImportContext(factory, notification);
				return NewBusinessObject(value, context);
			}

			public void TestFromXmlValueObjectCore(BaseJobComInvoiceGroupHeader bizObj, Xsd.InvoiceHeader value, ValueObjectImportContext context)
			{
				ImportFromValueObjectCore(bizObj, value, context);
			}

			public void TestToXmlValueObject(BaseJobComInvoiceGroupHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectExportContext context)
			{
				ExportToValueObject(invoiceHeader, xmlInvoiceHeader, context);
			}
		}

		GroupInvoiceXmlDataAdapterTestClass groupInvoiceXmlDataAdapter;

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
