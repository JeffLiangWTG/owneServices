using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class ShipmentsForTotallingCollection : BusinessObjectCollectionView<CommonShipment>
	{
		public ShipmentsForTotallingCollection(BusinessObjectCollection collectionToFilter, CommonConsol parentConsol)
			: base(collectionToFilter)
		{
			this.parentConsol = Argument.NotNull(parentConsol, "parentConsol");
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// doing nothing here to avoid virtual member call happening in SubsetBusinessObjectCollection constructor. Instead Rebuild() is called from the local constructor after all the variables are set.
		}

		readonly CommonConsol parentConsol;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var shipment = (CommonShipment)element;
			var master = shipment.CoLoadMasterShipment;

			return
				!shipment.JS_IsCancelled
				&&
				(
					master == null
					|| master.IsBuyersConsolLead
					|| master.IsShippersConsolLead
					|| !parentConsol.IsLinkedTo(master)
				);
		}
	}
}
