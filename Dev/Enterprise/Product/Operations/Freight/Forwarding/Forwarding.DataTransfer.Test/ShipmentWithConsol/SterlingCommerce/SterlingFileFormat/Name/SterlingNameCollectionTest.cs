using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingNameCollection))]
	class SterlingNameCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingNameCollection>
	{
		#region Test Overrides

		protected override SterlingNameCollection GetCollectionToTest()
		{
			var master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());

			return new SterlingNameCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingName();
		}

		#endregion

		public void TestSterlingNameCollection()
		{
			Xsd.Consol consol = new Xsd.Consol();
			Xsd.Shipment shipment = consol.Shipments.AddNew();

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(0, sterling.NameInfo.Count);

			CreateName(shipment.ShipmentDetails.Deliver.CartageCompany, "DeliveryCompany", "AUSYD");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(1, sterling.NameInfo.Count);

			CreateName(shipment.ShipmentDetails.LocalClient, "LocalClient", "AUBNE");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.NameInfo.Count);

			CreateName(shipment.ShipmentDetails.Pickup.CartageCompany, "PickUpCompany", "USLAX");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(3, sterling.NameInfo.Count);

			CreateName(shipment.ShipmentDetails.Consignee, "Consignee", "USCHI");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(4, sterling.NameInfo.Count);

			CreateName(shipment.ShipmentDetails.Consignor, "Consignor", "RUMOW");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(5, sterling.NameInfo.Count);

			CreateName(shipment.ShipmentDetails.NotifyParty.Organisation, "NotifyParty", "SGSIN");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(6, sterling.NameInfo.Count);

			CreateName(consol.ConsolDetail.Carrier, "Carrier", "USLAX");
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(7, sterling.NameInfo.Count);
		}

		public void TestSterlingNameForShipToAndShipFrom()
		{
			var shipment = new Xsd.Shipment();

			var pickUpCartageOrg = shipment.ShipmentDetails.Pickup.CartageCompany;
			pickUpCartageOrg.EDICode = "TEASYD";

			var pickUpDocAddress = shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(Xsd.DocAddressAddressType.CRG);
			pickUpDocAddress.AddressReference.AddressSequenceRef = 1;
			pickUpDocAddress.CountryCode = "AU";

			var pickUpAddressCollection = pickUpDocAddress.AddressReference.Organisation.OrganisationDetails.Addresses;
			var pickupOrgAddress = pickUpAddressCollection.AddNew(Xsd.AddressCapabilityAddressType.PIC);
			pickupOrgAddress.CompanyName = "TEASYD";
			pickupOrgAddress.AddressLine1 = "15a Tea St";
			pickupOrgAddress.AddressLine2 = "Sydney CBD";
			pickupOrgAddress.StateOrProvince = "NSW";
			pickupOrgAddress.Sequence = 1;
			pickupOrgAddress.PostCode = "2000";
			pickupOrgAddress.Email = "tea@AfternoonPickUp.com";
			pickupOrgAddress.IsSpecified = true;

			var deliveryCartageOrg = shipment.ShipmentDetails.Deliver.CartageCompany;
			deliveryCartageOrg.EDICode = "SILVER";

			var deliveryDocAddress = shipment.ShipmentDetails.DocAddresses.DocAddress.AddNew(Xsd.DocAddressAddressType.CEG);
			deliveryDocAddress.AddressType = Xsd.DocAddressAddressType.CEG;
			deliveryDocAddress.CompanyName = "Silver Versailles";
			deliveryDocAddress.AddressLine1 = "Versailles";
			deliveryDocAddress.AddressLine2 = "Yvelines";
			deliveryDocAddress.StateOrProvince = "YVE";
			deliveryDocAddress.PostCode = "78646";
			deliveryDocAddress.CountryCode = "FR";

			var sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, null, shipment, new Xsd.InterchangeInfo());

			AssertEquals("Expected two names to be generated", 2, sterling.NameInfo.Count);

			AssertContains("Expected to generate address ShipFrom address from pick up org address",
				"N01|SF|TEASYD|1|TEASYD|15a Tea St|Sydney CBD||NSW|2000||MAIN|||tea@AfternoonPickUp.com|>",
				sterling.NameInfo[0].Record);

			AssertContains("Expected to generate address ShipTo information from the delivery doc address",
				"N01|ST|SILVER|0|Silver Versailles|Versailles|Yvelines||YVE|78646|FR|MAIN||||>",
				sterling.NameInfo[1].Record);
		}

		public void TestSterlingNameOrgAddressFallBack()
		{
			var shipment = new Xsd.Shipment();

			var pickUpCartageOrg = shipment.ShipmentDetails.Pickup.CartageCompany;
			pickUpCartageOrg.EDICode = "PICKUP";
			var pickUpOrgAddress = pickUpCartageOrg.OrganisationDetails.Addresses.AddNew(Xsd.AddressCapabilityAddressType.MAIN);
			pickUpOrgAddress.CompanyName = "PICKUP ORG";
			pickUpOrgAddress.AddressLine1 = "352 Main St";
			pickUpOrgAddress.AddressLine2 = "Kings Cross";
			pickUpOrgAddress.StateOrProvince = "NSW";
			pickUpOrgAddress.PostCode = "2000";
			pickUpOrgAddress.Email = "pickUpOn@KingsCross.com";

			var deliveryCartageOrg = shipment.ShipmentDetails.Deliver.CartageCompany;
			deliveryCartageOrg.EDICode = "DELIVERY";
			var deliveryOrgAddress = deliveryCartageOrg.OrganisationDetails.Addresses.AddNew(Xsd.AddressCapabilityAddressType.DLV);
			deliveryOrgAddress.CompanyName = "DELIVERY ORG";
			deliveryOrgAddress.AddressLine1 = "23 Stork Rd";
			deliveryOrgAddress.AddressLine2 = "Ashfield, Sydney";
			deliveryOrgAddress.StateOrProvince = "NSW";
			deliveryOrgAddress.PostCode = "2131";
			deliveryOrgAddress.Email = "stork@deliveries.com.au";

			var consigneeOrg = shipment.ShipmentDetails.Consignee;
			consigneeOrg.EDICode = "CONSIGNEE";
			var consigneeOrgAddress = consigneeOrg.OrganisationDetails.Addresses.AddNew(Xsd.AddressCapabilityAddressType.MAIN);
			consigneeOrgAddress.CompanyName = "Consignee";
			consigneeOrgAddress.AddressLine1 = "23/7a John Ave";
			consigneeOrgAddress.AddressLine2 = "Sydney";
			consigneeOrgAddress.StateOrProvince = "NSW";
			consigneeOrgAddress.PostCode = "2000";
			consigneeOrgAddress.Email = "john@something.com";

			var sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, null, shipment, new Xsd.InterchangeInfo());

			AssertEquals("Expected three names to be generated", 3, sterling.NameInfo.Count);

			AssertContains("Expected to generate address ShipFrom address from org address belonging to the pick up cartage company",
				"N01|SF|PICKUP|1|PICKUP ORG|352 Main St|Kings Cross||NSW|2000||MAIN|||pickUpOn@KingsCross.com|>",
				sterling.NameInfo[0].Record);

			AssertContains("Expected to generate address ShipTo information from main address on the delivery cartage company",
				"N01|ST|DELIVERY|1|DELIVERY ORG|23 Stork Rd|Ashfield, Sydney||NSW|2131||MAIN|||stork@deliveries.com.au|>",
				sterling.NameInfo[1].Record);

			AssertContains("Expected to generate address from related org's main address",
				"N01|CN|CONSIGNEE|1|Consignee|23/7a John Ave|Sydney||NSW|2000||MAIN|The Import Manager||john@something.com|>",
				sterling.NameInfo[2].Record);

			var deliveryCartagePickUpAndDeliveryAddress = deliveryCartageOrg.OrganisationDetails.Addresses.AddNew(Xsd.AddressCapabilityAddressType.PAD);
			deliveryCartagePickUpAndDeliveryAddress.CompanyName = "DELIVERY";
			deliveryCartagePickUpAndDeliveryAddress.AddressLine1 = "PickUp and Delivery";
			deliveryCartagePickUpAndDeliveryAddress.AddressLine2 = "Arncliffe";
			deliveryCartagePickUpAndDeliveryAddress.StateOrProvince = "NSW";
			deliveryCartagePickUpAndDeliveryAddress.PostCode = "2205";
			deliveryCartagePickUpAndDeliveryAddress.Email = "pickupanddeliveries@deliveries.com";

			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, null, shipment, new Xsd.InterchangeInfo());

			AssertContains("Expected choose the pick up and delivery address over just the delivery address",
				"N01|ST|DELIVERY|2|DELIVERY|PickUp and Delivery|Arncliffe||NSW|2205||MAIN|||pickupanddeliveries@deliveries.com|>",
				sterling.NameInfo[1].Record);
		}

		#region Implementation

		void CreateName(Xsd.Organisation org, ZString orgCode, ZString location)
		{
			org.EDICode = orgCode;
			org.OrganisationDetails.Name = orgCode + "Full Name";
			org.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, location);
			org.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = orgCode;

			Xsd.OrgAddressCollection addressCollection = org.OrganisationDetails.Addresses;
			Xsd.OrgAddress mainAddress = addressCollection.AddNew(Xsd.AddressCapabilityAddressType.MAIN);
			mainAddress.CompanyName = orgCode;
			mainAddress.AddressLine1 = orgCode + "Versailles";
			mainAddress.AddressLine2 = orgCode + "Yvelines";
			mainAddress.StateOrProvince = orgCode + location;
			mainAddress.Sequence = 1;
			mainAddress.PostCode = "123321";
			mainAddress.Email = orgCode + "@BabiesDelivery.com";
		}

		#endregion
	}
}
