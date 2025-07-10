using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchLoadListWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code

		public override string Code => WorkflowDescriptors.TransitDispatchLoadList;

		#endregion

		#region Description

		public override IMultilingualString Description =>
			ResString.GetMultilingualString("WhsItemDispatchLoadList|WhsItemDispatchLoadListWorkflowDescriptor|Description", "Transit Dispatch Load List");

		#endregion

		#region ControllerID

		public override ControllerID ControllerID => ControllerIDs.WhsItemDispatchLoadList;

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType => typeof(WhsItemDispatchLoadList);

		#endregion

		#region RequiresClient

		public override bool RequiresClient => false;

		#endregion

		#region RequiresWarehouse

		public override bool RequiresWarehouse => true;

		#endregion

		#region WarehouseType

		public override WarehouseCollectionType WarehouseType => WarehouseCollectionType.TransitWarehouse;

		#endregion

		#region SupportsEventTracking

		public override bool SupportsEventTracking => true;

		#endregion

		#region IncludeWorkflowTriggerActionXMLDebtorBalance

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance => false;

		#endregion

		#region SupportsWorkflowTriggerActionUniversalShipmentXML

		protected override bool SupportsWorkflowTriggerActionUniversalShipmentXML => false;

		#endregion

		#region SupportsBufferManagement

		public override bool SupportsBufferManagement => false;

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.TransitDspLoadList };

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) =>
			MessageRecipientPartyType.Warehouse |
			MessageRecipientPartyType.OrgProxy |
			MessageRecipientPartyType.Email;

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var loadList = (WhsItemDispatchLoadList)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Warehouse)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(loadList?.Warehouse?.WarehouseAddress));
			}
		}

		#endregion
	}
}
