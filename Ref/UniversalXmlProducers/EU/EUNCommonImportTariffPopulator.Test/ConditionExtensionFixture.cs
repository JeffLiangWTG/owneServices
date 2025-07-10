using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ConditionExtensionFixture
	{
		[TestCase("04", false)]
		[TestCase("09", false)]
		[TestCase("10", false)]
		[TestCase("16", false)]
		[TestCase("24", true)]
		[TestCase("28", true)]
		[TestCase("29", true)]
		[TestCase("36", true)]
		public void ShouldAllow(string actionCode, bool allow)
		{
			var condition = new measureCondition { conditionCodeId = "R", actionCode = actionCode };
			Assert.AreEqual(allow, condition.ShouldAllow());
		}

		[Test]
		public void WithDutyAmount()
		{
			var condition1 = new measureCondition { dutyAmountSpecified = true, dutyAmount = 10.0m, measurementUnitCode = "KMG" };
			var condition2 = new measureCondition { certificateCode = "001", certificateType = "L" };
			Assert.AreEqual(true, condition1.WithDutyAmount());
			Assert.AreEqual(false, condition2.WithDutyAmount());
		}

		[Test]
		public void WithCertificate()
		{
			var condition1 = new measureCondition { dutyAmountSpecified = true, dutyAmount = 10.0m, measurementUnitCode = "KMG" };
			var condition2 = new measureCondition { certificateCode = "001", certificateType = "L" };
			Assert.AreEqual(false, condition1.WithCertificate());
			Assert.AreEqual(true, condition2.WithCertificate());
		}

		[TestCase("A", "220", true)]
		[TestCase("C", "221", true)]
		[TestCase("E", "222", true)]
		[TestCase("Z", "223", true)]
		[TestCase("L", "135", true)]
		[TestCase("L", "136", false)]
		[TestCase("N", "234", true)]
		[TestCase("N", "235", false)]
		[TestCase("Y", "021", false)]
		[TestCase("Y", "022", true)]
		public void CertificateOfSUP(string certificateType, string certificateCode, bool isSUP)
		{
			var condition = new measureCondition { certificateType = certificateType, certificateCode = certificateCode };
			Assert.AreEqual(isSUP, condition.CertificateOfSUP());
		}
	}
}
