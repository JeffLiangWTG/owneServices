using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbConsignmentLoaderTest : DtbConsignmentTestCaseWithFactory
	{
		public void TestGetRelatedTransportConsignmentsAndChildrenForLocatingEDocs()
		{
			var transportBookingConsignmentTestHelper = new TransportBookingConsignmentTestHelper(Factory);

			var booking = transportBookingConsignmentTestHelper.CreateBooking("TB1");
			var consignment1 = Helper.CreateConsignment("LTC1");
			consignment1.LTC_KM_Booking = booking.PK;
			var consignment2 = Helper.CreateConsignment("LTC2");
			consignment2.LTC_KM_Booking = booking.PK;
			var actionForConsignment1 = Helper.CreateConsignmentAddressWithAction(consignment1, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery).Actions.First();

			Factory.Save();

			var loader = new DtbConsignmentLoader();
			var relatedObjects = loader.GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(booking);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { consignment1, consignment2, actionForConsignment1 }, relatedObjects);

			var consignment3 = Helper.CreateConsignment("LTC3");
			consignment3.LTC_KM_Booking = booking.PK;
			var actionForConsignment3 = Helper.CreateConsignmentAddressWithAction(consignment3, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp).Actions.First();

			Factory.Save();

			relatedObjects = loader.GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(booking);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { consignment1, consignment2, actionForConsignment1, consignment3, actionForConsignment3 }, relatedObjects);
		}

		public void TestGetRelatedTransportConsignmentsAndChildrenForLocatingEDocs_DbHits()
		{
			var transportBookingConsignmentTestHelper = new TransportBookingConsignmentTestHelper(Factory);
			var booking = transportBookingConsignmentTestHelper.CreateBooking("TB1");
			const int numberOfConsignments = 10;

			for (var i = 0; i < numberOfConsignments; i++)
			{
				var consignment = Helper.CreateConsignment("LTC" + i);
				consignment.LTC_KM_Booking = booking.PK;
				Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery).Actions.First();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var bookingInNewFactory = newFactory.Load<IDtbBooking>(booking.PK);
			var loader = new DtbConsignmentLoader();
			var expectedHits = new Dictionary<string, int>
			{
				{ DtbConsignmentSchema.Constants.TableName, 1 },
				{ DtbConsignmentActionSchema.Constants.TableName, 1 },
				{ DtbConsignmentAddressSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsForAllFactories("There should just be one db hit to load all actions, not one or more per consignment.", expectedHits))
			{
				var objects = loader.GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(bookingInNewFactory);
				AssertEquals("We need to enumerate the result in order to actually load the objects and hit the database. This ensures the test is working. Please do not remove this assertion.",
					numberOfConsignments * 2, objects.Count());
			}
		}
	}
}
