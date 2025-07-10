using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(FreeTextAddressCollection<DummyConsignment>))]

public class FreeTextAddressCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FreeTextAddressCollection<DummyConsignment>>
{
	protected override void SetUp()
	{
		base.SetUp();
		conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		collection = new FreeTextAddressCollection<DummyConsignment>(conversion);
	}

	protected override FreeTextAddressCollection<DummyConsignment> GetCollectionToTest()
	{
		return collection;
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var consignment = CreateDummyConsignment(false, false);
		return new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee);
	}

	public void TestAddFromConsignments_AddsConsigneeAddresses_WhenConsigneeIsNotOrganisation()
	{
		var consignment = CreateDummyConsignment(false, true); // Consignee is not org, Shipper is org
		var consignments = new List<DummyConsignment> { consignment };

		collection.AddFromConsignments(consignments);

		AssertEquals(1, collection.Count);
		var address = collection.Cast<FreeTextAddress<DummyConsignment>>().First();
		AssertEquals(ConsignmentAddressType.Consignee, address.AddressType);
		AssertEquals(consignment, address.Consignment);
	}

	public void TestAddFromConsignments_AddsShipperAddresses_WhenShipperIsNotOrganisation()
	{
		var consignment = CreateDummyConsignment(true, false); // Consignee is org, Shipper is not org
		var consignments = new List<DummyConsignment> { consignment };

		collection.AddFromConsignments(consignments);

		AssertEquals(1, collection.Count);
		var address = collection.Cast<FreeTextAddress<DummyConsignment>>().First();
		AssertEquals(ConsignmentAddressType.Shipper, address.AddressType);
		AssertEquals(consignment, address.Consignment);
	}

	public void TestAddFromConsignments_AddsBothAddresses_WhenNeitherIsOrganisation()
	{
		var consignment = CreateDummyConsignment(false, false); // Neither consignee nor shipper is org
		var consignments = new List<DummyConsignment> { consignment };

		collection.AddFromConsignments(consignments);

		AssertEquals(2, collection.Count);

		var addressTypes = collection
			.Cast<FreeTextAddress<DummyConsignment>>()
			.Select(a => a.AddressType)
			.OrderBy(t => t)
			.ToList();

		AssertEquals(ConsignmentAddressType.Consignee, addressTypes[0]);
		AssertEquals(ConsignmentAddressType.Shipper, addressTypes[1]);

		foreach (var address in collection.Cast<FreeTextAddress<DummyConsignment>>())
		{
			AssertEquals(consignment, address.Consignment);
		}
	}

	public void TestAddFromConsignments_DoesNotAddAddresses_WhenBothAreOrganisations()
	{
		var consignment = CreateDummyConsignment(true, true); // Both consignee and shipper are orgs
		var consignments = new List<DummyConsignment> { consignment };

		collection.AddFromConsignments(consignments);

		AssertEquals(0, collection.Count);
	}

	public void TestAddFromConsignments_HandlesMultipleConsignments()
	{
		var consignments = new List<DummyConsignment>
		{
			CreateDummyConsignment(false, true),  // Consignee only
			CreateDummyConsignment(true, false),  // Shipper only
			CreateDummyConsignment(false, false), // Both consignee and shipper
		};

		collection.AddFromConsignments(consignments);

		AssertEquals(4, collection.Count); // 1 + 1 + 2 = 4

		var consigneeCount = collection.Cast<FreeTextAddress<DummyConsignment>>().Count(a => a.AddressType == ConsignmentAddressType.Consignee);

		var shipperCount = collection.Cast<FreeTextAddress<DummyConsignment>>().Count(a => a.AddressType == ConsignmentAddressType.Shipper);

		AssertEquals(2, consigneeCount);
		AssertEquals(2, shipperCount);
	}

	public void TestAddFromConsignments_ImportsConsignmentsFromDifferentFactory()
	{
		var otherFactory = new BusinessObjectFactory();
		var consignment = otherFactory.New<DummyConsignment>();

		consignment.ConsigneeIsOrganisation = false;
		consignment.ShipperIsOrganisation = false;

		var consignments = new List<DummyConsignment> { consignment };

		collection.AddFromConsignments(consignments);

		AssertEquals(2, collection.Count);

		foreach (var address in collection.Cast<FreeTextAddress<DummyConsignment>>())
		{
			// The consignment should be imported into the collection's factory
			AssertEquals(Factory, address.Factory);

			// The consignment in the address should be from our factory, not the original factory
			AssertNotEquals(consignment, address.Consignment);
			AssertEquals(Factory, address.Consignment.Factory);
		}
	}

	public void TestCreateNonPersistentBusinessObject_CreatesNewFreeTextAddress()
	{
		var result = collection.AddNew();

		AssertNotNull(result);
		AssertType<FreeTextAddress<DummyConsignment>>(result);
		AssertEquals(Factory, result.Factory);
	}

	DummyConsignment CreateDummyConsignment(bool consigneeIsOrg, bool shipperIsOrg)
	{
		var consignment = Factory.NewMoq<DummyConsignment>().Object;
		consignment.ConsigneeIsOrganisation = consigneeIsOrg;
		consignment.ShipperIsOrganisation = shipperIsOrg;

		// Set up basic properties
		consignment.WaybillNumber = "WB123";

		// Consignee details
		consignment.ConsigneeName = "Test Consignee";
		consignment.ConsigneeAddress1 = "123 Main St";
		consignment.ConsigneeCity = "New York";
		consignment.ConsigneeState = "NY";
		consignment.ConsigneePostcode = "10001";
		consignment.ConsigneeCountryCode = "US";

		// Shipper details
		consignment.ShipperName = "Test Shipper";
		consignment.ShipperAddress1 = "456 Elm St";
		consignment.ShipperCity = "Los Angeles";
		consignment.ShipperState = "CA";
		consignment.ShipperPostcode = "90001";
		consignment.ShipperCountryCode = "US";

		return consignment;
	}

	FreeTextAddressConversion<DummyConsignment> conversion;
	FreeTextAddressCollection<DummyConsignment> collection;
}
