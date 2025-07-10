using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthority3rdPartyValidation : PortAuthorityValidation
	{
		public PortAuthority3rdPartyValidation(PortAuthority parent)
			: base(parent) { }

		protected override void CheckSenderId()
		{
			base.CheckSenderId();
			MandatoryValidation.CheckEntered(Parent.SenderIdInfo);
		}

		protected override void CheckRecipientId()
		{
			base.CheckRecipientId();
			MandatoryValidation.CheckEntered(Parent.RecipientIdInfo);
		}

		protected override void CheckEmailAddress()
		{
			base.CheckEmailAddress();
			MandatoryValidation.CheckEntered(Parent.EmailAddressInfo);
			EmailAddressValidation.ValidateEmailAddress(Parent.EmailAddressInfo);
		}

		protected override void CheckVersion()
		{
			base.CheckVersion();
			MandatoryValidation.CheckEntered(Parent.VersionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.VersionInfo, Parent.Lookups.Version_List);
		}
	}
}


