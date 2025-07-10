using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Collection of Shipments which are CoLoaded (ie. have a master shipment)
	/// </summary>
	public class CoLoadShipmentCollectionView : BusinessObjectCollectionView<CommonShipment>
	{
		public CoLoadShipmentCollectionView(ConsolShipmentCollection collectionToFilter, CommonConsol parentConsol)
			: base(collectionToFilter)
		{
			this.ParentConsol = parentConsol;
		}

		public CommonConsol ParentConsol { get; private set; }

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var shipment = element as CommonShipment;
			var master = shipment != null ? shipment.CoLoadMasterShipment : null;

			return
				master != null
				&& master.Consols.Contains(ParentConsol);
		}
	}
}
