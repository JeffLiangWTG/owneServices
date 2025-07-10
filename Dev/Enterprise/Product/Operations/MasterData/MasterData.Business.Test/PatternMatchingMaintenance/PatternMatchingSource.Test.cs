using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public abstract class PatternMatchingSourceTest<T, U> : TestCaseWithFactory where T : BusinessObject where U : IPatternMatchingMaintenance
	{
		public void TestCreateOrUpdatePatternMatchingTables()
		{
			SetupBizO();
			MaintenanceObject.CreateOrUpdatePatternMatchingTables();

			AssertPatternMatchingDataExists();
		}

		protected abstract void SetupBizO();

		protected abstract void AssertPatternMatchingDataExists();

		protected abstract T BizO { get; }

		protected abstract U MaintenanceObject { get; }

		protected OrgHeader CreateOrganisation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_RL_NKClosestPort = "AUSYD";

			return org;
		}
	}

	public class PatternMatchingSourceTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCreateOrUpdatePatternMatchingTables_WithQueueForDuplication()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var orgMaintenance = new PatternMatchingOrgHeaderMaintenance(org);

			org.OH_Code = "TXTXU";
			org.OH_FullName = "TOLL LTD";
			org.OH_RL_NKClosestPort = "AUSYD";

			factory.Save();

			orgMaintenance.CreateOrUpdatePatternMatchingTables();

			var patternResults = factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, org.PK));

			AssertEquals(1, patternResults.Length);

			var patternResult = patternResults[0];

			AssertEquals(PatternMatchingResult.StatusCodes.Queued, patternResult.PMT_Status);
			AssertEquals(org.PK, patternResult.PMT_MasterPK);
		}
	}
}
