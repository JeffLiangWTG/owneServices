using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountWaveWorkflowDescriptor : WorkflowDescriptor
	{
		#region ControllerID

		public override ControllerID ControllerID => ControllerIDs.WhsCycleCountWave;

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType => typeof(WhsCycleCountWave);

		#endregion

		#region SupportsWorkflowTemplates

		public override bool SupportsWorkflowTemplates => false;

		#endregion

		#region SupportsUniversalTemplates

		public override bool SupportsUniversalTemplates => false;

		#endregion

		#region Workflow Triggers

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			return result;
		}

		#endregion

		#region Code

		public override string Code => WorkflowDescriptors.WhsCycleCountWaveWorkflowDescriptorCode;

		#endregion

		#region Description

		public override IMultilingualString Description => DataTransfer.ResString.GetMultilingualString("Warehouse|WhsCycleCountWaveWorkflowDescriptor|Description", "Warehouse Cycle Count Wave");

		#endregion

		#region Events

		public override bool SupportsEventTracking => false;

		#endregion
	}
}
