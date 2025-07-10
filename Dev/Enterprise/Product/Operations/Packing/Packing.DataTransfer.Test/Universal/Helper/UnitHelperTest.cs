using NUnit.Framework;
using static Enterprise.Packing.DataTransfer.Universal.UnitHelper;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	sealed class UnitHelperTest : TestCase
	{
		public void TestGetValueFromValueAndUnitIfValid()
		{
			AssertEquals("Should return the value if the input is valid", 3m, GetValueFromValueAndUnit("3cm"));
			AssertEquals("Should return the value if the input is valid", 10.05m, GetValueFromValueAndUnit("10.05km"));
		}

		public void TestGetValueFromValueAndUnitIfInvalid()
		{
			AssertEquals("Should return 0 if the input is invalid", 0m, GetValueFromValueAndUnit("kg"));
			AssertEquals("Should return 0 if the input is invalid", 0m, GetValueFromValueAndUnit("127.0.0.1cm"));
			//AssertEquals("Should return 0 if the input is invalid", 0m, GetValueFromValueAndUnit("-300cc"));
			AssertEquals("Should return 0 if the input is invalid", 0m, GetValueFromValueAndUnit(string.Empty));
		}

		public void TestGetUnitFromValueAndUnitIfValid()
		{
			AssertEquals("Should return the unit if the input is valid", "cm", GetUnitFromValueAndUnit("3cm"));
			AssertEquals("Should return the unit if the input is valid", "km", GetUnitFromValueAndUnit("10.05km"));
		}

		public void TestGetUnitFromValueAndUnitIfInvalid()
		{
			AssertEquals("Should return empty string if the input is invalid", string.Empty, GetUnitFromValueAndUnit("40"));
			//AssertEquals("Should return empty string if the input is invalid", string.Empty, GetUnitFromValueAndUnit("127.0.0.1cm"));
			//AssertEquals("Should return empty string if the input is invalid", string.Empty, GetUnitFromValueAndUnit("-300cc"));
			AssertEquals("Should return empty string if the input is invalid", string.Empty, GetUnitFromValueAndUnit(string.Empty));
		}
	}
}
