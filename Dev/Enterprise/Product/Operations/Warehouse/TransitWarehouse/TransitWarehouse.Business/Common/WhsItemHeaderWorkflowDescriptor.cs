using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business.Common;

namespace Enterprise.Warehouse.Transit.Business
{
	public abstract class WhsItemHeaderWorkflowDescriptor<T> : WorkflowDescriptor
		where T : BusinessObject, IDocAddresses, IItemHeader
	{
		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(T); }
		}

		#endregion

		#region RequiresClient

		public override bool RequiresClient => false;

		#endregion

		#region SupportsBufferManagement

		public override bool SupportsBufferManagement => false;

		#endregion

		#region SupportsEventTracking

		public override bool SupportsEventTracking => true;

		#endregion

		#region SupportsWorkflowTriggerActionUniversalEventXML

		protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Warehouse |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.TransportCo |
				MessageRecipientPartyType.CustomsOutturnAgent |
				MessageRecipientPartyType.Forwarder |
				MessageRecipientPartyType.GateManagement |
				MessageRecipientPartyType.Email;
		}

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var header = (T)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Warehouse)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(header?.Warehouse?.WarehouseAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.TransportCo)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(header?.TransportCompany));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent)
			{
				var addresses = TransitWarehouseHelper.GetMatchingOrgAddresses(bizObj, UniversalDataBuss.Integration.DataContextType.SeaCargoOutturn);

				if (addresses.Any())
				{
					foreach (var address in addresses)
					{
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(address));
					}
				}
				else
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(GlbCompany.CurrentCompany.OrgProxy.MainAddress));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.GateManagement)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(header?.Warehouse?.WarehouseAddress));
			}
		}

		#endregion
	}
}
