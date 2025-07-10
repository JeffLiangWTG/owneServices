namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool SupportsAutoMergeCore => false;

		protected override bool RequiresMergeCore => false;
	}
}
