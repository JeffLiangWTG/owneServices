using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupCollection : DependentBusinessObjectCollection<CYDPickup, CYDPickupHeader>
	{
		public CYDPickupCollection(CYDPickupHeader master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CYDPickupSchema.YPL_YPH_PickupHeader; }
		}
	}
}
