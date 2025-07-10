using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.LandedCosting.Module
{
	public abstract class LandedCostingControllerBase : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LandedCostingControllerBase()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("Only for PlugIn");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsDeclarationLandedCost;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsDeclarationLandedCost;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsDeclarationLandedCost;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsDeclarationLandedCost;

		public override Type TypeOfTopLevelBusinessObject => typeof(LandedCostHeader);
	}
}
