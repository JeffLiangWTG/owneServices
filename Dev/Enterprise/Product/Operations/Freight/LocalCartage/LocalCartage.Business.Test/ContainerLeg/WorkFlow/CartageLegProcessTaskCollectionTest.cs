using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageLegProcessTaskCollection))]
	class CartageLegProcessTaskCollectionTest : ProcessTaskCollectionTest<CartageLegProcessTaskCollection>
	{
		protected override CartageLegProcessTaskCollection GetCollectionToTestCore()
		{
			return new CartageLegProcessTaskCollection(CartageLeg);
		}

		CommonCartageLeg CartageLeg
		{
			get
			{
				if (cartageLeg == null)
				{
					CommonCartage cartage = Factory.NewWithValidTestData<CommonCartage>();
					cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
				}

				return cartageLeg;
			}
		}

		CommonCartageLeg cartageLeg;
	}
}
