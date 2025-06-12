using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.FWB2UniversalShipment;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.FWB2UniversalShipmentTests
{
	[TestClass]
	public class ScriptsTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestParseWeightVolume()
		{
			string TotalNoOfPacks;
			string TotalWeightCode;
			string TotalWeight;
			string TotalVolumeCode;
			string TotalVolume;

			string input = "49K7.1CC200";
			TestScripts.ParsePackWeightVolume(input, out TotalNoOfPacks, out TotalWeightCode, out TotalWeight, out TotalVolumeCode, out TotalVolume);
			Assert.AreEqual("49", TotalNoOfPacks);
			Assert.AreEqual("K", TotalWeightCode);
			Assert.AreEqual("7.1", TotalWeight);
			Assert.AreEqual("CC", TotalVolumeCode);
			Assert.AreEqual("200", TotalVolume);

			input = "49K731";
			TestScripts.ParsePackWeightVolume(input, out TotalNoOfPacks, out TotalWeightCode, out TotalWeight, out TotalVolumeCode, out TotalVolume);
			Assert.AreEqual("49", TotalNoOfPacks);
			Assert.AreEqual("K", TotalWeightCode);
			Assert.AreEqual("731", TotalWeight);
			Assert.AreEqual(string.Empty, TotalVolumeCode);
			Assert.AreEqual(string.Empty, TotalVolume);

			input = string.Empty;
			TestScripts.ParsePackWeightVolume(input, out TotalNoOfPacks, out TotalWeightCode, out TotalWeight, out TotalVolumeCode, out TotalVolume);
			Assert.AreEqual(string.Empty, TotalNoOfPacks);
			Assert.AreEqual(string.Empty, TotalWeightCode);
			Assert.AreEqual(string.Empty, TotalWeight);
			Assert.AreEqual(string.Empty, TotalVolumeCode);
			Assert.AreEqual(string.Empty, TotalVolume);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSubstringInAllLetterOrAllDigit()
		{
			string input = "49K7.1";
			Assert.AreEqual("49", TestScripts.FindLettersOrDigits(input));
			Assert.AreEqual("49", TestScripts.FindLettersOrDigits(input, 0));
			Assert.AreEqual("K", TestScripts.FindLettersOrDigits(input, 2));
			Assert.AreEqual("7.1", TestScripts.FindLettersOrDigits(input, 3));

			Assert.AreEqual(string.Empty, TestScripts.FindLettersOrDigits(input, 6));
			Assert.AreEqual("1", TestScripts.FindLettersOrDigits(input, 5));

			input = string.Empty;
			Assert.AreEqual(string.Empty, TestScripts.FindLettersOrDigits(input));
			Assert.AreEqual(string.Empty, TestScripts.FindLettersOrDigits(input, 2));
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
