namespace Enterprise.Customs.TR.Business.Declaration
{
	public class MergeManager : EU.Business.Declaration.MergeManager
	{
		public MergeManager(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger((JobDeclaration)Declaration);

		protected override void OnMerged()
		{
			base.OnMerged();

			((JobDeclaration)Declaration).CusEntryHeader.CreateStampDutyIfApplicable();
		}
	}
}
