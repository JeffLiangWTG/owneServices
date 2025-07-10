using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	public interface IStowPlanContainerStockData
	{
		ZString ContainerOperator { get; }
		ZString EquipmentSizeType { get; }
	}
}
