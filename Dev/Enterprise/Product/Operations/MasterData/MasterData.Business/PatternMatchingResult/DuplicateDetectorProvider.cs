using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DuplicateDetectorProvider : IDuplicateDetectorProvider
	{
		public IEnumerable<DuplicateDetectorProviderOutput> DetectDuplicates(OrgHeader header)
		{
			var finder = GetOrgHeaderFinder(header);
			var duplicationScoringResults = finder.FindPotentialDuplicates(false);
			LastRunStatus = ((ISupportDuplicationFinder)finder).LastRunStatus;

			if (LastRunStatus != DuplicationStatus.OK)
			{
				return Enumerable.Empty<DuplicateDetectorProviderOutput>();
			}

			var results = new List<DuplicateDetectorProviderOutput>();

			foreach (var scoringResult in duplicationScoringResults)
			{
				results.Add(new DuplicateDetectorProviderOutput(OrgHeaderSchema.Constants.Prefix, scoringResult.MasterPK, OrgHeaderSchema.Constants.Prefix, scoringResult.TargetPK, scoringResult.Score));
			}

			return results;
		}

		public IEnumerable<DuplicateDetectorProviderOutput> DetectDuplicates(GlbPerson person)
		{
			var finder = GetGlbPersonFinder(person);
			var duplicationScoringResults = finder.FindPotentialDuplicates(true);
			LastRunStatus = ((ISupportDuplicationFinder)finder).LastRunStatus;

			if (LastRunStatus != DuplicationStatus.OK)
			{
				return Enumerable.Empty<DuplicateDetectorProviderOutput>();
			}

			var results = new List<DuplicateDetectorProviderOutput>();

			foreach (var scoringResult in duplicationScoringResults)
			{
				results.Add(new DuplicateDetectorProviderOutput(GlbPersonSchema.Constants.Prefix, scoringResult.MasterPK, GlbPersonSchema.Constants.Prefix, scoringResult.TargetPK, scoringResult.Score));
			}

			return results;
		}

		public DuplicationStatus LastRunStatus { private set; get; }

		protected virtual OrgHeaderDuplicationFinder GetOrgHeaderFinder(OrgHeader header) => OrgHeaderDuplicationFinder.CreateInstance(header, useMaxRecords: true, timeOut: 120);

		protected virtual GlbPersonDuplicationFinder GetGlbPersonFinder(GlbPerson person) => GlbPersonDuplicationFinder.CreateInstance(person, useMaxRecords: true, timeOut: 120);

		public static void AddDuplicateRecordToResults(DuplicateDetectorProviderOutput output, BusinessObjectFactory factory)
		{
			var record = PatternMatchingResult.CreatePatternMatchingResult(output.MasterSourceTable, output.MasterPK, factory, PatternMatchingResult.StatusCodes.PotentialDuplicate);
			record.PMT_TargetPK = output.TargetPK;
			record.PMT_TargetTableCode = output.TargetSourceTable;
			record.PMT_ScorePercent = (byte)(output.Score * 100);
		}

		public static void AddBothDuplicateRecords(DuplicateDetectorProviderOutput output, BusinessObjectFactory factory)
		{
			AddDuplicateRecordToResults(output, factory);

			var reverseRecord = PatternMatchingResult.CreatePatternMatchingResult(output.TargetSourceTable, output.TargetPK, factory, PatternMatchingResult.StatusCodes.PotentialDuplicate);
			reverseRecord.PMT_TargetPK = output.MasterPK;
			reverseRecord.PMT_TargetTableCode = output.MasterSourceTable;
			reverseRecord.PMT_ScorePercent = (byte)(output.Score * 100);
		}
	}
}
