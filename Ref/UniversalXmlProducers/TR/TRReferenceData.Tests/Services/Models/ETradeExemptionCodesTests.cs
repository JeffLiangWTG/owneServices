using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Models
{
	public class ETradeExemptionCodesTests
	{
		[Test]
		public void ShouldHaveCorrectProperties()
		{
			var expectedPropertyCount = 10;
			var expectedPropertyNames = new string[]
			{
			"ExemptionCode",
			"ExemptionDescEnglish",
			"DutyPercent",
			"DutyFormula",
			"ExemptionDescTurkish",
			"RateCode",
			"RateType",
			"TradeGroup",
			"StartDate",
			"EndDate"
			};

			
			var actualProperties = typeof(ETradeExemptionCodes).GetProperties();
			Assert.AreEqual(expectedPropertyCount, actualProperties.Length);


			foreach (var expectedPropertyName in expectedPropertyNames)
			{
				Assert.IsTrue(actualProperties.Any(p => p.Name == expectedPropertyName),
					$"Property '{expectedPropertyName}' not found in ETradeExemptionCodes.");
			}
		}
	}
}
