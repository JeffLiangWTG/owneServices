using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocAddressesTest : TestCaseWithFactory
	{
		public void TestSupportedAddressTypesWithBooking()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var addressTypes = ((IDocAddresses)shipment).SupportedAddressTypes;
			AssertCollectionContains(DocAddressType.BookingPartyDocumentaryAddress, addressTypes);

			shipment.JS_IsBooking = true;

			addressTypes = ((IDocAddresses)shipment).SupportedAddressTypes;
			AssertCollectionContains(DocAddressType.BookingPartyDocumentaryAddress, addressTypes);
		}

		public void TestGetOrgHeaderList()
		{
			var shipmentWithDocsAndCartage = Factory.New<ForwardingShipment>();
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.ControllingCustomer, OrganisationTypes.ControllingCustomer, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.ControllingAgent, OrganisationTypes.ControllingAgent, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.ConsignorDocumentaryAddress, OrganisationTypes.Consignor, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.ConsigneeDocumentaryAddress, OrganisationTypes.Consignee, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.PickupAgent, OrganisationTypes.Forwarder, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.ConsignorPickupDeliveryAddress, OrganisationTypes.None, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.ConsigneePickupDeliveryAddress, OrganisationTypes.None, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(shipmentWithDocsAndCartage, DocAddressType.NotifyParty, OrganisationTypes.None, OrganisationsSubTypeList.Codes.NotifyParty);
		}

		void AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(
			IDocAddresses docAddress,
			DocAddressType docAddressTypeForTest,
			OrganisationTypes expectedOrganisationTypes,
			ZString expectedOrganisationSubType)
		{
			var collection = docAddress.GetOrgHeaderList(docAddressTypeForTest);
			AssertEquals(typeof(OrganisationsFindBoxCollection), collection.GetType());
			var findBoxCollection = collection as OrganisationsFindBoxCollection;
			AssertEquals(expectedOrganisationTypes, findBoxCollection.OrganisationType);
			AssertEquals(expectedOrganisationSubType, findBoxCollection.OrganisationSubType);
		}

		public void TestSupportedAddressTypesWithDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var addressTypesWithoutDeclaration = ((IDocAddresses)shipment).SupportedAddressTypes;
			AssertCollectionNotContains(DocAddressType.AQISProcessingEstablishment, addressTypesWithoutDeclaration);

			var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "EXP";
			var addressTypesWithEXPDeclaration = ((IDocAddresses)shipment).SupportedAddressTypes;
			AssertContainsExactElementsInAnyOrder(addressTypesWithoutDeclaration, addressTypesWithEXPDeclaration);

			declaration.JE_MessageType = "AQS";
			var addressTypesWithAQSDeclaration = ((IDocAddresses)shipment).SupportedAddressTypes;

			AssertContainsExactElementsInAnyOrder("Quarantine address type was added",
				addressTypesWithoutDeclaration.Concat(new[] { DocAddressType.AQISProcessingEstablishment }),
				addressTypesWithAQSDeclaration);
		}

		public void TestSupportedAddressTypes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var addressTypesWithoutDeclaration = ((IDocAddresses)shipment).SupportedAddressTypes;
			AssertCollectionContains(DocAddressType.ControllingAgent, addressTypesWithoutDeclaration);
			AssertCollectionContains(DocAddressType.HouseBillIssuingParty, addressTypesWithoutDeclaration);
			AssertCollectionContains(DocAddressType.WarehouseClient, addressTypesWithoutDeclaration);
			AssertCollectionContains(DocAddressType.Warehouse, addressTypesWithoutDeclaration);
			AssertCollectionContains(DocAddressType.Creditor, addressTypesWithoutDeclaration);
			AssertCollectionContains(DocAddressType.SupplierDocumentaryAddress, addressTypesWithoutDeclaration);
		}

		public void TestGetDocAddressRequirement()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var iDocAddresses = (IDocAddresses)shipment;

			var addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.ControllingAgent).DefaultDocAddressType;
			AssertEquals(DocAddressType.ControllingAgent, addressType);

			var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "AQS";

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment).DefaultDocAddressType;
			AssertEquals(DocAddressType.AQISProcessingEstablishment, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.HouseBillIssuingParty).DefaultDocAddressType;
			AssertEquals(DocAddressType.HouseBillIssuingParty, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.WarehouseClient).DefaultDocAddressType;
			AssertEquals(DocAddressType.WarehouseClient, addressType);
		}

		public void TestWarehouseDocAddressRequirement_CannotOverride()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var warehouseAddressReq = ((IDocAddresses)shipment).GetDocAddressRequirement(DocAddressType.WarehouseClient);
			AssertEquals("Should not be able to override Warehouse Client Address.", false, warehouseAddressReq.CanOverride);
		}

		public void TestHouseBillIssuingParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(false, shipment.HouseBillIssuingPartyDocumentaryAddress.IsValidAddress);
			AssertEquals(null, shipment.HouseBillIssuingParty);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(false, shipment.HouseBillIssuingPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(null, shipment.HouseBillIssuingParty);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, shipment.HouseBillIssuingPartyDocumentaryAddress.HasRealOrganisation);
			AssertEquals(org, shipment.HouseBillIssuingParty);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;

			shipment.HouseBillIssuingPartyDocumentaryAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK);
			AssertEquals(contact.PK, shipment.HouseBillIssuingPartyDocumentaryAddress.ContactPK);
		}

		public void TestWarehouseClient_PK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.DocAddresses.CreateWithAddressType(DocAddressType.WarehouseClient).E2_OA_Address = org.MainAddress.PK;

			var supporter = new ForwardingShipment.ForwardingShipmentInvoicingSupporter(shipment);
			AssertEquals("WarehouseClient PK", org.PK, supporter.WarehouseClient.PK);
		}

		public void TestWarehouseClient_Error()
		{
			var wareHouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			wareHouseOrg.OH_IsWarehouseClient = true;
			var nonWareHouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			nonWareHouseOrg.OH_IsWarehouseClient = false;
			Factory.Save();

			var errorMessage = "An Organization added for a Warehouse Client Address must have an Organization Type of Warehouse.";
			var shipment = Factory.New<ForwardingShipment>();
			var warehouseClientAddress = shipment.DocAddresses.CreateWithAddressType(DocAddressType.WarehouseClient);

			warehouseClientAddress.OrganisationPK = wareHouseOrg.PK;
			AssertNoError(warehouseClientAddress.OrganisationPKInfo, errorMessage);

			warehouseClientAddress.OrganisationPK = nonWareHouseOrg.PK;
			AssertHasError(warehouseClientAddress.OrganisationPKInfo, errorMessage);
		}

		public void TestSupplierDocAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Assert(shipment.SupplierDocAddress.IsEmpty);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MISC";
			shipment.SupplierDocAddress.OrganisationPK = org.PK;
			AssertEquals(false, shipment.SupplierDocAddress.HasRealOrganisation);

			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			AssertEquals(true, shipment.SupplierDocAddress.HasRealOrganisation);
			AssertEquals(org.PK, shipment.SupplierDocAddress.OrganisationPK);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ccc";

			shipment.SupplierDocAddress.ContactPK = contact.PK;
			AssertEquals(org.PK, shipment.SupplierDocAddress.OrganisationPK);
			AssertEquals(contact.PK, shipment.SupplierDocAddress.ContactPK);

			var supportedAddressTypes = (shipment as IDocAddresses).SupportedAddressTypes;
			var indexes = supportedAddressTypes
				.Select((item, index) => new { item, index })
				.Where(x => x.item  is DocAddressType.SupplierDocumentaryAddress || x.item is DocAddressType.BuyerDocumentaryAddress)
				.Select(x => x.index)
				.OrderBy(x => x)
				.ToList();
			AssertEquals("There are only 2", 2, indexes.Count);
			AssertEquals("They are adjacent", 1, indexes[1] - indexes[0]);
		}
	}
}
