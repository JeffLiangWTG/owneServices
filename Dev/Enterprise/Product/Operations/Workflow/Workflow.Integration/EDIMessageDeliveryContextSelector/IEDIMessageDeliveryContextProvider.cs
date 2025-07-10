using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IEDIMessageDeliveryContextProvider
	{
		ZGuid EDIMessageDeliveryContextSelectorPK { get; }
		BusinessObject[] GetRoots();
	}
}
