using System;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
	public class BooleanHelperTest
	{

		[Test]
		[TestCase("0", false)]
		[TestCase("1", true)]
		public void TestParseBool_WithValidBooleanString_ReturnsBool(string booleanAsString, bool expected)
		{
			bool? result = BooleanHelper.ParseBool(booleanAsString);

			Assert.That(result, Is.EqualTo(expected));
		}


		[Test]
		public void TestParseBool_WithNull_ReturnsNull()
		{
			Assert.That(BooleanHelper.ParseBool(null), Is.Null);
			Assert.That(BooleanHelper.ParseBool(""), Is.Null);
		}

		[Test]
		public void TestParseBool_WithInvalidDateString_ThrowsFormatException()
		{
			string invalidBoolean = "invalid-boolean";

			var ex = Assert.Throws<FormatException>(() => BooleanHelper.ParseBool(invalidBoolean));
			Assert.That(ex.Message, Does.Contain("Parse bool Error"));
		}
	}
}
