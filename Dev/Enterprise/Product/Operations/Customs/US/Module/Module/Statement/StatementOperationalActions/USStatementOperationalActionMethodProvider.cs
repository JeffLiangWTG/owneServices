using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module
{
	public class USStatementOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new StatementOperationalActionMethod()
			};
		}
	}
}
