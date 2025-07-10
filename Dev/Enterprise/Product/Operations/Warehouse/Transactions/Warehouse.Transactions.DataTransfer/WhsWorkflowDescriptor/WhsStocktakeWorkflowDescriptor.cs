using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeWorkflowDescriptor : WorkflowDescriptor
	{
		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var stocktake = (WhsStocktake)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(stocktake.Client, ZString.Empty));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.Warehouse && stocktake.Warehouse != null && stocktake.Warehouse.WarehouseAddress != null)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(stocktake.Warehouse.WarehouseAddress));
			}
		}

		#endregion

		#region Code / Description / ControllerID

		public override string Code => WorkflowDescriptors.WhsStocktakeWorkflowDescriptorCode;

		public override IMultilingualString Description => JobInvoicingConsumerTypes.WarehouseStocktake.MultilingualDescription;

		public override ControllerID ControllerID => ControllerIDs.WhsStocktake;

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.WhsStocktake };

		#endregion

		#region Flags

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsEventTracking => true;

		public override bool RequiresWarehouse => true;

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.Client
				| MessageRecipientPartyType.Warehouse;
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType => typeof(WhsStocktake);

		#endregion
	}
}
