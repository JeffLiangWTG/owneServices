using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobDeclarationFetchStrategy : EU.Business.FetchStrategies.JobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
		}
	}
}
