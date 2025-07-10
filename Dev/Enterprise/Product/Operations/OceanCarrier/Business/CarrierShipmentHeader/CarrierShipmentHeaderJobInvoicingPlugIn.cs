using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderJobInvoicingPlugIn : IJobInvoicingPlugIn
	{
		public CarrierShipmentHeaderJobInvoicingPlugIn(CarrierShipmentHeader shipment)
		{
			this.shipment = shipment ?? throw new ArgumentNullException(nameof(shipment));
		}

		readonly CarrierShipmentHeader shipment;

		#region IJobInvoicingPlugIn Members

		public IJobInvoicingSupporter InvoicingSupporter => invoicingSupporter ?? (invoicingSupporter = new CarrierShipmentHeaderInvoicingSupporter(shipment));
		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		public string JobNumber => shipment.JobNumber;
		public ZGuid PK => shipment.PK;
		public string TableName => CarrierShipmentHeaderSchema.Constants.TableName;
		public bool IsInDatabase => shipment.IsInDatabase;
		public BusinessObjectFactory Factory => shipment.Factory;
		public bool IsDeleted => shipment.IsDeleted;
		public bool AllowInvoiceDeletion => true;

		public void SetJobNumberFieldOnSaving()
		{
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}
	}
}
