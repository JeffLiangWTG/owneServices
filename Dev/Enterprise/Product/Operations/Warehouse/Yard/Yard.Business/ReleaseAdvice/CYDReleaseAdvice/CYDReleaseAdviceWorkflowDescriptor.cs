using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseAdviceWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CYDReleaseAdviceWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("ReleaseAdviceWorkflowDescriptor|Description", "Container Yard Release Advice");

		public override ControllerID ControllerID => ControllerIDs.CYDReleaseAdvice;

		public override Type WorkflowProviderType => typeof(CYDReleaseAdvice);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;
	}
}
