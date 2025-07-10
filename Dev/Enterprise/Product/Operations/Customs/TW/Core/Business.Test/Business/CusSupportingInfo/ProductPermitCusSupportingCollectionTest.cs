using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ProductPermitCusSupportingCollection))]
	sealed class ProductPermitCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ProductPermitCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<ProductPermitCusSupporting> GetCusSupportingInfoCollection()
		{
			var pivot = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew();
			return new ProductPermitCusSupportingCollection(pivot);
		}
	}
}
