using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;
using GlbStaff = Enterprise.MasterFiles.Business.GlbStaff;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class MessagePreSendingValidation(NctsHeader nctsHeader) : MessagePreSendingValidationBase
{
	readonly NctsHeader nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));

	protected override void ValidateCore(INotifications notifications)
	{
		base.ValidateCore(notifications);

		if (HasNoEoriToGenerateLrn)
		{
			notifications.AddError(Res.GetString("PLNCTSMenu|MissingPrincipalOrRepresentative", "EORI number of the Representative or Principal is required for generation of the valid LRN."));
		}

		if (HasErrorInRegistry)
		{
			notifications.AddError(Res.GetString("PLNCTSMenu|MissingEmailAddress", "You have not entered email address for Communication Channel Email. Please enter the Email - either in Credentials tab of Staff record or in Registry Form > Edit System Registry > Customs > Country of Region Specific > Poland > Communication Channel Email."));
		}
	}

	bool HasNoEoriToGenerateLrn => nctsHeader.IsDepartureMovement
									&& EuEoriResolver.GetRegNoWithCountryCode(nctsHeader.MovementHeader.Representative.Organisation).IsEmpty
									&& EuEoriResolver.GetRegNoWithCountryCode(nctsHeader.Principal.Organisation).IsEmpty;

	static bool HasErrorInRegistry => !IsSeapIdInUse && IsCommunicationEmailMissing;

	static bool IsSeapIdInUse =>
		PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.Value.IsSeapID
		&& !(PL.Business.GlbStaffWrapper.Get(GlbStaff.CurrentUser).GetGlbExternalPassword<SeapId>(PasswordTypesList.Codes.PLN, GlbCompany.CurrentCompany.PK)?.GP_UserID.IsEmpty ?? true);

	static bool IsCommunicationEmailMissing =>
		string.IsNullOrEmpty(PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value)
		&& (PL.Business.GlbStaffWrapper.Get(GlbStaff.CurrentUser).GetGlbExternalPassword<CommunicationChannel>(PasswordTypesList.Codes.PLC, GlbCompany.CurrentCompany.PK)?.GP_MailBoxID.IsEmpty ?? true);
}
