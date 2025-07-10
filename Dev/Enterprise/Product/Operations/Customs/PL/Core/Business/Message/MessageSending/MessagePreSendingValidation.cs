using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class MessagePreSendingValidation(JobDeclaration declaration) : MessagePreSendingValidationBase
{
	readonly JobDeclaration declaration = Argument.NotNull(declaration, nameof(declaration));

	protected override void ValidateCore(INotifications notifications)
	{
		base.ValidateCore(notifications);

		if (declaration.IsImport && !CurrentUserHavePublishedPhoneNumber())
		{
			notifications.AddError(Res.GetString("e0a7c55c-9406-441e-8120-6f727fd76832|MissingStaffPhoneNumber", "The current user does not have any Phone number marked for publication in Staff data."));
		}

		if (declaration.CustomsEntryInstructions.Count == 0
			|| declaration.Invoices.Count == 0
			|| declaration.InvoiceLines.Count == 0)
		{
			notifications.AddError(Res.GetString("65a6ed5c-1530-4189-a61a-7a01601b76a0|MissingData", "Declaration needs at least one of each 'Entry Instruction', 'Invoice Header', 'Invoice Line'."));
		}

		if (declaration.CustomsEntryHeaders.Count == 0)
		{
			notifications.AddError(Res.GetString("4959afc3-a544-4d03-8fdc-ecd448ce3808|NotMerged", "Declaration lacks entries."));
		}
	}

	static bool CurrentUserHavePublishedPhoneNumber()
	{
		var user = GlbStaff.CurrentUser;
		return (!user.GS_WorkPhone.IsEmpty && user.GS_PublishWorkPhone)
				|| (!user.GS_HomePhone.IsEmpty && user.GS_PublishHomePhone)
				|| (!user.GS_MobilePhone.IsEmpty && user.GS_PublishMobilePhone);
	}
}
