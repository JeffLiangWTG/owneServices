using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AIIURecordCollection))]
	sealed class AIIURecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AIIURecordCollection>
	{
		protected override AIIURecordCollection GetCollectionToTest()
		{
			return new AIIURecordCollection(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AIIURecord();
		}
	}
}
