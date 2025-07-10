using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public abstract class NumberController : ZSingletonController
	{
		public override Type TypeOfTopLevelBusinessObject => null;

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => null;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new NotImplementedException("Needs to override this");

		protected override ZArchitecture.Core.ODisplayMode GetDisplayModeForNew() => ZArchitecture.Core.ODisplayMode.Browse;
	}
}
