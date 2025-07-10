using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ContainerLoadListLineWorkflowDescriptor : ContainerLoadListLineWorkflowDescriptorBase
	{
		protected override ValidationToolSettings GetValidationToolSettings() => new ContainerLoadListLineValidationToolSettings(this);

		public override string Code => WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Forwarding|ContainerLoadListLineWorkflowDescriptor|Description", "Container Load List Line");
	}
}
