using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefDocOrgCusCodeController : ZController
	{
		public override ControllerID ID => ControllerIDs.RefDocOrgCusCode;

		public override ModuleIdentifier ModuleID => ModuleIDs.RefDocOrgCusCode;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefDocOrgCusCode);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity) => new RefDocOrgCusCodeForm(businessEntity as RefDocOrgCusCode);
	}
}
