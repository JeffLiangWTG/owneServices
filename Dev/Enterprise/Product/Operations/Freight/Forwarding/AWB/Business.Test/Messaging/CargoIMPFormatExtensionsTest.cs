using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class CargoIMPFormatExtensionsTest : TestCase
	{
		public void TestIsCargoIMPEmpty()
		{
			ZString alphaOnly = "ABC";
			ZString numericOnly = "123";
			ZString textOnly = "-. ";
			ZString specialCharsOnly = "#$%";

			AssertStringsAreCargoIMPEmpty(CIMPFieldFormats.Alpha, numericOnly, textOnly, specialCharsOnly);
			AssertStringsAreNotCargoIMPEmpty(CIMPFieldFormats.Alpha, alphaOnly);

			AssertStringsAreCargoIMPEmpty(CIMPFieldFormats.Numeric, alphaOnly, textOnly, specialCharsOnly);
			AssertStringsAreNotCargoIMPEmpty(CIMPFieldFormats.Numeric, numericOnly);

			AssertStringsAreCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric, textOnly, specialCharsOnly);
			AssertStringsAreNotCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric, alphaOnly, numericOnly);

			AssertStringsAreCargoIMPEmpty(CIMPFieldFormats.Text, specialCharsOnly);
			AssertStringsAreNotCargoIMPEmpty(CIMPFieldFormats.Text, alphaOnly, numericOnly, textOnly);
		}

		void AssertStringsAreCargoIMPEmpty(CIMPFieldFormats format, params ZString[] empties)
		{
			foreach (ZString empty in empties)
			{
				Assert(empty.IsCargoIMPEmpty(format));
			}
		}

		void AssertStringsAreNotCargoIMPEmpty(CIMPFieldFormats format, params ZString[] nonEmpties)
		{
			foreach (ZString nonEmpty in nonEmpties)
			{
				Assert(!nonEmpty.IsCargoIMPEmpty(format));
			}
		}
	}
}
