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
	public class StmUpgradeController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.StmUpgrade; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmUpgrade); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			StmUpgradeForm upgradeForm = new StmUpgradeForm((StmUpgradeCollectionContainer)businessEntity);
			return upgradeForm;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new StmUpgradeCollectionContainer(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.Upgrades; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
