using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AdministrationPanelController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.AdministrationPanel; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AdministrationPanel; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AdministrationPanelManager); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AdministrationPanelForm((AdministrationPanelManager)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.MdmAdministrationPanel;

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new AdministrationPanelManager(Factory);
		}
	}
}
