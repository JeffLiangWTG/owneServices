//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgDocumentCopyRecipientValidation
//
//    This class should be used for overriding validation in AutoOrgDocumentCopyRecipientValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDocumentCopyRecipientValidation : AutoOrgDocumentCopyRecipientValidation
	{
		public OrgDocumentCopyRecipientValidation(AutoOrgDocumentCopyRecipient parent) : base(parent)
		{
		}

		protected override void CheckODR_EmailAddress()
		{
			base.CheckODR_EmailAddress();
			MandatoryValidation.CheckEntered(Parent.ODR_EmailAddressInfo);
			if (!EmailAddressValidation.IsEmailAddressValid(Parent.ODR_EmailAddress))
			{
				Parent.ODR_EmailAddressInfo.AddError(Res.GetString("1A4C1381-9672-4DA1-AB9D-D176CDD64C6D", "The email address is invalid."));
			}
		}
	}
}