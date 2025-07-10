using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageDestinationDependentCollection))]
	sealed class VoyageDestinationDependentCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var voyage = Factory.New<JobVoyage>();
			return new VoyageDestinationDependentCollection(voyage, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<VoyageDestination>();
		}
	}
}
