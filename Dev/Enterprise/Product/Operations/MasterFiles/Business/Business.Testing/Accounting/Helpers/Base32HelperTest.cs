using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(Base32Helper))]
	public sealed class Base32HelperTest : TestCase
	{
		public void TestToBase32_ShouldConvertByteArrayToBase32String_WithoutPadding()
		{
			var encodedString = Base32Helper.ToBase32(Encoding.UTF8.GetBytes("Hello World"), padOutput: false);

			AssertEquals("JBSWY3DPEBLW64TMMQ", encodedString);
		}

		public void TestToBase32_ShouldConvertByteArrayToBase32String_WithPadding()
		{
			var encodedString = Base32Helper.ToBase32(Encoding.UTF8.GetBytes("Hello World!"), padOutput: true);

			AssertEquals("JBSWY3DPEBLW64TMMQQQ====", encodedString);
		}

		public void TestFromBase32_ShouldConvertBase32StringToByteArray()
		{
			var validBase32String = "JBSWY3DPEBLW64TMMQQQ====";
			var validByteArray = new byte[] { 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33 };

			var result = Base32Helper.FromBase32(validBase32String);

			AssertEquals(validByteArray, result);
		}

		public void TestFromBase32_ShouldThrowFormatException_ForInvalidBase32String()
		{
			var invalidBase32String = "1InvalidBase32"; // '1' is invalid in Base32 encoding

			AssertExceptionThrown<FormatException>(() => Base32Helper.FromBase32(invalidBase32String));
		}

		public void TestToBase32_ShouldReturnEmptyString_WhenByteArrayIsEmpty()
		{
			var encodedString = Base32Helper.ToBase32(Array.Empty<byte>(), padOutput: false);

			AssertEquals(string.Empty, encodedString);
		}

		public void TestFromBase32_ShouldReturnEmptyByteArray_WhenBase32StringIsEmpty()
		{
			var decodedBytes = Base32Helper.FromBase32(string.Empty);

			AssertEquals(Array.Empty<byte>(), decodedBytes);
		}
	}
}
