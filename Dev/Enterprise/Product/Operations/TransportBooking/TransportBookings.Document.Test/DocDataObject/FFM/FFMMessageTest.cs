using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Document.Testing
{
	[TestedType(typeof(FFMMessage))]
	sealed class FFMMessageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new TransportBookingsCommonContext(Factory);

			return new FFMMessage()
			{
				VoyageFlightNo = "000-001",
				FlightDate = new ZDate(2038, 1, 20),
				AirportOfDestinationCode = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "BKK" },
				PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "MXP" },
				DriverDocumentID = "123 1234 1234",
				RegulatedAgentCountry = "IT",
				RegulatedAgentID = "000-007",
				Packages = new[]
				{
					new FFMMessagePackage()
					{
						ContainerNumber = "CNT-1",
						GoodsDescription = "Test-Goods",
						PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "BKK" },
						PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "MXP" },
						Quantity = 1,
						Volume = 10.0m,
						Weight = 5.0m,
						WeightMetric = "KG",
						VolumeMetric = "M3",
						WayBillNumber = "123-123"
					}
				}
			};
		}
	}
}
