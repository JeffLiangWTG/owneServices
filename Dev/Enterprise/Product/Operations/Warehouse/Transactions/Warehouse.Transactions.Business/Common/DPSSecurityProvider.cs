using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class DPSSecurityProvider : IDPSSecurityProvider
	{
		public DPSSecurityProvider(INotifications notifications)
		{
			NotificationSubscriber = Argument.NotNull(notifications, nameof(notifications));
		}

		INotifications NotificationSubscriber { get; }

		public bool ValidateDPS(WhsDocket docket)
		{
			var isRestricted = WarehouseDataRegistry.Instance.ValidateDPSRestrictionsOnFinalisation.Value && docket.IsDPSMovementRestricted();
			var e = isRestricted ? OKToBypassSecurity(docket) : null;
			var authorised = !isRestricted || e.IsAllowedToProceed;

			return authorised;
		}

		SecurityLoginEventArgs OKToBypassSecurity(WhsDocket docket)
		{
			var msg = GetSecurityMessage(docket);
			var e = ShowLogin(msg);
			while (!e.IsAllowedToProceed && !e.WasCancelled)
			{
				NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.NoSecurityRights, Res.GetString("FE931AA1-7B75-4F89-964C-F2BF608C2781", "The login name and password were incorrect or the user does not have sufficient security rights to override a matched DPS")));
				e = ShowLogin("");
			}

			return e;
		}

		SecurityLoginEventArgs ShowLogin(ZString msg)
		{
			var e = msg.IsEmpty ? new SecurityLoginEventArgsWithCustomMessageBox((NoResString)string.Empty, (NoResString)string.Empty, (s) => s.DpsAllowUpdateToMatched, new CustomMessageBoxCallback((a, b) => ZDialogResult.Yes)) : new SecurityLoginEventArgs((NoResString)msg, (NoResString)string.Empty, (s) => s.DpsAllowUpdateToMatched);
			e.HideApprovalRequestButton = true;
			var provider = ObjectFactory.New<ISecurityLoginProvider>(Res.GetString("99C8E3E4-C7CA-477A-845D-2211D3DEF3C9", "Denied Party Screening Matched"));
			provider.ShowDocumentLoginForDocuments(e);
			return e;
		}

		ZString GetSecurityMessage(WhsDocket docket)
		{
			var statusDescription = docket.Lookups.ScreeningStatusesList.GetDescriptionFromCode(docket.WD_ScreeningStatus) ?? docket.WD_ScreeningStatus;
			return Res.GetString("99D98288-3824-4B38-939E-AFD1EE5264EE", @"This {0} cannot be finalized due to a Screening Status of '{1}'.

Contact the supervisor for further instructions or escalate to a user who has security rights to override this restriction.

Do you wish to override and allow finalization?", docket.Description, statusDescription);
		}
	}
}

//Tests at Enterprise.Warehouse.Transactions.GUI.Testing.DPSSecurityProviderTest
