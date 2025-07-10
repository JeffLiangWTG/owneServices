using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentStmNoteCollection : StmNoteCollection
	{
		public ForwardingShipmentStmNoteCollection(ForwardingShipment shipment)
			: base(shipment, shipment.Factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingShipmentStmNote);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			if (!FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration)
			{
				var additionalFilter = new ZQuery(StmNoteSchema.ST_Description, SQLComparisonOperator.NotEqual, PredefinedNoteTypes.Instance.OriginalBillNotes.Description);
				return base.CreateAdditionalFilter().AddToFilter(additionalFilter);
			}

			return base.CreateAdditionalFilter();
		}
	}
}
