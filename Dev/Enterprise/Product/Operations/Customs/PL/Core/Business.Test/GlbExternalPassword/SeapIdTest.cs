using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(SeapId))]
sealed class SeapIdTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<SeapId>
{
	public void TestGP_UserID_Caption()
	{
		var seapId = Factory.New<SeapId>();
		AssertEquals("SEAP ID", DataBoundResourceStrings.GetDataForProperty(seapId.GP_UserIDInfo).Caption);
	}

	public void TestSetDefaultValues()
	{
		var password = Factory.New<SeapId>();

		CombineAssertions(() =>
		{
			AssertEquals("Password type", PasswordTypesList.Codes.PLN, password.GP_PasswordType);
			AssertEquals("Name", "SeapId", password.GP_Name);
		});
	}
}
