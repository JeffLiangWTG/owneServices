using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IEDIMessageDeliveryContextEvaluator
	{
		IEnumerable<IEDIMessageDeliveryContextResult> GetValues(BusinessObjectFactory factory, IEDIMessageDeliveryContextProvider provider, INotifications notifications);
	}
}
