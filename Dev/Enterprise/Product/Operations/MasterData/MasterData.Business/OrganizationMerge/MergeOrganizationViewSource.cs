using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;

namespace Enterprise.MasterData.Business
{
	public class MergeOrganizationViewSource
	{
		public MergeOrganizationViewSource(DeduplicationOrgHeader master, IEnumerable<DeduplicationOrgHeader> target, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultModels, ZGuid selectedItemPK)
		{
			Master = master;
			Target = target;
			Results = results;
			ResultModels = resultModels;
			SelectedItemPK = selectedItemPK;
		}

		public DeduplicationOrgHeader Master { get; }
		public IEnumerable<DeduplicationOrgHeader> Target { get; }
		public IEnumerable<ScoringResult> Results { get; }
		public IEnumerable<PatternMatchingResultModel> ResultModels { get; }
		public ZGuid SelectedItemPK { get; }
	}
}
