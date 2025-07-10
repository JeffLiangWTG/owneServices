using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPasswordAuthorisationValidation))]
	public abstract class GlbExternalPasswordAuthorisationValidationTest<T, TValidation> : BusinessObjectValidationTestCase
			where T : GlbExternalPasswordAuthorisation
			where TValidation : GlbExternalPasswordAuthorisationValidation
	{
		#region Implementation

		#region GlbExternalPasswordAuthorisation

		protected T GlbExternalPasswordAuthorisation
		{
			get
			{
				if (glbExternalPasswordAuthorisation == null)
				{
					glbExternalPasswordAuthorisation = CreateNewGlbExternalPasswordAuthorisation();
				}
				return glbExternalPasswordAuthorisation;
			}
		}
		T glbExternalPasswordAuthorisation;

		protected virtual T CreateNewGlbExternalPasswordAuthorisation()
		{
			var result = Factory.New<T>();
			result.GEA_GP = GlbExternalPassword.PK;
			return result;
		}

		#endregion

		#region GlbExternalPassword

		protected GlbExternalPassword GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = Factory.New<GlbExternalPassword>();
					glbExternalPassword.GP_MailBoxID = "TestCert";
					glbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
					glbExternalPassword.CurrentDecryptedPassword = X509Certificate2TestHelper.ValidPassword;
				}
				return glbExternalPassword;
			}
		}
		GlbExternalPassword glbExternalPassword;

		#endregion
		#endregion
	}
}
