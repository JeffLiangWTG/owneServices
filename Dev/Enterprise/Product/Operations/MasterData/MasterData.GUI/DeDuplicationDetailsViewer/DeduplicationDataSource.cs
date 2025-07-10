using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterData.Business;

namespace Enterprise.MasterData.GUI
{
	public abstract class DeduplicationDataSource<TGlow, TCandidate> : IDeduplicationDataSource
		where TCandidate : DuplicationCandidate
	{
		readonly IEnumerable<IGrouping<Guid, DeduplicationPresenterModel>> presenterModels;

		public TGlow MasterGlow { get; }
		public IEnumerable<TGlow> TargetGlows { get; }

		protected DeduplicationDataSource(TGlow master, IEnumerable<TGlow> targetGlows, IEnumerable<DeduplicationPresenterModel> results)
		{
			presenterModels = results.Where(x => x.GlowTargetType == typeof(TGlow)).GroupBy(x => x.TargetID).ToArray();
			MasterGlow = master;
			TargetGlows = targetGlows;
		}

		protected abstract string CountryCode { get; }

		protected abstract Guid GetPK(TGlow target);

		#region WPF

		public IEnumerable<PotentialDuplicationModel> GetPotentialDuplicates()
		{
			var list = GetPotentialDuplicationModels();

			var tempList = new List<PotentialDuplicationModel>();

			if (list.Count > 0)
			{
				var ratingGroups = list.OrderByDescending(x => x.Confidence).GroupBy(x => x.Confidence);

				foreach (var group in ratingGroups)
				{
					var groupSimilarToMaster = group.Where(x => !string.IsNullOrEmpty(x.UNLOCO) && x.UNLOCO.StartsWith(CountryCode, StringComparison.Ordinal)).OrderByDescending(x => x.Score);
					var sortedGroup = group.Where(x => !string.IsNullOrEmpty(x.UNLOCO) && !x.UNLOCO.StartsWith(CountryCode, StringComparison.Ordinal)).OrderByDescending(x => x.Score).ThenBy(x => x.UNLOCO);
					var invalidGroup = group.Where(x => string.IsNullOrEmpty(x.UNLOCO) && !x.IsDummy).OrderByDescending(x => x.Score);

					tempList.AddRange(groupSimilarToMaster);
					tempList.AddRange(sortedGroup);
					tempList.AddRange(invalidGroup);
				}
			}

			return tempList;
		}

		List<PotentialDuplicationModel> GetPotentialDuplicationModels()
		{
			var list = new List<PotentialDuplicationModel>();
			var currentTargets = TargetGlows.Where(x => presenterModels.Select(p => p.Key).Contains(GetPK(x)));

			foreach (var target in currentTargets)
			{
				var currentPresenterModel = presenterModels.Single(x => x.Key == GetPK(target));

				if (currentPresenterModel != null)
				{
					list.Add(GetPotentialDuplicationModel(target, currentPresenterModel));
				}
			}
			return list;
		}

		protected abstract PotentialDuplicationModel GetPotentialDuplicationModel(TGlow currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target);

		#endregion WPF

		#region WinForm

		public IEnumerable<DuplicationCandidate> GetDuplicationCandidates()
		{
			var candidates = new List<TCandidate>();
			var targets = TargetGlows.Where(x => presenterModels.Select(p => p.Key).Contains(GetPK(x)));

			foreach (var target in targets)
			{
				candidates.Add(GetDuplicationCandidate(target, presenterModels.Single(x => x.Key == GetPK(target))));
			}

			return RatingCandidates(candidates);
		}

		protected abstract TCandidate GetDuplicationCandidate(TGlow currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target);

		protected abstract IEnumerable<TCandidate> RatingCandidates(IEnumerable<TCandidate> candidates);

		#endregion WinForm
	}
}
