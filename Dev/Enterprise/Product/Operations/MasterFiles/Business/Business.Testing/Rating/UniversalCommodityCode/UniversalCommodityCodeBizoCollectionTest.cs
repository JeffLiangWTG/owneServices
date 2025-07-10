using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(UniversalCommodityCodeBizoCollection))]
	public class UniversalCommodityCodeBizoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UniversalCommodityCodeBizoCollection>
	{
		protected override UniversalCommodityCodeBizoCollection GetCollectionToTest()
		{
			return new UniversalCommodityCodeBizoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UniversalCommodityCodeBizo(Factory);
		}
	}
}
