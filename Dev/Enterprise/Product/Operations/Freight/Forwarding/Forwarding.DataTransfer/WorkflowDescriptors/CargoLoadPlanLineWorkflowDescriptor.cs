using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class CargoLoadPlanLineWorkflowDescriptor : ContainerLoadListLineWorkflowDescriptorBase
	{
		protected override ValidationToolSettings GetValidationToolSettings() => new CargoLoadPlanLineValidationToolSettings(this);

		public override string Code => WorkflowDescriptors.CargoLoadPlanLineWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Forwarding|CargoLoadPlanLineWorkflowDescriptor|Description", "Cargo Load Plan Line");
	}
}
