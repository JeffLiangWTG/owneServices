using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(PlannedVoyagesCollection))]
	sealed class PlanningVoyagesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PlannedVoyagesCollection>
	{
		protected override PlannedVoyagesCollection GetCollectionToTest()
		{
			return new PlannedVoyagesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PlannedVoyage();
		}
	}
}
