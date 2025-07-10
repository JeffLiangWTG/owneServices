using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.DataTransfer.Res;
using ResString = Enterprise.Customs.DataTransfer.ResString;

namespace Enterprise.Customs.Business
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public class CommercialInvoiceWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("CommercialInvoiceWorkflowDescriptor|Description", "Commercial Invoice"); }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return ""; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("6650b7b1-2691-4ee2-bb26-849b0d96bbd7", "Job Type"), JobDeclarationWorkflowDescriptor.GetJobMessageTypeList(this)));
				return list.ToArray();
			}
		}

		#endregion

		#region Criteria Requirements

		public override bool RequiresBranch { get { return true; } }
		public override bool RequiresDepartment { get { return false; } }
		public override bool RequiresPort1 { get { return false; } }
		public override bool RequiresPort2 { get { return false; } }
		public override bool SupportsEventTracking { get { return true; } }
		public override bool SupportsTasks { get { return true; } }
		public override bool SupportsScreenLayout { get { return false; } }
		#endregion

		#region Workflow Trigger

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.Forwarder;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			BaseJobComInvoiceHeader invoice = (BaseJobComInvoiceHeader)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.Consignee:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(invoice.Buyer ?? invoice.JobDeclaration?.Consignee, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.Consignor:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(invoice.Supplier ?? invoice.JobDeclaration?.Consignor, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.PickupCartage:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(invoice.JobDeclaration?.DocsAndCartage?.PickupCartageCoAddr));
					break;
				case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(invoice.JobDeclaration?.DocsAndCartage?.DeliveryCartageCoAddr));
					break;
				case MessageRecipientPartyTypeList.Codes.Forwarder:
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(invoice.JobDeclaration?.Forwarder, ZString.Empty));
					break;
				case MessageRecipientPartyTypeList.Codes.BillToParty:
					var jobHeaderParent = invoice.JobDeclaration as IJobHeaderParent;
					var org = jobHeaderParent != null ? new JobHeader.Loader(jobHeaderParent).Load()?.LocalCharges : null;
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(org, ZString.Empty));
					break;
			}
		}

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.CommercialInvoice };

		#endregion

		public override Type WorkflowProviderType
		{
			get { return typeof(BaseJobComInvoiceHeader); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.CommercialInvoice; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new CommercialInvoiceFormCustomisationSettingsProvider();
		}

		public override bool SupportsUniversalTemplates => false;

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			// base implementation uses NewWithValidTestData which creates not a standalone invoice, but invoice attached to a declaration
			return factory.New(WorkflowProviderType);
		}

#endif
	}
}
