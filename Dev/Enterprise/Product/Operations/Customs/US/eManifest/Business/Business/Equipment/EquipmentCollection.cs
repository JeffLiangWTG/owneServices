using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class EquipmentCollection : ActiveBusinessObjectCollection<Equipment>
	{
		internal EquipmentCollection(Trip master, bool includingConveyance)
			: base(master.Factory, master, GetEquipmentFilter(includingConveyance), CusInBondEquipmentSchema.BJ_BH_Header)
		{
			this.includingConveyance = includingConveyance;
		}

		static ZQuery GetEquipmentFilter(bool includingConveyance)
		{
			return includingConveyance ? new ZQuery() : new ZQuery(CusInBondEquipmentSchema.BJ_IsConveyance, SQLComparisonOperator.NotEqual, ZBool.True);
		}

		protected override void SetRelationshipDefaultsForElementCore(Equipment newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			if (!includingConveyance)
			{
				newElement.BJ_IsConveyance = false;
			}
		}

		protected override void OnAdded(Equipment equipment)
		{
			base.OnAdded(equipment);
			var trip = Relationship.Master as Trip;
			if (trip?.AllEquipmentIncludingMainConveyance.Count == 1)
			{
				trip.PopulateCommodityWithEquipment(equipment.PK);
			}
		}

		public override void Delete(Equipment equipment)
		{
			equipment.Trip?.PopulateCommodityWithEquipment(ZGuid.Empty, x => x.BY_BJ_Equipment == equipment.PK);
			base.Delete(equipment);
		}

		readonly bool includingConveyance;
	}
}
