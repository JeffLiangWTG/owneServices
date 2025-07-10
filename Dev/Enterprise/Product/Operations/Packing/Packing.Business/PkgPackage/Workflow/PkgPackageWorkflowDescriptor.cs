using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Business
{
	class PkgPackageWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.PkgPackageWorkflowDecriptorCode; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("PkgPackageWorkflowDescriptor|Description", "Package"); }
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return null; } // Does not have Controller ID. There are no workflow item collections on the PkgPackage type, so there is no need for a controller ID.
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			// Note: PkgPackage is intentionally not an IWorkflowProvider as we are using the LineTriggerSupport functionality.
			get { return typeof(PkgPackage); }
		}

		#endregion

		#region Flags

		public override bool SupportsWorkflowTemplates => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsUniversalTemplates => false;

		protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var package = (bizObj as PkgPackage);

			if (package?.PackageJob?.ParentJob is IPackingParentWithTransportCompanyAndBookingParty parentJob)
			{
				if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryCartage || partyType == MessageRecipientPartyTypeList.Codes.PickupCartage)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parentJob.GetTransportCompany(package.KP_PackageID, partyType)));
				}
				else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parentJob.GetBookingParty(package.KP_PackageID, partyType)));
				}
			}
		}

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.PickupCartage;
		}

		#endregion

	}
}

