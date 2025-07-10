using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public abstract class ConsolidatedDeclarationController : ZController, ZControllerInternals
	{
		public override ControllerID ID => ControllerIDs.Customs.ConsolidatedDeclaration;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.ConsolidatedDeclaration;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsConsolidatedDeclaration;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsConsolidatedDeclarationNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsConsolidatedDeclarationEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsConsolidatedDeclarationDelete;

		protected override string GetIDForFormCache(IBusiness businessEntity) => businessEntity.IsInDatabase ? ID.ToString() : $"New{ID}";

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity.IsInDatabase)
			{
				return CreateEditForm(businessEntity);
			}
			else
			{
				var jobDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as ZFilterGridModule;
				jobDeclarationModule.DoNotShowRecentItems = true;
				jobDeclarationModule.DoNotCheckOrSaveChanges = true;
				jobDeclarationModule.FilterBusinessObject.ParentModuleID = ModuleID;
				return new NewOrAttachConsolidatedDeclarationForm((ConsolidatedDeclaration)businessEntity, jobDeclarationModule, false);
			}
		}

		protected override ODisplayMode GetDisplayModeForNew() => ODisplayMode.Undefined;

		protected virtual IZForm CreateEditForm(IBusiness businessEntity) => new ZChildForm(businessEntity);

		IZForm ZControllerInternals.GetForm(IBusiness businessEntity) => CreateEditForm(businessEntity);
	}
}
