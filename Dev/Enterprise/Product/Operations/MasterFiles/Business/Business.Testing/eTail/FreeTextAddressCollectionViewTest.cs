using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(FreeTextAddressCollectionView<DummyConsignment>))]
public class FreeTextAddressCollectionViewTest : NonPersistentBusinessObjectCollectionViewTestCase<FreeTextAddressCollectionView<DummyConsignment>>
{
	protected override void SetUp()
	{
		base.SetUp();
		conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		collection = new FreeTextAddressCollection<DummyConsignment>(conversion);
		view = new FreeTextAddressCollectionView<DummyConsignment>(collection, conversion);
	}

	protected override FreeTextAddressCollectionView<DummyConsignment> GetCollectionToTest()
	{
		return view;
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var consignment = CreateDummyConsignment(false, false);
		return new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee);
	}

	public void TestOnCountChanged_SetsFinishedToTrue_WhenLastItemIsRemoved()
	{
		var consignment = Factory.NewWithValidTestData<DummyConsignment>();
		consignment.ConsigneeIsOrganisation = false;

		var address = new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee);
		collection.Add(address);

		conversion.Finished = false;

		collection.Remove(address); // This should trigger OnCountChanged in the view

		Assert(conversion.Finished);
	}

	public void TestIsThisPartOfTheCollection_ReturnsTrue_ForNonSettledAddresses()
	{
		var consignment = Factory.NewWithValidTestData<DummyConsignment>();
		consignment.ConsigneeIsOrganisation = false;
		var address = new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee);

		Assert(IsPartOfCollection(address));
	}

	public void TestIsThisPartOfTheCollection_ReturnsFalse_ForSettledAddresses()
	{
		var consignment = Factory.NewWithValidTestData<DummyConsignment>();
		consignment.ConsigneeIsOrganisation = false;
		var address = new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee);
		address.Ignored = true;

		Assert(!IsPartOfCollection(address));
	}

	public void TestIsThisPartOfTheCollection_ReturnsFalse_ForNonFreeTextAddressObjects()
	{
		var otherObject = Factory.New<OrgHeader>();

		Assert(!IsPartOfCollection(otherObject));
	}

	public void TestAllowNew_ReturnsFalse()
	{
		Assert(!view.AllowNew);
	}

	bool IsPartOfCollection(BusinessObject element)
	{
		var methodInfo = typeof(FreeTextAddressCollectionView<DummyConsignment>).GetMethod("IsThisPartOfTheCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		return (bool)methodInfo?.Invoke(view, [element])!;
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
	FreeTextAddressCollectionView<DummyConsignment> view;
}
