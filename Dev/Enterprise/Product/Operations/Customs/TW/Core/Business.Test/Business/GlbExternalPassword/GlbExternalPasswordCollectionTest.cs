using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordCollection))]
	sealed class GlbExternalPasswordCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbExternalPasswordCollection(Factory.NewWithValidTestData<GlbStaff>());
		}

		[ExpectNoExceptions]
		public void TestFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company1.GC_Code = "TC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "TB1";
			branch1.GB_IsActive = true;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company2.GC_Code = "TC2";
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "TB2";
			branch2.GB_IsActive = true;
			Factory.Save();
			using (DisposableEnvironment.ForCompany(company1.GC_Code))
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_Code = "TS1";
				var tvaPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
				tvaPassword.GP_GC = company1.PK;
				tvaPassword.GP_GS = staff1.PK;
				tvaPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
				var uvcPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
				uvcPassword.GP_GC = company1.PK;
				uvcPassword.GP_GS = staff1.PK;
				uvcPassword.GP_PasswordType = PasswordTypesList.Codes.UVC;
				var itbPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
				itbPassword.GP_GC = company1.PK;
				itbPassword.GP_GS = staff1.PK;
				itbPassword.GP_PasswordType = PasswordTypesList.Codes.ITB;
				var tvaPasswordCo2 = Factory.NewWithValidTestData<GlbExternalPassword>();
				tvaPasswordCo2.GP_GC = company2.PK;
				tvaPasswordCo2.GP_GS = staff1.PK;
				tvaPasswordCo2.GP_PasswordType = PasswordTypesList.Codes.TVA;
				Factory.Save();
				var collection = new GlbExternalPasswordCollection(staff1);
				collection.Load();
				NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.Some.EqualTo(tvaPassword), "TVA");
				NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.Some.EqualTo(uvcPassword), "UVC");
				NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.None.EqualTo(itbPassword), "ITB");
				NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.None.EqualTo(tvaPasswordCo2), "TVA other company");
				NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(2), "collection.Count");
			}
		}

		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChild()
		{
			var collection = new GlbExternalPasswordCollection(Factory.New<GlbStaff>());
			var password1 = collection.AddNew();
			password1.CurrentDecryptedCertificatePassphrase = "invalid";
			password1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			NUnit.Framework.Assert.That(password1.CertificateStatus, NUnit.Framework.Is.EqualTo(GlbExternalPasswordWithCertificate.CertificateInvalid).Using(CustomComparers.TypeComparison));
			var password2 = collection.AddNew();
			NUnit.Framework.Assert.That(password2.GP_Certificate, NUnit.Framework.Is.EqualTo(ZBlob.Empty));
			NUnit.Framework.Assert.That(password2.CurrentDecryptedCertificatePassphrase, NUnit.Framework.Is.EqualTo(ZString.Empty));
			collection.Remove(password2);
			password1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			NUnit.Framework.Assert.That(password1.CertificateStatus, NUnit.Framework.Is.EqualTo(GlbExternalPasswordWithCertificate.CertificateLoaded).Using(CustomComparers.TypeComparison));
			password2 = collection.AddNew();
			NUnit.Framework.Assert.That(password2.GP_Certificate.Equals(X509Certificate2TestHelper.ValidCertificate), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(password2.CurrentDecryptedCertificatePassphrase, NUnit.Framework.Is.EqualTo(X509Certificate2TestHelper.ValidPassword).Using(CustomComparers.TypeComparison));
		}
	}
}
