using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CommodityCollection : ActiveBusinessObjectCollection<Commodity>
	{
		internal CommodityCollection(Shipment master)
			: base(master.Factory, master, new ZQuery(CusInBondCargoDescSchema.BY_ParentTableCode, master.TablePrefix), CusInBondCargoDescSchema.BY_ParentID)
		{
		}

		protected override void SetDefaultsForNewElementCore(Commodity newElement)
		{
			using (newElement.SuspendSettingHasChanges())
			{
				base.SetDefaultsForNewElementCore(newElement);

				new CommodityDefaultsManager(newElement).SetDefaults();

				DefaultShipmentQtyAndWeightBalanceToNewCommodity((Shipment)Relationship.Master, newElement);
			}
		}

		protected override void OnLoadedIntoCollectionCore(Commodity loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Shipment = (Shipment)Relationship.Master;
		}

		void DefaultShipmentQtyAndWeightBalanceToNewCommodity(Shipment shipment, Commodity newElement)
		{
			if (shipment != null)
			{
				var totalBY_PieceCount = this.Sum(x => x.BY_PieceCount);
				var totalBY_GrossWeight = this.Sum(x => x.BY_GrossWeight);
				var totalBY_MonetaryValue = this.Sum(x => x.BY_MonetaryValue);

				if (shipment.B0_ManifestQty > totalBY_PieceCount)
				{
					newElement.BY_PieceCount = shipment.B0_ManifestQty - totalBY_PieceCount;
				}

				if (shipment.B0_Weight > totalBY_GrossWeight)
				{
					newElement.BY_GrossWeight = shipment.B0_Weight - totalBY_GrossWeight;
				}

				if (shipment.B0_GoodsValue > totalBY_MonetaryValue)
				{
					newElement.BY_MonetaryValue = shipment.B0_GoodsValue - totalBY_MonetaryValue;
				}
			}
		}
	}
}
