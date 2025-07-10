using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IGlobalCommercialInvoiceJobProcessor
	{
		void ConvertBookingWithQuoteToShipment(ZGuid bookingPK, ZGuid shipmentPK, BusinessObjectFactory factory);

		void UpdateUnitOfMeasurement(IGlobalCommercialInvoiceComplianceProcessor.IGlobalCommercialInvoiceProvider provider, ZPropertyInfo propertyInfo, ZString oldValue, ZString newValue);
	}
}
