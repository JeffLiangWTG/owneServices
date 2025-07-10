using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGAEntityIdentificationCodeAndNumberDetailsTest : TestCaseWithFactory
	{
		public void TestEntityIDEmptyWhenEntityCodeIsEmpty()
		{
			AssertEquals("Entity Code", "DEC", new PGAEntityIdentificationCodeAndNumberDetails("DEC", "888").EntityIdentificationCode);
			AssertEquals("Entity Number", "888", new PGAEntityIdentificationCodeAndNumberDetails("DEC", "888").EntityNumber);
			AssertEquals("Entity Code", "", new PGAEntityIdentificationCodeAndNumberDetails("LAP", "").EntityIdentificationCode);
			AssertEquals("Entity Number", "", new PGAEntityIdentificationCodeAndNumberDetails("LAP", "").EntityNumber);
			AssertEquals("Empty", "333", new PGAEntityIdentificationCodeAndNumberDetails("333", "888").EntityIdentificationCode);
			AssertEquals("Entity Number", "888", new PGAEntityIdentificationCodeAndNumberDetails("333", "888").EntityNumber);
			AssertEquals("Empty", "", new PGAEntityIdentificationCodeAndNumberDetails("333", "").EntityIdentificationCode);
			AssertEquals("Entity Number", "", new PGAEntityIdentificationCodeAndNumberDetails("333", "").EntityNumber);
		}
	}
}
