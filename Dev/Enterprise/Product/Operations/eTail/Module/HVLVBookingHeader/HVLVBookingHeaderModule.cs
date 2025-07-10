using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module
{
	public class HVLVBookingHeaderModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.HVLVBookingHeader;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) =>
			ZControllerFactory.Create(ControllerIDs.HVLVBookingHeader);

		protected override IFilterControl GetNewFilterControl() =>
			new HVLVBookingHeaderFilterControl(GridCollection, (HVLVBookingHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new HVLVBookingHeaderFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HVLVBookingHeader;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		public override bool AllowNew => Env.CurrentUser.IsSupportUser;

		public override bool AllowDelete => Env.CurrentUser.IsSupportUser;

		public override bool AllowDefaultActivateDeactivate => true;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result, screeningNotEnabledMessage: GetScreeningNotEnabledMessage);
			return result.ToArray();
		}

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.HVLVBookingHeaderWorkflowDescriptorCode;

		#endregion

		#region Implementation

		Func<string> GetScreeningNotEnabledMessage => () =>
		{
			var message = string.Empty;
			if (!HVLVDataRegistry.Instance.HVLVEnablePartyScreening.Value.EnableHVLVPartyScreening)
			{
				message = Res.GetString("87597ebe-dba0-40b6-840f-fafb05dbc629", @"HVLV Party Screening has not been enabled.
To enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.");
			}

			return message;
		};

		#endregion
	}
}
