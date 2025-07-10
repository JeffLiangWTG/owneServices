using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveAdviceWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CYDReceiveAdviceWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("ReceiveAdviceWorkflowDescriptor|Description", "Container Yard Receive Advice");

		public override ControllerID ControllerID => ControllerIDs.CYDReceiveAdvice;

		public override Type WorkflowProviderType => typeof(CYDReceiveAdvice);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;
	}
}
