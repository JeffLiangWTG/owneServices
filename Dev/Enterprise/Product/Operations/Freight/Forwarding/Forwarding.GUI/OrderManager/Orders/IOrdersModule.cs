using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public interface IOrdersModule
	{
		IZForm ShowFormForSplit(Order orderToSplit, CreateOrderType splitType);
	}
}
