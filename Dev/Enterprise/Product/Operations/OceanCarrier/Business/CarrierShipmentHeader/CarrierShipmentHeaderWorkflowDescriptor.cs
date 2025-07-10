using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CarrierShipmentHeaderWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("C4DDBCC1-8890-4DBC-873F-A7B6B4882C7C", "Ocean Carrier Shipment");
		public override ControllerID ControllerID => ControllerIDs.CarrierShipmentHeader;
		public override Type WorkflowProviderType => typeof(CarrierShipmentHeader);
		public override bool RequiresClient => false;
		public override bool RequiresPort1 => true;
		public override bool RequiresPort2 => true;
		public override bool RequiresBranch => true;
		public override bool RequiresDepartment => true;
		public override bool SupportsEventTracking => true;
		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				return new[]
				{
					new ProcessTemplateSubType(Res.GetString("8301bf2f-250f-474f-8613-825367199e5d", "Entity Type"), new CodeDescriptionPairList(OLookUpEditType.OceanCarrierShipmentTypes)),
				};
			}
		}
	}
}
