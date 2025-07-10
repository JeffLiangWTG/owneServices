using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class FindFreeStorageDaysStrategy : IFindFreeStorageDaysStrategy
	{
		public bool Filter(CYDYardStorageFreeDays row, CYDYardUnitState yardUnit)
		{
			var yardUnitLoad = yardUnit.Delivery.UnitLineItem.YLI_IsEmpty ? ContainerYardConstants.YardUnitLoad.Codes.EMP : ContainerYardConstants.YardUnitLoad.Codes.LAD;
			return DefaultOrEqual(row.YFD_WW_Yard, yardUnit.CurrentYard.PK)
				&& DefaultOrEqual(row.YFD_UnitType, yardUnit.Delivery.UnitLineItem.YLI_Type)
				&& DefaultOrEqual(row.YFD_YardUnitLength, yardUnit.Delivery.UnitLineItem.ContainerType.RC_Length)
				&& DefaultOrEqual(row.YFD_ContainerClass, yardUnit.Delivery.UnitLineItem.ContainerType.RC_StorageClass)
				&& DefaultOrEqual(row.YFD_UnitLoad, yardUnitLoad)
				&& DefaultOrEqual(row.YFD_TransportMode, nameof(FreightMode.ROA));
		}

		public int GetPriority(CYDYardStorageFreeDays row)
		{
			return new CYDYardStorageFreeDaysPriority(row).Priority;
		}

		bool DefaultOrEqual(ZGuid value, ZGuid comparison)
		{
			return value == ZGuid.Empty || value == comparison;
		}

		bool DefaultOrEqual(ZString value, ZString comparison)
		{
			return value == ZString.Empty || value == comparison;
		}

		bool DefaultOrEqual(ZDecimal value, ZDecimal comparison)
		{
			return value == ZDecimal.Zero || value == comparison;
		}
	}

	class CYDYardStorageFreeDaysPriority(CYDYardStorageFreeDays yardStorageFreeDays)
	{
		public int Priority
		{
			get
			{
				if (priority < 0)
				{
					// The sequence of AddPriority is important. The first has the highest priority and the last has the lowest priority.
					priority = 0;
					AddPriority(yardStorageFreeDays.YFD_WW_Yard);
					AddPriority(yardStorageFreeDays.YFD_UnitType);
					AddPriority(yardStorageFreeDays.YFD_YardUnitLength);
					AddPriority(yardStorageFreeDays.YFD_ContainerClass);
					AddPriority(yardStorageFreeDays.YFD_UnitLoad);
					AddPriority(yardStorageFreeDays.YFD_TransportMode);
				}
				return priority;
			}
		}

		int priority = -1;

		void AddPriority(ZGuid value)
		{
			priority = priority * 2 + (value == ZGuid.Empty ? 0 : 1);
		}

		void AddPriority(ZString value)
		{
			priority = priority * 2 + (value == ZString.Empty ? 0 : 1);
		}

		void AddPriority(ZDecimal value)
		{
			priority = priority * 2 + (value == ZDecimal.Zero ? 0 : 1);
		}
	}
}
