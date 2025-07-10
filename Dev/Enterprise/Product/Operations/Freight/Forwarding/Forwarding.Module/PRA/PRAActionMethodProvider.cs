using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	public sealed class PRAActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			if (typeof(ForwardingConsol).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[]
				{
					new PRASendActionMethod(),
					new PRAReSendActionMethod(),
					new PRACancelActionMethod()
				};
			}
			else
			{
				return null;
			}
		}
	}
}
