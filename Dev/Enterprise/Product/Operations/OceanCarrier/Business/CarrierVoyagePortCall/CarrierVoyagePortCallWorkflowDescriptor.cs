using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CarrierVoyagePortCallWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("D5B0D2D8-4A60-46E5-ABF7-B506BE8D0D6F", "Ocean Carrier Voyage Port Call");
		public override ControllerID ControllerID => null; // Does not have Controller ID
		public override Type WorkflowProviderType => typeof(CarrierVoyagePortCall);
		public override bool RequiresClient => false;
		public override bool RequiresBranch => true;
		public override bool RequiresDepartment => true;
		public override bool SupportsEventTracking => true;
		public override bool SupportsBufferManagement => false;
		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				return Array.Empty<ProcessTemplateSubType>();
			}
		}
	}
}
