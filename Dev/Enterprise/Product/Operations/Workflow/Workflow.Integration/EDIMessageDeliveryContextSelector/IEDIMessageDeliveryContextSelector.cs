using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IEDIMessageDeliveryContextSelector
	{
		ZString ECS_ProcessType { get; set; }
		ZString ECS_Code { get; set; }
		ZString ECS_Description { get; set; }
	}
}
