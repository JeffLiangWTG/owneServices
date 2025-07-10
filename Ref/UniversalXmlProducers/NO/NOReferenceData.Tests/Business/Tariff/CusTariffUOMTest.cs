using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff.Tests
{
	sealed class CusTariffUOMTest
	{
		[Test]
		public void TestRefCusTariffUOM()
		{
			Assert.Multiple(() =>
			{
				AssertTestRefCusTariffUOM(true, "KG", "Kilogram", "KGM");
				AssertTestRefCusTariffUOM(true, "C", "Carat", "HE");
				AssertTestRefCusTariffUOM(true, "STK", "Antall enheter", "NMB");
				AssertTestRefCusTariffUOM(false, "XXX", "Not valid", "YYY");
				AssertTestRefCusTariffUOM(false, string.Empty, "Empty, not valid", "YYY");
			});
		}

		void AssertTestRefCusTariffUOM(bool isValid, string noCode, string noDesc, string uomCode)
		{
			var tariffId = "123456";
			TariffParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to parse Varenummer code due to empty or missing code.
DETAILS:
Varenummer: {tariffId}
Unit: {noCode}
Unit description: {noDesc}
";

			var refCusTariffUOM = CusTariffUOM.ConvertRefCusTariffUOM(tariffId, noCode, noDesc, "1");
			Assert.AreEqual(isValid, (refCusTariffUOM != null));
			if (isValid)
			{
				Assert.That(refCusTariffUOM.ZZ8_Type, Is.EqualTo("CU1"));
				Assert.That(refCusTariffUOM.ZZ8_UOM, Is.EqualTo(uomCode));
			}
			else
			{
				Assert.That(refCusTariffUOM, Is.EqualTo(null));
				Assert.That(TariffParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
			}
		}
	}
}
