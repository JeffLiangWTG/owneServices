using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingLineCollection))]
	sealed class PackingLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PackingLineCollection>
	{
		protected override PackingLineCollection GetCollectionToTest()
		{
			return new PackingLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PackingLine(Factory);
		}
	}
}
