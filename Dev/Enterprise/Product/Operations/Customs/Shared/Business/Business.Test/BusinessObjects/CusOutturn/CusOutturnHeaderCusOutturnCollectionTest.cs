using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderCusOutturnCollection))]
	sealed class CusOutturnHeaderCusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusOutturnHeaderCusOutturnCollection(Factory.New<CusOutturnHeader>());
		}
	}
}
