using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions
{
	public class USInBondOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new SendDepartureAddOperationalActionMethod(),
				new BatchMarkAsClosedOperationActionMethod()
			};
		}
	}
}
