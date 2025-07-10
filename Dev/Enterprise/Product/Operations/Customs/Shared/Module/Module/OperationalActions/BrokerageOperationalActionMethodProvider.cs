using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class BrokerageOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new CreateBrokerageJobOperationalActionMethod(),
				new CreateAndSubmitBrokerageOperationalActionMethod()
			};
		}
	}
}
