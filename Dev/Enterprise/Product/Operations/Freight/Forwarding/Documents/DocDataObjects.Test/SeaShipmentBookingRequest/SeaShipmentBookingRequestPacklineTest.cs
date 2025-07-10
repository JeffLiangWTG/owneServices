using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(SeaShipmentBookingRequestPackLine))]
	sealed class SeaShipmentBookingRequestPackLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SeaShipmentBookingRequestPackLine()
			{
				CargoWeight = new Measurement()
				{
					Value = 22,
					Unit = new DummyCodeDescription
					{
						Code = Core.Constants.Weight.Kilograms
					}
				},
				MarksAndNumbersOnPackages = "MarksAndNumbersOnPackages",
				CargoVolume = new Measurement()
				{
					Value = 22,
					Unit = new DummyCodeDescription
					{
						Code = Core.Constants.Volume.CubicMetres
					}
				},
				GoodsDescription = "Description",
				DangerousGoods = new List<DangerousGood>(),
				HarmonizedCodesCollection = new List<HarmonizedCode>()
			};
		}
	}
}
