using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CYDPickupHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("PickupHeaderWorkflowDescriptor|Description", "Container Yard Pickup Header");

		public override ControllerID ControllerID => null;

		public override Type WorkflowProviderType => typeof(CYDPickupHeader);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;
	}
}
