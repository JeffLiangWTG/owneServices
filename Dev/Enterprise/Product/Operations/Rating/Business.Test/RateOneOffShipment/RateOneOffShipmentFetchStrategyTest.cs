using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateOneOffShipmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var shipment = Factory.NewWithValidTestData<RateOneOffShipment>();
			shipment.TT_TH = Factory.NewWithValidTestData<Quote>().PK;
			shipment.TT_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Core.Constants.ContainerModes.FCL;

			Factory.Save();

			var factory = new BusinessObjectFactory();
			factory.Load<RateOneOffShipment>(shipment.PK);

			var expectedHits = new Dictionary<string, int>
			{
				{ RateOneOffShipmentSchema.Constants.TableName, 1 },
				{ RateOneOffContainersSchema.Constants.TableName, 1 },
				{ RateOneOffPackLineSchema.Constants.TableName, 1 },
				{ RatingHeaderSchema.Constants.TableName, 1 },
				{ JobShipmentSchema.Constants.TableName, 1 }, // When we load RateOneOffShipment with Mode FCL, we need to check if it has bind to Booking.
			};

			AssertDbHits(expectedHits, factory);
		}
	}
}
