using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobSupplierBookingModule : GlowOnlyModule, INotifications
	{
		public override ModuleIdentifier ID => ModuleIDs.SupplierBooking;

		protected override ControllerID ControllerID => ControllerIDs.SupplierBooking;

		protected override IFilterControl GetNewFilterControl() {
			return new JobSupplierBookingFilterControl(GridCollection, (JobSupplierBookingFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobSupplierBookingCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobSupplierBookingFilterBusinessObject();

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var items = new List<MenuItem>(base.GetNewActionMenuItems());
			items.Add(new ZMenuItem(ResString.GetMultilingualString("70a445e6-9098-43ee-93b0-08390b1ae479", "Open in Web Portal"), new EventHandler(HandleOpenInWebPortal)));
			return items.ToArray();
		}

		public void HandleOpenInWebPortal(object sender, EventArgs e)
		{
			foreach (JobSupplierBooking booking in Grid.SelectedElements)
			{
				GlowLinksHelper.OpenEnityInGlow(this, "goto/JobSupplierBooking", booking);
			}
		}

		public void Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.SupplierBooking;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.JobSupplierBookingWorkflowDescriptorCode;
	}
}
