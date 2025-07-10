using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(InternalPackLineNonDependentCollection))]
	sealed class InternalPackLineNonDependentCollectionBOCollectionTest : PackLineNonDependentCollectionBOCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InternalPackLineNonDependentCollection(Factory);
		}

		public void TestAllowNew()
		{
			InternalPackLineNonDependentCollection collection = (InternalPackLineNonDependentCollection)GetCollectionToTest();
			Assert("Expecting allow new to be false.", !collection.AllowNew);
		}
	}
}
