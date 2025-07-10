using System;
using System.Collections.Generic;
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
	public class WhsItemReceiveTransportationUnitWorkflowDescriptor : WhsItemHeaderWorkflowDescriptor<WhsItemReceiveTransportationUnit>, IWorkflowParentWithLines
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.TransitReceiveTransportationUnit; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get
			{
				return ResString.GetMultilingualString("WhsItemReceiveTransportationUnit|TransitReceiveTransportationUnitWorkflowDescriptor|Description",
						"Transit Receive Transportation Unit");
			}
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsItemReceiveTransportationUnit; }
		}

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

		#region SupportsWorkflowTriggerActionUniversalEventXML

		protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(WhsItemReceiveTransportationUnit); }
		}

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

		#region IWorkflowParentWithLines Memebers

		IEnumerable<string> IWorkflowParentWithLines.SupportedTriggerLineTypes
		{
			get
			{
				return new[] { TriggerLineTypes.Codes.PkgPackage };
			}
		}

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.TransitRecTranspUnt };

		#endregion

		#region AddToMessageRecipientPartyList

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			var rtu = (WhsItemReceiveTransportationUnit)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				foreach (var asn in rtu.ReceiveASNs)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(asn.BookingPartyDocAddress));
				}
			}
		}

		#endregion
	}
}
