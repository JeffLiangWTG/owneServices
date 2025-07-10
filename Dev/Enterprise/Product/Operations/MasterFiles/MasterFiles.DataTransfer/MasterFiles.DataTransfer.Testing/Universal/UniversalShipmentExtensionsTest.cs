using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UniversalShipmentExtensionsTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestFindBestCarrierMatch()
		{
			List<OrganizationAddress> list = null;
			AssertNull(list.FindBestCarrierMatch());
			list = new List<OrganizationAddress>();
			AssertNull(list.FindBestCarrierMatch());
			var supplierAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierDocumentaryAddress) };
			list.Add(supplierAddress);
			AssertNull(list.FindBestCarrierMatch());

			var shippingLine = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = AddressTypes.ShippingLine };
			list.Add(shippingLine);
			AssertEquals(shippingLine, list.FindBestCarrierMatch());

			var shippingLineAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ShippingLineAddress) };
			list.Add(shippingLineAddress);
			AssertEquals("ShippingLineAddress is preferred over ShippingLine", shippingLineAddress, list.FindBestCarrierMatch());

			list.Remove(shippingLine);
			list.Insert(1, shippingLine);
			AssertEquals(2, list.IndexOf(shippingLineAddress));
			AssertEquals("ShippingLineAddress is preferred over ShippingLine", shippingLineAddress, list.FindBestCarrierMatch());

			var carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Carrier) };
			list.Add(carrier);
			AssertEquals("Carier is preferred over ShippingLineAddress", carrier, list.FindBestCarrierMatch());

			AssertEquals("SupplierDocumentaryAddress is selected as an override", supplierAddress, list.FindBestCarrierMatch(nameof(DocAddressType.SupplierDocumentaryAddress)));
			AssertEquals("ShippingLineAddress is selected as an override", shippingLineAddress, list.FindBestCarrierMatch(nameof(DocAddressType.ShippingLineAddress)));
			AssertEquals("Carier is selected as DepartureCFSAddress is not in the list", carrier, list.FindBestCarrierMatch(nameof(DocAddressType.DepartureCFSAddress)));
		}

		public void TestFindBestNotifyPartyMatch()
		{
			List<OrganizationAddress> list = null;
			AssertNull(list.FindBestNotifyPartyMatch());
			list = new List<OrganizationAddress>();
			AssertNull(list.FindBestNotifyPartyMatch());
			var supplierAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierDocumentaryAddress) };
			list.Add(supplierAddress);
			AssertNull(list.FindBestNotifyPartyMatch());

			var notifyParty3Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.NotifyParty3) };
			list.Add(notifyParty3Address);
			AssertEquals(notifyParty3Address, list.FindBestNotifyPartyMatch());

			var notifyParty2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.NotifyParty2) };
			list.Add(notifyParty2Address);
			AssertEquals("NotifyParty2 is preferred over NotifyParty3", notifyParty2Address, list.FindBestNotifyPartyMatch());

			list.Remove(notifyParty3Address);
			list.Insert(2, notifyParty3Address);
			AssertEquals(2, list.IndexOf(notifyParty3Address));
			AssertEquals("NotifyParty2 is preferred over NotifyParty3", notifyParty2Address, list.FindBestNotifyPartyMatch());

			var notifyPartyAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.NotifyParty) };
			list.Add(notifyPartyAddress);
			AssertEquals("NotifyParty is preferred over NotifyParty2", notifyPartyAddress, list.FindBestNotifyPartyMatch());

			AssertEquals("SupplierDocumentaryAddress is selected as an override", supplierAddress, list.FindBestNotifyPartyMatch(nameof(DocAddressType.SupplierDocumentaryAddress)));
			AssertEquals("NotifyParty2 is selected as an override", notifyParty2Address, list.FindBestNotifyPartyMatch(nameof(DocAddressType.NotifyParty2)));
			AssertEquals("NotifyParty is selected as DepartureCFSAddress is not in the list", notifyPartyAddress, list.FindBestNotifyPartyMatch(nameof(DocAddressType.DepartureCFSAddress)));
		}

		public void TestFindBestConsigneeMatch()
		{
			List<OrganizationAddress> list = null;
			AssertNull(list.FindBestConsigneeMatch());
			list = new List<OrganizationAddress>();
			AssertNull(list.FindBestConsigneeMatch());
			var supplierAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierDocumentaryAddress) };
			list.Add(supplierAddress);
			AssertNull(list.FindBestConsigneeMatch());

			var importerDelivery = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ImporterPickupDeliveryAddress) };
			list.Add(importerDelivery);
			AssertEquals(importerDelivery, list.FindBestConsigneeMatch());

			var importerDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ImporterDocumentaryAddress) };
			list.Add(importerDoc);
			AssertEquals("ImporterDocumentaryAddress is preferred over ImporterPickupDeliveryAddress", importerDoc, list.FindBestConsigneeMatch());

			list.Remove(importerDelivery);
			list.Insert(1, importerDelivery);
			AssertEquals(2, list.IndexOf(importerDoc));
			AssertEquals("ImporterDocumentaryAddress is preferred over ImporterPickupDeliveryAddress", importerDoc, list.FindBestConsigneeMatch());

			var importer = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = AddressTypes.Importer };
			list.Add(importer);
			AssertEquals("Importer is preferred over ImporterDocumentaryAddress", importer, list.FindBestConsigneeMatch());

			var consigneeDelivery = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneePickupDeliveryAddress) };
			list.Add(consigneeDelivery);
			AssertEquals("ConsigneePickupDeliveryAddress is preferred over Importer", consigneeDelivery, list.FindBestConsigneeMatch());

			var consignee = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress) };
			list.Add(consignee);
			AssertEquals("ConsigneeAddress is preferred over ConsigneePickupDeliveryAddress", consignee, list.FindBestConsigneeMatch());

			var consigneeDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress) };
			list.Add(consigneeDoc);
			AssertEquals("ConsigneeDocumentaryAddress is preferred over ConsigneeAddress", consigneeDoc, list.FindBestConsigneeMatch());

			AssertEquals("SupplierDocumentaryAddress is selected as an override", supplierAddress, list.FindBestConsigneeMatch(nameof(DocAddressType.SupplierDocumentaryAddress)));
			AssertEquals("ConsigneePickupDeliveryAddress is selected as an override", consigneeDelivery, list.FindBestConsigneeMatch(nameof(DocAddressType.ConsigneePickupDeliveryAddress)));
			AssertEquals("ConsigneeDocumentaryAddress is selected as DepartureCFSAddress is not in the list", consigneeDoc, list.FindBestConsigneeMatch(nameof(DocAddressType.DepartureCFSAddress)));
		}

		public void TestFindBestConsignorMatch()
		{
			List<OrganizationAddress> list = null;
			AssertNull(list.FindBestConsignorMatch());
			list = new List<OrganizationAddress>();
			AssertNull(list.FindBestConsignorMatch());
			var importerAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ImporterDocumentaryAddress) };
			list.Add(importerAddress);
			AssertNull(list.FindBestConsignorMatch());

			var supplierPickup = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierPickupDeliveryAddress) };
			list.Add(supplierPickup);
			AssertEquals(supplierPickup, list.FindBestConsignorMatch());

			var supplierDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierDocumentaryAddress) };
			list.Add(supplierDoc);
			AssertEquals("SupplierDocumentaryAddress is preferred over SupplierPickupDeliveryAddress", supplierDoc, list.FindBestConsignorMatch());

			list.Remove(supplierPickup);
			list.Insert(1, supplierPickup);
			AssertEquals(2, list.IndexOf(supplierDoc));
			AssertEquals("SupplierDocumentaryAddress is preferred over SupplierPickupDeliveryAddress", supplierDoc, list.FindBestConsignorMatch());

			var supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = AddressTypes.Supplier };
			list.Add(supplier);
			AssertEquals("Supplier is preferred over SupplierDocumentaryAddress", supplier, list.FindBestConsignorMatch());

			var consignorPickup = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress) };
			list.Add(consignorPickup);
			AssertEquals("ConsignorPickupDeliveryAddress is preferred over Supplier", consignorPickup, list.FindBestConsignorMatch());

			var consignorDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress) };
			list.Add(consignorDoc);
			AssertEquals("ConsignorDocumentaryAddress is preferred over ConsignorPickupDeliveryAddress", consignorDoc, list.FindBestConsignorMatch());

			AssertEquals("ImporterDocumentaryAddress is selected as an override", importerAddress, list.FindBestConsignorMatch(nameof(DocAddressType.ImporterDocumentaryAddress)));
			AssertEquals("ConsignorPickupDeliveryAddress is selected as an override", consignorPickup, list.FindBestConsignorMatch(nameof(DocAddressType.ConsignorPickupDeliveryAddress)));
			AssertEquals("ConsignorDocumentaryAddress is selected as DepartureCFSAddress is not in the list", consignorDoc, list.FindBestConsignorMatch(nameof(DocAddressType.DepartureCFSAddress)));
		}

		public void TestFindBestImporterMatch()
		{
			List<OrganizationAddress> list = null;
			AssertNull(list.FindBestImporterMatch());
			list = new List<OrganizationAddress>();
			AssertNull(list.FindBestImporterMatch());
			var supplierAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierDocumentaryAddress) };
			list.Add(supplierAddress);
			AssertNull(list.FindBestImporterMatch());

			var consigneeDelivery = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneePickupDeliveryAddress) };
			list.Add(consigneeDelivery);
			AssertEquals(consigneeDelivery, list.FindBestImporterMatch());

			var consigneeDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress) };
			list.Add(consigneeDoc);
			AssertEquals("ConsigneeDocumentaryAddress is preferred over ConsigneePickupDeliveryAddress", consigneeDoc, list.FindBestImporterMatch());

			list.Remove(consigneeDelivery);
			list.Insert(1, consigneeDelivery);
			AssertEquals(2, list.IndexOf(consigneeDoc));
			AssertEquals("ConsigneeDocumentaryAddress is preferred over ConsigneePickupDeliveryAddress", consigneeDoc, list.FindBestImporterMatch());

			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress) };
			list.Add(consigneeAddress);
			AssertEquals("ConsigneeAddress is preferred over ConsigneeDocumentaryAddress", consigneeAddress, list.FindBestImporterMatch());

			var importerDelivery = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ImporterPickupDeliveryAddress) };
			list.Add(importerDelivery);
			AssertEquals("ImporterPickupDeliveryAddress is preferred over ConsigneeAddress", importerDelivery, list.FindBestImporterMatch());

			var importerDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ImporterDocumentaryAddress) };
			list.Add(importerDoc);
			AssertEquals("ImporterDocumentaryAddress is preferred over ImporterPickupDeliveryAddress", importerDoc, list.FindBestImporterMatch());

			var importer = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = AddressTypes.Importer };
			list.Add(importer);
			AssertEquals("Importer is preferred over ImporterDocumentaryAddress", importer, list.FindBestImporterMatch());

			AssertEquals("SupplierDocumentaryAddress is selected as an override", supplierAddress, list.FindBestImporterMatch(nameof(DocAddressType.SupplierDocumentaryAddress)));
			AssertEquals("ConsigneeAddress is selected as an override", consigneeAddress, list.FindBestImporterMatch(nameof(DocAddressType.ConsigneeAddress)));
			AssertEquals("Importer is selected as DepartureCFSAddress is not in the list", importer, list.FindBestImporterMatch(nameof(DocAddressType.DepartureCFSAddress)));
		}

		public void TestAddOrgAddress_DocAddress()
		{
			var bizObj = Factory.New<DummyBizObjWithDocAddresses>();
			IDataWritingManager writeManager = new DataWritingManager(new ActionInfo(null, bizObj));
			Shipment shipment = null;
			var consigneeAddress = AddDocAddress(bizObj, DocAddressType.ConsigneeAddress, "BOB THE BUILDER");

			AssertNull(shipment.AddOrgAddress(writeManager, null));
			AssertNull(shipment.AddOrgAddress(writeManager, (JobDocAddress)null, DocAddressType.BuyerDocumentaryAddress));

			var address = shipment.AddOrgAddress(writeManager, consigneeAddress);
			AssertAddress(address, DocAddressType.ConsigneeAddress, "BOB THE BUILDER");

			address = shipment.AddOrgAddress(writeManager, consigneeAddress, DocAddressType.BuyerDocumentaryAddress);
			AssertAddress(address, DocAddressType.BuyerDocumentaryAddress, "BOB THE BUILDER");
		}

		public void TestAddOrgAddresses()
		{
			var bizObj = Factory.New<DummyBizObjWithDocAddresses>();
			var consigneeAddress = AddDocAddress(bizObj, DocAddressType.ConsigneeAddress, "BOB THE BUILDER");
			var goodsOwner1 = AddDocAddress(bizObj, DocAddressType.GoodsOwner, "GOODS OWNER 1");
			var goodsOwner2 = AddDocAddress(bizObj, DocAddressType.GoodsOwner, "GOODS OWNER 2");
			goodsOwner2.E2_AddressSequence = 1;
			goodsOwner1.E2_AddressSequence = 2;
			var customsWarehouseAddress = AddDocAddress(bizObj, DocAddressType.CustomsWarehouseAddress, "CUSTOMS WHS");
			IDataWritingManager writeManager = null;
			Shipment shipment = null;
			AssertEquals(false, shipment.AddOrgAddresses(writeManager, null).Any());
			writeManager = new DataWritingManager(new ActionInfo(null, bizObj));
			AssertEquals(false, shipment.AddOrgAddresses(writeManager, null).Any());
			AssertEquals(false, shipment.AddOrgAddresses(writeManager, bizObj).Any());
			shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertEquals(false, shipment.AddOrgAddresses(writeManager, null).Any());
			AssertEquals(false, shipment.AddOrgAddresses(null, bizObj).Any());
			var addresses = shipment.AddOrgAddresses(writeManager, bizObj).ToArray();
			AssertEquals(3, addresses.Length);
			AssertAddress(addresses[0], DocAddressType.CustomsWarehouseAddress, "CUSTOMS WHS");
			AssertAddress(addresses[1], DocAddressType.GoodsOwner, "GOODS OWNER 2");
			AssertAddress(addresses[2], DocAddressType.GoodsOwner, "GOODS OWNER 1");

			AssertEquals(3, shipment.OrganizationAddressCollection.Count);
			AssertAddress(shipment.OrganizationAddressCollection[0], DocAddressType.CustomsWarehouseAddress, "CUSTOMS WHS");
			AssertAddress(shipment.OrganizationAddressCollection[1], DocAddressType.GoodsOwner, "GOODS OWNER 2");
			AssertAddress(shipment.OrganizationAddressCollection[2], DocAddressType.GoodsOwner, "GOODS OWNER 1");
		}

		void AssertAddress(OrganizationAddress organizationAddress, DocAddressType addressType, ZString companyName)
		{
			AssertEquals("AddressType", addressType.ToString(), organizationAddress.AddressType);
			AssertEquals("CompanyName", companyName, organizationAddress.CompanyName);
			AssertEquals("Address1", companyName + "ADD 1", organizationAddress.Address1);
		}

		JobDocAddress AddDocAddress(IDocAddresses bizObj, DocAddressType addressType, ZString companyName)
		{
			var docAddress = bizObj.DocAddresses.AddNew(addressType);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = companyName;
			docAddress.E2_Address1 = companyName + "ADD 1";
			return docAddress;
		}

		class DummyBizObjWithDocAddresses : Business.Testing.JobDocAddressPersistentParentForTesting, IDocAddresses
		{
			public DummyBizObjWithDocAddresses(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.CustomsWarehouseAddress, DocAddressType.GoodsOwner, DocAddressType.Consolidator };
		}

		public void TestFindBestSupplierMatch()
		{
			List<OrganizationAddress> list = null;
			AssertNull(list.FindBestSupplierMatch());
			list = new List<OrganizationAddress>();
			AssertNull(list.FindBestSupplierMatch());
			var importerAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ImporterDocumentaryAddress) };
			list.Add(importerAddress);
			AssertNull(list.FindBestSupplierMatch());

			var consignorPickup = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress) };
			list.Add(consignorPickup);
			AssertEquals(consignorPickup, list.FindBestSupplierMatch());

			var consignorDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress) };
			list.Add(consignorDoc);
			AssertEquals("ConsignorDocumentaryAddress is preferred over ConsignorPickupDeliveryAddress", consignorDoc, list.FindBestSupplierMatch());

			list.Remove(consignorPickup);
			list.Insert(1, consignorPickup);
			AssertEquals(2, list.IndexOf(consignorDoc));
			AssertEquals("ConsignorDocumentaryAddress is preferred over ConsignorPickupDeliveryAddress", consignorDoc, list.FindBestSupplierMatch());

			var supplierPickup = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierPickupDeliveryAddress) };
			list.Add(supplierPickup);
			AssertEquals("SupplierPickupDeliveryAddress is preferred over ConsignorDocumentaryAddress", supplierPickup, list.FindBestSupplierMatch());

			var supplierDoc = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.SupplierDocumentaryAddress) };
			list.Add(supplierDoc);
			AssertEquals("SupplierDocumentaryAddress is preferred over SupplierPickupDeliveryAddress", supplierDoc, list.FindBestSupplierMatch());

			var supplier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = AddressTypes.Supplier };
			list.Add(supplier);
			AssertEquals("Supplier is preferred over SupplierDocumentaryAddress", supplier, list.FindBestSupplierMatch());

			AssertEquals("ImporterDocumentaryAddress is selected as an override", importerAddress, list.FindBestSupplierMatch(nameof(DocAddressType.ImporterDocumentaryAddress)));
			AssertEquals("ConsignorPickupDeliveryAddress is selected as an override", consignorPickup, list.FindBestSupplierMatch(nameof(DocAddressType.ConsignorPickupDeliveryAddress)));
			AssertEquals("Supplier is selected as DepartureCFSAddress is not in the list", supplier, list.FindBestSupplierMatch(nameof(DocAddressType.DepartureCFSAddress)));
		}
	}
}
