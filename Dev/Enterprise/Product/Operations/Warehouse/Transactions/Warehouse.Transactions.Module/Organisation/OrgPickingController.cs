using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrgPickingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ControllerID

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsConfigOrgPicking; }
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Organisation; }
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgHeader); }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region PlugIn

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|WhsConfigOrgPicking", "Picking", "The Warehouse Picking tab."); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrganisationFormWhsPickingPlugIn((OrgHeader)businessEntity);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}

		#endregion
	}
}
