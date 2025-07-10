using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderCollectionView))]
	sealed class CusSeaManOBLHeaderCollectionViewTest : CusSeaManOBLHeaderCollectionViewTest<CusSeaManOBLHeaderCollectionView>
	{
		protected override CusSeaManOBLHeaderCollectionView GetCollectionToTest()
		{
			return View;
		}
	}
}
