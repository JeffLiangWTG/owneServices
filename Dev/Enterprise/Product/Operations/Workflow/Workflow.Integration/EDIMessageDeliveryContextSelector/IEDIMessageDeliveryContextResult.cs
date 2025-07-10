using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IEDIMessageDeliveryContextResult
	{
		ZString ContextType { get; }
		ZString Description { get; }
		ZString ContextValue { get; }
	}
}
