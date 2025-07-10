using System.Linq;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ForwardingContainerWithPackLinesDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Add Info Collection

		public void TestContainerAddInfoCollection_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var booking = Factory.New<JobSupplierBooking>();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var container1 = consol.Containers.AddNew();
				container1.FillWithValidTestData();
				container1.JC_JSB_SupplierBooking = booking.PK;

				var loadList = Factory.New<CYContainerLoadList>();
				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.FillWithValidTestData();
				bookingLine.JSL_JSB_Booking = booking.PK;
				var container2 = consol.Containers.AddNew();
				container2.FillWithValidTestData();
				container2.JC_JSB_SupplierBooking = booking.PK;
				var loadListLine = loadList.LoadListLines.AddNew();
				loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine.CLL_JC_Container = container2.PK;

				var loadPlan = Factory.New<CFSContainerLoadList>();
				var container3 = consol.Containers.AddNew();
				container3.FillWithValidTestData();
				container3.JC_CLH_LoadListPlan = loadPlan.PK;

				var container4 = consol.Containers.AddNew();
				container4.FillWithValidTestData();

				var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
				var consolDataObject = writer.GetDataObject(consol);
				AssertNotNull(consolDataObject);

				var containerCollection = consolDataObject.ContainerCollection;
				AssertEquals(4, containerCollection.Count);
				var container1DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container1.JC_ContainerNum));
				var container2DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container2.JC_ContainerNum));
				var container3DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container3.JC_ContainerNum));
				var container4DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container4.JC_ContainerNum));

				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(container1DataObject.AddInfoCollection, count: 1, bookingKey: booking.JSB_BookingId);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(container2DataObject.AddInfoCollection, count: 2, bookingKey: booking.JSB_BookingId, containerLoadListKey: loadList.CLH_LoadListId);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(container3DataObject.AddInfoCollection, count: 1, containerLoadListKey: loadPlan.CLH_LoadListId);
				AssertNull(container4DataObject.AddInfoCollection);

				loadPlan.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				loadList.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
				consolDataObject = writer.GetDataObject(consol);
				AssertNotNull(consolDataObject);
				containerCollection = consolDataObject.ContainerCollection;
				container1DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container1.JC_ContainerNum));
				container2DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container2.JC_ContainerNum));
				container3DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container3.JC_ContainerNum));

				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(container1DataObject.AddInfoCollection, count: 1, bookingKey: booking.JSB_BookingId);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(container2DataObject.AddInfoCollection, count: 1, bookingKey: booking.JSB_BookingId);
				AssertNull(container3DataObject.AddInfoCollection);

				booking.JSB_Status = Constants.SupplierBookingStatus.Cancelled;
				writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
				consolDataObject = writer.GetDataObject(consol);
				AssertNotNull(consolDataObject);
				containerCollection = consolDataObject.ContainerCollection;
				container1DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container1.JC_ContainerNum));
				container2DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container2.JC_ContainerNum));

				AssertNull(container1DataObject.AddInfoCollection);
				AssertNull(container2DataObject.AddInfoCollection);
			});
		}

		public void TestContainerAddInfoCollection_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var booking = Factory.New<JobSupplierBooking>();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var container1 = consol.Containers.AddNew();
				container1.FillWithValidTestData();
				container1.JC_JSB_SupplierBooking = booking.PK;

				var loadList = Factory.New<CYContainerLoadList>();
				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.FillWithValidTestData();
				bookingLine.JSL_JSB_Booking = booking.PK;
				var container2 = consol.Containers.AddNew();
				container2.FillWithValidTestData();
				container2.JC_JSB_SupplierBooking = booking.PK;
				var loadListLine = loadList.LoadListLines.AddNew();
				loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine.CLL_JC_Container = container2.PK;

				var loadPlan = Factory.New<CFSContainerLoadList>();
				var container3 = consol.Containers.AddNew();
				container3.FillWithValidTestData();
				container3.JC_CLH_LoadListPlan = loadPlan.PK;

				var container4 = consol.Containers.AddNew();
				container4.FillWithValidTestData();

				var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
				var consolDataObject = writer.GetDataObject(consol);
				AssertNotNull(consolDataObject);

				var containerCollection = consolDataObject.ContainerCollection;
				AssertEquals(4, containerCollection.Count);
				var container1DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container1.JC_ContainerNum));
				var container2DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container2.JC_ContainerNum));
				var container3DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container3.JC_ContainerNum));
				var container4DataObject = containerCollection.First(c => c.ContainerNumber.Equals(container4.JC_ContainerNum));
				
				AssertNull(container1DataObject.AddInfoCollection);
				AssertNull(container2DataObject.AddInfoCollection);
				AssertNull(container3DataObject.AddInfoCollection);
				AssertNull(container4DataObject.AddInfoCollection);
			});
		}

		#endregion
	}
}
