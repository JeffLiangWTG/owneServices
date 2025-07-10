using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.DataTransfer
{
	public class HVLVOuterPackageWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code => WorkflowDescriptors.HVLVOuterPackageWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("eTail|HVLVOuterPackageWorkflowDescriptors|Description", "HVLV Outer Package");

		#endregion

		#region Requirements

		public override bool RequiresClient => true;

		public override ZString ClientName => Res.GetString("d583dcea-12d3-4b3d-9bb8-e4a88cc7d121", "eTailer");

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => false;

		public override bool RequiresPort1 => true;

		public override ZString Port1Name => Res.GetString("7316b2ef-80c6-40ee-8609-29035f1692d5", "Dispatch UNLOCO");

		#endregion

		public override bool SupportsCreateTransportBooking => false;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override Type WorkflowProviderType => typeof(HVLVOuterPackage);

		public override ControllerID ControllerID => ControllerIDs.HVLVOuterPackage;

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.HVLVOuterPackage };

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.OrgProxy;
	}
}
