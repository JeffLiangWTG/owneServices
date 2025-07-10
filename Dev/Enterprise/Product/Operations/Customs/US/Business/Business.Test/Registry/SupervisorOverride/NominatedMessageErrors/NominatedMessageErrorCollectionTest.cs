using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(NominatedMessageErrorCollection))]
	sealed class NominatedMessageErrorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NominatedMessageErrorCollection>
	{
		protected override NominatedMessageErrorCollection GetCollectionToTest()
		{
			return new NominatedMessageErrorCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NominatedMessageError();
		}
	}
}
