using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class StmEntityScreeningLogValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPJ_HighConfidenceResults()
		{
			var status = Factory.New<StmEntityScreeningLog>();
			AssertCheckResult(status, "Suggested High Confidence Matches can't be empty", status.PJ_HighConfidenceResultsInfo, u => u.PJ_HighConfidenceResults = string.Empty, u => u.PJ_HighConfidenceResults = "ABC");
		}

		public void TestCheckPJ_MediumConfidenceResults()
		{
			var status = Factory.New<StmEntityScreeningLog>();
			AssertCheckResult(status, "Suggested Medium Confidence Matches can't be empty", status.PJ_MediumConfidenceResultsInfo, u => u.PJ_MediumConfidenceResults = string.Empty, u => u.PJ_MediumConfidenceResults = "ABC");
		}

		public void TestCheckPJ_ExcludedLists()
		{
			var status = Factory.New<StmEntityScreeningLog>();
			AssertCheckResult(status, "Excluded Lists can't be empty", status.PJ_ExcludedListsInfo, u => u.PJ_ExcludedLists = string.Empty, u => u.PJ_ExcludedLists = "ABC");
		}

		public void TestCheckPJ_IncludedLists()
		{
			var status = Factory.New<StmEntityScreeningLog>();
			AssertCheckResult(status, "Included Lists can't be empty", status.PJ_IncludedListsInfo, u => u.PJ_IncludedLists = string.Empty, u => u.PJ_IncludedLists = "ABC");
		}

		void AssertCheckResult(StmEntityScreeningLog status, string errorMessage, ZPropertyInfo zPropertyInfo, Action<StmEntityScreeningLog> setValueToEmpty, Action<StmEntityScreeningLog> setValueToNotEmpty)
		{
			var shouldNotCheckStatusArray = new[] { LogsScreeningStatus.NoScreeningPerformed, LogsScreeningStatus.InvalidatedByContentUpdate, LogsScreeningStatus.InvalidatedByLocalDataChanges };
			var shouldCheckStatusArray = new[] { LogsScreeningStatus.ScreenedClear, LogsScreeningStatus.MatchedDeniedParty, LogsScreeningStatus.ScreenedCanceled, LogsScreeningStatus.PotentialMatchesFound };

			foreach (var shouldNotCheckStatus in shouldNotCheckStatusArray)
			{
				status.PJ_Status = shouldNotCheckStatus;
				setValueToNotEmpty(status);
				AssertNoErrors(zPropertyInfo);

				setValueToEmpty(status);
				AssertNoErrors(zPropertyInfo);
			}

			foreach (var shouldCheckStatus in shouldCheckStatusArray)
			{
				status.PJ_Status = shouldCheckStatus;
				setValueToNotEmpty(status);
				AssertNoErrors(zPropertyInfo);

				setValueToEmpty(status);
				AssertHasError(zPropertyInfo, errorMessage);
			}
		}
	}
}
