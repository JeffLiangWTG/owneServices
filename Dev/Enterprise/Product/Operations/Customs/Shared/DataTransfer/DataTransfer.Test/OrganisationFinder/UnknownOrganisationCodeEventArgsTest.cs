using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class UnknownOrganisationCodeEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			var arg = new UnknownOrganisationCodeEventArgs("1", "2", "3", "4", "5", "6", "7", "8", "9");
			AssertEquals("1", arg.Code);
			AssertEquals("2", arg.Name);
			AssertEquals("3", arg.RegistrationNo);
			AssertEquals("4", arg.Phone);
			AssertEquals("5", arg.Street);
			AssertEquals("6", arg.Street2);
			AssertEquals("7", arg.City);
			AssertEquals("8", arg.PostCode);
			AssertEquals("9", arg.Country);
		}
	}
}
