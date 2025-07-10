using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class DuplicationFinderTest : TestCaseWithFactory
	{
		bool rawEnableDeduplicationFinder;
		bool rawExcludePotentialDuplicatesFromOtherCountries;
		bool rawExcludeInactivePotentialDuplicates;

		protected override void SetUp()
		{
			rawEnableDeduplicationFinder = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			rawExcludePotentialDuplicatesFromOtherCountries = OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.Value;
			rawExcludeInactivePotentialDuplicates = OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.Value;

			OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.SetUp();
		}

		protected override void TearDown()
		{
			OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableDeduplicationFinder);
			OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawExcludePotentialDuplicatesFromOtherCountries);
			OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawExcludeInactivePotentialDuplicates);
			base.TearDown();
		}

		public void TestComputeMessagesErrorOccurred()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);
			duplicationFinder.SetLastRunStatusForTest(DuplicationStatus.ErrorOccurred);
			AssertEquals(DuplicationResponseMessages.Exception, duplicationFinder.ComputeResponseMessageForTest("STD", list[1]));
		}

		public void TestComputeMessagesTimeout()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);
			duplicationFinder.SetLastRunStatusForTest(DuplicationStatus.Timeout);
			AssertEquals("Compute response message timeout", DuplicationResponseMessages.Timeout, duplicationFinder.ComputeResponseMessageForTest("STD", list[1]));
		}

		public void TestComputeMessagesPartialTimeout()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			IDuplicationFinder<OrgHeader, OrgHeader> duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);
			((OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest)duplicationFinder).SetLastRunStatusForTest(DuplicationStatus.Timeout);
			duplicationFinder.GetPotentialTargets(ZString.Empty);
			AssertEquals("Compute response message partial timeout", DuplicationResponseMessages.PartialTimeout, ((OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest)duplicationFinder).ComputeResponseMessageForTest("STD", list[1]));
		}

		public void TestComputeMessagesExclusion()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_GS_NKExcludeBy = "BLA";
			pmt.PMT_Status = "EXC";
			pmt.PMT_MasterPK = org.PK;
			pmt.PMT_MasterTableCode = "OH";
			Factory.Save();

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);
			AssertEquals("Compute response message EXC", DuplicationResponseMessages.Exclusion, duplicationFinder.ComputeResponseMessageForTest("STD", org1));
		}

		public void TestComputeMessagesTIGExisted()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_GS_NKExcludeBy = "STD";
			pmt.PMT_Status = "TIG";
			pmt.PMT_MasterPK = org.PK;
			pmt.PMT_MasterTableCode = pmt.PMT_TargetTableCode = "OH";
			pmt.PMT_TargetPK = org1.PK;
			Factory.Save();

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);
			AssertEquals("Compute response message TIG", DuplicationResponseMessages.TIGExisted, duplicationFinder.ComputeResponseMessageForTest("STD", org1));

			AssertEquals("Compute response message TIG for other user", DuplicationResponseMessages.Success, duplicationFinder.ComputeResponseMessageForTest("OST", org1));
		}

		public void TestComputeMessagesPIGExisted()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];
			var org1 = list[1];
			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_GS_NKExcludeBy = "BLA";
			pmt.PMT_Status = "PIG";
			pmt.PMT_MasterPK = org.PK;
			pmt.PMT_MasterTableCode = pmt.PMT_TargetTableCode = "OH";
			pmt.PMT_TargetPK = org1.PK;
			Factory.Save();

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);
			AssertEquals("Compute response message PIG", DuplicationResponseMessages.PIGExisted, duplicationFinder.ComputeResponseMessageForTest("STD", org1));

			AssertEquals("Compute response message IG", DuplicationResponseMessages.PIGExisted, duplicationFinder.ComputeResponseMessageForTest("OST", org1));
		}

		public void TestComputeIgnoreStatus()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org = list[0];

			((IDeduplicatable)org).ShouldRunDeduplication = true;

			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTest(org, false);

			AssertEquals("Ignore temporary", UserIgnoreStatus.TemporaryIgnore.Name, duplicationFinder.ComputeIgnoreForTest(DuplicationResponseMessages.TIGExisted).Name);
			AssertEquals("Ignore excluded", UserIgnoreStatus.ExcludedIgnore.Name, duplicationFinder.ComputeIgnoreForTest(DuplicationResponseMessages.Exclusion).Name);
			AssertEquals("Ignore permanent", UserIgnoreStatus.PermanentIgnore.Name, duplicationFinder.ComputeIgnoreForTest(DuplicationResponseMessages.PIGExisted).Name);
			AssertNull("Ignore null", duplicationFinder.ComputeIgnoreForTest(DuplicationResponseMessages.Success));
		}

		[ExpectNoExceptions]
		public void TestFindPotentialDuplicates_GetRegistryTimeoutFailed()
		{
			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTestWithException(null, false)
			{
				throwSqlExceptionFromRegistryTimeout = true
			};

			var result = duplicationFinder.GetDuplicationsAsync_Exposed();

			AssertNotNull(result);
			AssertEquals(0, result.Count());

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertType<SqlException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFindPotentialDuplicates_TokenCancelled()
		{
			var duplicationFinder = new OrgHeaderDuplicationFinderTest.OrgHeaderDuplicationFinderForTestWithException(null, false);
			duplicationFinder.TokenSource.Cancel();

			AssertExceptionThrown<OperationCanceledException>(() => duplicationFinder.GetDuplicationsAsync_Exposed());
		}
	}

	public abstract class DuplicationFinderWithoutFactoryBaseTest : TestCase
	{
		public abstract void TestWithoutPlaceholders();
		public abstract void TestGetPotentialTargets();
		public abstract void TestGetPotentialTargetsAsync();
		public abstract void TestGetPotentialTargetsByEmailDomainAsync();
		public abstract void TestWithPlaceholders();
		public abstract void TestWithoutPlaceholders_WithoutStandardizationRules();
	}

	[TestsSubclassesOf(typeof(IDuplicationFinder<BusinessObject, BusinessObject>))]
	public abstract class DuplicationFinderBaseTest<T, TMasterBizO, TTargetBizO, TGlow> : TestCaseWithFactory
		where T : DuplicationFinder<TMasterBizO, TTargetBizO, TGlow>
		where TMasterBizO : BusinessObject, IDeduplicatable
		where TTargetBizO : BusinessObject, IDeduplicatable
	{
		protected virtual PatternMatchingResultModel GetNewPatternMatchingResultModel(TMasterBizO parent, TTargetBizO target, int hash, string countryCode = "AU")
		{
			return new PatternMatchingResultModel
			{
				CountryCode = countryCode,
				HashedValue = hash,
				ParentID = parent.PK.ToGuid(),
				ParentTablePrefix = parent.TablePrefix,
			};
		}

		public abstract void TestFindPotentialDuplicates();

		public abstract void TestScoringResultWithNoTarget_ReturnsEmptyList();

		public abstract void TestGetOrderedList();

		#region IDuplicationFinderTests

		protected string standardStaffCodeForTest = GlbStaff.CurrentUser.GS_Code;

		public static void GetPatternMatchingResultForTest(BusinessObjectFactory factory, ZGuid masterPK, string masterTableCode, ZGuid targetPK, string status, string excludedBy)
		{
			var patternMatchingResult = factory.NewWithValidTestData<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = masterPK;
			patternMatchingResult.PMT_MasterTableCode = masterTableCode;
			patternMatchingResult.PMT_TargetPK = targetPK;
			patternMatchingResult.PMT_TargetTableCode = targetPK == ZGuid.Empty ? string.Empty : masterTableCode;
			patternMatchingResult.PMT_Status = status;
			patternMatchingResult.PMT_GS_NKExcludeBy = excludedBy;
			patternMatchingResult.PMT_ScorePercent = (ZByte)(status == "PDU" ? 82 : 0);
		}

		public void TestCompareBizos()
		{
			AssertCompareBizos<TMasterBizO, TTargetBizO, T>();
		}

		public abstract void AssertCompareBizos<TMaster, TTarget, TFinder>()
			where TFinder : DuplicationFinder<TMaster, TTarget, TGlow>
			where TMaster : BusinessObject, IDeduplicatable
			where TTarget : BusinessObject, IDeduplicatable;

		public void TestAddIgnore()
		{
			AssertAddIgnore<TMasterBizO, TTargetBizO, T>();
		}

		public abstract void AssertAddIgnore<TMaster, TTarget, TFinder>()
			where TFinder : DuplicationFinder<TMaster, TTarget, TGlow>
			where TMaster : BusinessObject, IDeduplicatable
			where TTarget : BusinessObject, IDeduplicatable;

		public void TestAddExclusion()
		{
			AssertAddExclusion<TMasterBizO, TTargetBizO, T>();
		}

		public abstract void AssertAddExclusion<TMaster, TTarget, TFinder>()
			where TFinder : DuplicationFinder<TMaster, TTarget, TGlow>
			where TMaster : BusinessObject, IDeduplicatable
			where TTarget : BusinessObject, IDeduplicatable;

		public void TestRemoveExclusion()
		{
			AssertRemoveExclusion<TMasterBizO, TTargetBizO, T>();
		}

		public abstract void AssertRemoveExclusion<TMaster, TTarget, TFinder>()
			where TFinder : DuplicationFinder<TMaster, TTarget, TGlow>
			where TMaster : BusinessObject, IDeduplicatable
			where TTarget : BusinessObject, IDeduplicatable;

		public void TestRemoveIgnore()
		{
			AssertRemoveIgnore<TMasterBizO, TTargetBizO, T>();
		}

		public abstract void AssertRemoveIgnore<TMaster, TTarget, TFinder>()
			where TFinder : DuplicationFinder<TMaster, TTarget, TGlow>
			where TMaster : BusinessObject, IDeduplicatable
			where TTarget : BusinessObject, IDeduplicatable;

		public void TestAddExclude_ShouldNotThrowConcurrencyError()
		{
			AssertAddExclude_ShouldNotThrowConcurrencyError<TMasterBizO, TTargetBizO, T>();
		}

		public abstract void AssertAddExclude_ShouldNotThrowConcurrencyError<TMaster, TTarget, TFinder>()
			where TFinder : DuplicationFinder<TMaster, TTarget, TGlow>
			where TMaster : BusinessObject, IDeduplicatable
			where TTarget : BusinessObject, IDeduplicatable;

		#endregion

		public abstract void TestFindPotentialDuplicatesWithMultiLanguage();

		public abstract void TestLoadTargetBizosForTIG();

		public abstract void TestLoadTargetBizosForPIG();

		public abstract void TestLoadTargetBizosForEXC();

		protected static SqlException GetNewSqlException()
		{
			SqlException exception = null;

			try
			{
#pragma warning disable CW1116 // Suppress for testing purpose
				new SqlConnection(@"Data Source=.;Database=GUARANTEED_TO_FAIL;Connection Timeout=1").Open();
#pragma warning restore CW1116 // DoSuppress for testing purpose
			}
			catch (SqlException ex)
			{
				exception = ex;
			}

			return exception;
		}
	}

	class IOExceptionMessages
	{
		internal readonly string ReadWhenNoDataIsPresent = "Invalid attempt to read when no data is present.";
		internal readonly string ReadWhenReaderIsClosedt = "Invalid attempt to call Read when reader is closed.";
		internal readonly string TheConnectionIsClosed = "Invalid operation. The connection is closed.";
		internal readonly string NextResultWhenReaderIsClosed = "Invalid attempt to call NextResult when reader is closed.";
		internal readonly string FieldCoiuntWhenReaderIsClosed = "Invalid attempt to call FieldCount when reader is closed.";
		internal readonly string InternalConnectionError = "Internal connection fatal error. Error state: 15, Token : 0";
		internal readonly string CheckDataIsReadyWhenReaderIsClosed = "Invalid attempt to call CheckDataIsReady when reader is closed.";
		internal readonly string OpenAndAvailableConnectionError = "ExecuteReader requires an open and available Connection. The connection's current state is open.";
	}
}
