using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Business.Testing
{
	public class ContactDataRestrictionProviderTest : TestCaseWithFactory
	{
		public void TestGetFilterWithInvalidParameters()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();
			var orgContactPK = ZGuid.ParseSafe("2619e349-72e1-4bc2-a39f-eeb1eef1c700");
			var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.NotAssigned, orgContactPK, Factory);
			Assert("Query should be no result query if module id is not assigned.", query.IsNoResultQuery);

			query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingAccounts, orgContactPK, Factory);
			Assert("Query should be no result query if module id is not supported.", query.IsNoResultQuery);

			query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingShipments, ZGuid.Empty, Factory);
			Assert("Query should be no result query if organisation contact PK is empty.", query.IsNoResultQuery);

			query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingShipments, ZGuid.Invalid, Factory);
			Assert("Query should be no result query if organisation contact PK is invalid.", query.IsNoResultQuery);

			query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingShipments, orgContactPK, Factory);
			Assert("Query should be no result query if organisation contact PK is not exist.", query.IsNoResultQuery);
		}

		public void TestGetOrgansationFilter()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();

			var linkedBuyingConsignee = Factory.NewWithValidTestData<OrgHeader>();
			linkedBuyingConsignee.OH_FullName = "Org1";
			var linkedSupplyingConsignor = Factory.NewWithValidTestData<OrgHeader>();
			linkedSupplyingConsignor.OH_FullName = "Org2";
			var linkedBuyer = Factory.NewWithValidTestData<OrgHeader>();
			linkedBuyer.OH_FullName = "Org3";
			var linkedSupplier = Factory.NewWithValidTestData<OrgHeader>();
			linkedSupplier.OH_FullName = "Org4";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Org5";
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Org6";

			var relatedConsign = Factory.NewWithValidTestData<OrgHeader>();

			var orgContact = relatedConsign.Contacts.AddNew();
			orgContact.OC_ContactName = "contactname";
			orgContact.OC_Email = "email@email.em";
			orgContact.SetHashedPassword("pswrd");
			orgContact.OC_WebAccessEnabled = true;

			linkedBuyingConsignee.OH_IsConsignee = ZBool.True;
			relatedConsign.BuyerLinks.AddNew(linkedBuyingConsignee);
			linkedSupplyingConsignor.OH_IsConsignor = ZBool.True;
			relatedConsign.SupplierLinks.AddNew(linkedSupplyingConsignor);

			relatedConsign.BuyerLinks.AddNew(linkedBuyer);
			relatedConsign.SupplierLinks.AddNew(linkedSupplier);

			consignee.OH_IsConsignee = ZBool.True;
			consignor.OH_IsConsignor = ZBool.True;

			Factory.Save();

			var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.OrganisationTracking, orgContact.PK, Factory);

			var matchedOrgs = new OrgHeaderCollection(Factory);
			matchedOrgs.Load(query);

			AssertEquals("Collection", 3, matchedOrgs.Count);
			AssertCollectionContains("RelatedConsign should always be included", relatedConsign, matchedOrgs);
			AssertCollectionContains("LinkedBuyingConsignee should be included - Consignee which Buys from RelatedConsign", linkedBuyingConsignee, matchedOrgs);
			AssertCollectionContains("LinkedSupplyingConsignor should be included - Consignor which Supplies to RelatedConsign", linkedSupplyingConsignor, matchedOrgs);
			AssertCollectionNotContains("LinkedSupplier should not be included - Not a Consignee/Consignor", linkedSupplier, matchedOrgs);
			AssertCollectionNotContains("LinkedBuyer should not be included - Not  a Consignee/Consignor", linkedBuyer, matchedOrgs);
			AssertCollectionNotContains("Consignee should not be included - No Supplier/Buyer link", consignee, matchedOrgs);
			AssertCollectionNotContains("Consignor should not be included - No Supplier/Buyer link", consignor, matchedOrgs);
		}

		public void TestGetShipmentFilter()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "contactname";
			orgContact.OC_Email = "email@email.em";
			orgContact.SetHashedPassword("pswrd");
			orgContact.OC_WebAccessEnabled = true;

			using (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingShipments, orgContact.PK, Factory);
				AssertNotNull(query);
				Assert("Includes JobHeader SubQuery", query.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
				Assert("Includes JobConShipLink SubQuery", query.LiteralTextADO.Contains(JobConShipLinkSchema.Constants.TableName));
				Assert("Includes JobDocAddress SubQuery", query.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
				Assert("Includes SCP Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ControllingCustomer));
				Assert("Includes CED Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress));
				Assert("Includes CEG Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress));
				Assert("Includes NPP Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.NotifyParty));
				Assert("Includes N2D Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.NotifyParty2));
				Assert("Includes N3D Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.NotifyParty3));
				Assert("Includes BKD Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress));
			}

			using (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingShipments, orgContact.PK, Factory);
				AssertNotNull(query);
				Assert("Does not include SCP Address Type Condition", !query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ControllingCustomer));
			}
		}

		public void TestGetServiceLevelFilter()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "contactname";
			orgContact.OC_Email = "email@email.em";
			orgContact.SetHashedPassword("pswrd");
			orgContact.OC_WebAccessEnabled = true;

			var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.RefServiceLevel, orgContact.PK, Factory);
			AssertEquals(new ZQuery(RefServiceLevelSchema.RS_Code, new string[] { "D2D", "DEF", "DIR", "STD", "TSP" }).LiteralTextSqlFormatted, query.LiteralTextSqlFormatted);

			foreach (var serviceLevel in orgHeader.OrgServiceLevels)
			{
				serviceLevel.PM_IsPublished = false;
			}

			query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.RefServiceLevel, orgContact.PK, Factory);
			AssertEquals(ZQuery.NoResultQuery, query);
		}

		public void TestGetOrgPartRelationFilter()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();

			var orgHeader0 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = orgHeader0.PK;
			relatedParty.PR_OH_RelatedParty = orgHeader1.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			var orgContact = orgHeader1.Contacts.AddNew();
			orgContact.OC_ContactName = "contactname";
			orgContact.OC_Email = "email@email.em";
			orgContact.SetHashedPassword("pswrd");
			orgContact.OC_WebAccessEnabled = true;

			Factory.Save();

			using (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.OrgSupplierPartTracking, orgContact.PK, Factory).LiteralTextADO;
				Assert(string.Concat("Contains ", OrgPartRelationSchema.OU_OH.Name), query.Contains(OrgPartRelationSchema.OU_OH.Name));
				AssertEquals("Contains orgHeader0 PK", true, query.Contains(orgHeader0.PK.ToString()));
				Assert("Contains OrgHeader1 PK", query.Contains(orgHeader1.PK.ToString()));
			}

			using (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.OrgSupplierPartTracking, orgContact.PK, Factory).LiteralTextADO;
				Assert(string.Concat("Contains ", OrgPartRelationSchema.OU_OH.Name), query.Contains(OrgPartRelationSchema.OU_OH.Name));
				AssertEquals("Contains orgHeader0 PK", false, query.Contains(orgHeader0.PK.ToString()));
				Assert("Contains OrgHeader1 PK", query.Contains(orgHeader1.PK.ToString()));
			}
		}

		public void TestGetWarehouseFilter()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();

			var envHelper = new WhsTestHelperFunctionsEnv(Factory);
			var staffWarehouse = envHelper.CreateWarehouse("STAFF", "STF", "AA");
			staffWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var groupWarehouse = envHelper.CreateWarehouse("GROUP", "GRP", "BB");
			groupWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var badWarehouse = envHelper.CreateWarehouse("BAD", "BAD", "CC");
			badWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			var whsHepler = new WhsTestHelperFunctions(Factory);
			var client = whsHepler.CreateClient();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "123456@qq.com";
			var password = "123";
			contact.SetHashedPassword(password);
			contact.OC_WebAccessEnabled = true;

			whsHepler.CreateWhsOrder(client, staffWarehouse);
			whsHepler.ProhibitWarehouseAccessForOrgContact(badWarehouse, contact);
			Factory.Save();

			var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingWarehouse, contact.PK, Factory);
			AssertContainsExactElementsInAnyOrder("Return the warehouse that is orderd and not prohibited.",
				new[] { staffWarehouse.PK }, new BusinessObjectFactory().Load<WhsWarehouse>(query).Select(w => w.PK));
		}

		public void TestGetBookingQuery()
		{
			var contactDataRestrictionProvider = new ContactDataRestrictionProvider();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "contactname";
			orgContact.OC_Email = "email@email.em";
			orgContact.SetHashedPassword("pswrd");
			orgContact.OC_WebAccessEnabled = true;

			var query = contactDataRestrictionProvider.GetFilter(WebModuleIDs.TrackingBookings, orgContact.PK, Factory);

			AssertNotNull(query);

			Assert("Includes JobDocAddress SubQuery", query.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			Assert("Includes JobHeader SubQuery", query.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
			Assert("Includes RatingHeader SubQuery", query.LiteralTextADO.Contains(RatingHeaderSchema.Constants.TableName));
			Assert("Includes JobShipment Delivery Agent", query.LiteralTextADO.Contains(JobShipmentSchema.Constants.JS_OH_DeliveryAgent));
			Assert("Includes JobShipment Export Broker", query.LiteralTextADO.Contains(JobShipmentSchema.Constants.JS_OH_ExportBroker));
			Assert("Includes Pickup Agent address", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.PickupAgent));
			Assert("Should allow deactivated bookings", !query.LiteralTextADO.Contains(string.Format("{0} = 0", JobShipmentSchema.Constants.JS_IsCancelled)));
		}
	}
}
