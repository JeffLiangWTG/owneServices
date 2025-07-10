using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Business
{
	class AsycudaManifestHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("AsycudaManifestHeaderWorkflowDescriptor", "Outturn & Gate In/Out"); }
		}

		public override ControllerID ControllerID
		{
			get { return ZAControllerIDs.OutturnAndGateInOut; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AsycudaManifestHeader); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		protected override bool SupportsTaskLineTriggersCore
		{
			get { return false; }
		}
	}
}
