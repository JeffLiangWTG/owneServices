using System;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class ShipmentDeclarationCollection : NonPersistentBusinessObjectCollection<NonPersistentBusinessObject>
	{
		public ShipmentDeclarationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			shipments = new TrackingShipmentCollection(factory);
			trackingDeclarations = new TrackingDeclarationCollection(factory);
		}

		public new IShipmentDeclaration this[int index]
		{
			get { return (IShipmentDeclaration)Elements[index]; }
		}

		public void Load(ZQuery shipmentFilter, ZQuery declarationFilter, string sortColumnName, ListSortDirection sortOrder)
		{
			RemoveAll();

			int? maximumRows = shipmentFilter.MaximumRows;
			if (maximumRows != null)
			{
				declarationFilter.MaximumRows = shipmentFilter.MaximumRows;
			}

			shipments.Load(shipmentFilter);
			trackingDeclarations.Load(declarationFilter);

			foreach (TrackingShipment shipment in shipments)
			{
				Add(shipment);
			}

			AddRange(trackingDeclarations);

			Sort(sortColumnName, sortOrder);
			CutToMaximumRowsCount(maximumRows);
		}

		#region Overriden Methods

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ShipmentDeclarationFetchStrategy(this);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation

		readonly TrackingShipmentCollection shipments;
		readonly TrackingDeclarationCollection trackingDeclarations;

		void CutToMaximumRowsCount(int? maximumRows)
		{
			if (maximumRows != null && maximumRows < Count)
			{
				for (int i = Count - 1; i >= maximumRows; i--)
				{
					Remove((BusinessObject)this[i]);
				}
			}
		}

		#endregion

	}
}
