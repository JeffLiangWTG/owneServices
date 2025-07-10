using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeliveryOrPickupAddressChangedTest : TestCaseWithFactory
	{
		public void TestDeliveryAddressAlwaysUpdatesWhenNoDeliveryAddressFilledInAndImporterChanged()
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.MainAddress.OA_Address1 = "I AM NUMBER ONE";
			var importer2 = Factory.New<OrgHeader>();
			importer2.MainAddress.OA_Address1 = "YOU ARE NUMBER TWO";
			var importer3 = Factory.New<OrgHeader>();
			importer3.MainAddress.OA_Address1 = "THREE IS A FIGMENT OF YOUR IMAGINATION";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Importer = importer1.PK;
			AssertEquals("Declaration.ImporterDeliveryAddress.E2_OA_Address", importer1.MainAddress.PK, Declaration.ImporterDeliveryAddress.E2_OA_Address);

			Declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("Declaration.ImporterDeliveryAddress.OrganisationPK", ZGuid.Empty, Declaration.ImporterDeliveryAddress.OrganisationPK);
			AssertEquals("Declaration.ImporterDeliveryAddress.E2_OA_Address", ZGuid.Empty, Declaration.ImporterDeliveryAddress.E2_OA_Address);

			Declaration.JE_OH_Importer = importer1.PK;
			AssertEquals("Declaration.ImporterDeliveryAddress.OrganisationPK", importer1.PK, Declaration.ImporterDeliveryAddress.OrganisationPK);
			AssertEquals("Declaration.ImporterDeliveryAddress.E2_OA_Address", importer1.MainAddress.PK, Declaration.ImporterDeliveryAddress.E2_OA_Address);

			Declaration.JE_OH_Importer = importer2.PK;
			AssertEquals("Declaration.ImporterDeliveryAddress.OrganisationPK", importer2.PK, Declaration.ImporterDeliveryAddress.OrganisationPK);
			AssertEquals("Declaration.ImporterDeliveryAddress.E2_OA_Address", importer2.MainAddress.PK, Declaration.ImporterDeliveryAddress.E2_OA_Address);

			Declaration.ImporterDeliveryAddress.OrganisationPK = importer3.PK;
			AssertEquals("Declaration.ImporterDeliveryAddress.OrganisationPK", importer3.PK, Declaration.ImporterDeliveryAddress.OrganisationPK);
			AssertEquals("Declaration.ImporterDeliveryAddress.E2_OA_Address", importer3.MainAddress.PK, Declaration.ImporterDeliveryAddress.E2_OA_Address);

			Declaration.JE_OH_Importer = importer1.PK;
			AssertEquals("Declaration.ImporterDeliveryAddress.OrganisationPK", importer3.PK, Declaration.ImporterDeliveryAddress.OrganisationPK);
			AssertEquals("Declaration.ImporterDeliveryAddress.E2_OA_Address", importer3.MainAddress.PK, Declaration.ImporterDeliveryAddress.E2_OA_Address);
		}

		public void TestImporterDeliveryAddressChanged()
		{
			var importer = Factory.New<OrgHeader>();
			var addressPickup = importer.Addresses.AddNew();
			addressPickup.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery = importer.Addresses.AddNew();
			addressDelivery.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			addressDelivery.OA_AIREquipmentNeeded = "AAA";
			addressDelivery.OA_LCLEquipmentNeeded = "BBB";
			addressDelivery.OA_FCLEquipmentNeeded = "CCC";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			Declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "LCL";
			AssertEquals("ImporterDeliveryAddressChangedCount", 0, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 0, Declaration.SupplierPickupAddressChangedCount);

			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 1, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 0, Declaration.SupplierPickupAddressChangedCount);

			Declaration.ImporterDeliveryAddress.E2_OA_Address = addressPickup.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 2, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 0, Declaration.SupplierPickupAddressChangedCount);

			Declaration.ImporterDeliveryAddress.E2_OA_Address = addressPickup.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 2, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 0, Declaration.SupplierPickupAddressChangedCount);

			AssertEquals("Equipment needed before change of delivery address", "PSL", Declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = addressDelivery.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 3, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 0, Declaration.SupplierPickupAddressChangedCount);
			AssertEquals("Equipment needed set when delivery address changes", "BBB", Declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
		}

		public void TestImporterDeliveryAddressChanged_SetsDefaultLocalTransportProvider()
		{
			var localTransportPortOne = Factory.New<OrgHeader>();
			var localTransportPortTwo = Factory.New<OrgHeader>();
			var localTransportPortThree = Factory.New<OrgHeader>();
			var query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Declaration.CountryCode);
			var portOneInLocalCountry = Factory.LoadTop1<RefUNLOCO>(query);
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, portOneInLocalCountry.RL_Code);
			var portTwoInLocalCountry = Factory.LoadTop1<RefUNLOCO>(query);
			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_Address1 = "TEST";
			importer.MainAddress.OA_RL_NKRelatedPortCode = portOneInLocalCountry.RL_Code;
			var otherAddress = importer.Addresses.AddNew();
			otherAddress.OA_RL_NKRelatedPortCode = portTwoInLocalCountry.RL_Code;
			otherAddress.OA_Address1 = "Other";
			var addressThree = importer.Addresses.AddNew();
			addressThree.OA_RL_NKRelatedPortCode = portTwoInLocalCountry.RL_Code;
			addressThree.OA_Address1 = "addressThree";
			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, localTransportPortOne, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty, portOneInLocalCountry.RL_Code);
			importer.AllRelatedParties.SetRelatedParty(otherAddress.PK, localTransportPortTwo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty, portTwoInLocalCountry.RL_Code);
			importer.AllRelatedParties.SetRelatedParty(addressThree.PK, localTransportPortThree, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty, portTwoInLocalCountry.RL_Code);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals(ZGuid.Empty, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals(localTransportPortOne.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = otherAddress.PK;
			AssertEquals("Changing delivery address updates cartage company", localTransportPortTwo.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = addressThree.PK;
			AssertEquals("Changing delivery address updates cartage company", localTransportPortThree.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
		}

		public void TestSupplierPickupAddressChanged_SetsDefaultLocalTransportProvider()
		{
			var localTransport = Factory.New<OrgHeader>();
			localTransport.MainAddress.OA_Address1 = "TEST";

			var supplier = Factory.New<OrgHeader>();
			supplier.AllRelatedParties.SetRelatedParty(supplier.MainAddress.PK, localTransport, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "", "");

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_OH_Supplier = supplier.PK;
			Declaration.SupplierPickupAddress.E2_OA_Address = supplier.MainAddress.PK;
			AssertEquals(localTransport.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
		}

		public void TestSetDefaultTransportCompanyWithMultipleTransportModesSameAddress()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo3 = Factory.NewWithValidTestData<OrgHeader>();
			var fclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			var lclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();

			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, transportCo1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);
			var parties = importer.AllRelatedParties;

			var relatedParty2 = parties.AddNew();
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty2.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty2.PR_OH_RelatedParty = transportCo2.PK;
			relatedParty2.PR_OA = importer.Addresses[0].PK;

			var relatedParty3 = parties.AddNew();
			relatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty3.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			relatedParty3.PR_OH_RelatedParty = transportCo3.PK;
			relatedParty3.PR_OA = importer.Addresses[0].PK;

			var relatedParty4FCL = parties.AddNew();
			relatedParty4FCL.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty4FCL.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty4FCL.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty4FCL.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;
			relatedParty4FCL.PR_OH_RelatedParty = fclCartageLTT.PK;
			relatedParty4FCL.PR_OA = importer.Addresses[0].PK;

			var relatedParty5LCL = parties.AddNew();
			relatedParty5LCL.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty5LCL.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty5LCL.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty5LCL.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;
			relatedParty5LCL.PR_OH_RelatedParty = lclCartageLTT.PK;
			relatedParty5LCL.PR_OA = importer.Addresses[0].PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals("Transport Provider should show related party for ALL as fallback, when no specific Mode relationship has been established", transportCo1.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Should now pick up specific AIR relationship T/P", transportCo3.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Should now pick up specific SEA relationship T/P", transportCo2.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Should now pick up specific SEA/FCL relationship T/P", fclCartageLTT.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Should now pick up specific SEA/LCL relationship T/P", lclCartageLTT.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("FCX container type should pick up specific SEA/FCL relationship T/P. i.e. treated as FCL shipment.", fclCartageLTT.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
		}

		public void TestSetDefaultTransportCompanyWithMultipleAddressesMultipleTransportModes()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.Addresses.AddNew();
			importer.Addresses[1].OA_Address1 = "STORE ADDRESS";
			importer.Addresses[1].OA_Address2 = "SOMEWHERE";
			importer.Addresses[1].OA_City = "SYDNEY";

			importer.Addresses.AddNew();
			importer.Addresses[2].OA_Address1 = "THIRD ADDRESS";
			importer.Addresses[2].OA_Address2 = "MASCOT DISTRIBUTION CENTRE";
			importer.Addresses[2].OA_City = "SYDNEY";

			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo3 = Factory.NewWithValidTestData<OrgHeader>();
			var fclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			var lclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();

			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, transportCo1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);
			var parties = importer.AllRelatedParties;

			var relatedParty2 = parties.AddNew();
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty2.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty2.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;
			relatedParty2.PR_OH_RelatedParty = transportCo2.PK;
			relatedParty2.PR_OA = importer.Addresses[1].PK;

			var relatedParty3 = parties.AddNew();
			relatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty3.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			relatedParty3.PR_OH_RelatedParty = transportCo3.PK;
			relatedParty3.PR_OA = importer.Addresses[2].PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals("Initial Importer T/P default should pick up transport provider for SEA as ALL transport mode fallback", transportCo1.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.Addresses[2].PK;
			AssertEquals("Changing to a different address, T/P should be defaulted from Importer relationship set up for appropriate Importer address & Mode chosen", transportCo3.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			importer.Addresses.AddNew();
			importer.Addresses[3].OA_Address1 = "FOURTH ADDRESS";
			importer.Addresses[3].OA_Address2 = "ANOTHER ADDRESS";
			importer.Addresses[3].OA_City = "Perth";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Declaration.JE_OA_DeliveryOrPickupCartageCoAddr = ZGuid.Empty;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.Addresses[3].PK;
			AssertEquals("Transport Provider should be empty as no has been established for this specific address, nor fallback of ALL for this address", ZGuid.Empty, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			importer.Addresses.AddNew();
			importer.Addresses[4].OA_Address1 = "FCL ADDRESS";
			importer.Addresses[4].OA_Address2 = "BOTANY WAREHOUSE";
			importer.Addresses[4].OA_City = "SYDNEY";

			var relatedParty4FCL = parties.AddNew();
			relatedParty4FCL.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty4FCL.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty4FCL.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty4FCL.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;
			relatedParty4FCL.PR_OH_RelatedParty = fclCartageLTT.PK;
			relatedParty4FCL.PR_OA = importer.Addresses[4].PK;

			importer.Addresses.AddNew();
			importer.Addresses[5].OA_Address1 = "LCL ADDRESS";
			importer.Addresses[5].OA_Address2 = "225 BOTANY ROAD";
			importer.Addresses[5].OA_City = "MASCOT";

			var relatedParty5LCL = parties.AddNew();
			relatedParty5LCL.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty5LCL.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty5LCL.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty5LCL.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;
			relatedParty5LCL.PR_OH_RelatedParty = lclCartageLTT.PK;
			relatedParty5LCL.PR_OA = importer.Addresses[5].PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.Addresses[4].PK;
			AssertEquals("When finer FCL container type granularity used as well for SEA, specific FCL transport provider should be found", fclCartageLTT.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.Addresses[5].PK;
			AssertEquals("When finer LCL container type granularity used as well for SEA, specific LCL transport provider should be found", lclCartageLTT.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = importer.Addresses[1].PK;
			AssertEquals("Change of address should now recognise a different LCL TPP", transportCo2.Addresses[0].PK, Declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
		}

		public void TestDeliverAddressIsShipmentDeliveryAddress()
		{
			var supplierOrg = Factory.New<OrgHeader>();
			var addressPickup3 = supplierOrg.Addresses.AddNew();
			addressPickup3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery3 = supplierOrg.Addresses.AddNew();
			addressDelivery3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var importer1 = Factory.New<OrgHeader>();
			var addressPickup1 = importer1.Addresses.AddNew();
			addressPickup1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery1 = importer1.Addresses.AddNew();
			addressDelivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var importer2 = Factory.New<OrgHeader>();
			var addressPickup2 = importer2.Addresses.AddNew();
			addressPickup2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery2 = importer2.Addresses.AddNew();
			addressDelivery2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			shipment.ConsigneePK = importer2.PK;
			shipment.ConsignorPK = supplierOrg.PK;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_OH_Importer = importer1.PK;
			Declaration.JE_OH_Supplier = supplierOrg.PK;
			AssertEquals("Delivery address is same object as shipment delivery address", shipment.ConsigneeDeliveryAddress, Declaration.ImporterDeliveryAddress);
			AssertEquals("JobDocAddress count , plus WarehouseDocAddress", 4, Declaration.DocAddresses.Count);
		}

		public void TestDeliverPickupAddressWhenNoShipment()
		{
			var importerOrg = Factory.New<OrgHeader>();
			var addressPickup1 = importerOrg.Addresses.AddNew();
			addressPickup1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery1 = importerOrg.Addresses.AddNew();
			addressDelivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var supplierOrg = Factory.New<OrgHeader>();
			var addressPickup2 = supplierOrg.Addresses.AddNew();
			addressPickup2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery2 = supplierOrg.Addresses.AddNew();
			addressDelivery2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			Declaration.JE_OH_Importer = importerOrg.PK;
			Declaration.JE_OH_Supplier = supplierOrg.PK;
			AssertEquals("Delivery address", addressDelivery1, Declaration.ImporterDeliveryAddress.Address);
			AssertEquals("Pickup address", addressPickup2, Declaration.SupplierPickupAddress.Address);
			AssertEquals("JobDocAddress count, plus WarehouseDocAddress", 6, Declaration.DocAddresses.Count);
		}

		public void TestPickupAddressIsShipmentPickupAddress()
		{
			var importerOrg = Factory.New<OrgHeader>();
			var addressPickup3 = importerOrg.Addresses.AddNew();
			addressPickup3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery3 = importerOrg.Addresses.AddNew();
			addressDelivery3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var supplier1 = Factory.New<OrgHeader>();
			var addressPickup1 = supplier1.Addresses.AddNew();
			addressPickup1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery1 = supplier1.Addresses.AddNew();
			addressDelivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var supplier2 = Factory.New<OrgHeader>();
			var addressPickup2 = supplier2.Addresses.AddNew();
			addressPickup2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery2 = supplier2.Addresses.AddNew();
			addressDelivery2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			shipment.ConsignorPK = supplier2.PK;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_OH_Importer = importerOrg.PK;
			Declaration.JE_OH_Supplier = supplier1.PK;
			AssertEquals("Pickup address is same object as shipment Pickup address", shipment.ConsignorPickupAddress, Declaration.SupplierPickupAddress);
			AssertEquals("JobDocAddress count , plus WarehouseDocAddress", 4, Declaration.DocAddresses.Count);
		}

		public void TestBrokerChangingDeliveryAddressSetsHasChangesOnJobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CODE";
			var mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_Address1 = "Address1";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDeliveryAddress.OrganisationPK = header.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "12345";
			declaration.ImporterDeliveryAddress.E2_Address1 = "Address1";
			Factory.Save();
			Assert("Pre-condition", !declaration.HasChanges);
			shipment.UserHasEnteredDeliverAddressControl = true;
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "Changed Address";
			shipment.UserHasEnteredDeliverAddressControl = false;
			Assert("Freight changing delivery address does not set HasChanges on JobDeclaration", !declaration.HasChanges);
			Factory.Save();
			declaration.ImporterDeliveryAddress.E2_Address1 = "Changed Address Again";
			Assert("Broker changing delivery address does set HasChanges on JobDeclaration", declaration.HasChanges);
		}

		public void TestSupplierPickupAddressChanged()
		{
			var supplier = Factory.New<OrgHeader>();
			var addressPickup = supplier.Addresses.AddNew();
			addressPickup.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			var addressDelivery = supplier.Addresses.AddNew();
			addressDelivery.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("ImporterDeliveryAddressChangedCount", 0, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 0, Declaration.SupplierPickupAddressChangedCount);

			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 0, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 1, Declaration.SupplierPickupAddressChangedCount);

			Declaration.SupplierPickupAddress.E2_OA_Address = addressDelivery.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 0, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 2, Declaration.SupplierPickupAddressChangedCount);

			Declaration.SupplierPickupAddress.E2_OA_Address = addressDelivery.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 0, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 2, Declaration.SupplierPickupAddressChangedCount);

			Declaration.SupplierPickupAddress.E2_OA_Address = addressPickup.PK;
			AssertEquals("ImporterDeliveryAddressChangedCount", 0, Declaration.ImporterDeliveryAddressChangedCount);
			AssertEquals("SupplierPickupAddressChangedCount", 3, Declaration.SupplierPickupAddressChangedCount);
		}

		public void TestIEDocsProviderMembers()
		{
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), ((IEDocsProvider)Declaration).GetEDocsProviderSupporter().GetType());
		}

		public void TestPickupDeliveryAddressFromSupplierBuyerLink()
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var consignor = OrgHeader.New(Factory);
			consignor.OH_RL_NKClosestPort = "USLAX";
			consignor.OH_Code = "ORG1";
			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = "USCHI";
			consignee.OH_Code = "ORG2";
			var customsAddress = consignee.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, false);
			customsAddress.OA_Address1 = "test1";
			var deliveryAddress1 = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			deliveryAddress1.OA_Address1 = "test2";
			var deliveryAddress2 = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			deliveryAddress2.OA_Address1 = "test3";
			var pickupAddress1 = consignor.Addresses.AddNew(OrgAddressType.Pickup, false);
			pickupAddress1.OA_Address1 = "test4";
			var pickupAddress2 = consignor.Addresses.AddNew(OrgAddressType.Pickup, false);
			pickupAddress2.OA_Address1 = "test5";

			var link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			var trnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			trnMode.PF_OA_OverrideDeliveryAddress = deliveryAddress2.PK;
			trnMode.PF_OA_OverridePickupAddress = pickupAddress2.PK;
			trnMode.PF_TransportMode = "AIR";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "AIR";
			AssertEquals(declaration.ImporterDeliveryAddress.E2_OA_Address, deliveryAddress2.PK);

			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			AssertEquals(declaration.SupplierPickupAddress.E2_OA_Address, pickupAddress2.PK);
			GlbCompany.CurrentCompany.SetCountry(currentCountry);
		}

		public void TestJobComInvoiceLineVatCaption()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				AssertEquals("VAT", declaration.CustomsVATTypeCaption);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				AssertEquals("TVA", declaration.CustomsVATTypeCaption);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			{
				AssertEquals("NIF", declaration.CustomsVATTypeCaption);
			}
		}

		public void TestOnETDChange_UpdateMilestone()
		{
			TestOnDepartureArrivalDateChange_UpdateEvent(JobDeclarationSchema.JE_DateAtOrigin, Events.Departure);
		}

		public void TestOnETAChange_UpdateMilestone()
		{
			TestOnDepartureArrivalDateChange_UpdateEvent(JobDeclarationSchema.JE_DateAtFinalDestination, Events.Arrival);
		}

		public void TestArrivalDepartureLogsAndMilestone()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-11);
			declaration.JE_RL_NKPortOfArrival = "USPHL";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);

			declaration.JE_RL_NKOrigin = "AUMEL";
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(-12);
			declaration.JE_RL_NKPortOfArrival = "USCHI";
			declaration.JE_DateOfArrival = ZDateTime.Today;

			var depMilestone = declaration.WorkflowItems.Milestones.AddNew();
			depMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			var arrMilestone = declaration.WorkflowItems.Milestones.AddNew();
			arrMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			AssertEquals(ZDateTime.Today.AddDays(-12), depMilestone.P9_ScheduledDate.ToZDateTime());
			AssertEquals("should not default actual dates", ZDateTime.Empty, depMilestone.P9_ActualDate.ToZDateTime());

			AssertEquals(ZDateTime.Today, arrMilestone.P9_ScheduledDate.ToZDateTime());
			AssertEquals("should not default actual dates", ZDateTime.Empty, arrMilestone.P9_ActualDate.ToZDateTime());
		}

		public void TestMilestonesConcurencyOtherWay()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration1 = factory1.New<TestDeclaration>();

			var milestone1 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			var milestone2 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			var milestone3 = declaration1.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			factory1.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration2 = factory2.Load<TestDeclaration>(declaration1.PK);

			declaration1.JE_DateAtOrigin = new ZDateTime(2005, 1, 2);

			declaration2.JE_DateAtFinalDestination = new ZDateTime(2005, 1, 3);

			factory2.Save();

			AssertEquals(ZDateTime.Empty, declaration2.WorkflowItems.Milestones[Events.Departure].P9_ScheduledDate.ToZDateTime());
			AssertEquals(new ZDateTime(2005, 1, 3), declaration2.WorkflowItems.Milestones[Events.Arrival].P9_ScheduledDate.ToZDateTime());

			factory1.Save();

			AssertEquals(new ZDateTime(2005, 1, 2), declaration1.WorkflowItems.Milestones[Events.Departure].P9_ScheduledDate.ToZDateTime());
			AssertEquals(new ZDateTime(2005, 1, 3), declaration1.WorkflowItems.Milestones[Events.Arrival].P9_ScheduledDate.ToZDateTime());
		}

		public void TestWorkflowItems()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<BaseJobDeclaration>();
			var shipment = factory.New<ForwardingShipment>();

			declaration.WorkflowItems.Milestones.AddNew();

			AssertEquals(1, declaration.WorkflowItems.Count);
			AssertEquals(0, shipment.WorkflowItems.Count);

			declaration = factory.New<BaseJobDeclaration>();
			shipment = factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.WorkflowItems.Milestones.AddNew();

			AssertEquals(1, declaration.WorkflowItems.Count);
			AssertEquals(1, shipment.WorkflowItems.Count);
		}

		public void TestShouldRestoreHandlingInformationGetter_DeclarationAttachedToShipment_ReturnFalse()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = Factory.New<CommonShipment>().PK;

			AssertEquals("ShouldRestoreHandlingInformation", false, ((IBuyerSupplierRelationshipConsumer)declaration).ShouldRestoreHandlingInformation);
		}

		public void TestShouldRestoreHandlingInformationGetter_DeclarationIsNotAttachedToShipment_ReturnTrue()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			AssertEquals("ShouldRestoreHandlingInformation", true, ((IBuyerSupplierRelationshipConsumer)declaration).ShouldRestoreHandlingInformation);
		}

		TestDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<TestDeclaration>();
				}
				return declaration;
			}
		}
		TestDeclaration declaration;

		void TestOnDepartureArrivalDateChange_UpdateEvent(SchemaDateTimeColumn departureOrArrivalProperty, Event departureOrArrivalEvent)
		{
			var now = ZDateTime.Now;
			Declaration[departureOrArrivalProperty] = now;
			var isFound = false;
			foreach (StmALog log in Declaration.Logs.GetAllLogs())
			{
				if (log.SL_SE_NKEvent == departureOrArrivalEvent.Code && log.SL_EventTime == now && log.SL_IsEstimate)
				{
					isFound = true;
				}
			}

			Assert("Log found", isFound);

			var shipment = Factory.New<ForwardingShipment>();
			Declaration.Logs.RemoveAndDeleteAll();
			Declaration.JE_JS = shipment.PK;
			Declaration[departureOrArrivalProperty] = now;
			isFound = false;
			foreach (StmALog log in Declaration.Logs.GetAllLogs())
			{
				if (log.SL_SE_NKEvent == departureOrArrivalEvent.Code && log.SL_EventTime == now && log.SL_IsEstimate)
				{
					isFound = true;
				}
			}

			Assert("Do not add log for declaration on shipment", !isFound);
		}

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SupplierPickupAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
			{
				SupplierPickupAddressChangedCount++;
			}
			public int SupplierPickupAddressChangedCount;
			protected override void ImporterDeliveryAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
			{
				ImporterDeliveryAddressChangedCount++;
			}
			public int ImporterDeliveryAddressChangedCount;
		}
	}
}
