using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TransportNonDependentCollection))]
	sealed class TransportNonDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransportNonDependentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TransportNonDependent>();
		}
	}
}
