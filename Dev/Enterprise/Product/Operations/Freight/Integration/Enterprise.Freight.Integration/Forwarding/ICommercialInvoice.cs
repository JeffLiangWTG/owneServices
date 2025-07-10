namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface ICommercialInvoice
		{
			void PopulateCommercialInvoice(IForwardingPackLineCollection packLines);
		}
	}
}
