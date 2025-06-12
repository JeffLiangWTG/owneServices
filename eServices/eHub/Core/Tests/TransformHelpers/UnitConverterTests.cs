using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Core.Tests
{
	[TestClass]
	public class UnitConverterTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestConvert()
		{
			var converter = new UnitConverter();
			Assert.AreEqual(1M, converter.Convert(1M, "IN", "IN", 4));
			Assert.AreEqual(2.540M, converter.Convert(1M, "IN", "CM", 3));
			Assert.AreEqual(0.39M, converter.Convert(1M, "CM", "IN", 2));
			Assert.AreEqual(0M, converter.Convert(1M, "BLAH", "IN", 3));

			Assert.AreEqual("1.0", converter.Convert("1.0", "IN", "IN"));
			Assert.AreEqual("2.540", converter.Convert("1.0", "IN", "CM"));
			Assert.AreEqual("0.3937", converter.Convert("1.0", "CM", "IN"));
			Assert.AreEqual("", converter.Convert("abc", "IN", "CM"));

			Assert.AreEqual("1.0003", converter.Convert("1.00025", "cm", "CM"));
			Assert.AreEqual("1.0004", converter.Convert("1.00035", "cm", "CM"));

			Assert.AreEqual("1000000", converter.Convert("1.0", "M3", "CC", 10));
		}
	}
}
