using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestionAnswerListCollection))]
	public class RefCusProfileQuestionAnswerListCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusProfileQuestionAnswerListCollection>
	{
		protected override RefCusProfileQuestionAnswerListCollection GetCollectionToTest()
		{
			return new RefCusProfileQuestionAnswerListCollection(Factory.New<RefCusProfileQuestion>());
		}
	}
}
