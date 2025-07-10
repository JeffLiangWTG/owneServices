using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	//DO NOT MOVE this test class INSIDE InvoiceValueObjectDataAdapter class. Test end-to-end rather than a specific private or protected method
	//use DataAdapter.ImportFromValueObject or ExportToValueObject to invoke an intended method.
	[TestedType(typeof(InvoiceValueObjectDataAdapter))]
	public class InvoiceValueObjectDataAdapterTest : InvoiceDataTransferTest
	{
		public void TestImportInvoiceLineDetail_NoCusContainer()
		{
			BaseJobComInvoiceHeader invoice = JobDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			var rC_20GP_PK = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20GP")).PK;
			BaseCusContainer container1 = AddCusContainer(JobDec.CusContainers, "CONTAINER1", rC_20GP_PK, Core.Constants.ContainerModes.FCL);

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.ContainerNumbers = new string[] { "CONTAINER9" };

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("Declaration should only container the original container", 1, JobDec.CusContainers.Count);
			AssertCollectionContains("Declaration should only container the original container", container1, JobDec.CusContainers);

			AssertEquals("InvoiceLine Containers should be empty", 0, invoiceLine.ContainersPivot.Count);
		}

		public void TestImportLineLevelContainerInformation()
		{
			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = JobDec.Invoices[0].JobComInvoiceLines.AddNew();

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();

			BaseCusContainer container1 = AddCusContainer(JobDec.CusContainers, "CONTAINER1", RC_20GP_PK, Core.Constants.ContainerModes.FCL);
			BaseCusContainer container2 = AddCusContainer(JobDec.CusContainers, "CONTAINER2", RC_20GP_PK, Core.Constants.ContainerModes.FCL);
			BaseCusContainer container3 = AddCusContainer(JobDec.CusContainers, "CONTAINER3", RC_20GP_PK, Core.Constants.ContainerModes.FCL);

			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.ContainerNumbers = new string[2];
			xmlInvoiceLine.ContainerNumbers[0] = "CONTAINER1";
			xmlInvoiceLine.ContainerNumbers[1] = "CONTAINER2";

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("Container Count", 2, invoiceLine.ContainersPivot.Count);
			Assert("InvoiceLine Containers should contain Container1", invoiceLine.ContainersPivot.Contains(container1));
			Assert("InvoiceLine Containers should contain Container2", invoiceLine.ContainersPivot.Contains(container2));
			Assert("InvoiceLine Containers should not contain Container3", !invoiceLine.ContainersPivot.Contains(container3));
		}

		public void TestImportInvoiceLineDetail_DuplicateCusContainer()
		{
			BaseJobComInvoiceHeader invoice = InvoiceHeader;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			JobDec.JE_RL_NKPortOfArrival = "AUMEL";
			JobDec.JE_MessageType = "IMP";

			var rC_20GP_PK = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20GP")).PK;
			BaseCusContainer container1 = AddCusContainer(JobDec.CusContainers, "CONTAINER1", rC_20GP_PK, Core.Constants.ContainerModes.FCL);

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.ContainerNumbers = new string[] { "CONTAINER1", "CONTAINER1" };

			invoiceDataAdapter.ImportFromValueObject(invoice, xmlInvoiceHeader, importContext);

			AssertEquals("Declaration should only container the original container", 1, JobDec.CusContainers.Count);
			AssertCollectionContains("Declaration should only container the original container", container1, JobDec.CusContainers);

			AssertEquals("InvoiceLine Containers should only have one", 1, invoiceLine.ContainersPivot.Count);
		}

		public void TestExportLineLevelContainerInformation()
		{
			BaseJobComInvoiceHeader invoice = GetInvoiceHeaderWithTestData();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines[0];

			BaseCusContainer container1 = AddCusContainer(JobDec.CusContainers, "CONTAINER1", RC_20GP_PK, Core.Constants.ContainerModes.FCL);
			BaseCusContainer container2 = AddCusContainer(JobDec.CusContainers, "CONTAINER2", RC_20GP_PK, Core.Constants.ContainerModes.FCL);
			BaseCusContainer container3 = AddCusContainer(JobDec.CusContainers, "CONTAINER3", RC_20GP_PK, Core.Constants.ContainerModes.FCL);

			invoiceLine.ContainersPivot.AddPivotFor(container1);
			invoiceLine.ContainersPivot.AddPivotFor(container3);

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines[0];

			AssertNotNull("ContainerNumbers", xmlInvoiceLine.ContainerNumbers);
			AssertEquals("ContainerNumbers Length", 2, xmlInvoiceLine.ContainerNumbers.Length);
			AssertCollectionContains("ContainerNumbers should contain CONTAINER1", "CONTAINER1", xmlInvoiceLine.ContainerNumbers);
			AssertCollectionNotContains("ContainerNumbers should not contain CONTAINER2", "CONTAINER2", xmlInvoiceLine.ContainerNumbers);
			AssertCollectionContains("ContainerNumbers should contain CONTAINER3", "CONTAINER3", xmlInvoiceLine.ContainerNumbers);
		}

		public void TestExportLandedCostingValues()
		{
			if (IsExportToValueObjectSupported)
			{
				var accTaxRate = AccTaxRate.FindExistingTaxRate(Factory, "GST", AccTaxRate.Types.Rated, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				if (accTaxRate == null)
				{
					accTaxRate = Factory.New<AccTaxRate>();
					accTaxRate.AT_Code = "GST";
					accTaxRate.AT_IsActive = true;
					accTaxRate.AT_Type = AccTaxRate.Types.Rated;
					accTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				accTaxRate.SetRateNumerator_ForTestOnly(GSTVATRate);

				using (AccountingConfigurationRegistry.Instance.MainGSTTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, accTaxRate.PK.ToGuid()))
				{
					TestExportLandedCostingValuesCore();
				}
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestAllPropertiesExistinLandedCosting()
		{
			if (IsExportToValueObjectSupported)
			{
				BusinessObject landedCostingHistory = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.LandedCosting.ILandedCostHistory)));

				for (int i = 1; i <= 3; i++)
				{
					landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.EffectiveMarkUpPercentage + i.ToString()].GetType();
					landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.LH_LandedCostMarginPercent + i.ToString()].GetType();
					landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.RoundedSellPrice + i.ToString() + InvoiceDataTransferTool.LandedCostPropertyNames.ExGST].GetType();
					landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.RoundedSellPrice + i.ToString() + InvoiceDataTransferTool.LandedCostPropertyNames.IncGST].GetType();
				}

				for (int i = 1; i <= 6; i++)
				{
					landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.LH_LandedCostGroup + i.ToString()].GetType();
				}

				landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.RoundedPerUnitCustomsDisbursementCharges].GetType();
				landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.RoundedPerUnitLandingCost].GetType();
				landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.RoundedPerUnitTotalCost].GetType();
				landedCostingHistory[InvoiceDataTransferTool.LandedCostPropertyNames.UnitPriceInLocalCurrency].GetType();
			}
		}

		public const string EmptyInvoiceFileName = "EmptyInvoice_Whidbey.xml";

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
					"StandAloneInvoiceDirection",
					"PackingDetails",//only applicable in the context of a invoice attached to a declaration
					"Packages",//only applicable in the context of a invoice attached to a declaration
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

		protected BaseJobComInvoiceLine SetupLandedCostHistoryForInvoiceLine()
		{
			BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeaderWithTestData();

			BaseJobDeclaration jobDec = invoiceHeader.JobDeclaration;
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = jobDec.LocalCurrencyCode;
			invoiceHeader.JZ_InvoiceCurrLandedCostExRate = 1m;

			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_Description = "TOYS";
			invoiceLine.JI_LinePrice = 10000m;

			Factory.Save();

			BusinessObject landedCostingHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostingHeader[LandedCostHeaderSchema.LT_ParentTableCode] = "JE";
			landedCostingHeader[LandedCostHeaderSchema.LT_ParentID] = jobDec.PK;

			BusinessObject landedCostingHistory = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHistory>();
			landedCostingHistory[LandedCostHistorySchema.LH_ParentID] = invoiceLine.PK;
			landedCostingHistory[LandedCostHistorySchema.LH_ParentTableCode] = JobComInvoiceLineSchema.Constants.Prefix;
			AddChildItemToLandedCostHistory(landedCostingHistory, "ENT", 20m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "GR1", 100m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "GR2", 200m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "GR3", 300m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "GR4", 400m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "GR5", 500m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "GR6", 0m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "MSC", 0m);
			landedCostingHistory[LandedCostHistorySchema.LH_LT] = landedCostingHeader.PK;
			AddChildItemToLandedCostHistory(landedCostingHistory, "ST1", 5m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "ST2", 6m);
			AddChildItemToLandedCostHistory(landedCostingHistory, "ST3", 7m);
			landedCostingHistory[LandedCostHistorySchema.LH_LandedCostMarginPercent1] = 8m;
			landedCostingHistory[LandedCostHistorySchema.LH_LandedCostMarginPercent2] = 9m;
			landedCostingHistory[LandedCostHistorySchema.LH_LandedCostMarginPercent3] = 10m;
			AddChildItemToLandedCostHistory(landedCostingHistory, "OTH", 300m);
			landedCostingHistory[LandedCostHistorySchema.LH_LandedCostHistoryLineType] = "ACT";
			Factory.Save();

			return invoiceLine;
		}

		protected virtual ZInt GSTVATRate => 0;

		protected virtual void TestExportLandedCostingValuesCore()
		{
			BaseJobComInvoiceLine invoiceLine = SetupLandedCostHistoryForInvoiceLine();

			NotificationBuffer notify = new NotificationBuffer();

			Xsd.InvoiceHeader xmlInvoice = (Xsd.InvoiceHeader)invoiceDataAdapter.ExportToValueObject(invoiceLine.InvoiceHeader, new ValueObjectExportContext(notify));
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoice.InvoiceLines[0];

			Xsd.LandedCostingInfo landedCostingXml = xmlInvoiceLine.LandedCosting;

			AssertEquals("LandedCosting is specified", true, xmlInvoiceLine.LandedCosting.IsSpecified);
			AssertEquals("Invoice Line Type", "ACT", landedCostingXml.LineType);
			AssertEquals("Excise shouldn't be specified", false, landedCostingXml.Excise.IsSpecified);

			var localCurrencyCode = invoiceLine.InvoiceHeader.LocalCurrencyCode;
			AsserteFinancialValue("Total Cost Per Unit", landedCostingXml.TotalCostPerUnit, 118.38m, localCurrencyCode);
			AsserteFinancialValue("Unit Price in Local Currency", landedCostingXml.UnitPriceInLocalCurrency, 100m, localCurrencyCode);
			AsserteFinancialValue("Duty And Taxes Per Unit", landedCostingXml.DutiesAndTaxesPerUnit, 3.38m, localCurrencyCode);
			AsserteFinancialValue("Entry Fees", landedCostingXml.EntryFees, 20m, localCurrencyCode);
			AsserteFinancialValue("Landing Cost Per Unit", landedCostingXml.LandingCostPerUnit, 15m, localCurrencyCode);

			AssertEquals("selldetails is specified", true, landedCostingXml.SellDetails.IsSpecified);
			AssertEquals("lcGroupCharges is specified", true, landedCostingXml.LCGroupCharges.IsSpecified);
			AssertEquals("lcMisc is not specified", false, landedCostingXml.LCMisc.IsSpecified);

			//group charge
			Assert("Group 1", AssertGroupCharges(1, landedCostingXml.LCGroupCharges, 100m, false, localCurrencyCode));
			Assert("Group 2", AssertGroupCharges(2, landedCostingXml.LCGroupCharges, 200m, false, localCurrencyCode));
			Assert("Group 3", AssertGroupCharges(3, landedCostingXml.LCGroupCharges, 300m, false, localCurrencyCode));
			Assert("Group 4", AssertGroupCharges(4, landedCostingXml.LCGroupCharges, 400m, false, localCurrencyCode));
			Assert("Group 5", AssertGroupCharges(5, landedCostingXml.LCGroupCharges, 500m, false, localCurrencyCode));
			Assert("Group 6", AssertGroupCharges(6, landedCostingXml.LCGroupCharges, 0m, true, localCurrencyCode));

			//sell Details
			AssertEquals("Sell Details", true, AssertSellDetails(1, landedCostingXml.SellDetails, 127.85m, 127.85m, 8m, 8m, localCurrencyCode));
			AssertEquals("Sell Details", true, AssertSellDetails(2, landedCostingXml.SellDetails, 129.03m, 129.03m, 9m, 9m, localCurrencyCode));
			AssertEquals("Sell Details", true, AssertSellDetails(3, landedCostingXml.SellDetails, 130.22m, 130.22m, 10m, 10m, localCurrencyCode));

			AssertEquals("Other Duty", false, landedCostingXml.OtherDuty.IsSpecified);

			//special Tax
			AssertEquals("Special Tax", true, AssertSpecialTax(1, landedCostingXml.SpecialTax, 5m, localCurrencyCode));
			AssertEquals("Special Tax", true, AssertSpecialTax(2, landedCostingXml.SpecialTax, 6m, localCurrencyCode));
			AssertEquals("Special Tax", true, AssertSpecialTax(3, landedCostingXml.SpecialTax, 7m, localCurrencyCode));
		}

		protected bool AssertSellDetails(int sequence, Xsd.LandedCostSellDetailsCollection landedCostSellDetails,
			ZDecimal sellAmountExGST, ZDecimal sellAmountIncGST,
			ZDecimal effectiveMarkUpPercent, ZDecimal markUpPercent, ZString localCurrencyCode)
		{
			bool result = false;

			foreach (Xsd.LandedCostSellDetails current in landedCostSellDetails)
			{
				if (current.Sequence == sequence)
				{
					AsserteFinancialValue(InvoiceDataTransferTool.LandedCostPropertyNames.RoundedSellPrice + sequence.ToString() + InvoiceDataTransferTool.LandedCostPropertyNames.ExGST, current.SellPriceExGST, sellAmountExGST, localCurrencyCode);
					AsserteFinancialValue(InvoiceDataTransferTool.LandedCostPropertyNames.RoundedSellPrice + sequence.ToString() + InvoiceDataTransferTool.LandedCostPropertyNames.IncGST, current.SellPriceIncGST, sellAmountIncGST, localCurrencyCode);

					if (markUpPercent != 0)
					{
						AssertEquals("CostMarginPercentage " + sequence.ToString(), markUpPercent, current.MarkUpPercentage);
					}
					else
					{
						AssertEquals("CostMarginPercentage " + sequence.ToString(), false, current.MarkUpPercentage.IsEmpty);
					}
					AssertEquals("EffectiveCostMarginPercentage " + sequence.ToString(), effectiveMarkUpPercent, current.MarkUpPercentage);
					result = true;
					break;
				}
			}

			return result;
		}

		protected bool AssertGroupCharges(int id, Xsd.LandedCostGroupChargeCollection landedCostCharges, ZDecimal amount, bool shouldBeNull, ZString localCurrencyCode)
		{
			Xsd.LandedCostGroupCharge grpChargeXml = null;

			bool result = false;

			foreach (Xsd.LandedCostGroupCharge current in landedCostCharges)
			{
				if (current.Id == id)
				{
					AsserteFinancialValue(InvoiceDataTransferTool.LandedCostPropertyNames.LH_LandedCostGroup + id.ToString(), current.ChargesAmount, amount, localCurrencyCode);
					result = true;
					grpChargeXml = current;
					break;
				}
			}

			if (shouldBeNull)
			{
				AssertNull("Group charge with id " + id.ToString() + "is not exported", grpChargeXml);
				result = true;
			}

			return result;
		}

		protected bool AssertSpecialTax(int sequence, Xsd.SpecialTaxCollection specialTaxes, ZDecimal amount, ZString localCurrencyCode)
		{
			Xsd.SpecialTax taxXml = null;

			bool result = false;

			foreach (Xsd.SpecialTax current in specialTaxes)
			{
				if (current.Sequence == sequence)
				{
					AsserteFinancialValue(InvoiceDataTransferTool.LandedLineCostTypes.SpecialTaxPrefix + sequence.ToString(), current.TaxAmount, amount, localCurrencyCode);
					result = true;
					taxXml = current;
					break;
				}
			}

			return result;
		}

		protected void AsserteFinancialValue(ZString propertyName, Xsd.FinancialValue financialValue, ZDecimal amount, ZString localCurrencyCode)
		{
			AssertEquals(propertyName + "'s amount", amount, financialValue.Value);
			AssertEquals(propertyName + "'s currency code", localCurrencyCode, financialValue.CurrencyCode);
		}

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter() => new InvoiceValueObjectDataAdapter(JobDec);

		protected override bool IsExportToCollectionSupported => false;

		protected override string ExpectedRootCollectionElementName => null;

		protected override string ExpectedRootElementName => "InvoiceHeader";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), TestFileHelper.GetPathForTesting(EmptyInvoiceFileName), ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetInvoiceHeaderWithTestData(), TestFileHelper.GetPathForTesting("PopulatedInvoice.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		ZGuid RC_20GP_PK
		{
			get
			{
				if (fRC_20GP_PK.IsEmpty)
				{
					fRC_20GP_PK = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20GP")).PK;
				}
				return fRC_20GP_PK;
			}
		}
		ZGuid fRC_20GP_PK;

		BaseCusContainer AddCusContainer(ICusContainerCollection<BaseCusContainer> containers, ZString containerNum, ZGuid containerType, ZString mode)
		{
			BaseCusContainer container = containers.AddNew();
			container.CO_ContainerNumber = containerNum;
			container.CO_RC = containerType;
			container.CO_FCL_LCL_AIR = mode;
			return container;
		}

		void AddChildItemToLandedCostHistory(BusinessObject landedCostingHistory, string costType, decimal costAmount)
		{
			BusinessObject landedLineCostItem = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedLineCostItem>();
			landedLineCostItem[LandedLineCostItemSchema.LZ_CostAmount] = costAmount;
			landedLineCostItem[LandedLineCostItemSchema.LZ_CostType] = costType;
			landedLineCostItem[LandedLineCostItemSchema.LZ_LH] = landedCostingHistory.PK;
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
