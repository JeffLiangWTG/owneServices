using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	public interface IStowPlanShipmentData
	{
		ZString JS_HouseBill { get; }
		ZString PortOfLading { get; }
		ZString PortOfDischarge { get; }
		IEnumerable<IStowPlanContainerData> Containers { get; }
	}
}
