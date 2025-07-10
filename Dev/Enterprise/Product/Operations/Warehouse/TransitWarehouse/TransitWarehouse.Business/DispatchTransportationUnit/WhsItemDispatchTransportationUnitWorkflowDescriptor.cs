using System.Linq;
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
	public class WhsItemDispatchTransportationUnitWorkflowDescriptor : WhsItemHeaderWorkflowDescriptor<WhsItemDispatchTransportationUnit>
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.TransitDispatchTransportationUnit; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get
			{
				return ResString.GetMultilingualString("WhsItemDispatchTransportationUnit|WhsItemDispatchTransportationUnitWorkflowDescriptor|Description",
					"Transit Dispatch Transportation Unit");
			}
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsItemDispatchTransportationUnit; }
		}

		#endregion

		#region IncludeWorkflowTriggerActionXMLDebtorBalance

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance
		{
			get { return false; }
		}

		#endregion

		#region SupportsWorkflowTriggerActionUniversalShipmentXML

		protected override bool SupportsWorkflowTriggerActionUniversalShipmentXML => false;

		#endregion

		#region SupportsSetFieldTriggerAction

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return true;
		}

		#endregion

		#region SupportsAutoRateCostsAndRevenue

		// Disable it for now, waiting on rating team's WI00566202 - Support Autorating actions in Workflow per type of Accounting tab
		public override bool SupportsAutoRateCostsAndRevenue => false;

		#endregion

		#region RequiresWarehouse

		public override bool RequiresWarehouse
		{
			get { return true; }
		}

		#endregion

		#region WarehouseType

		public override WarehouseCollectionType WarehouseType => WarehouseCollectionType.TransitWarehouse;

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.TransitDispTranspUnt };

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var dtu = (WhsItemDispatchTransportationUnit)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				var dcns = dtu.DispatchLoadLists.SelectMany(dll => dll.PackageStates.Where(p => p.DispatchConsignment != null).Select(p => p.DispatchConsignment));

				foreach (var dcn in dcns)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(dcn.BookingPartyDocAddress));
				}
			}
		}

		#endregion
	}
}
