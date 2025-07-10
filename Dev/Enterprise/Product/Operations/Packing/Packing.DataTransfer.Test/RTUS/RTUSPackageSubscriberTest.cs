using System;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class RTUSPackageSubscriberTest : PackingTestCaseWithFactory
	{
		public void TestRTUSPackageSubscriber_Subscribe()
		{
			Data.CreatePackingData();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var rtusSubscriber = new RTUSPackageSubscriber();
				var result = rtusSubscriber.SubscribePackage(package, RequestType.Booking);
				AssertEquals("Subscription is successful.", true, result.Success);
				AssertEquals("Subscription is successful.", true, string.IsNullOrEmpty(result.Message));
			}
		}

		public void TestRTUSPackageSubscriber_SubscribeFailed()
		{
			Data.CreatePackingData();

			var mockHandle = new DummyHandle(() => throw new InvalidOperationException("Oops!"));

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var rtusSubscriber = new RTUSPackageSubscriber();
				var result = rtusSubscriber.SubscribePackage(package, RequestType.Booking);
				AssertEquals("Subscription failed.", false, result.Success);
				AssertEquals("Subscription failed.", "Failed to generate Universal Shipment from Package 'ABC'.\r\nDetails below:\r\nOops!", result.Message);
			}
		}

		public void TestRTUSPackageSubscriber_IsPackageSubscribed()
		{
			Data.CreatePackingData();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var rtusSubscriber = new RTUSPackageSubscriber();
				var result = rtusSubscriber.SubscribePackage(package, RequestType.Booking);
				AssertEquals("Precondition: Subscription is successful.", true, result.Success);

				AssertEquals("Package is subscribed.", true, rtusSubscriber.IsPackageSubscribed(package));
				AssertEquals("Random package is not subscribed.", false, rtusSubscriber.IsPackageSubscribed(Factory.New<PkgPackage>()));
			}
		}

		public void TestRTUSPackageSubscriber_GetRequestXML()
		{
			Data.CreatePackingData();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var rtusSubscriber = new RTUSPackageSubscriber();
				var result = rtusSubscriber.SubscribePackage(package, RequestType.Booking);
				AssertEquals("Precondition: Subscription is successful.", true, result.Success);

				var request = rtusSubscriber.GetRequestXML(package);
				AssertEquals("Returned request is correct.", "ABC", request.PackageID);
				AssertEquals("Returned request is correct.", Data.PackageJob.PK, request.PackageJobPK);
				AssertEquals("Returned request is correct.", Data.Dummy, request.PackingParent);
				AssertEquals("Returned request is correct.", "TEST", request.ShipmentDataObject.BookingConfirmationReference);
			}
		}

		public void TestRTUSPackageSubscriber_GetRequestXML_NotSubscribedPackage()
		{
			Data.CreatePackingData();

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => parentShipment);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("PLT", "ABC");
				var rtusSubscriber = new RTUSPackageSubscriber();
				var result = rtusSubscriber.SubscribePackage(package, RequestType.Booking);
				AssertEquals("Precondition: Subscription is successful.", true, result.Success);

				var package2 = Data.PackageJob.Packages.AddNew("PLT", "DEF");

				var request = rtusSubscriber.GetRequestXML(package2);
				AssertEquals("Returned package id is the package's id.", "DEF", request.PackageID);
				AssertEquals("Returned package job pk is the package job's PK.", Data.PackageJob.PK, request.PackageJobPK);
				AssertNull("Returned package parent is null.", request.PackingParent);
				AssertNull("Returned shipment data object is null.", request.ShipmentDataObject);
			}
		}
	}
}
