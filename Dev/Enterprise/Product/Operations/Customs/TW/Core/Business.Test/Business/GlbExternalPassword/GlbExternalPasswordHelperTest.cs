using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GlbExternalPasswordHelperTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestIsTesting()
		{
			var factory = new BusinessObjectFactory();
			var password = factory.New<GlbExternalPassword>();
			password.GP_PasswordType = PasswordTypesList.Codes.TVA;
			password.GP_UserID = "TEST";
			NUnit.Framework.Assert.That(password.IsTesting(), NUnit.Framework.Is.True);
			password.GP_UserID = "test";
			NUnit.Framework.Assert.That(password.IsTesting(), NUnit.Framework.Is.True);
			password.GP_UserID = "TESTA";
			NUnit.Framework.Assert.That(!password.IsTesting(), NUnit.Framework.Is.True);
			password.GP_UserID = "TEST";
			password.GP_PasswordType = PasswordTypesList.Codes.AUB;
			NUnit.Framework.Assert.That(!password.IsTesting(), NUnit.Framework.Is.True);
			password.GP_UserID = "TEST";
			password.GP_PasswordType = PasswordTypesList.Codes.UVC;
			NUnit.Framework.Assert.That(password.IsTesting(), NUnit.Framework.Is.True);
			password.GP_UserID = "TES.T";
			password.GP_PasswordType = PasswordTypesList.Codes.UVC;
			NUnit.Framework.Assert.That(password.IsTesting(), NUnit.Framework.Is.True);
			password.GP_UserID = "TES.T";
			password.GP_PasswordType = PasswordTypesList.Codes.TVA;
			NUnit.Framework.Assert.That(!password.IsTesting(), NUnit.Framework.Is.True);
		}
	}
}
