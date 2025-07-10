namespace Enterprise.Customs.SG.V4.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger(Declaration);
		}

		protected override bool RequiresMergeCore
		{
			get { return Declaration.Invoices.Count > 0 && Declaration.FilteredInvoiceLines.Count > 0 && HasChangesSinceMergeHunter.HasChangesSinceLastMark; }
		}
	}
}
