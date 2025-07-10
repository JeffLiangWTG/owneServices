using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class ClientRatesController : RatingController<ClientRate>
	{
		#region Standard Controller Overrides

		public override ControllerID ID => ControllerIDs.ClientRates;

		public override ModuleIdentifier ModuleID => ModuleIDs.ClientRates;

		protected override IZForm GetForm(IBusiness businessEntity) =>
			new ActiveRatesForm((ClientRate)businessEntity);

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ClientRatesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ClientRatesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ClientRatesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ClientRatesDelete;

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			var baseResult = base.GetCheckPointForCopy(inMemorySourceEntity);
			return baseResult != null && !baseResult.IsAllowed ? baseResult : Env.Security.ClientRatesCopy;
		}

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalClientRatesView : base.GetCheckPointForView(bizObject);

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalClientRatesNew : base.GetCheckPointForNew(bizObject);

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalClientRatesEdit : base.GetCheckPointForEdit(bizObject);

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject) =>
			IsGlobal(bizObject) ? Env.Security.GlobalClientRatesDelete : base.GetCheckPointForDelete(bizObject);

		protected override CRMSecurityProvider<ClientRate> SecurityProvider =>
			securityProvider ?? (securityProvider = new ClientRatesCRMSecurityProvider());
		ClientRatesCRMSecurityProvider securityProvider;

		#endregion
	}
}

