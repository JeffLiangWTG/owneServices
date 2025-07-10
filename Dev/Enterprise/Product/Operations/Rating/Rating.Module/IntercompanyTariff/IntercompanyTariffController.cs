using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class IntercompanyTariffController : RatingController<IntercompanyTariff>
	{
		#region Standard Controller Overrides

		public override ControllerID ID => ControllerIDs.IntercompanyTariffs;

		public override ModuleIdentifier ModuleID => ModuleIDs.IntercompanyTariffs;

		protected override IZForm GetForm(IBusiness businessEntity) =>
			new IntercompanyTariffForm((IntercompanyTariff)businessEntity);

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.IntercompanyTariffsView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.IntercompanyTariffsNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.IntercompanyTariffsEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.IntercompanyTariffsDelete;

		protected override CRMSecurityProvider<IntercompanyTariff> SecurityProvider => null;

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity) => Env.Security.IntercompanyTariffsCopy;

		#endregion

		#region Showing Form

		public override IZForm ShowViewForm(BusinessObject sourceEntity) =>
			ShowFormAfterAdditionalCheck(sourceEntity, CheckPointForView.Code, base.ShowViewForm);

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var serviceProvider = (sourceEntity as IntercompanyTariff)?.Header;

			if (IsOrgProxyAccessible(serviceProvider, CheckPointForEdit.Code))
			{
				return base.ShowEditForm(sourceEntity);
			}

			return ShowFormAfterAdditionalCheck(sourceEntity, CheckPointForView.Code, base.ShowViewForm);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity) =>
			ShowFormAfterAdditionalCheck(sourceEntity, CheckPointForDelete.Code, base.ShowDeleteForm);

		[SuppressMessage("Microsoft.Maintainability", "CA1502")]
		IZForm ShowFormAfterAdditionalCheck(BusinessObject sourceEntity, string securityRight, Func<BusinessObject, IZForm> showForm)
		{
			var serviceProvider = (sourceEntity as IntercompanyTariff)?.Header;
			if (!IsOrgProxyAccessible(serviceProvider, securityRight))
			{
				var caption = ResString.GetMultilingualString("C3BA2B87-6FD4-4467-A7C4-D141765A56DB", "Access Denied: Intercompany Tariffs with Service Provider");
				var message = ResString.GetMultilingualString("53C0B7E0-009B-47F1-A5A5-495491E14CA1", @"You do not have the appropriate security rights to action on the Intercompany Tariffs with Service Provider of '{0}'.

If you require access to this function, ask your administrator to change either your Staff or Group Security Rights to allow access to:
Manage -> Tariffs & Rates -> Intercompany Tariffs -> Action of View/Edit/Delete", serviceProvider.OH_Code);

				Globals.Message.ShowError(message, caption);

				return LastShownForm;
			}

			return showForm(sourceEntity);
		}

		bool IsOrgProxyAccessible(OrgHeader serviceProvider, string securityRight)
		{
			var serviceProviderPK = serviceProvider?.PK ?? ZGuid.Empty;
			return IntercompanyTariffSecurity.HasCurrentUserAccessToOrgProxy(Factory, serviceProviderPK, securityRight);
		}

		#endregion
	}
}
