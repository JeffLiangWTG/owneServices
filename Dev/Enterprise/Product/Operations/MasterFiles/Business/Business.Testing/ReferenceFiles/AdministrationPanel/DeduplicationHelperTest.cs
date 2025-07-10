using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DeduplicationHelperTest : TestCaseWithFactory
	{
		public void TestStatusConsants()
		{
			AssertEquals("To Be Processed", DeduplicationHelper.StatusConstants.ToBeProcessed);
			AssertEquals("Processed", DeduplicationHelper.StatusConstants.Processed);
		}

		public void TestDeleteDeduplicationResults()
		{
			var bizO = Factory.NewWithValidTestData<DummyBusinessObject>();

			var dedupeResult1 = Factory.New<PatternMatchingResult>();
			dedupeResult1.PMT_MasterPK = bizO.PK;
			dedupeResult1.PMT_TargetPK = Guid.NewGuid();
			dedupeResult1.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			dedupeResult1.PMT_MasterTableCode = dedupeResult1.PMT_TargetTableCode = "OH";
			dedupeResult1.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			dedupeResult1.PMT_GS_NKExcludeBy = "";

			var dedupeResult2 = Factory.New<PatternMatchingResult>();
			dedupeResult2.PMT_MasterPK = bizO.PK;
			dedupeResult2.PMT_TargetPK = Guid.NewGuid();
			dedupeResult2.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			dedupeResult2.PMT_MasterTableCode = dedupeResult2.PMT_TargetTableCode = "PER";
			dedupeResult2.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			dedupeResult2.PMT_GS_NKExcludeBy = "";

			Factory.Save();

			var dedupeResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, bizO.PK));
			AssertEquals("Precondition: should find 2 duplicate results that belong to bizO", 2, dedupeResults.Length);

			DeduplicationHelper.DeleteDeduplicationResults(bizO);

			dedupeResults = Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, bizO.PK));
			AssertEquals("All duplicate results that belong to bizO are deleted", 0, dedupeResults.Length);
		}
	}
}
