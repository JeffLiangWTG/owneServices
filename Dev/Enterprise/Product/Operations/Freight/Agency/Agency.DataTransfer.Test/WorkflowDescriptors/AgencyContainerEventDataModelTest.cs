using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	internal class AgencyContainerEventDataModelTest : TestCaseWithFactory
	{
		#region Origin
		public void TestOriginGetter_ReturnOriginPortFromBooking()
		{
			Booking.JS_NKLoadPort = "UAIEV";
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Property value", "UAIEV", model.Origin);
		}

		#endregion
		#region Destination
		public void TestDestinationGetter_ReturnDestinationPortFromBooking()
		{
			Booking.JS_NKDischargePort = "UAIEV";
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Property value", "UAIEV", model.Destination);
		}

		#endregion
		#region FirstLeg
		public void TestFirstLegGetter_SomeLegsExist_ReturnModelForBookingFirstLeg()
		{
			var transport1 = Booking.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Booking.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Booking.Transports.New(from: "USNYC", to: "UAIEV");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport1, model.FirstLeg.Parent_DebugOnly);
		}

		public void TestFirstLegGetter_NoLegsExist_ReturnNull()
		{
			Booking.Transports.RemoveAndDeleteAll();
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.FirstLeg);
		}

		#endregion
		#region SecondLeg
		public void TestSecondLegGetter_MoreThanTwoLegsExist_ReturnModelForBookingSecondLeg()
		{
			var transport1 = Booking.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Booking.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Booking.Transports.New(from: "USNYC", to: "UAIEV");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport2, model.SecondLeg.Parent_DebugOnly);
		}

		public void TestSecondLegGetter_LessThanTwoLegsExist_ReturnNull()
		{
			Booking.Transports.New(from: "AUSYD", to: "USLAX");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.SecondLeg);
		}

		#endregion
		#region ThirdLeg
		public void TestThirdLegGetter_MoreThanThreeLegsExist_ReturnModelForBookingThirdLeg()
		{
			var transport1 = Booking.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Booking.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Booking.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = Booking.Transports.New(from: "UAIEV", to: "AUMEL");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport3, model.ThirdLeg.Parent_DebugOnly);
		}

		public void TestThirdLegGetter_LessThanThreeLegsExist_ReturnNull()
		{
			Booking.Transports.New(from: "AUSYD", to: "USLAX");
			Booking.Transports.New(from: "USLAX", to: "USNYC");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.ThirdLeg);
		}

		#endregion
		#region FourthLeg
		public void TestFourthLegGetter_MoreThanFourLegsExist_ReturnModelForBookingFourthLeg()
		{
			var transport1 = Booking.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Booking.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Booking.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = Booking.Transports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = Booking.Transports.New(from: "AUMEL", to: "NZAKL");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport4, model.FourthLeg.Parent_DebugOnly);
		}

		public void TestFourthLegGetter_LessThanFourLegsExist_ReturnNull()
		{
			Booking.Transports.New(from: "AUSYD", to: "USLAX");
			Booking.Transports.New(from: "USLAX", to: "USNYC");
			Booking.Transports.New(from: "USNYC", to: "UAIEV");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.FourthLeg);
		}

		#endregion
		#region LastLeg
		public void TestLastLegGetter_SomeLegsExist_ReturnModelForBookingLastLeg()
		{
			var transport1 = Booking.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = Booking.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = Booking.Transports.New(from: "USNYC", to: "UAIEV");
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", transport3, model.LastLeg.Parent_DebugOnly);
		}

		public void TestLastLegGetter_NoLegsExist_ReturnNull()
		{
			Booking.Transports.RemoveAndDeleteAll();
			var model = new AgencyContainerEventDataModel(Container);
			AssertEquals("Wrapped transport", null, model.LastLeg);
		}

		#endregion
		#region Implementation
		AgencyShipment Booking
		{
			get
			{
				if (booking == null)
				{
					booking = Factory.New<AgencyShipment>();
				}

				return booking;
			}
		}

		AgencyShipment booking;
		AgencyShipmentContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Booking.ShippingContainers.AddNew();
				}

				return container;
			}
		}

		AgencyShipmentContainer container;
		#endregion
	}
}
