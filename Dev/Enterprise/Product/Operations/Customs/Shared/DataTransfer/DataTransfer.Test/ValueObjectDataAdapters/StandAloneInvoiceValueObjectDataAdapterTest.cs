using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(StandAloneInvoiceValueObjectDataAdapter))]
	public class StandAloneInvoiceValueObjectDataAdapterTest : InvoiceDataTransferTest
	{
		public void TestNew()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals(ObjectFactory.GetType<Integration.Customs.AU.IAUStandAloneInvoiceValueObjectDataAdapter>(), StandAloneInvoiceValueObjectDataAdapter.New().GetType());

			GlbCompany.CurrentCompany.SetCountry("NZ");
			AssertEquals(ObjectFactory.GetType<Integration.Customs.NZ.INZStandAloneInvoiceValueObjectDataAdapter>(), StandAloneInvoiceValueObjectDataAdapter.New().GetType());

			GlbCompany.CurrentCompany.SetCountry("US");
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.IUSStandAloneInvoiceDataAdapter>(), StandAloneInvoiceValueObjectDataAdapter.New().GetType());

			GlbCompany.CurrentCompany.SetCountry("PR");
			AssertEquals(ObjectFactory.GetType<Integration.Customs.US.IUSStandAloneInvoiceDataAdapter>(), StandAloneInvoiceValueObjectDataAdapter.New().GetType());

			GlbCompany.CurrentCompany.SetCountry("ER");
			AssertEquals(typeof(StandAloneInvoiceValueObjectDataAdapter), StandAloneInvoiceValueObjectDataAdapter.New().GetType());
		}

		public void TestRefreshesApportionmentAfterImportIsDone()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvoiceHeader invoice = GetInvoiceHeader();

				Xsd.InvoiceHeader xmlInvoice = new Xsd.InvoiceHeader();
				xmlInvoice.InvoiceAmount = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10000), invoice.LocalCurrency);
				Xsd.InvoiceCharge charge = xmlInvoice.InvoiceCharges.AddNew();
				charge.ChargeType = "OFT";
				charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), invoice.LocalCurrency);
				charge.DutyApplies = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@false;
				charge.GstApplies = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@true;

				Xsd.InvoiceLine xmlInvoiceLine = xmlInvoice.InvoiceLines.AddNew();
				xmlInvoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10000), invoice.LocalCurrency);

				invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoice, new ValueObjectImportContext(Factory, new NotificationBuffer()));

				AssertEquals("InvoiceLine should have an apportioned charge", true, invoice.JobComInvoiceLines[0].ApportionedCharges.Count > 0);
				AssertEquals("Invoice apportionment is not dirty any more", false, invoice.JobDeclaration.ApportionmentDirty);
			}
		}

		public void TestExportInvoiceDirection()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader();
			invoiceHeader.JZ_MessageType = "ABC";

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Invoice Direction", "ABC", xmlInvoiceHeader.StandAloneInvoiceDirection);
		}

		public void TestImportFromBranchDetails()
		{
			var branchABC = SetupNewBranch();

			var invoiceXsd = new Xsd.InvoiceHeader();
			invoiceXsd.InvoiceNumber = "TESTINVOICE";
			invoiceXsd.InvoiceAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(123.45), Core.Constants.CurrencyCodes.Afghanistan);
			invoiceXsd.InvoiceDate = ZDate.Today;
			invoiceXsd.Consignee.EDICode = "EDICUS";
			invoiceXsd.Consignor.EDICode = "ABIGASBNE";
			var invoiceLine = invoiceXsd.InvoiceLines.AddNew();
			invoiceLine.InvoiceQty.Value = 10;
			invoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(4.5), Core.Constants.CurrencyCodes.Afghanistan);
			invoiceLine.ProductNumber = "BEER";

			var notification = new NotificationBuffer();
			var importContext = new ValueObjectImportContext(Factory, notification);

			BaseJobComInvoiceHeader importedInvoice = (BaseJobComInvoiceHeader)invoiceDataAdapter.CreateOrUpdateFromValueObject(invoiceXsd, importContext);

			AssertNotNull(importedInvoice);
			AssertEquals("Branch is not specified on the xml, invoice will be allocated tot he current login branch", Env.CurrentBranch.PK, importedInvoice.JZ_GB);
			AssertContains(string.Format("Branch Code is not specified - Invoice branch is set to the current Login Branch '{0}'", Env.CurrentBranch.Code), notification.AsString);

			notification.Clear();
			var xmlInterchange = new Xsd.XmlInterchange();
			xmlInterchange.InterchangeInfo.Target.BranchCode = "ABC";

			importContext = new ValueObjectImportContext(Factory, xmlInterchange, notification);
			importedInvoice = (BaseJobComInvoiceHeader)invoiceDataAdapter.CreateOrUpdateFromValueObject(invoiceXsd, importContext);

			AssertNotNull(importedInvoice);
			AssertEquals("Invoice branch should set to the branch specified in xml", branchABC.PK, importedInvoice.JZ_GB);
			AssertContains("Invoice branch is set to 'ABC'", notification.AsString);

			notification.Clear();
			branchABC.GB_IsActive = false;
			importedInvoice = (BaseJobComInvoiceHeader)invoiceDataAdapter.CreateOrUpdateFromValueObject(invoiceXsd, importContext);

			AssertNotNull(importedInvoice);
			AssertEquals("Invoice branch should set to current branch since the one specified in xml is an inactive branch", Env.CurrentBranch.PK, importedInvoice.JZ_GB);
			AssertContains(string.Format("Branch '{0}' is not found or inactive - Invoice branch is set to the current Login Branch '{1}'", branchABC.GB_Code, Env.CurrentBranch.Code), notification.AsString);

			notification.Clear();
			xmlInterchange.InterchangeInfo.Target.BranchCode = "XXX";
			invoiceXsd.InvoiceNumber = "TESTINVOICE1";

			AssertNull("Precondition: branch 'XXX' doesn't exist", Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "XXX")));
			importContext = new ValueObjectImportContext(Factory, xmlInterchange, notification);
			importedInvoice = (BaseJobComInvoiceHeader)invoiceDataAdapter.CreateOrUpdateFromValueObject(invoiceXsd, importContext);
			AssertNotNull(importedInvoice);
			AssertEquals("Invoice branch should set to current branch since the one specified in xml is an invalid branch", Env.CurrentBranch.PK, importedInvoice.JZ_GB);
			AssertContains(string.Format("Branch '{0}' is not found or inactive - Invoice branch is set to the current Login Branch '{1}'", "XXX", Env.CurrentBranch.Code), notification.AsString);
		}

		public void TestExportWithRouting()
		{
			BaseJobComInvoiceHeader header = GetInvoiceHeader();
			Transport transport = header.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "DCV VESSEL";
			transport.JW_VoyageFlight = "DF2";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2010, 1, 10);
			transport.JW_ATD = new ZDateTime(2010, 1, 11);
			transport.JW_ETA = new ZDateTime(2010, 1, 12);
			transport.JW_ATA = new ZDateTime(2010, 1, 13);

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(header, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("USLAX", xmlInvoiceHeader.Routings[0].PortOfDischarge.Port.Value);
			AssertEquals("AUSYD", xmlInvoiceHeader.Routings[0].PortOfLoading.Port.Value);
			AssertEquals(Xsd.TransportMode.SEA, xmlInvoiceHeader.Routings[0].TransportMode);
			AssertEquals(Xsd.PlannedLegTransportType.MainVessel, xmlInvoiceHeader.Routings[0].TransportType);
			AssertEquals(new ZDateTime(2010, 1, 10), xmlInvoiceHeader.Routings[0].Item.ETD);
			AssertEquals(new ZDateTime(2010, 1, 11), xmlInvoiceHeader.Routings[0].Item.ATD);
			AssertEquals(new ZDateTime(2010, 1, 12), xmlInvoiceHeader.Routings[0].Item.ETA);
			AssertEquals(new ZDateTime(2010, 1, 13), xmlInvoiceHeader.Routings[0].Item.ATA);
		}

		public void TestImportWithRouting()
		{
			BaseJobComInvoiceHeader header = GetInvoiceHeader();

			Xsd.InvoiceHeader xmlInvoice = new Xsd.InvoiceHeader();
			xmlInvoice.Routings.AddNew();
			xmlInvoice.Routings[0].PortOfDischarge.Port.Value = "USLAX";
			xmlInvoice.Routings[0].PortOfLoading.Port.Value = "AUSYD";
			xmlInvoice.Routings[0].Item.ATA = new ZDateTime(2010, 1, 12);

			invoiceDataAdapter.ImportFromValueObject(header, xmlInvoice, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(1, header.Transports.Count);
			AssertEquals("USLAX", header.Transports[0].JW_RL_NKDiscPort);
			AssertEquals("AUSYD", header.Transports[0].JW_RL_NKLoadPort);
		}

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter()
			=> StandAloneInvoiceValueObjectDataAdapter.New();

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), TestFileHelper.GetPathForTesting("EmptyStandAloneInvoice.xml"), ValidationKind.None, "Empty Invoice");

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), TestFileHelper.GetPathForTesting("EmptyStandAloneInvoice.xml"), ValidationKind.None, "Empty Invoice");

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
			=> new BusinessObjectAndExpectedOutputFileName(GetInvoiceHeaderWithTestData(), TestFileHelper.GetPathForTesting("PopulatedStandAloneInvoice.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override BaseJobComInvoiceHeader GetEmptyInvoiceHeader()
		{
			BaseJobComInvoiceHeader result = base.GetEmptyInvoiceHeader();

			result.JZ_MessageType = JobMessageTypeList.Codes.Export;

			return result;
		}

		protected override BaseJobComInvoiceHeader GetInvoiceHeaderWithTestData()
		{
			BaseJobComInvoiceHeader result = base.GetInvoiceHeaderWithTestData();

			result.JZ_MessageType = JobMessageTypeList.Codes.Export;

			return result;
		}

		protected override string ExpectedRootCollectionElementName => "Invoices";

		protected override string ExpectedRootElementName => "InvoiceHeader";

		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			BaseJobComInvoiceHeader result = Factory.New<BaseJobComInvoiceHeader>();

			new FakeDeclarationCreatorForInvoice(result);

			result.JZ_MessageType = JobMessageTypeList.Codes.Export;

			return result;
		}

		protected override BaseJobDeclaration GetJobDeclaration() => InvoiceHeader.JobDeclaration;

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"CountryPayload",

					"Consignor/OrganisationDetails",
					"Consignee/OrganisationDetails",
					"InvoiceLines",
					"AddCustomsDetails",
					"RelatedGroupInvoiceNumber",
					"Consignor",
					"Consignee",
					"Volume/Description",
					"Weight/Description",
					"PackingDetails",//only applicable in the context of a invoice attached to a declaration
					"Packages", //only applicable in the context of a invoice attached to a declaration
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

		GlbBranch SetupNewBranch()
		{
			var result = Factory.New<GlbBranch>();
			result.GB_GC = GlbCompany.CurrentCompany.PK;
			result.GB_Code = "ABC";

			return result;
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		protected TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
