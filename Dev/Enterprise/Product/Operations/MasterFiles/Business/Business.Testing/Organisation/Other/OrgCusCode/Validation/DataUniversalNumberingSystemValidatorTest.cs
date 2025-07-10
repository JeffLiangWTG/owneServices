using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DataUniversalNumberingSystemValidatorTest : TestCase
	{
		public void TestGetDUNSNumberError()
		{
			AssertEquals("No Error", ZString.Empty, DataUniversalNumberingSystemValidator.GetDUNSNumberError("123456789"));
			AssertEquals("Has Error", DataUniversalNumberingSystemValidator.DUNSNumberFormat, DataUniversalNumberingSystemValidator.GetDUNSNumberError("12345678A"));
			AssertEquals("Has Error", DataUniversalNumberingSystemValidator.DUNSNumberFormat, DataUniversalNumberingSystemValidator.GetDUNSNumberError("1234567890"));
			AssertEquals("Has Error", DataUniversalNumberingSystemValidator.DUNSNumberFormat, DataUniversalNumberingSystemValidator.GetDUNSNumberError("123456"));
		}
	}
}
