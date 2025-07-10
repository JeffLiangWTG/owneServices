using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DuplicateSearchingFinishedEventArgs : EventArgs
	{
		public DuplicateSearchingFinishedEventArgs(IEnumerable<ScoringResult> scoringResults)
		{
			ScoringResults = scoringResults;
		}

		public IEnumerable<ScoringResult> ScoringResults { get; }
	}

	public static class DeduplicationHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public static void SaveDeduplicationResults(BusinessObject masterObject, BusinessObject dedupeObject, IEnumerable<ScoringResult> scoringResults, PatternMatchingResultCollection matchingResultCollection, ZString status, object lockerObj)
		{
			if (status == DeduplicationHelper.StatusConstants.ToBeProcessed)
			{
				lock (lockerObj)
				{
					if (status == DeduplicationHelper.StatusConstants.ToBeProcessed)
					{
						if (scoringResults != null && scoringResults.Any())
						{
							foreach (var result in scoringResults)
							{
								var pmt = matchingResultCollection.AddNew();
								pmt.PMT_MasterPK = masterObject.PK;
								pmt.PMT_TargetPK = result.TargetPK;
								pmt.PMT_MasterTableCode = masterObject.TablePrefix;
								pmt.PMT_TargetTableCode = masterObject.TablePrefix;
								pmt.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
								pmt.PMT_FoundTimeUtc = ZDateTime.UtcNow;
								pmt.PMT_ScorePercent = result.Score > 1 ? (ZByte)100 : (ZByte)Math.Ceiling(result.Score * 100);
							}
						}
						else if (scoringResults != null)
						{
							var pmt = dedupeObject.Factory.New<PatternMatchingResult>();
							pmt.PMT_MasterPK = masterObject.PK;
							pmt.PMT_MasterTableCode = masterObject.TablePrefix;
							pmt.PMT_Status = PatternMatchingResult.StatusCodes.NoDuplicates;
							pmt.PMT_FoundTimeUtc = DateTime.UtcNow;
							pmt.PMT_ScorePercent = 0;
						}
						else
						{
							var pmt = dedupeObject.Factory.New<PatternMatchingResult>();
							pmt.PMT_MasterPK = masterObject.PK;
							pmt.PMT_MasterTableCode = masterObject.TablePrefix;
							pmt.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
							pmt.PMT_FoundTimeUtc = DateTime.UtcNow;
							pmt.PMT_ScorePercent = 0;
						}

						try
						{
							dedupeObject.Factory.Save();
						}
						catch (ZSaveException saveEx)
						{
							var key = "DeduplicationHelper|SaveDeduplicationResults|ZSaveException";
							ErrorReporter.ReportOnce(key, "Failed to save PatternMatchingResults", saveEx);
						}

						dedupeObject.Reload();
					}
				}
			}
		}

		public static void DeleteDeduplicationResults(BusinessObject masterObject)
		{
			var newFactory = new BusinessObjectFactory();
			var orgsPatternMatchingScoresToDelete = newFactory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, masterObject.PK));

			orgsPatternMatchingScoresToDelete.DeleteAll();
			newFactory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public static class StatusConstants
		{
			public const string ToBeProcessed = "To Be Processed";
			public const string Processed = "Processed";
			public const string Excluded = "Excluded";
			public const string Error = "Error";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public static class IgnoredStatusConstants
		{
			public const string AllIgnores = "All Ignores";
			public const string TemporaryIgnores = "Temporary Ignores";
			public const string PermanentIgnores = "Permanent Ignores";
			public const string NoIgnores = "No Ignores";
		}
	}
}
