using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FreeTextAddressConversion<DummyConsignment>))]
	public class FreeTextAddressConversionTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			mockConsignment1 = Factory.NewMoq<DummyConsignment>();
			mockConsignment2 = Factory.NewMoq<DummyConsignment>();
			SetupConsignmentDefaults(mockConsignment1);
			SetupConsignmentDefaults(mockConsignment2);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FreeTextAddressConversion<DummyConsignment>(Factory);
		}

		public void TestConstructor_InitializesProperties()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;

			AssertNotNull(conversion.AddressesToConvertView);
			Assert(!conversion.Finished);
		}

		public void TestAddToList_AddsConsignmentsToAddressesToConvertView()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var consignments = new[] { mockConsignment1.Object, mockConsignment2.Object };

			conversion.AddToList(consignments);

			AssertEquals("With both address types, we should have 4 addresses (2 consignments * 2 address types)", 4, conversion.AddressesToConvertView.Count);

			var addedConsignments = conversion.AddressesToConvertView
				.Cast<FreeTextAddress<DummyConsignment>>()
				.Select(a => a.Consignment)
				.Distinct()
				.ToList();

			AssertEquals("Verify that all consignments were added", 2, addedConsignments.Count);
		}

		public void TestHasSelectedAddresses_ReturnsFalse_WhenNoAddressesSelected()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var consignments = new[] { mockConsignment1.Object };

			mockConsignment1.Setup(c => c.ConsigneeAddressId).Returns(ZGuid.Empty);
			mockConsignment1.Setup(c => c.ShipperAddressId).Returns(ZGuid.Empty);

			conversion.AddToList(consignments);

			Assert(!conversion.HasSelectedAddresses);
		}

		public void TestHasSelectedAddresses_ReturnsTrue_WhenAddressIsSelected()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var consignments = new[] { mockConsignment1.Object };

			mockConsignment1.Setup(c => c.ConsigneeAddressId).Returns(new ZGuid("12345678-1234-1234-1234-123456789012"));

			conversion.AddToList(consignments);

			Assert(conversion.HasSelectedAddresses);
		}

		public void TestHasChanges_ReturnsFalse_WhenNoAddressesHaveChanges()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var consignments = new[] { mockConsignment1.Object };

			mockConsignment1.Setup(c => c.HasChanges).Returns(false);

			conversion.AddToList(consignments);

			Assert(!conversion.HasChanges);
		}

		public void TestHasChanges_ReturnsTrue_WhenAddressHasChanges()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var consignments = new[] { mockConsignment1.Object };

			mockConsignment1.Setup(c => c.HasChanges).Returns(true);

			conversion.AddToList(consignments);

			Assert(conversion.HasChanges);
		}

		public void TestFinished_SetToTrue_RaisesOnFinishedEvent()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var eventRaised = false;

			conversion.OnFinished += (s, e) => eventRaised = true;
			conversion.Finished = true;

			Assert(conversion.Finished);
			Assert(eventRaised);
		}

		public void TestFinished_SetToFalse_DoesNotRaiseOnFinishedEvent()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;
			var eventRaised = false;

			conversion.OnFinished += (s, e) => eventRaised = true;
			conversion.Finished = false;

			Assert(!eventRaised);
		}

		public void TestAddToList_WithEmptyConsignmentList_DoesNotAddAnyAddresses()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;

			conversion.AddToList(Array.Empty<DummyConsignment>());

			AssertEquals(0, conversion.AddressesToConvertView.Count);
		}

		public void TestAddToList_WithNullConsignmentList_DoesNotThrowException()
		{
			var conversion = (FreeTextAddressConversion<DummyConsignment>)CachedBusinessObject;

			AssertNoExceptionThrown(() => conversion.AddToList(null));
		}

		void SetupConsignmentDefaults(Mock<DummyConsignment> mockConsignment)
		{
			mockConsignment.Setup(c => c.WaybillNumber).Returns("WB123");
			mockConsignment.Setup(c => c.ConsigneeIsOrganisation).Returns(false);
			mockConsignment.Setup(c => c.ShipperIsOrganisation).Returns(false);

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

		Mock<DummyConsignment> mockConsignment1;
		Mock<DummyConsignment> mockConsignment2;
	}
}
