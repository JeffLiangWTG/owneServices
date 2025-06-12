using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Products.GlobalInvoice.Common.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.GlobalInvoice.Common.Tests.Helpers
{
	[TestClass]
	public class FilenameHelperTests
	{
		[TestMethod]
		public void TestBase36Encoding()
		{
			var helper = new FilenameHelper();
			Assert.AreEqual("00001", helper.Base36Encoding((int)Math.Pow(36, 0), 5));
			Assert.AreEqual("00010", helper.Base36Encoding((int)Math.Pow(36, 1), 5));
			Assert.AreEqual("00100", helper.Base36Encoding((int)Math.Pow(36, 2), 5));
			Assert.AreEqual("01000", helper.Base36Encoding((int)Math.Pow(36, 3), 5));
			Assert.AreEqual("10000", helper.Base36Encoding((int)Math.Pow(36, 4), 5));
			Assert.AreEqual("ZZZZZ", helper.Base36Encoding((int)Math.Pow(36, 5) - 1, 5));
			Assert.AreEqual("100000", helper.Base36Encoding((int)Math.Pow(36, 5), 5));

			try
			{
				helper.Base36Encoding(-1, 5);
				Assert.Fail("ArgumentOutOfRangeException Expected");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Assert.AreEqual(
					"Specified argument was out of the range of valid values.\r\nParameter name: input cannot be negative: -1",
					ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("input must be a number: AAA", ex.Message);
			}
		}
	}
}
