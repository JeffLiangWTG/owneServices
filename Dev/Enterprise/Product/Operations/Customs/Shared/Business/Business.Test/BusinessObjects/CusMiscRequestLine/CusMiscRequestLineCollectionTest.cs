using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMiscRequestLineCollection))]
	sealed class CusMiscRequestLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusMiscRequestLineCollection>
	{
		protected override CusMiscRequestLineCollection GetCollectionToTest()
		{
			return new CusMiscRequestLineCollection(Factory.New<CusMiscRequestHeader>());
		}
	}
}
