using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CommercialInvoiceMatcherTest : TestCaseWithFactory
	{
		public void TestTryGetMatchedSupplier()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "SUP00";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "SUP01";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV000";
			invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV000";
			invoiceHeader2.JZ_OH_Supplier = orgHeader2.PK;

			Factory.Save();

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(),
					SubGroupCollection = new List<CommercialInfo>()
				}
			};

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV000",
				Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance),
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

			invoiceHeaderDataObject.Supplier.OrganizationCode = new ZCodeMappedZString("SUP01");
			invoiceHeaderDataObject.CommercialInvoiceLineCollection.Content = CollectionContent.Partial;

			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invoiceHeaderDataObject);

			var logger = new TestErrorLogger();
			var matcher = new CommercialInfoMatcher(declaration, logger, new UniversalObjectFactory(Factory), declarationDataObject.CommercialInfo, null);

			Assert("Should be false as we didnt cache the full matching history for invoice data.", !matcher.TryGetMatchedSupplier(invoiceHeaderDataObject.Supplier, out var orgAddress));
			Assert("Cache the match history.", matcher.HasMatchedResult);

			OrgAddress matchedAddress;

			Assert("Should be true as we had cache the full matching history for invoice data.", matcher.TryGetMatchedSupplier(invoiceHeaderDataObject.Supplier, out matchedAddress));
			AssertEquals("Should be the address of orgHeader2 as it has a same organization code.", orgHeader2.PK, matchedAddress.OA_OH);
		}

		public void TestGetInvoices()
		{
			#region Universal Shipment

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(),
					SubGroupCollection = new List<CommercialInfo>()
				}
			};

			var invDataObject1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV001",
				Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance),
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

			var invDataObject2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV002",
				Supplier = null,
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

			var invDataObject5 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV001",
				Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance),
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

			invDataObject1.Supplier.OrganizationCode = new ZCodeMappedZString("SUP00");
			invDataObject5.Supplier.OrganizationCode = new ZCodeMappedZString("SUP00");

			invDataObject1.CommercialInvoiceLineCollection.Content = CollectionContent.Partial;
			invDataObject2.CommercialInvoiceLineCollection.Content = CollectionContent.Partial;
			invDataObject5.CommercialInvoiceLineCollection.Content = CollectionContent.Partial;

			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invDataObject1);
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invDataObject2);
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invDataObject5);

			var subGroupInfo = new CommercialInfo
			{
				Name = "GROUP000",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(),
				SubGroupCollection = new List<CommercialInfo>()
			};

			declarationDataObject.CommercialInfo.SubGroupCollection.Add(subGroupInfo);

			var invDataObject3 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV003",
				Supplier = null,
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

			invDataObject3.CommercialInvoiceLineCollection.Content = CollectionContent.Partial;

			var invDataObject4 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV004",
				Supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance),
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));

			invDataObject4.CommercialInvoiceLineCollection.Content = CollectionContent.Complete;
			invDataObject4.Supplier.OrganizationCode = new ZCodeMappedZString("SUP00");

			subGroupInfo.CommercialInvoiceCollection.Add(invDataObject3);
			subGroupInfo.CommercialInvoiceCollection.Add(invDataObject4);

			#endregion

			#region Declaration

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUP00";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_OH_Supplier = supplier.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV002";
			invoice2.JZ_OH_Supplier = ZGuid.Empty;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "INV003";
			invoice3.JZ_OH_Supplier = supplier.PK;

			var subGroup = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			subGroup.JZ_InvoiceNumber = "GROUP000";

			var invoice4 = subGroup.JobComInvoiceHeaders.AddNew();
			invoice4.JZ_InvoiceNumber = "INV003";
			invoice4.JZ_OH_Supplier = supplier.PK;

			var invoice5 = subGroup.JobComInvoiceHeaders.AddNew();
			invoice5.JZ_InvoiceNumber = "INV004";
			invoice5.JZ_OH_Supplier = supplier.PK;

			var invoice6 = subGroup.JobComInvoiceHeaders.AddNew();
			invoice6.JZ_InvoiceNumber = "INV005";
			invoice6.JZ_OH_Supplier = supplier.PK;

			Factory.Save();

			#endregion

			var logger = new TestErrorLogger();
			var matcher = new CommercialInfoMatcher(declaration, logger, new UniversalObjectFactory(Factory), declarationDataObject.CommercialInfo, null);

			Assert("Should find some invoices with the partial matching mode.", matcher.HasMatchedResult);

			var expectedPks = new[] { invoice1.PK, invoice2.PK, invoice3.PK, invoice5.PK };
			var actualPks = matcher.GetInvoices().Select(c => c.PK);

			AssertContainsExactElementsInAnyOrder("Should find these invoices.", expectedPks, actualPks);

			AssertEquals("Should find the invoice1 as they have same invoice number and supplier.", invoice1, matcher.GetInvoice(invDataObject1));
			AssertEquals("Should find the invoice2 as they have same invoice number.", invoice2, matcher.GetInvoice(invDataObject2));

			AssertEquals("Should find the invoice4 in Partial mode as they have same invoice number, supplier and parent group.", invoice3, matcher.GetInvoice(invDataObject3));
			AssertEquals("Should find the invoice5 in Complete mode as they have same invoice number, supplier and parent group.", invoice5, matcher.GetInvoice(invDataObject4));
		}
	}
}
