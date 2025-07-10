using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(GlbExternalPasswordAuthorisation))]
	public abstract class GlbExternalPasswordAuthorisationTest<T, TPassword> : EnterpriseBusinessObjectTestCase
			where T : GlbExternalPasswordAuthorisation
			where TPassword : GlbExternalPassword
	{
		#region Implementation

		#region GlbExternalPasswordAuthorisation

		protected T GlbExternalPasswordAuthorisation
		{
			get
			{
				if (glbExternalPasswordAuthorisation == null)
				{
					glbExternalPasswordAuthorisation = CreateNewGlbExternalPasswordAuthorisation(Factory);
				}
				return glbExternalPasswordAuthorisation;
			}
		}
		T glbExternalPasswordAuthorisation;

		protected virtual T CreateNewGlbExternalPasswordAuthorisation(BusinessObjectFactory factory)
		{
			var result = factory.New<T>();
			result.GEA_GP = ExternalPassword.PK;
			return result;
		}

		#endregion

		#region GlbExternalPassword

		protected TPassword ExternalPassword
		{
			get
			{
				if (externalPassword == null)
				{
					externalPassword = Factory.New<TPassword>();
					externalPassword.GP_MailBoxID = "Test1";
					externalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
					externalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				}
				return externalPassword;
			}
		}
		TPassword externalPassword;

		#endregion

		#region GlbStaff

		protected GlbStaff Staff
		{
			get
			{
				if (glbStaff == null)
				{
					glbStaff = Factory.New<GlbStaff>();
					glbStaff.GS_Code = "ZAC";
				}
				return glbStaff;
			}
		}
		GlbStaff glbStaff;

		#endregion

		protected override BusinessObject GetNewBusinessObject() => GlbExternalPasswordAuthorisation;

		#endregion
	}
}
