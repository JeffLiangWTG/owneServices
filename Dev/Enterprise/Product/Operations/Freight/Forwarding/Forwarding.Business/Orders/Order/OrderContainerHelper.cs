using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public static class OrderContainerHelper
	{
		public static ForwardingContainer PopulateForwardingContainerFromOrderContainer(OrderContainer orderContainer, ForwardingContainer container)
		{
			container.JC_ContainerNum = orderContainer.J1_ContainerNumber;
			container.JC_ContainerCount = orderContainer.J1_ContainerCount;
			container.JC_SealNum = orderContainer.J1_SealNum;
			container.JC_AdditionalSealNum = orderContainer.J1_AdditionalSealNum;
			container.JC_Additional2SealNum = orderContainer.J1_Additional2SealNum;
			container.JC_RC = orderContainer.J1_RC;

			return container;
		}
	}
}
