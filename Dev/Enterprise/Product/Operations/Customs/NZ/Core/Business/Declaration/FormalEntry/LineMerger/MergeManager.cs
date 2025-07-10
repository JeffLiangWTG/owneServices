namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override bool RequiresMergeCore => Declaration.CustomsEntryHeaders.Count > 0
											&& Declaration.ExistingCusEntryHeader != null
											&& Declaration.ExistingCusEntryHeader.MergedLines.Count > 0
											&& HasChangesSinceMergeHunter.HasChangesSinceLastMark;
	}
}
