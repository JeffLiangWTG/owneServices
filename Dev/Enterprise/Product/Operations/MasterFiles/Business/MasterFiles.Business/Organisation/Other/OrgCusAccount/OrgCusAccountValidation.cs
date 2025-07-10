//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCusAccountValidation
//
//    This class should be used for overriding validation in AutoOrgCusAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusAccountValidation : AutoOrgCusAccountValidation
	{
		public OrgCusAccountValidation(AutoOrgCusAccount parent)
			: base(parent)
		{
		}

		new OrgCusAccount Parent => (OrgCusAccount)base.Parent;

		protected override void CheckCZ_Account()
		{
			base.CheckCZ_Account();

			if (IsCZ_AccountMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.CZ_AccountInfo);
			}

			if (Parent.Lookups.AccountList.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CZ_AccountInfo);
			}
		}

		protected virtual bool IsCZ_AccountMandatory => true;

		protected override void CheckCZ_Code()
		{
			base.CheckCZ_Code();
			if (IsCZ_CodeMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.CZ_CodeInfo);
			}

			if (Parent.Lookups.CodeList.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CZ_CodeInfo);
			}
		}

		protected virtual bool IsCZ_CodeMandatory => true;

		public void ValidateDecryptedPassword() => ValidateCalculatedProperty(Parent.DecryptedPasswordInfo);

		protected virtual void CheckDecryptedPassword()
		{
			if (IsCZ_PasswordMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.DecryptedPasswordInfo);
			}
		}

		protected virtual bool IsCZ_PasswordMandatory => true;
	}
}
