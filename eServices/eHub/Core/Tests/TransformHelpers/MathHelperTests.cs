using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Core.Tests
{
	[TestClass]
	public class MathHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRound()
		{
			var helper = new MathHelper();

			Assert.AreEqual("", helper.RoundAwayFromZero("a"));

			Assert.AreEqual("2.4", helper.RoundAwayFromZero("2.4", "k"));
			Assert.AreEqual("3", helper.RoundAwayFromZero("2.5"));
			Assert.AreEqual("3", helper.RoundAwayFromZero("3.4"));
			Assert.AreEqual("4", helper.RoundAwayFromZero("3.5"));

			Assert.AreEqual("2", helper.RoundToEven("2.4"));
			Assert.AreEqual("2", helper.RoundToEven("2.5"));
			Assert.AreEqual("3", helper.RoundToEven("3.4"));
			Assert.AreEqual("4", helper.RoundToEven("3.5"));
		}
	}
}
