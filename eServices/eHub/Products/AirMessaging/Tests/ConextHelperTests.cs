using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class ConextHelperTests
	{
		private TestContext testContextInstance;

		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ConvertMessageTypeToStandard()
		{
			Assert.AreEqual("FSU", ConextHelper.ConvertMessageTypeToStandard("CIMFSU"));
			Assert.AreEqual("FMA", ConextHelper.ConvertMessageTypeToStandard("CIMFMA"));
			Assert.AreEqual("FNA", ConextHelper.ConvertMessageTypeToStandard("CIMFNA"));
			Assert.AreEqual("FSA", ConextHelper.ConvertMessageTypeToStandard("CIMFSA"));
			Assert.AreEqual("TEST", ConextHelper.ConvertMessageTypeToStandard("TEST"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ValidateHAWB_Invalid()
		{
			Assert.AreEqual(false, ConextHelper.ValidateHAWB(""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ValidateHAWB_Valid()
		{
			Assert.AreEqual(true, ConextHelper.ValidateHAWB("243535242323232323"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ValidateMAWB_Invalid()
		{
			Assert.AreEqual(false, ConextHelper.ValidateMAWB("0813265231"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ValidateMAWB_Valid()
		{
			Assert.AreEqual(true, ConextHelper.ValidateMAWB("08132652312"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ValidateReference_Invalid()
		{
			Assert.AreEqual(false, ConextHelper.ValidateReference("0813265231"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ValidateReference_Valid()
		{
			Assert.AreEqual(true, ConextHelper.ValidateReference("08132652312"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AddOriginalMessage_FHL()
		{
			var result = ConextHelper.AddOriginalMessage(@"FMA/0
ACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER
/HBS 5640199661", "0813265231224243535242323232323");

			Assert.AreEqual(@"FMA/0
ACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER
/HBS 5640199661
FHL
MBI/081-32652312/
HBS/24243535242323232323/////
", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AddOriginalMessage_FWB()
		{
			var result = ConextHelper.AddOriginalMessage(@"FMA/0
ACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER
/HBS 5640199661", "08132652312");

			Assert.AreEqual(@"FMA/0
ACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER
/HBS 5640199661
FWB
081-32652312
", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AddOriginalMessage_InvalidMAWB()
		{
			var result = ConextHelper.AddOriginalMessage(@"FMA/0
ACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER
/HBS 5640199661", "123");

			Assert.AreEqual(@"FMA/0
ACK/.YOUR MESSAGE HAS BEEN FORWARDED TO THE AIRLINE OR GROUND HANDLER
/HBS 5640199661", result);
		}
	}
}
