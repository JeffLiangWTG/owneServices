using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStorageLinesCollection : ActiveBusinessObjectCollection<CYDYardStorageLines>
	{
		public CYDYardUnitStorageLinesCollection(CYDYardUnitState yardUnit)
			: base(yardUnit.Factory, yardUnit, null, CYDYardStorageLinesSchema.YSL_YUS_YardUnit)
		{
		}
	}
}
