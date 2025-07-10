using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementDeletedEntry))]
	sealed class StatementDeletedEntryCollectionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StatementDeletedEntry("11111", "2222", "ABC");
		}
	}
}
