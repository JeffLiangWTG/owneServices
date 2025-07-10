using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ContainerLoadPlanModule : GlowOnlyModule, INotifications
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ContainerLoadPlan; }
		}

		protected override ControllerID ControllerID => ControllerIDs.ContainerLoadPlan;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ContainerLoadPlan);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ContainerLoadPlanFilterControl(GridCollection, (ContainerLoadPlanFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ContainerLoadPlanCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ContainerLoadPlanFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var items = new List<MenuItem>(base.GetNewActionMenuItems());
			items.Add(new ZMenuItem(ResString.GetMultilingualString("b346f6a0-38fe-4bce-a3fb-fcec6b5ba8b0", "Open in Web Portal"), new EventHandler(HandleOpenInWebPortal)));
			return items.ToArray();
		}

		public void Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		public void HandleOpenInWebPortal(object sender, EventArgs e)
		{
			foreach (CFSContainerLoadList header in Grid.SelectedElements)
			{
				GlowLinksHelper.OpenEnityInGlow(this, "goto/ContainerLoadPlan", header);
			}
		}
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.ContainerLoadPlan;

		public override string WorkflowType => WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode;

		public override bool SupportsWorkflow => true;
	}
}
