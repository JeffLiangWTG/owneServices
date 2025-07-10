using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class GlbExternalPassword_PL : GlbExternalPasswordWithCertificate
{
	public GlbExternalPassword_PL(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
		Logs.AutoCreatedLogDefaultSL_Reference = PasswordTypesList.Codes.PLB;
	}

	protected override ZString HumanReadableNameCore => Res.GetString("6DA036CC-45AD-4BBF-B5D1-4075CA954A4F", "PLB Certificate");

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.PLB;
		GP_PasswordStatus = ZString.Empty;
	}

	[ResourceStringData("PLGlbExternalPassword_PL|GP_MailBoxID", Caption = "PUESC login")]
	public override ZString GP_MailBoxID
	{
		get => base.GP_MailBoxID;
		set
		{
			var oldValue = GP_MailBoxID;
			base.GP_MailBoxID = value;

			if (!IsCopying && oldValue != value && value.IsEmpty && !CurrentDecryptedPassword.IsEmpty)
			{
				CurrentDecryptedPassword = ZString.Empty;
			}
		}
	}

	protected bool EmptyGP_MailBoxID => GP_MailBoxID.IsEmpty;

	[ReadOnlyMember(nameof(EmptyGP_MailBoxID))]
	[ResourceStringData("PLGlbExternalPassword_PL|CurrentDecryptedPassword", Caption = "Password")]
	public override ZString CurrentDecryptedPassword
	{
		get => base.CurrentDecryptedPassword;
		set => base.CurrentDecryptedPassword = value;
	}

	public new GlbExternalPasswordValidation_PL Validation => (GlbExternalPasswordValidation_PL)GetNewValidation();

	protected override GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPasswordValidation_PL(this);

	protected override AutologState AutoLoggingState => AutologState.AutoLogged;

	[ReadOnly(true)]
	public override ZString GP_PasswordStatus
	{
		get => base.GP_PasswordStatus;
		set => base.GP_PasswordStatus = value;
	}
}
