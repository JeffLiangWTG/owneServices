using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class ShipmentCollectionView : BusinessObjectCollectionView<CommonShipment>
	{
		public ShipmentCollectionView(BusinessObjectCollection collectionToFilter, Predicate<CommonShipment> isShipmentPartOfTheCollection)
			: base(collectionToFilter)
		{
			IsShipmentPartOfTheCollection = isShipmentPartOfTheCollection;
			Rebuild();
		}

		readonly Predicate<CommonShipment> IsShipmentPartOfTheCollection;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			CommonShipment shipment = element as CommonShipment;
			if (shipment != null && IsShipmentPartOfTheCollection != null)
			{
				return IsShipmentPartOfTheCollection(shipment);
			}

			return false;
		}
	}
}
