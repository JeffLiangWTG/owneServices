using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefLanguageTypeCollection))]
	class RefLanguageTypeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefLanguageTypeCollection>
	{
		protected override RefLanguageTypeCollection GetCollectionToTest()
		{
			return new RefLanguageTypeCollection(Factory);
		}
	}
}
