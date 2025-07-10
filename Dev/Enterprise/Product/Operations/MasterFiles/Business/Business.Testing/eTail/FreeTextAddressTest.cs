using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(FreeTextAddress<DummyConsignment>))]
public class FreeTextAddressTest : NonPersistentBusinessObjectTestCase
{
	Mock<DummyConsignment> mockConsignment;

	protected override void SetUp()
	{
		base.SetUp();
		mockConsignment = Factory.NewMoq<DummyConsignment>();
		SetupConsignmentDefaults();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var consignment = mockConsignment.Object;
		return new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Consignee);
	}

	public void TestConstructor_ThrowsException_ForReturnAddressType()
	{
		var consignment = mockConsignment.Object;

		AssertExceptionThrown<DeveloperNotificationException>("Return address is not supported.",() =>
		{
			_ = new FreeTextAddress<DummyConsignment>(consignment, Enterprise.Integration.Customs.ConsignmentAddressType.Return);
		});
	}

	public void TestConstructor_SetsConsigneeProperties_ForConsigneeAddressType()
	{
		var consignment = mockConsignment.Object;

		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		AssertEquals(ConsignmentAddressType.Consignee, address.AddressType);
		AssertEquals("Consignee", address.PartyType);
		AssertEquals("Test Consignee", address.PartyName);
		AssertEquals("123 Main St", address.Address1);
		AssertEquals("Suite 101", address.Address2);
		AssertEquals("New York", address.City);
		AssertEquals("NY", address.State);
		AssertEquals("10001", address.Postcode);
		AssertEquals("US", address.Country);
		AssertEquals("212-555-1212", address.Phone);
		AssertEquals("917-555-1212", address.Mobile);
		AssertEquals("212-555-1213", address.Fax);
		AssertEquals("consignee@example.com", address.Email);
	}

	public void TestConstructor_SetsShipperProperties_ForShipperAddressType()
	{
		var consignment = mockConsignment.Object;

		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Shipper);

		AssertEquals(ConsignmentAddressType.Shipper, address.AddressType);
		AssertEquals("Shipper", address.PartyType);
		AssertEquals("Test Shipper", address.PartyName);
		AssertEquals("456 Elm St", address.Address1);
		AssertEquals("Floor 2", address.Address2);
		AssertEquals("Los Angeles", address.City);
		AssertEquals("CA", address.State);
		AssertEquals("90001", address.Postcode);
		AssertEquals("CA", address.Country);
		AssertEquals("213-555-1212", address.Phone);
		AssertEquals("310-555-1212", address.Mobile);
		AssertEquals("213-555-1213", address.Fax);
		AssertEquals("shipper@example.com", address.Email);
	}
	public void TestHasSelectedAddress_ReturnsTrue_WhenConsigneeAddressIdIsNotEmpty()
	{
		mockConsignment.Setup(c => c.ConsigneeAddressId).Returns(new ZGuid("12345678-1234-1234-1234-123456789012"));
		var consignment = mockConsignment.Object;

		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		Assert(address.HasSelectedAddress);
	}

	public void TestHasSelectedAddress_ReturnsFalse_WhenConsigneeAddressIdIsEmpty()
	{
		mockConsignment.Setup(c => c.ConsigneeAddressId).Returns(ZGuid.Empty);
		var consignment = mockConsignment.Object;

		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		Assert(!address.HasSelectedAddress);
	}

	public void TestHasSelectedAddress_ReturnsTrue_WhenShipperAddressIdIsNotEmpty()
	{
		mockConsignment.Setup(c => c.ShipperAddressId).Returns(new ZGuid("12345678-1234-1234-1234-123456789012"));
		var consignment = mockConsignment.Object;

		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Shipper);

		Assert(address.HasSelectedAddress);
	}

	public void TestHasSelectedAddress_ReturnsFalse_WhenShipperAddressIdIsEmpty()
	{
		mockConsignment.Setup(c => c.ShipperAddressId).Returns(ZGuid.Empty);
		var consignment = mockConsignment.Object;

		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Shipper);

		Assert(!address.HasSelectedAddress);
	}

	public void TestAddressSettled_ReturnsTrue_WhenIgnoredIsTrue()
	{
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee)
		{
			Ignored = true
		};

		Assert(address.AddressSettled);
	}

	public void TestAddressSettled_ReturnsTrue_WhenHasSelectedAddress()
	{
		mockConsignment.Setup(c => c.ConsigneeAddressId).Returns(new ZGuid("12345678-1234-1234-1234-123456789012"));
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		Assert(address.AddressSettled);
	}

	public void TestAddressSettled_ReturnsFalse_WhenNotIgnoredAndNoSelectedAddress()
	{
		mockConsignment.Setup(c => c.ConsigneeAddressId).Returns(ZGuid.Empty);
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		Assert(!address.AddressSettled);
	}

	public void TestCreateNewOrganizationByCurrentAddressInfo_CreatesOrgWithConsigneeDetails()
	{
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		var org = address.CreateNewOrganizationByCurrentAddressInfo();

		AssertNotNull(org);
		Assert(org.OH_IsConsignee);
		AssertEquals("Test Consignee", org.OH_FullName);
		AssertEquals("123 Main St", org.MainAddress.OA_Address1);
		AssertEquals("Suite 101", org.MainAddress.OA_Address2);
		AssertEquals("New York", org.MainAddress.OA_City);
		AssertEquals("NY", org.MainAddress.OA_State);
		AssertEquals("10001", org.MainAddress.OA_PostCode);
		AssertEquals("US", org.MainAddress.OA_RN_NKCountryCode);
		AssertEquals("212-555-1212", org.MainAddress.OA_Phone);
		AssertEquals("917-555-1212", org.MainAddress.OA_Mobile);
		AssertEquals("212-555-1213", org.MainAddress.OA_Fax);
		AssertEquals("consignee@example.com", org.MainAddress.OA_Email);
	}

	public void TestCreateNewOrganizationByCurrentAddressInfo_CreatesOrgWithShipperDetails()
	{
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Shipper);

		var org = address.CreateNewOrganizationByCurrentAddressInfo();

		AssertNotNull(org);
		Assert(org.OH_IsConsignor);
		AssertEquals("Test Shipper", org.OH_FullName);
		AssertEquals("456 Elm St", org.MainAddress.OA_Address1);
		AssertEquals("Floor 2", org.MainAddress.OA_Address2);
		AssertEquals("Los Angeles", org.MainAddress.OA_City);
		AssertEquals("CA", org.MainAddress.OA_State);
		AssertEquals("90001", org.MainAddress.OA_PostCode);
		AssertEquals("CA", org.MainAddress.OA_RN_NKCountryCode);
		AssertEquals("213-555-1212", org.MainAddress.OA_Phone);
		AssertEquals("310-555-1212", org.MainAddress.OA_Mobile);
		AssertEquals("213-555-1213", org.MainAddress.OA_Fax);
		AssertEquals("shipper@example.com", org.MainAddress.OA_Email);
	}

	public void TestSelectAddress_UpdatesConsigneeAddressId()
	{
		var addressPK = ZGuid.NewZGuid();
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		address.SelectAddress(addressPK);

		AssertEquals(addressPK, consignment.ConsigneeAddressId);
	}

	public void TestSelectAddress_UpdatesShipperAddressId()
	{
		var addressPK = ZGuid.NewZGuid();
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Shipper);

		address.SelectAddress(addressPK);

		AssertEquals(addressPK, consignment.ShipperAddressId);
	}

	public void TestIgnore_SetsIgnoredToTrue()
	{
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		address.Ignore();

		Assert(address.Ignored);
	}

	public void TestHasChanges_ReturnsTrueWhenConsignmentHasChanges()
	{
		mockConsignment.Setup(c => c.HasChanges).Returns(true);
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		Assert(address.HasChanges);
	}

	public void TestHasChanges_ReturnsFalseWhenConsignmentHasNoChanges()
	{
		mockConsignment.Setup(c => c.HasChanges).Returns(false);
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		Assert(!address.HasChanges);
	}

	public void TestSimilarOrgMatches_IsNotNull()
	{
		var consignment = mockConsignment.Object;
		var address = new FreeTextAddress<DummyConsignment>(consignment, ConsignmentAddressType.Consignee);

		var matches = address.SimilarOrgMatches;

		AssertNotNull(matches);
	}

	void SetupConsignmentDefaults()
	{
		mockConsignment.Setup(c => c.WaybillNumber).Returns("WB123");

		mockConsignment.Setup(c => c.ConsigneeName).Returns("Test Consignee");
		mockConsignment.Setup(c => c.ConsigneeAddress1).Returns("123 Main St");
		mockConsignment.Setup(c => c.ConsigneeAddress2).Returns("Suite 101");
		mockConsignment.Setup(c => c.ConsigneeCity).Returns("New York");
		mockConsignment.Setup(c => c.ConsigneeState).Returns("NY");
		mockConsignment.Setup(c => c.ConsigneePostcode).Returns("10001");
		mockConsignment.Setup(c => c.ConsigneeCountryCode).Returns("US");
		mockConsignment.Setup(c => c.ConsigneePhone).Returns("212-555-1212");
		mockConsignment.Setup(c => c.ConsigneeMobile).Returns("917-555-1212");
		mockConsignment.Setup(c => c.ConsigneeFax).Returns("212-555-1213");
		mockConsignment.Setup(c => c.ConsigneeEmail).Returns("consignee@example.com");

		mockConsignment.Setup(c => c.ShipperName).Returns("Test Shipper");
		mockConsignment.Setup(c => c.ShipperAddress1).Returns("456 Elm St");
		mockConsignment.Setup(c => c.ShipperAddress2).Returns("Floor 2");
		mockConsignment.Setup(c => c.ShipperCity).Returns("Los Angeles");
		mockConsignment.Setup(c => c.ShipperState).Returns("CA");
		mockConsignment.Setup(c => c.ShipperPostcode).Returns("90001");
		mockConsignment.Setup(c => c.ShipperCountryCode).Returns("CA");
		mockConsignment.Setup(c => c.ShipperPhone).Returns("213-555-1212");
		mockConsignment.Setup(c => c.ShipperMobile).Returns("310-555-1212");
		mockConsignment.Setup(c => c.ShipperFax).Returns("213-555-1213");
		mockConsignment.Setup(c => c.ShipperEmail).Returns("shipper@example.com");
	}
}
