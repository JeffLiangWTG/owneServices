using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling.Testing
{
	[TestedType(typeof(ISFDocAddressesExcludeManufacturerCollection))]
	sealed class ISFDocAddressesExcludeManufacturerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionContainsCorrectAddresses()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			AssertNotNull(header.BookingParty);
			AssertNotNull(header.MainShipToParty);
			AssertNotNull(header.SellingParty);
			AssertNotNull(header.BuyingParty);
			AssertNotNull(header.StuffingLocation);
			AssertNotNull(header.Consolidator);
			AssertEquals(header.ManufacturerAddresses.Count, 0);
			AssertEquals(header.DocAddresses.Count, 6);
			AssertEquals(header.DocAddressesExcludeManufacturer.Count, 0);

			header.ManufacturerAddresses.AddNew();
			AssertEquals(header.ManufacturerAddresses.Count, 1);
			AssertEquals(header.DocAddresses.Count, 7);
			AssertEquals(header.DocAddressesExcludeManufacturer.Count, 0);

			header.DocAddressesExcludeManufacturer.AddNew();
			AssertEquals(header.ManufacturerAddresses.Count, 1);
			AssertEquals(header.DocAddresses.Count, 8);
			AssertEquals(header.DocAddressesExcludeManufacturer.Count, 1);
		}

		public void TestNewElementInCollectionAppearsOnDocAddresses()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			header.DocAddressesExcludeManufacturer.AddNew();
			AssertEquals("new element added", header.DocAddressesExcludeManufacturer.Count, 1);
			AssertEquals("element appeared in DocAddresses", header.DocAddressesExcludeManufacturer.Count, header.DocAddresses.Count - 6);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ISFDocAddressesExcludeManufacturerCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			return new ISFDocAddressesExcludeManufacturerCollection(header);
		}
	}
}
