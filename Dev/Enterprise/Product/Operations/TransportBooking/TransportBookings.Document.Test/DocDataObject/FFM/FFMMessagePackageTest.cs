using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Document.Testing
{
	[TestedType(typeof(FFMMessagePackage))]
	sealed class FFMMessagePackageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new TransportBookingsCommonContext(Factory);

			return new FFMMessagePackage()
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
			};
		}
	}
}
