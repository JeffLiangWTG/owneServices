using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentDocumentSupporter))]
	public class HVLVConsignmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetDocumentWrappers_DeliveryLabel()
		{
			var consignment = Factory.New<HVLVBookingHeader>().Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Blahblah";

			var supporter = consignment.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(1, wrappers.Length);

			menuItem.SU_MenuName = "HVLV Delivery Label";
			wrappers = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(2, wrappers.Length);
		}

		public void TestGetDocumentWrappers_RoutingLabel()
		{
			var consignment = Factory.New<HVLVBookingHeader>().Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Blahblah";

			var supporter = consignment.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(1, wrappers.Length);

			menuItem.SU_MenuName = "HVLV Routing Label";
			wrappers = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(2, wrappers.Length);
		}

		public void TestGetDocumentWrappersForHVLVItem()
		{
			var consignment = Factory.New<HVLVBookingHeader>().Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "HVLV Routing Label";

			var consignmentSupporter = consignment.DocumentSupporter;
			var wrappersForConsignment = consignmentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(2, wrappersForConsignment.Length);

			var item1Supporter = item1.DocumentSupporter;
			var wrappersForItem1 = item1Supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(1, wrappersForItem1.Length);

			var item2Supporter = item2.DocumentSupporter;
			var wrappersForItem2 = item2Supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, menuItem);
			AssertEquals(1, wrappersForItem2.Length);
		}

		public void TestGetContactOrganisation()
		{
			var consigneeOrg = Factory.New<OrgHeader>();
			var consigneeOrgAddress = consigneeOrg.MainAddress;
			var shipperOrg = Factory.New<OrgHeader>();
			var shipperOrgAddress = shipperOrg.MainAddress;
			var lastMileCarrierOrg = Factory.New<OrgHeader>();
			var destinationDepotOrg = Factory.New<OrgHeader>();
			var destinationDepotOrgAddress = destinationDepotOrg.MainAddress;

			var consignment = GetDocumentSupportableBusinessObject() as HVLVConsignment;

			CombineAssertions(() =>
			{
				// ContactType.Consignee
				AssertNull("Consignment Consignee is not org", GetDocumentContactOrgPK(ContactType.Find("CNE")));
				consignment.HVC_OA_ConsigneeAddress = consigneeOrgAddress.PK;
				AssertEquals("Consignment Consignee is org", consigneeOrg.PK, GetDocumentContactOrgPK(ContactType.Find("CNE")));

				// ContactType.Consignor
				AssertNull("Consignment Shipper is not org", GetDocumentContactOrgPK(ContactType.Find("CNR")));
				consignment.HVC_OA_ShipperAddress = shipperOrgAddress.PK;
				AssertEquals("Consignment Shipper is org", shipperOrg.PK, GetDocumentContactOrgPK(ContactType.Find("CNR")));

				//ContactType.TransportServices
				AssertNull("Consignment Last Mile Carrier is emtpy", GetDocumentContactOrgPK(ContactType.Find("TRC")));
				consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
				AssertEquals("Consignment Last Mile Carrier is defined", lastMileCarrierOrg.PK, GetDocumentContactOrgPK(ContactType.Find("TRC")));

				// ContactType.ImportDepot
				AssertNull("Consignment destination depot is empty", GetDocumentContactOrgPK(ContactType.Find("UNP")));
				consignment.HVC_OA_DestinationDepot = destinationDepotOrgAddress.PK;
				AssertEquals("Consignment destination depot is defined", destinationDepotOrg.PK, GetDocumentContactOrgPK(ContactType.Find("UNP")));
			});

			ZGuid? GetDocumentContactOrgPK(IContactType contactType)
			{
				return consignment.DocumentSupporter.GetContactOrganisation("", contactType, DocumentDirection.ANY)?.OrgHeader.PK;
			}
		}

		public void TestGetOverriddenDeliveryDetails()
		{
			var orgAddress = Factory.New<OrgAddress>();
			var consignment = GetDocumentSupportableBusinessObject() as HVLVConsignment;

			CombineAssertions("ContactType.Consignee", () =>
			{
				var docAddress = GetDocumentDeliveryDetails(ContactType.Find("CNE"));
				AssertEquals("Default Contact", string.Empty, docAddress.E2_Contact);
				AssertEquals("Default email", string.Empty, docAddress.E2_Email);

				consignment.HVC_ConsigneeContact = "HVC Consignee Contact";
				consignment.HVC_ConsigneeEmail = "HVC_ConsigneeEmail@test.org";
				consignment.HVC_ConsigneeFax = "123456789";
				docAddress = GetDocumentDeliveryDetails(ContactType.Find("CNE"));
				AssertEquals("Contact", "HVC Consignee Contact".ToUpper(), docAddress.E2_Contact);
				AssertEquals("Email", "HVC_ConsigneeEmail@test.org".ToUpper(), docAddress.E2_Email);
				AssertEquals("Fax", "123456789", docAddress.E2_Fax);

				consignment.HVC_OA_ConsigneeAddress = orgAddress.PK;
				AssertNull("Consignee is org", GetDocumentDeliveryDetails(ContactType.Find("CNE")));
			});

			CombineAssertions("ContactType.Consignor", () =>
			{
				var docAddress = GetDocumentDeliveryDetails(ContactType.Find("CNR"));
				AssertEquals("Default Contact", string.Empty, docAddress.E2_Contact);
				AssertEquals("Default email", string.Empty, docAddress.E2_Email);

				consignment.HVC_ShipperContact = "HVC Shipper Contact";
				consignment.HVC_ShipperEmail = "HVC_ShipperEmail@test.org";
				consignment.HVC_ShipperFax = "987654321";
				docAddress = GetDocumentDeliveryDetails(ContactType.Find("CNR"));
				AssertEquals("Contact", "HVC Shipper Contact".ToUpper(), docAddress.E2_Contact);
				AssertEquals("Email", "HVC_ShipperEmail@test.org".ToUpper(), docAddress.E2_Email);
				AssertEquals("Fax", "987654321", docAddress.E2_Fax);

				consignment.HVC_OA_ShipperAddress = orgAddress.PK;
				AssertNull("Shipper is org", GetDocumentDeliveryDetails(ContactType.Find("CNR")));
			});

			JobDocAddress GetDocumentDeliveryDetails(IContactType contactType)
			{
				return consignment.DocumentSupporter.GetOverriddenDeliveryDetails("", contactType, DocumentDirection.ANY) as JobDocAddress;
			}
		}

		#region Recipients

		public void TestRecipientsForMenuItemWithAutoDeliveryAndContactCNE()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_PreventAutoDelivery = false;
			menuItem.SU_ContactType = ContactType.Consignee.Code;

			var consigneeOrg = Factory.New<OrgHeader>();
			var consigneeOrgAddress = consigneeOrg.MainAddress;
			var consigneeContact = consigneeOrg.Contacts.AddNew();
			consigneeContact.AddDocumentGroup(ContactType.Consignee.Code, true, null);
			consigneeContact.OC_ContactName = "OrgContact Name";
			consigneeContact.OC_Email = "OrgContact@test.com";

			var consignment = GetDocumentSupportableBusinessObject() as HVLVConsignment;
			consignment.HVC_OA_ConsigneeAddress = consigneeOrgAddress.PK;
			consignment.HVC_ConsigneeContact = "HVC Consignee Contact";
			consignment.HVC_ConsigneeEmail = "HVCConsigneeEmail@test.com";

			var task = new PrintTask_ForTest(menuItem);
			var pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = consignment.DocumentSupporter;
			task.Add(pack);

			CombineAssertions("Consignee Is Org", () =>
			{
				Assert("Precondition: ConsigneeIsOrganisation", consignment.ConsigneeIsOrganisation);
				var instructions = new DeliveryInstructions(pack);
				instructions.AllowAutoDelivery = task.PermissionForAutoDelivery_Exposed;
				AssertEquals("1 recipient added", 1, instructions.Recipients.Count);
				var recipient = instructions.Recipients[0];
				AssertEquals("Recipient Org", consignment.ConsigneeAddress.Header.PK, recipient.OrgHeaderPK);
				AssertEquals("Recipient Name", "OrgContact Name", recipient.Name);
				AssertEquals("Recipient Email", "OrgContact@test.com", recipient.DeliveryAddress);
			});

			CombineAssertions("Consignee Is Not Org", () =>
			{
				consignment.HVC_OA_ConsigneeAddress = ZGuid.Empty;
				Assert("Precondition: !ConsigneeIsOrganisation", !consignment.ConsigneeIsOrganisation);
				var instructions = new DeliveryInstructions(pack);
				instructions.AllowAutoDelivery = task.PermissionForAutoDelivery_Exposed;
				AssertEquals("1 recipient added", 1, instructions.Recipients.Count);
				var recipient = instructions.Recipients[0];
				AssertEquals("Recipient Org", ZGuid.Empty, recipient.OrgHeaderPK);
				AssertEquals("Recipient Name", "HVC Consignee Contact".ToUpper(), recipient.Name);
				AssertEquals("Recipient Email", "HVCConsigneeEmail@test.com".ToUpper(), recipient.DeliveryAddress);
			});
		}

		#endregion

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var consignment = Factory.New<HVLVBookingHeader>().Consignments.AddNew();
			consignment.Items.AddNew();

			return consignment;
		}

		class PrintTask_ForTest : PrintTask
		{
			public PrintTask_ForTest(IStmMenuItem menuItem) : base(menuItem) { }

			public bool PermissionForAutoDelivery_Exposed => PermissionForAutoDelivery;
		}
	}
}
