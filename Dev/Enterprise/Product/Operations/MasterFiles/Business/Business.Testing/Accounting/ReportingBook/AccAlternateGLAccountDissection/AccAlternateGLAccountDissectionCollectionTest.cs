using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccountDissectionCollection))]
	sealed class AccAlternateGLAccountDissectionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccAlternateGLAccountDissectionCollection(Factory.NewWithValidTestData<AccGLHeader>());
		}
	}
}
