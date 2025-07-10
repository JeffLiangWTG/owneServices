using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class CommunicationChannelValidation : GlbExternalPasswordValidation
{
	public CommunicationChannelValidation(GlbExternalPassword parent) : base(parent)
	{
	}

	protected override void CheckGP_MailBoxID()
	{
		base.CheckGP_MailBoxID();

		var parent = Parent;
		if (!parent.GP_MailBoxID.IsEmpty)
		{
			EmailAddressValidation.ValidateEmailAddress(parent.GP_MailBoxIDInfo);
		}
	}
}
