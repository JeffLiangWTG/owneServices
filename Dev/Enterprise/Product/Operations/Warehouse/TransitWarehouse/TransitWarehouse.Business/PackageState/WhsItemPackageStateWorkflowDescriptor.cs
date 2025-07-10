using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemPackageStateWorkflowDescriptor : WorkflowDescriptor
	{
		#region Code

		public override string Code
		{
			get { return WorkflowDescriptors.TransitPackage; }
		}

		#endregion

		#region Description

		public override IMultilingualString Description
		{
			get
			{
				return ResString.GetMultilingualString("TransitPackage|Description", "Transit Package");
			}
		}

		#endregion

		#region ControllerID

		public override ControllerID ControllerID
		{
			get { return null; }
		}

		#endregion

		#region IncludeWorkflowTriggerActionXMLDebtorBalance

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance
		{
			get { return true; }
		}

		#endregion

		#region RequiresClient

		public override bool RequiresClient
		{
			get { return true; }
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

		#region SupportsBufferManagement

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		#endregion

		#region SupportsEventTracking

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#endregion

		#region SupportsSetFieldTriggerAction

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return true;
		}

		#endregion

		#region SupportsWorkflowTriggerActionUniversalEventXML

		protected override bool SupportsWorkflowTriggerActionUniversalEventXML
		{
			get { return true; }
		}

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType
		{
			get { return typeof(WhsItemPackageState); }
		}

		#endregion
	}
}
