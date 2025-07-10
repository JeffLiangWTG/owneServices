using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CarrierVoyageWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("6CC8034C-B1EE-4877-95E7-B4E225E3EB4F", "Ocean Carrier Voyage");
		public override ControllerID ControllerID => null; // Does not have Controller ID
		public override Type WorkflowProviderType => typeof(CarrierVoyage);
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
				return new[] { new ProcessTemplateSubType(Res.GetString("97A87815-8D23-4E72-9241-98B09AD34B3C", "Published"), PublishList) };
			}
		}

		CodeDescriptionPairList PublishList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("65E9F6D6-237F-4FD4-95F7-0FE8F1500F62", "All"));
				result.AddPair("YES", Res.GetString("FC4DAF9B-315A-41B7-8027-68B76844B57D", "Yes"));
				result.AddPair("NO", Res.GetString("8494A11C-B5F2-41A9-B8E1-E3A3D6730D18", "No"));
				return result;
			}
		}
	}
}
