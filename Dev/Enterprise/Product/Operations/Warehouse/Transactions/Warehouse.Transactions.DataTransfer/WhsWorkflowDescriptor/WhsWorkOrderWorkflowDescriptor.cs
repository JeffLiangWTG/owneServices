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
	public class WhsWorkOrderWorkflowDescriptor : WhsDocketWorkflowDescriptor
	{
		#region Type & Code & Description

		public override Type WorkflowProviderType => typeof(WhsWorkOrder);

		protected override ControllerID GetControllerID() => ControllerIDs.WhsWorkOrder;

		protected override ZString GetCode() => WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode;

		protected override IMultilingualString GetDescription() => Enterprise.Warehouse.Transactions.DataTransfer.ResString.GetMultilingualString("Warehouse|WhsWorkOrderWorkflowDescriptor|Description", "Warehouse Work Order");

		#endregion

		#region Flags

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => true;

		public override bool SupportsCustomFields => false;
		public override bool SupportsRecognizeRevenue => false;
		public override bool SupportsPostAllRevenue => false;
		public override bool SupportsCreateJobInvoiceHeader => false;
		public override bool SupportsPostAllSisterCompanyCharges => false;
		public override bool SupportsPostLocalSisterCompanyChargesOnly => false;
		public override bool SupportsPostAllCosts => false;

		#endregion

		#region FormCustomisationSettingsProvider

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider() => null;

		#endregion

		#region Workflow Triggers

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.WhsWorkOrder };

		#endregion
	}
}
