using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTradeGroupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.RefCusTradeGroup;

		public override ControllerID ID => ControllerIDs.Customs.Universal.RefCusTradeGroup;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefCusTradeGroup);

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;
	}
}
