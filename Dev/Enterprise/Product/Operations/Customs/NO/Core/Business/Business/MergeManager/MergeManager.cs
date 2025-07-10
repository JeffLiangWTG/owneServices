namespace Enterprise.Customs.NO.Business;

sealed class MergeManager(JobDeclaration declaration) : Customs.Business.MergeManager(declaration)
{
	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);
}
