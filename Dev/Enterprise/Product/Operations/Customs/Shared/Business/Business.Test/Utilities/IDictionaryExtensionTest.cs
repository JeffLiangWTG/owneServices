using System.Collections.Generic;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Extensions.Testing
{
	class IDictionaryExtensionTest : TestCase
	{
		public void TestAddNewKeyOrAccumulateValueAndGetValue()
		{
			IDictionary<string, decimal> keyValuePairs = new Dictionary<string, decimal>();
			AssertEquals(0m, keyValuePairs.GetValue("DTY"));
			AssertEquals(0m, keyValuePairs.GetValue("VAT"));
			keyValuePairs.AddNewKeyOrAccumulateValue("DTY", 1m);
			keyValuePairs.AddNewKeyOrAccumulateValue("VAT", 1m);
			AssertEquals(1m, keyValuePairs.GetValue("DTY"));
			AssertEquals(1m, keyValuePairs.GetValue("VAT"));
			keyValuePairs.AddNewKeyOrAccumulateValue("DTY", 2m);
			keyValuePairs.AddNewKeyOrAccumulateValue("VAT", 3m);
			AssertEquals(3m, keyValuePairs.GetValue("DTY"));
			AssertEquals(4m, keyValuePairs.GetValue("VAT"));
		}
	}
}
