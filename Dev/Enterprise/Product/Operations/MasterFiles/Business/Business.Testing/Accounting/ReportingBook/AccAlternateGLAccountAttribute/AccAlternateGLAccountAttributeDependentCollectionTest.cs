using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccountAttributeDependentCollection))]
	sealed class AccAlternateGLAccountAttributeDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccAlternateGLAccountAttributeDependentCollection(Factory.NewWithValidTestData<AccAlternateGLAccount>());
		}
	}
}
