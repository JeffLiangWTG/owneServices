using System;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class EquipmentFact : IEquipmentFact
	{
		public EquipmentFact(IRefEquipment equipment, IRefContainer equipmentType)
		{
			Argument.NotNull(equipment, nameof(equipment));

			PK = equipment.PK.ToGuid();
			GroupCode = equipment.RQ_EquipmentGroup;
			TypeCode = equipmentType?.RC_Code ?? "";
		}

		public Guid PK { get; }

		public string TypeCode { get; }

		public string GroupCode { get; }
	}
}
