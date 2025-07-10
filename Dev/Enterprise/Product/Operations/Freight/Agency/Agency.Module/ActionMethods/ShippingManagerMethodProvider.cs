using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	public sealed class ShippingManagerActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			if (typeof(BillOfLading).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[]
				{
					new EIDOSendActionMethod(),
					new EIDOWithdrawActionMethod(),
					new UpdateReturnByActionMethod(),
				};
			}
			else if (typeof(BillOfLadingContainer).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[]
				{
					new EIDOSendActionMethod(),
					new EIDOWithdrawActionMethod(),
					new BulkMovementsActionMethod(),
					new UpdateReturnByActionMethod(),
					new ReleaseOrderSendActionMethod(),
					new ReleaseOrderWithdrawActionMethod()
				};
			}
			else if (typeof(RefContainerStock).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[]
				{
					new BulkMovementsActionMethod(),
				};
			}
			else if (typeof(ContainerMovement).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[]
				{
					new UpdateDetentionDaysActionMethod(),
				};
			}
			else if (typeof(AgencyBooking).IsAssignableFrom(actionSupporter.RootType))
			{
				return new OperationalActionMethod[]
				{
					new RollBookingActionMethod(),
				};
			}
			else
			{
				return null;
			}
		}
	}
}


