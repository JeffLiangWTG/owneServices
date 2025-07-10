using CargoWise.Types;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PickByBOMComponentKitRelation
	{
		public PickByBOMComponentKitRelation(ZGuid parentPickLinePK, ZGuid parentOrderLinePK, ZGuid componentPickLinePK, ZGuid componentProductPK)
		{
			ParentPickLinePK = parentPickLinePK;
			ParentOrderLinePK = parentOrderLinePK;
			ComponentPickLinePK = componentPickLinePK;
			ComponentProductPK = componentProductPK;
		}

		public ZGuid ParentPickLinePK;
		public ZGuid ParentOrderLinePK;
		public ZGuid ComponentPickLinePK;
		public ZGuid ComponentProductPK;

		public void Deconstruct(out ZGuid parentPickLinePK, out ZGuid parentOrderLinePK, out ZGuid componentPickLinePK, out ZGuid componentProductPK)
		{
			parentPickLinePK = ParentPickLinePK;
			parentOrderLinePK = ParentOrderLinePK;
			componentPickLinePK = ComponentPickLinePK;
			componentProductPK = ComponentProductPK;
		}
	}
}
