using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.MHUB.Testing
{
	sealed class ServerResponseTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			testServerResponse.Add("TestKey", "TestValue");
			Assert("The Key is in the list", testServerResponse.ContainsKey("TestKey"));
			AssertEquals("The key has the correct value", "TestValue", testServerResponse.GetValue("TestKey"));
		}

		public void TestContainsKey()
		{
			testServerResponse.Add("ABC", "123");
			testServerResponse.Add("ASD", "FGH");
			testServerResponse.Add("jhskd", "kh34");
			Assert("The key is in the list", testServerResponse.ContainsKey("ABC"));
			Assert("The lowercase key is in the list", testServerResponse.ContainsKey("jhskd"));
			Assert("The key is not in the list", !testServerResponse.ContainsKey("sadhkkjsad"));
		}

		public void TestToString()
		{
			testServerResponse.Add("ABC", "123");
			testServerResponse.Add("ASD", "FGH");
			testServerResponse.Add("jhskd", "kh34");
			AssertEquals("The ToString is correctly formatted", "ABC = 123 | ASD = FGH | jhskd = kh34 | ", testServerResponse.ToString());
		}

		public void TestGetValue()
		{
			testServerResponse.Add("TestKey", "TestValue");
			AssertEquals("The getvalue result is correct", "TestValue", testServerResponse.GetValue("TestKey"));
			AssertEquals("The getvalue result is empty", ZString.Empty, testServerResponse.GetValue("TestKey3"));
		}

		public void TestTryGetValue()
		{
			testServerResponse.Add("TestKey", "TestValue");
			AssertEquals("The trygetvalue result is correct", "TestValue", testServerResponse.GetValue("TestKey"));
			AssertEquals("The getvalue result is empty", string.Empty, testServerResponse.GetValue("TestKey3"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			testServerResponse = new ServerResponse();
		}
		ServerResponse testServerResponse;
	}
}
