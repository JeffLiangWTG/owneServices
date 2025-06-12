using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.UniversalShipment2FSU;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.UniversalShipment2FSUTests
{
	[TestClass]
	public class ScriptsTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormatNumberRaw()
		{
			Assert.AreEqual("123", TestScripts.FormatNumberRaw("123", 7));
			Assert.AreEqual("123.4", TestScripts.FormatNumberRaw("123.4", 7));
			Assert.AreEqual("123.4567", TestScripts.FormatNumberRaw("123.4567", 7));
			Assert.AreEqual("123.4567", TestScripts.FormatNumberRaw("123.45678", 7));
			Assert.AreEqual("1234567", TestScripts.FormatNumberRaw("1234567", 7));
			Assert.AreEqual("1234567", TestScripts.FormatNumberRaw("12345678.000", 7));
		}

		#region Implementation

		Scripts TestScripts
		{
			get
			{
				return testScripts ?? (testScripts = new Scripts());
			}
		}

		Scripts testScripts;

		#endregion
	}
}
