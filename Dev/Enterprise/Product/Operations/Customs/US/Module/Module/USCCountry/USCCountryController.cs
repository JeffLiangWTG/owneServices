using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCCountryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.USCCountry;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => typeof(USCCountry);

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity) => null;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("Will not be implemented");

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.None;
	}
}
