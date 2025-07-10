using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDAdHocServiceOrderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CYDAdHocServiceOrderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("CYDAdHocServiceOrderWorkflowDescriptor|Description", "Container Yard Ad Hoc Service Order");

		public override ControllerID ControllerID => ControllerIDs.CYDAdHocServiceOrder;

		public override Type WorkflowProviderType => typeof(CYDAdHocServiceOrder);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;
	}
}
