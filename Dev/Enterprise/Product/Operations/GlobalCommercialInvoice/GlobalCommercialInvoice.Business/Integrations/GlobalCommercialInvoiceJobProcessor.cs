using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.IGlobalCommercialInvoiceComplianceProcessor;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	public class GlobalCommercialInvoiceJobProcessor : IGlobalCommercialInvoiceJobProcessor
	{
		public void ConvertBookingWithQuoteToShipment(ZGuid bookingPK, ZGuid shipmentPK, BusinessObjectFactory factory) =>
			InvoiceDataTransfer.BookingWithQuoteToShipment(bookingPK, shipmentPK, factory);

		public void UpdateUnitOfMeasurement(IGlobalCommercialInvoiceProvider provider, ZPropertyInfo propertyInfo, ZString oldValue, ZString newValue)
		{
			if (!ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled)
			{
				return;
			}

			if (propertyInfo.Name.Equals(JobShipmentSchema.JS_UnitOfVolume.Name))
			{
				UpdateUnitOfVolumeUQ(provider, oldValue, newValue);
			}
			else if (propertyInfo.Name.Equals(JobShipmentSchema.JS_UnitOfWeight.Name))
			{
				UpdateUnitOfWeightUQ(provider, oldValue, newValue);
			}
		}

		void UpdateUnitOfVolumeUQ(IGlobalCommercialInvoiceProvider provider, ZString oldValue, ZString newValue)
		{
			foreach (var line in (provider.DataProvider as GlobalCommercialInvoiceBusinessObject).Lines)
			{
				if (line.GIL_VolumeUQ.Equals(oldValue))
				{
					line.GIL_VolumeUQ = newValue;
				}
			}
		}

		void UpdateUnitOfWeightUQ(IGlobalCommercialInvoiceProvider provider, ZString oldValue, ZString newValue)
		{
			foreach (var line in (provider.DataProvider as GlobalCommercialInvoiceBusinessObject).Lines)
			{
				if (line.GIL_GrossWeightUQ.Equals(oldValue))
				{
					line.GIL_GrossWeightUQ = newValue;
				}

				if (line.GIL_NetWeightUQ.Equals(oldValue))
				{
					line.GIL_NetWeightUQ = newValue;
				}
			}
		}
	}
}
