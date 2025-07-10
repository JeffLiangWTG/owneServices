using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestionAttributeCollection))]
	class RefCusProfileQuestionAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusProfileQuestionAttributeCollection>
	{
		protected override RefCusProfileQuestionAttributeCollection GetCollectionToTest()
		{
			return new RefCusProfileQuestionAttributeCollection(Factory.New<RefCusProfileQuestion>());
		}
	}
}
