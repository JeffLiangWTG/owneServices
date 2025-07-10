using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(HarmonizedCode))]
	sealed class HarmonizedCodeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToString()
		{
			var harmonizedCode = new HarmonizedCode();
			AssertEquals(string.Empty, harmonizedCode.ToString());

			harmonizedCode.Country = new Country(Factory, new CommonContext(Factory).Countries)
			{
				Code = "BR"
			};

			AssertEquals(string.Empty, harmonizedCode.ToString());

			harmonizedCode.Code = "1234";

			AssertEquals("NCM (BR): 1234", harmonizedCode.ToString());

			harmonizedCode.Country.Code = "CN";
			AssertEquals("HS (CN): 1234", harmonizedCode.ToString());

			harmonizedCode.Country.Code = string.Empty;
			AssertEquals("1234", harmonizedCode.ToString());
		}
	}
}
