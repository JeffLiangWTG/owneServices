using CargoWise.EntityFramework;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Global Commercial Invoice Plugin Business Object.
	/// Contains the headers and lines for a Global Commercial Invoice.
	/// It is a non-persistent business object, however the headers and lines are persistent.
	/// </summary>
	public sealed class GlobalCommercialInvoicePluginBusinessObject : GlobalCommercialInvoiceBusinessObject
	{
		/// <summary>
		/// Instantiates a new Global Commercial Invoice Plugin Business Object.
		/// </summary>
		/// <param name="hostBusinessEntity">The host (parent) business object. Normally a Shipment or Booking.</param>
		public GlobalCommercialInvoicePluginBusinessObject(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}
	}
}
