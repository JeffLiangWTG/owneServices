using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class GlbExternalPassword_NODValidation : GlbExternalPasswordWithCertificateValidation
{
	public GlbExternalPassword_NODValidation(GlbExternalPasswordWithCertificate parent) : base(parent)
	{
	}

	protected override void AddGP_ExpiryDateExpiredNotification()
	{
		Parent.GP_ExpiryDateInfo.AddError(Res.GetString("A3386A49-B468-A5B5-4526-B22F5F33C43B", "The certificate has expired."));
	}
}
