using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferWorkflowDescriptor : WhsDocketWorkflowDescriptor
	{
		#region Type & Code & Description

		public override Type WorkflowProviderType => typeof(WhsTransfer);

		protected override ControllerID GetControllerID() => ControllerIDs.WhsTransfer;

		protected override ZString GetCode() => WorkflowDescriptors.WhsTransferWorkflowDescriptorCode;

		protected override IMultilingualString GetDescription() => Enterprise.Warehouse.Transactions.DataTransfer.ResString.GetMultilingualString("Warehouse|WhsTransferWorkflowDescriptor|Description", "Warehouse Transfer");

		#endregion

		#region Flags

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => true;

		public override bool SupportsRecognizeRevenue => false;
		public override bool SupportsPostAllRevenue => false;
		public override bool SupportsCreateJobInvoiceHeader => false;
		public override bool SupportsPostAllSisterCompanyCharges => false;
		public override bool SupportsPostLocalSisterCompanyChargesOnly => false;
		public override bool SupportsPostAllCosts => false;

		protected override bool SupportDocketPlanningStatus => WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value;

		#endregion

		#region Workflow Triggers

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.WhsTransfer };

		#endregion
	}
}
