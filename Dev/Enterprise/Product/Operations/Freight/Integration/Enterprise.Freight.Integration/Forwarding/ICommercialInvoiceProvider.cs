using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface ICommercialInvoiceProvider
		{
			IBusinessObjectCollection Invoices { get; }
		}
	}
}
