using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public interface IFindFreeStorageDaysStrategy
	{
		public bool Filter(CYDYardStorageFreeDays row, CYDYardUnitState yardUnit);

		public int GetPriority(CYDYardStorageFreeDays row);
	}
}
