using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class DeclarationOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new BrokerageSubmitOperationalActionMethod()
			};
		}
	}
}
