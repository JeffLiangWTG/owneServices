using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusAuthorisationsController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CusAuthorisations;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CusAuthorisations;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusAuthorisationHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AuthorisationsView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.AuthorisationsNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AuthorisationsEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AuthorisationsDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new CusAuthorisationForm((CusAuthorisationHeader)businessEntity);
	}
}
