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
	public class HVLVOriginLoadListWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code => WorkflowDescriptors.HVLVOriginLoadListWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("eTail|HVLVOriginLoadListWorkflowDescriptors|Description", "HVLV Origin Load List");

		#endregion

		#region Requirements

		public override bool RequiresClient => true;

		public override ZString ClientName => Res.GetString("e43d89ea-a45f-430c-9685-dd1bc6d20925", "eTailer");

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => false;

		public override bool RequiresPort1 => true;

		public override ZString Port1Name => Res.GetString("78d69a81-8d62-4c32-ac51-ddad591bd50d", "Dispatch UNLOCO");

		#endregion

		public override bool SupportsCreateTransportBooking => true;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override Type WorkflowProviderType => typeof(HVLVOriginLoadList);

		public override ControllerID ControllerID => ControllerIDs.HVLVOriginLoadList;

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.HVLVOriginLoadList };

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.OrgProxy;
	}
}
