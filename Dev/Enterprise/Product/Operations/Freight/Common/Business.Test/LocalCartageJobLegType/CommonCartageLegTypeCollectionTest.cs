using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestedType(typeof(CommonCartageLegTypeCollection))]
	sealed class CommonCartageLegTypeCollectionTest : ActiveBusinessObjectCollectionTestCase<CommonCartageLegTypeCollection>
	{
		protected override CommonCartageLegTypeCollection GetCollectionToTest()
		{
			return new CommonCartageLegTypeCollection(Factory.New<CommonCartageType>());
		}

		public void TestIsNotBooking()
		{
			CommonCartageLegTypeCollection col = new CommonCartageLegTypeCollection(Factory.New<CommonCartageType>());
			CommonCartageLegType legType = col.AddNew();
			Assert(!legType.E4_IsBooking);
		}

		public void TestLegTypeCollectionStrategy()
		{
			CommonCartageType jobType = Factory.New<CommonCartageType>();

			CommonCartageLegType containerizedBooking = Factory.New<CommonCartageLegType>();
			containerizedBooking.E4_IsBooking = true;
			containerizedBooking.E4_ContainerMode = Constants.CartageContainerMode.Containerized;
			containerizedBooking.E4_E3 = jobType.PK;
			CommonCartageLegType looseBooking = Factory.New<CommonCartageLegType>();
			looseBooking.E4_IsBooking = true;
			looseBooking.E4_ContainerMode = Constants.CartageContainerMode.Loose;
			looseBooking.E4_E3 = jobType.PK;
			CommonCartageLegType containerizedLeg = Factory.New<CommonCartageLegType>();
			containerizedLeg.E4_IsBooking = false;
			containerizedLeg.E4_ContainerMode = Constants.CartageContainerMode.Containerized;
			containerizedLeg.E4_E3 = jobType.PK;
			CommonCartageLegType looseLeg = Factory.New<CommonCartageLegType>();
			looseLeg.E4_IsBooking = false;
			looseLeg.E4_ContainerMode = Constants.CartageContainerMode.Loose;
			looseLeg.E4_E3 = jobType.PK;

			LegTypeCollectionStrategy strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.Booking, LegTypeCollectionStrategy.LegContainerMode.Containerized);
			CommonCartageLegTypeCollection col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("Containerized Booking", 1, col.Count);
			AssertCollectionContains("Containerized Booking", containerizedBooking, col);

			strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.Booking, LegTypeCollectionStrategy.LegContainerMode.Loose);
			col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("Loose Booking", 1, col.Count);
			AssertCollectionContains("Loose Booking", looseBooking, col);

			strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.Booking, LegTypeCollectionStrategy.LegContainerMode.All);
			col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("All Bookings", 2, col.Count);
			AssertCollectionContains("Loose Booking", looseBooking, col);
			AssertCollectionContains("Containerized Booking", containerizedBooking, col);

			strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.NonBooking, LegTypeCollectionStrategy.LegContainerMode.Containerized);
			col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("Containerized Leg", 1, col.Count);
			AssertCollectionContains("Containerized Leg", containerizedLeg, col);

			strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.NonBooking, LegTypeCollectionStrategy.LegContainerMode.Loose);
			col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("Loose Booking", 1, col.Count);
			AssertCollectionContains("Loose Leg", looseLeg, col);

			strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.NonBooking, LegTypeCollectionStrategy.LegContainerMode.All);
			col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("Loose Booking", 2, col.Count);
			AssertCollectionContains("Loose Leg", looseLeg, col);
			AssertCollectionContains("Containerized Leg", containerizedLeg, col);

			strat = new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.All, LegTypeCollectionStrategy.LegContainerMode.All);
			col = new CommonCartageLegTypeCollection(jobType, strat);
			AssertEquals("All", 4, col.Count);
			AssertCollectionContains("All: Loose Booking", looseBooking, col);
			AssertCollectionContains("All: Containerized Booking", containerizedBooking, col);
			AssertCollectionContains("All: Loose Leg", looseLeg, col);
			AssertCollectionContains("All: Containerized Leg", containerizedLeg, col);
		}
	}
}
