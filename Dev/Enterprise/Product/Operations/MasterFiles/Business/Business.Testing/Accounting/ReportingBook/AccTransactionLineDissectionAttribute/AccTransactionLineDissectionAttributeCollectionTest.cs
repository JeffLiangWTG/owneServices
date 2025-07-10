using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionLineDissectionAttributeCollection))]
	sealed class AccTransactionLineDissectionAttributeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccTransactionLineDissectionAttributeCollection(Factory.NewWithValidTestData<AccTransactionLines>());
		}
	}
}
