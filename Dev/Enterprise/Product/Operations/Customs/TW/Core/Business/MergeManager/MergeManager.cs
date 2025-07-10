using System.Linq;

namespace Enterprise.Customs.TW.Business
{
	class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void OnMerged()
		{
			base.OnMerged();

			var entryHeader = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
			if (entryHeader != null)
			{
				entryHeader.CalculateCH_DeclarationIncoterm();
				entryHeader.Validation.ValidateCH_DeclarationIncoterm();
			}
		}

		#region Implementation
		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger(Declaration);
		}

		#endregion
	}
}
