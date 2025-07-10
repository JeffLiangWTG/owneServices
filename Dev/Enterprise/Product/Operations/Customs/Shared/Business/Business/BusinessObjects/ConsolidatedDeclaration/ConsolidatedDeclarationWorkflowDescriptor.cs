using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class ConsolidatedDeclarationWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ConsolidatedDeclarationWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("ConsolidatedDeclarationWorkflowDescriptor", "Consolidated Entry");
		public override Type WorkflowProviderType => typeof(ConsolidatedDeclaration);
		public override ControllerID ControllerID => ControllerIDs.Customs.ConsolidatedDeclaration;
		public override bool SupportsBufferManagement => false;
		public override bool SupportsEventTracking => true;

		protected override ValidationToolSettings GetValidationToolSettings() => new ConsolidatedDeclarationValidationToolSettings(this);
	}
}
