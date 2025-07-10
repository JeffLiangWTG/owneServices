using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceBulkCreator))]
	sealed class AccComplianceSequenceBulkCreatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestComplianceSequences()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			Factory.Save();

			var bulkCreator = new AccComplianceSequenceBulkCreator(Factory);

			AssertNotNull(bulkCreator.ComplianceSequences);
			AssertEquals(0, bulkCreator.ComplianceSequences.Count);
		}
	}
}
