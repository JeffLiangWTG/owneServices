using System;
using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using static Enterprise.Core.Constants;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public class CommercialInvoiceHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestUseUnmatchedOrganisationForMatching()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ABC";
			supplier.OH_FullName = "ABC INC.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeaderDataObject.InvoiceNumber = "123";

			invoiceHeaderDataObject.Supplier =
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.USPrincipalPartyInInterest), OrganizationCode = "AAA" };

			invoiceHeaderDataObject.Buyer =
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.UltimateConsignee), OrganizationCode = "BBB" };

			invoiceHeaderDataObject.OrganizationAddressCollection = new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierPickupDeliveryAddress), OrganizationCode = "CCC" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.IntermediateConsignee), OrganizationCode = "DDD" },
			};

			var helper = new UniversalDataObjectReaderHelper(Factory, CurrentCompany.GC_RN_NKCountryCode);
			var reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, logger, helper, declaration.TopGroupInvoice);
			var invoiceBO = (JobComInvoiceHeader)reader.ReadIntoBusinessObject();
			AssertEquals(true, invoiceBO.USPPIDocAddress.E2_AddressOverride);
			AssertEquals(true, invoiceBO.SupplierPickupAddress.E2_AddressOverride);
			AssertEquals(true, invoiceBO.UltimateConsigneeDocAddress.E2_AddressOverride);
			AssertEquals(true, invoiceBO.IntermediateConsigneeDocAddress.E2_AddressOverride);

			var unmatchedOrg = new UnmatchedOrganisation() { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg))
			{
				invoiceBO = (JobComInvoiceHeader)reader.ReadIntoBusinessObject();
				AssertEquals(false, invoiceBO.USPPIDocAddress.E2_AddressOverride);
				AssertEquals(unmatchedOrg.Code, invoiceBO.USPPIDocAddress.Organisation.OH_Code);
				AssertEquals(false, invoiceBO.UltimateConsigneeDocAddress.E2_AddressOverride);
				AssertEquals(unmatchedOrg.Code, invoiceBO.UltimateConsigneeDocAddress.Organisation.OH_Code);
				AssertEquals(false, invoiceBO.SupplierPickupAddress.E2_AddressOverride);
				AssertEquals(unmatchedOrg.Code, invoiceBO.SupplierPickupAddress.Organisation.OH_Code);
				AssertEquals(false, invoiceBO.IntermediateConsigneeDocAddress.E2_AddressOverride);
				AssertEquals(unmatchedOrg.Code, invoiceBO.IntermediateConsigneeDocAddress.Organisation.OH_Code);
			}
		}

		public void TestFillCountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var helper = new UniversalDataObjectReaderHelper(Factory, CurrentCompany.GC_RN_NKCountryCode);

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeaderDataObject.InvoiceNumber = "INVABC123";
			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoiceHeaderDataObject.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			var addInfo = new UniversalAddInfo()
			{
				Key = "UC_NKCountryOfOrigin",
				Value = CountryCodes.France
			};
			invoiceLine.AddInfoCollection.Add(addInfo);
			invoiceLine.CountryOfOrigin = new Country() { Code = CountryCodes.China };
			var reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, logger, helper, declaration.TopGroupInvoice);
			var invoiceBO = reader.ReadIntoBusinessObject();
			var invoiceLineBO = (JobComInvoiceLine)invoiceBO.JobComInvoiceLines[0];
			AssertEquals("Read country from UniversalAddInfo UC_NKCountryOfOrigin", CountryCodes.France, invoiceLineBO.US_UC_NKCountryOfOrigin);

			invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeaderDataObject.InvoiceNumber = "INVABC456";
			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			invoiceLine = new CommercialInvoiceLine();
			invoiceHeaderDataObject.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			addInfo = new UniversalAddInfo()
			{
				Key = "UC_NKCountryOfOrigin",
				Value = ""
			};
			invoiceLine.AddInfoCollection.Add(addInfo);
			invoiceLine.CountryOfOrigin = new Country() { Code = CountryCodes.China };
			reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, logger, helper, declaration.TopGroupInvoice);
			invoiceBO = reader.ReadIntoBusinessObject();
			invoiceLineBO = (JobComInvoiceLine)invoiceBO.JobComInvoiceLines[0];
			AssertEquals("Read country from CountryOfOrigin", CountryCodes.China, invoiceLineBO.US_UC_NKCountryOfOrigin);
		}
	}
}
