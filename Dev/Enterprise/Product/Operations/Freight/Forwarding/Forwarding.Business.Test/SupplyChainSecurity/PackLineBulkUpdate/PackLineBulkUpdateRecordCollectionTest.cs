using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PackLineBulkUpdateRecordCollection))]
	sealed class PackLineBulkUpdateRecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PackLineBulkUpdateRecordCollection>
	{
		protected override PackLineBulkUpdateRecordCollection GetCollectionToTest()
		{
			return new PackLineBulkUpdateRecordCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackLineBulkUpdateRecord result = new PackLineBulkUpdateRecord(packline);
			return result;
		}
	}
}
