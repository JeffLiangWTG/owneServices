using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageExRateDependentCollection))]
	sealed class VoyageExRateDependentCollectionBOTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<JobVoyage>().ExRates;
		}

		#endregion
	}
}
