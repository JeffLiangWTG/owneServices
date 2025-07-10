using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackage.PkgPackageItemDivotsWrapperCollection))]
	public class PkgPackageItemDivotsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PkgPackage.PkgPackageItemDivotsWrapperCollection>
	{
		protected override PkgPackage.PkgPackageItemDivotsWrapperCollection GetCollectionToTest()
		{
			return new PkgPackage.PkgPackageItemDivotsWrapperCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var package = Factory.New<PkgPackage>();
			var divot = package.PackedItemDivots.AddNew();
			var packableItemParent = Factory.New<DummyPackableItemParent>();
			return PkgPackageItemDivotsWrapper.New(divot, packableItemParent);
		}
	}
}
