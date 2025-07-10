using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDDeliveryCollection : DependentBusinessObjectCollection<CYDDelivery, CYDDeliveryHeader>
	{
		public CYDDeliveryCollection(CYDDeliveryHeader master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CYDDeliverySchema.YDL_YDH_DeliveryHeader; }
		}
	}
}
