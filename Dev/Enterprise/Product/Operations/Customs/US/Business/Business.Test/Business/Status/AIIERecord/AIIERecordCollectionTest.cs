using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AIIERecordCollection))]
	sealed class AIIERecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AIIERecordCollection>
	{
		protected override AIIERecordCollection GetCollectionToTest()
		{
			return new AIIERecordCollection(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AIIERecord();
		}
	}
}
