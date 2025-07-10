using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementDeletedEntryCollection))]
	sealed class StatementDeletedEntryCollectionTests : NonPersistentBusinessObjectCollectionTestCase<StatementDeletedEntryCollection>
	{
		protected override StatementDeletedEntryCollection GetCollectionToTest()
		{
			return new StatementDeletedEntryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StatementDeletedEntry("11111", "2222", "ABC");
		}
	}
}
