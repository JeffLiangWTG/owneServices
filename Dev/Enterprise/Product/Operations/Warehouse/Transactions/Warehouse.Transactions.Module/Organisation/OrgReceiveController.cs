using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrgReceiveController : MasterFiles.Module.OrganisationController
	{
		public OrgReceiveController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsConfigOrgReceive; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Warehouse.Transactions.Module.Res.GetData("PlugInTabPage|WhsConfigOrgReceive", "Receive", "The Warehouse Receive tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrganisationFormWhsReceivePlugin((OrgHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgWarehouseModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}
	}
}
