using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class PackageSequenceCalculator
	{
		public PackageSequenceCalculator(PkgPackageJob packageJob)
		{
			PackageJob = Argument.NotNull(packageJob, nameof(packageJob));
		}

		PkgPackageJob PackageJob { get; }

		#region Properties

		List<IPackageSequence> AllPackageSequences
		{
			get
			{
				var packageSequenceType = PackageJob.ParentJob?.PackageSequenceType;
				var resultCount = packageSequenceType == PackageSequenceType.OuterWithLooseID
								? PackageJob.Packages.Count + PackageJob.LoosePackagePivots.Count
								: PackageJob.Packages.Count;

				var result = new List<IPackageSequence>(resultCount);
				result.AddRange(PackageJob.Packages);

				if (packageSequenceType == PackageSequenceType.OuterWithLooseID)
				{
					result.AddRange(PackageJob.LoosePackagePivots);
				}

				return result;
			}
		}

		public ZShort CachedMaxSequence
		{
			get
			{
				if (!cachedMaxSequence.HasValue)
				{
					var allPackageSequences = AllPackageSequences;
					cachedMaxSequence = allPackageSequences.Any() ? allPackageSequences.Max(p => p.Sequence) : (ZShort)0;
				}

				return cachedMaxSequence.Value;
			}
			private set
			{
				cachedMaxSequence = value;
			}
		}
		ZShort? cachedMaxSequence;

		#endregion

		public void Sequence(IPackageSequence packageSequence)
		{
			if (packageSequence?.PackageJobFK == PackageJob.PK)
			{
				var packageSequenceType = PackageJob.ParentJob?.PackageSequenceType;
				var requiresSequencing = packageSequenceType != PackageSequenceType.Consolidated
					&& (packageSequenceType == PackageSequenceType.Standard || packageSequence.PackageHeaderFK != ZGuid.Empty);
				if (requiresSequencing && packageSequence.RequiresSequencing)
				{
					if (packageSequence.Sequence == 0)
					{
						if (CachedMaxSequence >= short.MaxValue)
						{
							ErrorReporter.ReportOnce(PackageJob.SequenceOverflowErrorMessage);
						}
						else
						{
							CachedMaxSequence++;
						}
						packageSequence.Sequence = CachedMaxSequence;
					}
				}
				else
				{
					packageSequence.Sequence = 0;
				}
			}
		}

		public void ShiftSequence(int currentSequence)
		{
			if (currentSequence > 0 && !PackageJob.IsDeletingPackageJob)
			{
				var requireClearCachedMaxSequence = currentSequence == CachedMaxSequence;
				var lastPackageIDSequence = AllPackageSequences.OrderByDescending(p => p.Sequence).FirstOrDefault();
				if (lastPackageIDSequence?.Sequence > currentSequence)
				{
					requireClearCachedMaxSequence = requireClearCachedMaxSequence || lastPackageIDSequence.Sequence == CachedMaxSequence;

					lastPackageIDSequence.Sequence = (ZShort)currentSequence;
				}

				if (requireClearCachedMaxSequence)
				{
					cachedMaxSequence = null;
				}
			}
		}
	}
}
