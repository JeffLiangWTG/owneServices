using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineCollection))]
sealed class CusTempStorageRegLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineCollection>
{
	protected override CusTempStorageRegLineCollection GetCollectionToTest() => new CusTempStorageRegLineCollection(Factory.New<CusTempStorageRegHeader>());
}
