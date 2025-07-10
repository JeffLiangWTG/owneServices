using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module
{
	public class HVLVOriginLoadListModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public HVLVOriginLoadListModule()
		{
			var checkpoint = ((IOperationalActionSupportable)this).OperationalActionSupporter.CustomizationSecurityCheckpoint;

			if (checkpoint.IsAllowed)
			{
				Plugins.Add(ControllerIDs.OperationalActions);
			}
		}

		public override ModuleIdentifier ID => ModuleIDs.HVLVOriginLoadList;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HVLVOriginLoadList;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new HVLVOriginLoadListFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() =>
			new HVLVOriginLoadListFilterControl(GridCollection, (HVLVOriginLoadListFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new HVLVOriginLoadListCollection(Factory);

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => true;

		public override bool AllowUniversalCopy => true;

		public override bool AllowView => true;

		protected override bool ShowRecentItemsCore() => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.HVLVOriginLoadList);

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var collection = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (!HVLVOriginLoadList.IsFunctionalTesting)
			{
				collection.Remove(NewMenuItem);
			}

			return collection.ToArray();
		}

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.HVLVOriginLoadListWorkflowDescriptorCode;

		#endregion

		#region IOperationalActionSupportable

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new HVLVOriginLoadListOperationalActionSupporter();

		#endregion
	}
}
