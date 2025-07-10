using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class CompanyTariffsController : RatingController<CompanyTariff>
	{
		#region Standard Controller Overrides

		public override ControllerID ID => ControllerIDs.GlobalRates;

		public override ModuleIdentifier ModuleID => ModuleIDs.GlobalRates;

		protected override IZForm GetForm(IBusiness businessEntity) =>
			new GlobalTariffsForm((CompanyTariff)businessEntity);

		protected override IBusiness GetNewBusinessEntityInLocalFactory() =>
			isGlobalRate ? Factory.New<GlobalTariff>() : base.GetNewBusinessEntityInLocalFactory();

		#endregion

		#region Delete Form

		/// <summary>
		/// Prevent deleting of Level 1 company tariff when other company tariff levels exist.
		/// </summary>
		/// <param name="sourceEntity">Rating Header</param>
		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (sourceEntity.CanDelete)
			{
				base.ShowDeleteForm(sourceEntity);
			}
			else
			{
				string caption = Res.GetString("8a23736c-81a5-4a31-8647-153f9c644e85", "Delete");
				Globals.Message.ShowError(sourceEntity.ReasonForNotAbleToDelete, caption);
			}
			return LastShownForm;
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CompanyTariffRatesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CompanyTariffRatesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CompanyTariffRatesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CompanyTariffRatesDelete;

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalTariffRatesView : base.GetCheckPointForView(bizObject);

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalTariffRatesNew : base.GetCheckPointForNew(bizObject);

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalTariffRatesEdit : base.GetCheckPointForEdit(bizObject);

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalTariffRatesDelete : base.GetCheckPointForDelete(bizObject);

		protected override CRMSecurityProvider<CompanyTariff> SecurityProvider => null;

		#endregion
	}
}

