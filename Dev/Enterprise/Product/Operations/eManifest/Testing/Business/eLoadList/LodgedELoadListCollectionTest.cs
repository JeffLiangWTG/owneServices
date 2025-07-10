
namespace Enterprise.eManifest.Testing.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Core;
	using Enterprise.eManifest.Business;
	using Enterprise.Freight.Business;
	using NUnit.Framework;

	[TestedType(typeof(LodgedELoadListCollection))]
	internal class LodgedELoadListCollectionTest : ActiveBusinessObjectCollectionTestCase<LodgedELoadListCollection>
	{
		public void TestConstructor_ConsolIsNull_ThrowArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => { var collectionToTest = new LodgedELoadListCollection(Factory, null); });
		}

		public void TestFiltering_ShouldReturnELoadListsAccordingToFilter()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			// Lodged, not empty, attached
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			list1.DO_Status = Constants.ELoadListStatuses.Lodged;
			var bookingLine1 = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine1.DL_DO_LoadList = list1.PK;
			bookingLine1.DL_JS_ApprovedShipment = shipment.PK;

			// Not lodged, not empty, not attached
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			list2.DO_Status = Constants.ELoadListStatuses.Consolidated;
			var bookingLine2 = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine2.DL_DO_LoadList = list2.PK;

			// Lodged, not empty, not attached
			var list3 = Factory.NewWithValidTestData<ELoadList>();
			list3.DO_Status = Constants.ELoadListStatuses.Lodged;
			var bookingLine3 = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine3.DL_DO_LoadList = list3.PK;

			// Not lodged, not empty, not attached
			var list4 = Factory.NewWithValidTestData<ELoadList>();
			list4.DO_Status = Constants.ELoadListStatuses.Consolidated;
			var bookingLine4 = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine4.DL_JS_ApprovedShipment = shipment.PK;

			// Lodged, empty, not attached
			var list5 = Factory.NewWithValidTestData<ELoadList>();
			list5.DO_Status = Constants.ELoadListStatuses.Lodged;

			Factory.Save();

			var collectionToTest = new LodgedELoadListCollection(Factory, consol);

			var expectedLists = new[] { list3 };
			AssertContainsExactElementsInAnyOrder("Only lodged, not empty and not attached eLoadLists should be returned", expectedLists, collectionToTest);
		}

		public void TestSelection_NonLodgedELoadListIsSelected_GenerateError()
		{
			var list = Factory.NewWithValidTestData<ELoadList>();
			list.DO_Status = Constants.ELoadListStatuses.Consolidated;

			Factory.Save();

			var errors = new StringCollectionX();
			var collectionToTest = new LodgedELoadListCollectionForTest(Factory);
			collectionToTest.AddNotificationForTest(errors, list);

			AssertEquals("Collection should have an error", 1, errors.Count);
		}

		public void TestSelection_EmptyELoadListIsSelected_GenerateError()
		{
			var list = Factory.NewWithValidTestData<ELoadList>();
			list.DO_Status = Constants.ELoadListStatuses.Lodged;

			Factory.Save();

			var errors = new StringCollectionX();
			var collectionToTest = new LodgedELoadListCollectionForTest(Factory);
			collectionToTest.AddNotificationForTest(errors, list);

			AssertEquals("Collection should have an error", 1, errors.Count);
		}

		public void TestSelection_AttachedELoadListIsSelected_GenerateError()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var list = Factory.NewWithValidTestData<ELoadList>();
			list.DO_Status = Constants.ELoadListStatuses.Lodged;

			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_DO_LoadList = list.PK;
			bookingLine.DL_JS_ApprovedShipment = shipment.PK;

			Factory.Save();

			var errors = new StringCollectionX();
			var collectionToTest = new LodgedELoadListCollectionForTest(Factory, consol);
			collectionToTest.AddNotificationForTest(errors, list);

			AssertEquals("Collection should have an error", 1, errors.Count);
		}

		public void TestSelection_AllowedELoadListIsSelected_CollectionShouldNotHaveErrors()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var list = Factory.NewWithValidTestData<ELoadList>();
			list.DO_Status = Constants.ELoadListStatuses.Lodged;

			var bookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			bookingLine.DL_DO_LoadList = list.PK;

			Factory.Save();

			var errors = new StringCollectionX();
			var collectionToTest = new LodgedELoadListCollectionForTest(Factory, consol);
			collectionToTest.AddNotificationForTest(errors, list);

			AssertEquals("Collection should not have errors", 0, errors.Count);
		}

		#region Types

		class LodgedELoadListCollectionForTest : LodgedELoadListCollection
		{
			public LodgedELoadListCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public LodgedELoadListCollectionForTest(BusinessObjectFactory factory, CommonConsol consol)
				: base(factory, consol)
			{
			}

			public void AddNotificationForTest(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			}
		}

		#endregion
	}
}
