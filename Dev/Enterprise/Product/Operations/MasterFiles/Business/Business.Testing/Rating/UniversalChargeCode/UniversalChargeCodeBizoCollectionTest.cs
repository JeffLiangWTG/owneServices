using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(UniversalChargeCodeBizoCollection))]
	public class UniversalChargeCodeBizoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UniversalChargeCodeBizoCollection>
	{
		protected override UniversalChargeCodeBizoCollection GetCollectionToTest()
		{
			return new UniversalChargeCodeBizoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UniversalChargeCodeBizo(Factory);
		}
	}
}
