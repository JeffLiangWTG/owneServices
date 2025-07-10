using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoManifestQueryBizObjCollection))]
	sealed class CargoManifestQueryBizObjCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CargoManifestQueryBizObjCollection>
	{
		protected override CargoManifestQueryBizObjCollection GetCollectionToTest() => new CargoManifestQueryBizObjCollection(header);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CargoManifestQueryBizObj(header);

		CargoManifestQueryHeader header;

		protected override void SetUp()
		{
			base.SetUp();
			header = new CargoManifestQueryHeader(Factory);
		}
	}
}
