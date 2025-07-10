using System;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Module
{
	public abstract class HandlingUnitController : ZController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(PkgHandlingUnit);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new NotSupportedException("Can only use this module to access Documents menu.");
	}
}
