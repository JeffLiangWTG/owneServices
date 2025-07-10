using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff.Tests
{
	sealed class CusVATApplicabilityTest
	{
		[Test]
		public void TestRefCusVATApplicability()
		{
			Assert.Multiple(() =>
			{
				AssertTestRefCusVATApplicability(true, "2001-02-03", "2024-01-01", "Description", "MV1", new DateTime(2001, 02, 03), new DateTime(2024, 01, 01, 23, 59, 0));
				AssertTestRefCusVATApplicability(false, "2001-13-03", "2079-01-01", "Description", "MV1", new DateTime(2001, 02, 03), new DateTime(2079, 01, 01));
			});
		}

		void AssertTestRefCusVATApplicability(bool isValid, string startDate, string endDate, string description, string taxOrFeeCode, DateTime expectedStart, DateTime expectedEnd)
		{
			TariffParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to parse VAT code due to empty code, empty description or invalid Dates.
DETAILS:
VAT code: {taxOrFeeCode}
Description: {description}
Start Date: {startDate}
End Date: {endDate}
";

			var refCusVATApplicability = CusVATApplicability.ConvertRefCusVATApplicability(startDate, endDate, description, taxOrFeeCode);
			Assert.AreEqual(isValid, (refCusVATApplicability != null));
			if (isValid)
			{
				Assert.That(refCusVATApplicability.ZX5_Description, Is.EqualTo(description));
				Assert.That(refCusVATApplicability.ZX5_StartDate, Is.EqualTo(expectedStart));
				Assert.That(refCusVATApplicability.ZX5_EndDate, Is.EqualTo(expectedEnd));
			}
			else
			{
				Assert.That(refCusVATApplicability, Is.EqualTo(null));
				Assert.That(TariffParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
			}
		}
	}
}
