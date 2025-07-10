using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	public interface IStowPlanContainerData
	{
		ZString EquipmentNumber { get; }
		ZString StowPosition { get; }
		ZDecimal GrossWeightInKG { get; }
		IEnumerable<ZString> HazardCodes { get; }
		IStowPlanContainerStockData Stock { get; }
		ZString ISOType { get; }
	}
}
