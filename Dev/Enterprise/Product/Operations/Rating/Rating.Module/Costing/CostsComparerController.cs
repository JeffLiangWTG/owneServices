using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class CostsComparerController : ZSingletonController
	{
		public CostsComparerController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CostsComparerForm();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CostingRatesView; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CostsComparer; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Costing); }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}

