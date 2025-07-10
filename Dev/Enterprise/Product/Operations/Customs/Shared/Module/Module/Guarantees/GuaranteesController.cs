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
	public class GuaranteesController : ZController
	{
		public GuaranteesController()
		{
		}
		public override ControllerID ID => ControllerIDs.Customs.Guarantees;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Guarantees;

		public override Type TypeOfTopLevelBusinessObject => typeof(BaseCusGuaranteeHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GuaranteesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GuaranteesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GuaranteesModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GuaranteesDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new GuaranteeForm((BaseCusGuaranteeHeader)businessEntity);
	}
}
