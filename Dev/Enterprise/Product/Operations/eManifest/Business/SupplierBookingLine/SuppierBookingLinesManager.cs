
using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingLinesManager : NonPersistentBusinessObject, IObsoleteValidation, IDisposable
	{
		public SupplierBookingLinesManager(CommonShipment shipment)
		{
			this.shipment = shipment;
			this.shipment.Factory.Saved += FactorySaved;
		}

		#region Lines

		[ChildEditable(true)]
		public SupplierBookingLineCollection Lines
		{
			get
			{
				if (supplierBookingLines == null)
				{
					supplierBookingLines = new SupplierBookingLineCollection(shipment);

					RefreshLines();
				}

				return supplierBookingLines;
			}
		}

		#endregion

		public void Dispose()
		{
			Dispose(true);
		}

		public void RefreshLines()
		{
			Lines.Load();

			foreach (var line in Lines)
			{
				if (line.IsInDatabase)
				{
					line.ReadOnly = true;
				}
				else
				{
					RegisterEditableChildObject(line);
					line.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.shipment.Factory.Saved -= FactorySaved;
			}
		}

		void FactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				RefreshLines();
			}
		}

		SupplierBookingLineCollection supplierBookingLines;
		readonly CommonShipment shipment;
	}
}
