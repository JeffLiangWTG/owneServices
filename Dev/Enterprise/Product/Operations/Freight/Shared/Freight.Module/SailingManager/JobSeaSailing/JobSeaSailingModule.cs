using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobSeaSailingModule : JobSailingModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobSeaSailing; }
		}

		protected override ControllerID ControllerID => ControllerIDs.JobSeaSailing;

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobSeaSailingFilterControl(GridCollection, (JobSeaSailingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobSeaSailingFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SailingSchedule; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (FreightDataRegistry.Instance.EnableScheduleFeedService.Value)
			{
				result.Add(new ZMenuItem("-"));
				result.Add(new ZMenuItem(ResString.GetMultilingualString("SeaSailingsModule.Actions.ImportGlobalSchedules",
					"Import Global Schedules"),
					ShowImportGlobalSchedulesModule));
			}

			return result.ToArray();
		}

		void ShowImportGlobalSchedulesModule(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OnlineSailingSchedules);
			controller.ShowNewForm();
		}
	}
}
