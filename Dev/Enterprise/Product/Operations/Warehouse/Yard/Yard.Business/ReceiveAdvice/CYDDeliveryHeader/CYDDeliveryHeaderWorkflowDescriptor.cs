using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDDeliveryHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CYDDeliveryHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("DeliveryHeaderWorkflowDescriptor|Description", "Container Yard Delivery Header");

		public override ControllerID ControllerID => ControllerIDs.CYDDeliveryHeader;

		public override Type WorkflowProviderType => typeof(CYDDeliveryHeader);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;
	}
}
