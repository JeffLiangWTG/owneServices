using Enterprise.MasterFiles.Business.Organisation.Other.OrgCusCode.Validation.IL;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LuhnAlgorithmTest : TestCase
	{
		public void TestSpecialCases()
		{
			Assert("Empty string", !LuhnAlgorithm.IsValidLastDigitChecksum(""));
			Assert("Non-numeric string", !LuhnAlgorithm.IsValidLastDigitChecksum("123ABC"));
		}

		public void TestRealLifeNumbers()
		{
			Assert("Numeric string with valid checksum (real VAT number)", LuhnAlgorithm.IsValidLastDigitChecksum("777777723"));
			Assert("Numeric string with valid checksum (real VAT number)", LuhnAlgorithm.IsValidLastDigitChecksum("777777715"));
			Assert("Numeric string with valid checksum (real VAT number)", LuhnAlgorithm.IsValidLastDigitChecksum("777777749"));
			Assert("Numeric string with valid checksum (real VAT number)", LuhnAlgorithm.IsValidLastDigitChecksum("777777731"));
		}

		public void TestInvalidNumbers()
		{
			Assert("Numeric string with invalid checksum", !LuhnAlgorithm.IsValidLastDigitChecksum("777777724"));
		}

		public void TestBasedOnCalculatedChecksum()
		{
			var partCode = "12345";
			var fullCode = partCode + LuhnAlgorithm.Calculate(partCode).ToString();
			Assert($"Numeric string with valid checksum (odd number of digits) - {fullCode}", LuhnAlgorithm.IsValidLastDigitChecksum(fullCode));

			partCode = "12345667788";
			fullCode = partCode + LuhnAlgorithm.Calculate(partCode).ToString();
			Assert($"Numeric string with valid checksum (odd number of digits) - {fullCode}", LuhnAlgorithm.IsValidLastDigitChecksum(fullCode));

			partCode = "123456";
			fullCode = partCode + LuhnAlgorithm.Calculate(partCode).ToString();
			Assert($"Numeric string with valid checksum (even number of digits) - {fullCode}", LuhnAlgorithm.IsValidLastDigitChecksum(fullCode));

			partCode = "123456778899";
			fullCode = partCode + LuhnAlgorithm.Calculate(partCode).ToString();
			Assert($"Numeric string with valid checksum (even number of digits) - {fullCode}", LuhnAlgorithm.IsValidLastDigitChecksum(fullCode));
		}
	}
}
