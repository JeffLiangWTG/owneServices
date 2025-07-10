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
	public class ContainerLoadListModule : GlowOnlyModule, INotifications
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ContainerLoadList; }
		}

		protected override ControllerID ControllerID => ControllerIDs.ContainerLoadList;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ContainerLoadList);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ContainerLoadListFilterControl(GridCollection, (ContainerLoadListFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ContainerLoadListCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ContainerLoadListFilterBusinessObject();
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
			foreach (CYContainerLoadList header in Grid.SelectedElements)
			{
				GlowLinksHelper.OpenEnityInGlow(this, "goto/ContainerLoadList", header);
			}
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.ContainerLoadList;

		public override string WorkflowType => WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode;

		public override bool SupportsWorkflow => true;
	}
}
