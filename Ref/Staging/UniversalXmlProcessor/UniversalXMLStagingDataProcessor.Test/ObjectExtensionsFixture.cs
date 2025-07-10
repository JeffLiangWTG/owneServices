using System;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using NUnit.Framework;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class ObjectExtensionsFixture
	{
		[Test]
		public void GetValue()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_TariffCode = "001" };
			Assert.AreEqual("001", tariff.GetValue(nameof(Stage.RefCusTariff.ZZ1_TariffCode)));
		}

		[Test]
		public void GetValues()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_TariffCode = "001", ZZ1_ZZI_NKTariffType = "1P1" };
			CollectionAssert.AreEqual(new[] { "001", "1P1" }, tariff.GetValues(new[]
			{
				nameof(Stage.RefCusTariff.ZZ1_TariffCode), nameof(Stage.RefCusTariff.ZZ1_ZZI_NKTariffType)
			}));
		}

		[Test]
		public void GetPKValue()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			Assert.AreEqual(tariff.ZZ1_PK, tariff.GetPKValue());
		}

		[Test]
		public void TestConvertToSafeValue()
		{
			var tariff = new Stage.RefCusTariff { ZZ1_TariffCode = "001", ZZ1_ZZI_NKTariffType = "1P1", ZZ1_StartDate = new DateTime(2024, 2, 29) };
			Assert.AreEqual(tariff.ZZ1_TariffCode, tariff.ZZ1_TariffCode.ConvertToSafeValue(typeof(string)));
			Assert.AreEqual(tariff.ZZ1_StartDate, tariff.ZZ1_StartDate.ConvertToSafeValue<DateTime>());

			string value = null;
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith($@"Unable to convert value <> to type <System.Int32>.
ErrorCode: {ErrorCodes.UnableToConvertToSafeValue}"), () => value.ConvertToSafeValue<int>());
		}
	}
}
