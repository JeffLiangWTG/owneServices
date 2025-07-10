using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeaderCompanyFilterCollection))]
	sealed class AccGLHeaderCompanyFilterCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccGLHeaderCompanyFilterCollection(Factory.New<AccGLHeader>());
		}
	}
}
