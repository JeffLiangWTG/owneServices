using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCACCaseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(USCACCase);

		public override ControllerID ID => ControllerIDs.Customs.US.USCACCase;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USCACCase;

		public override ZArchitecture.GUI.IZForm ShowEditForm(BusinessObject sourceEntity) => base.ShowViewForm(sourceEntity);

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity) => new USCACCaseForm((USCACCase)businessEntity);

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
