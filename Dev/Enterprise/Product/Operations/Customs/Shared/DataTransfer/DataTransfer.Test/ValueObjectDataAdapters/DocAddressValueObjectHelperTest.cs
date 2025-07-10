using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using ValueObjectImportContext = Enterprise.DataTransfer.DataAdapters.ValueObjectImportContext;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class DocAddressValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestStandAloneDeclarationDocAddresses()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var helper = new DocAddressValueObjectHelper("");
			var context = new ValueObjectImportContext(Factory, new ZArchitecture.NotificationBuffer());
			helper.ImportFromValueObjectCollection(docAddresses, declaration.DocAddresses, context);

			AssertDocAddresses(declaration.DocAddresses, "E", DocAddressType.ImporterDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, "B", DocAddressType.ImporterPickupDeliveryAddress);
			AssertDocAddresses(declaration.DocAddresses, "C", DocAddressType.SupplierDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, "D", DocAddressType.SupplierPickupDeliveryAddress);

			AssertDocAddresses(declaration.DocAddresses, "A", DocAddressType.ConsigneeDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, string.Empty, DocAddressType.ConsigneePickupDeliveryAddress);
			AssertDocAddresses(declaration.DocAddresses, string.Empty, DocAddressType.ConsignorDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, string.Empty, DocAddressType.ConsignorPickupDeliveryAddress);
		}

		public void TestShipmentDeclarationDocAddresses()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			var helper = new DocAddressValueObjectHelper("");
			var context = new ValueObjectImportContext(Factory, new ZArchitecture.NotificationBuffer());
			helper.ImportFromValueObjectCollection(docAddresses, declaration.DocAddresses, context);

			AssertDocAddresses(declaration.DocAddresses, "E", DocAddressType.ImporterDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, string.Empty, DocAddressType.ImporterPickupDeliveryAddress);
			AssertDocAddresses(declaration.DocAddresses, string.Empty, DocAddressType.SupplierDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, string.Empty, DocAddressType.SupplierPickupDeliveryAddress);

			AssertDocAddresses(declaration.DocAddresses, "A", DocAddressType.ConsigneeDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, "B", DocAddressType.ConsigneePickupDeliveryAddress);
			AssertDocAddresses(declaration.DocAddresses, "C", DocAddressType.ConsignorDocumentaryAddress);
			AssertDocAddresses(declaration.DocAddresses, "D", DocAddressType.ConsignorPickupDeliveryAddress);
		}

		public void TestShipmentDocAddresses()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var helper = new DocAddressValueObjectHelper("");
			var context = new ValueObjectImportContext(Factory, new ZArchitecture.NotificationBuffer());
			helper.ImportFromValueObjectCollection(docAddresses, shipment.DocAddresses, context);

			AssertDocAddresses(shipment.DocAddresses, "E", DocAddressType.ImporterDocumentaryAddress);
			AssertDocAddresses(shipment.DocAddresses, string.Empty, DocAddressType.ImporterPickupDeliveryAddress);
			AssertDocAddresses(shipment.DocAddresses, string.Empty, DocAddressType.SupplierDocumentaryAddress);
			AssertDocAddresses(shipment.DocAddresses, string.Empty, DocAddressType.SupplierPickupDeliveryAddress);

			AssertDocAddresses(shipment.DocAddresses, "A", DocAddressType.ConsigneeDocumentaryAddress);
			AssertDocAddresses(shipment.DocAddresses, "B", DocAddressType.ConsigneePickupDeliveryAddress);
			AssertDocAddresses(shipment.DocAddresses, "C", DocAddressType.ConsignorDocumentaryAddress);
			AssertDocAddresses(shipment.DocAddresses, "D", DocAddressType.ConsignorPickupDeliveryAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			docAddresses = new DocAddressCollection();
			AddDocAddress(docAddresses, DocAddressAddressType.CED, "A");
			AddDocAddress(docAddresses, DocAddressAddressType.CEG, "B");
			AddDocAddress(docAddresses, DocAddressAddressType.CRD, "C");
			AddDocAddress(docAddresses, DocAddressAddressType.CRG, "D");
			AddDocAddress(docAddresses, DocAddressAddressType.IMD, "E");
		}

		static void AssertDocAddresses(JobDocAddressDependentCollection addresses, string expectedCompanyName, DocAddressType addressType)
		{
			if (string.IsNullOrEmpty(expectedCompanyName))
			{
				AssertNull(addressType.ToString(), addresses.FindByDocAddressType(addressType));
			}
			else
			{
				AssertEquals(addressType + ".E2_CompanyName", expectedCompanyName, addresses.FindByDocAddressType(addressType).E2_CompanyName);
			}
		}

		static void AddDocAddress(DocAddressCollection docAddresses, DocAddressAddressType docAddressType, string companyName)
		{
			var address = docAddresses.AddNew(docAddressType);
			address.CompanyName = companyName;
		}

		DocAddressCollection docAddresses;
	}
}
