using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class JobSupplierBookingWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.JobSupplierBookingWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Forwarding|JobSupplierBookingWorkflowDescriptor|Description", "Supplier Booking");

		public override ControllerID ControllerID => ControllerIDs.SupplierBooking;

		public override Type WorkflowProviderType => typeof(JobSupplierBooking);

		protected override bool SupportsReleaseGroupRulesCore => false;

		public override bool SupportsBufferManagement => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return false; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.JobSupplierBooking }; }
		}

		#region Requires Port

		public override bool RequiresPort1 => true;

		public override bool RequiresPort2 => true;

		#endregion

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback
		{
			get { return false; }
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("bb528eb3-4dd6-4644-bfc2-1c44e6acdaa9", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("1006bc6a-aabd-442e-8697-1671a3501b6e", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.ControllingCustomer |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Email;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var supplierBooking = bizObj as JobSupplierBooking;

			if (supplierBooking != null)
			{
				if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(supplierBooking.BookingParty, ZString.Empty));
				}
				else if (partyType == MessageRecipientPartyTypeList.Codes.ControllingCustomer)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(supplierBooking.ControllingCustomerAddress?.Organisation, ZString.Empty));
				}
				else if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(supplierBooking.SupplierAddress?.Organisation, ZString.Empty));
				}
			}
			else
			{
				base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			}
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SetField, WorkflowTriggerActionTypeConstants.Descriptions.SetField);

			return result;
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => false;

		public override bool SupportsWorkflowTemplates => true;

		protected override ValidationToolSettings GetValidationToolSettings() => new JobSupplierBookingValidationToolSettings(this);

		public override bool SupportsScreenLayout => false;
	}
}

