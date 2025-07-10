using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(LegsCollection))]
	public class LegsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LegsCollection>
	{
		protected override LegsCollection GetCollectionToTest()
		{
			return new LegsCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Leg(Factory);
		}
	}
}
