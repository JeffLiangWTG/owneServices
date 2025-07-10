using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobMawbModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobMawb; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobMawb);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			MawbFilterControl = new JobMawbFilterControl(GridCollection, (JobMawbFilterBusinessObject)FilterBusinessObject);
			return MawbFilterControl;
		}
		JobMawbFilterControl MawbFilterControl;

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JobMawbCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobMawbFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.JobMAWB; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] baseMenu = base.GetNewStandardMenuItems();
			NewMenuItem.Text = Res.GetString("Forwarding|JobMawbModule|AddNewAirWaybillNumbers", "Add New Air Waybill Numbers");
			return baseMenu;
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewActionMenuItems());
			if (MawbFilterControl != null)
			{
				menu.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.MAWB.Action.Allocate", "Allocate to..."), new EventHandler(MawbFilterControl.HandleAllocateClick)));
			}
			return menu.ToArray();
		}
	}
}
