using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.PackLines)]
	public class ParentShipmentOuterPackLineCollection : ActiveBusinessObjectCollection<PackLine>
	{
		public ParentShipmentOuterPackLineCollection(BusinessObjectFactory factory, CommonShipment shipment)
			: base(factory, GetFilterForShipmentOuterPackLines(shipment))
		{
			this.shipment = shipment;
		}
		readonly CommonShipment shipment;

		public ParentShipmentOuterPackLineCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType))
		{
		}

		protected override void SetDefaultsForNewElementCore(PackLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.JL_FreightMode = FreightConstants.OuterPackType;
			newElement.JL_JS = shipment.PK;
		}

		static ZQuery GetFilterForShipmentOuterPackLines(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return new ZQuery();
			}

			var filter = new ZQuery(JobPackLinesSchema.JL_JS, shipment?.PK);
			filter.AddToFilter(new ZQuery(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType));

			return filter;
		}
	}
}
