using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobTradeLaneVoyageCollection))]
	sealed class JobTradeLaneVoyageCollectionTest : ActiveBusinessObjectCollectionTestCase<JobTradeLaneVoyageCollection>
	{
		#region Implementation

		protected override JobTradeLaneVoyageCollection GetCollectionToTest()
		{
			return Factory.New<JobVoyage>().TradeLanes;
		}

		#endregion
	}
}
