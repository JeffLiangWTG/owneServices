using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;

namespace Enterprise.MasterData.Business
{
	public class DuplicationModelDetailGroup : NonPersistentBusinessObject
	{
		public DuplicationModelDetailGroup(
			string groupHeader,
			IEnumerable<DuplicationModelDetail> masterModels,
			IEnumerable<DuplicationModelDetail> candidateModels,
			IEnumerable<string> masterColumns,
			IEnumerable<string> candidateColumns)
		{
			Header = DedupeTranslationHelper.GetModelHeaderCaption(groupHeader);
			MasterModels.AddRange(masterModels);
			CandidateModels.AddRange(candidateModels);
			MasterColumns = masterColumns.ToArray();
			CandidateColumns = candidateColumns.ToArray();

			IsSupportMerge = DuplicationModelDetail.IsGroupTypeSupportMerge(groupHeader);
			SetOverallConfidence(groupHeader, CandidateModels.Select(x => x.Score));
		}

		public ZPropertyInfo HeaderInfo => GetZPropertyInfo(nameof(Header));
		public ZString Header { get; }

		public ZPropertyInfo ConfidenceInfo => GetZPropertyInfo(nameof(Confidence));
		public ZString Confidence { get; private set; }

		public string Score { get; private set; }

		public bool IsSupportMerge { get; }

		public ConfidenceRating ConfidenceRating { get; private set; }

		public DuplicationModelDetailCollection MasterModels { get; } = new DuplicationModelDetailCollection();

		public DuplicationModelDetailCollection CandidateModels { get; } = new DuplicationModelDetailCollection();

		public IEnumerable<string> MasterColumns { get; }

		public IEnumerable<string> CandidateColumns { get; }

		void SetOverallConfidence(string groupType, IEnumerable<double> scores)
		{
			if (groupType == DeduplicationProvider.Constants.ActiveAssociations)
			{
				ConfidenceRating = ConfidenceRating.Undefined;
				return;
			}

			var validScores = scores.Where(x => x > 0).ToArray();
			var score = validScores.Length > 0 ? validScores.Average() : 0;
			Score = string.Format(CultureInfo.InvariantCulture, "{0}%", score * 100);
			ConfidenceRating = ScoringResult.CalculateConfidenceRating(score);
			Confidence = ConfidenceRating is ConfidenceRating.Undefined ? string.Empty : DedupeTranslationHelper.GetConfidenceDescription(ConfidenceRating.ToString());
		}
	}
}
