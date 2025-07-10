using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ConditionValueTypeCreatorFixture
	{
		[TestCase("A", "", "", "INF")]
		[TestCase("A", "A", "001", "SUP")]
		[TestCase("A", "D", "001", "SNR")]
		[TestCase("H", "N", "234", "SUP")]
		[TestCase("H", "N", "235", "SNR")]
		[TestCase("H", "Y", "021", "SNR")]
		[TestCase("H", "Y", "022", "SUP")]
		public void Get(string conditionCode, string certificateType, string certificateCode, string valueType)
		{
			var valueTypeCreator = new ConditionValueTypeCreator();
			var condition = new measureCondition { conditionCodeId = conditionCode, certificateType = certificateType, certificateCode = certificateCode };
			Assert.AreEqual(valueType, valueTypeCreator.Get(condition));

			var condition2 = new measureCondition { conditionCodeId = "E", dutyAmountSpecified = true, dutyAmount = 10.0m, measurementUnitCode = "KMG" };
			Assert.AreEqual("FRM", valueTypeCreator.Get(condition2));
		}

		[Test]
		public void GetForEAndI()
		{
			var valueTypeCreator = new ConditionValueTypeCreator();
			var condition1 = new measureCondition { conditionCodeId = "E", dutyAmountSpecified = true, dutyAmount = 10.0m, measurementUnitCode = "KMG" };
			var condition2 = new measureCondition { conditionCodeId = "E", certificateType = "A", certificateCode = "001" };
			var condition3 = new measureCondition { conditionCodeId = "E", certificateType = "Y", certificateCode = "Y923" };
			var condition4 = new measureCondition { conditionCodeId = "I", dutyAmountSpecified = true, dutyAmount = 10.0m, measurementUnitCode = "KMG" };
			var condition5 = new measureCondition { conditionCodeId = "I", certificateType = "A", certificateCode = "001" };
			var condition6 = new measureCondition { conditionCodeId = "I", certificateType = "Y", certificateCode = "Y923" };
			Assert.AreEqual("FRM", valueTypeCreator.Get(condition1));
			Assert.AreEqual("SUP", valueTypeCreator.Get(condition2));
			Assert.AreEqual("SNR", valueTypeCreator.Get(condition3));
			Assert.AreEqual("FRM", valueTypeCreator.Get(condition4));
			Assert.AreEqual("SUP", valueTypeCreator.Get(condition5));
			Assert.AreEqual("SNR", valueTypeCreator.Get(condition6));
		}
	}
}
