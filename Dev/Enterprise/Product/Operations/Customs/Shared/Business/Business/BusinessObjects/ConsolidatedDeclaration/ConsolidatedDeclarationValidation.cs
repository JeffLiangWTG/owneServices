
namespace Enterprise.Customs.Business
{
	public class ConsolidatedDeclarationValidation : CusReconBase.CusReconDeclarationValidation
	{
		public ConsolidatedDeclarationValidation(ConsolidatedDeclaration parent) : base(parent)
		{
			Parent = parent;
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckCongruence();
			CheckNumberOfEntryLines();
		}

		protected readonly new ConsolidatedDeclaration Parent;

		protected override void CheckCRD_JE_LeadDeclaration()
		{
			base.CheckCRD_JE_LeadDeclaration();
			if (!Parent.CRD_JE_LeadDeclaration.IsValid)
			{
				Parent.AddRowError(Res.GetString("284f58fc-689a-4cc1-a053-8a9bb5b9555b", "At least one declaration must be selected."));
			}
		}

		protected virtual void CheckCongruence()
		{ }

		protected virtual void CheckNumberOfEntryLines()
		{ }
	}
}
