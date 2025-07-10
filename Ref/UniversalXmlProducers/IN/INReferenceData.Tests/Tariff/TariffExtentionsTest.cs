using System;
using CargoWise.RefDbRepo.INReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.INReferenceData.Services.Tariff.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class TariffExtentionsTest
	{
		[Test]
		public void TestGetTariffDate()
		{
			var content = new Childcontentlist
			{
				titleEn = "Tariff 2020",
			};

			Assert.Multiple(() =>
			{
				Assert.IsNull(content.GetTariffDate(), "Title in Unknown format");

				content.titleEn = "Tariff (as on 30.06.2024)";
				Assert.AreEqual(new DateTime(2024, 6, 30), content.GetTariffDate(), "Title in valid format");

				content.titleEn = "Tariff (w.e.f. 01.05.2022)";
				Assert.AreEqual(new DateTime(2022, 5, 1), content.GetTariffDate(), "Title in valid format");
			});
		}

		[Test]
		public void TestGetDateInCbicString()
		{
			var date = new DateTime(2024, 6, 30);
			Assert.AreEqual("30.06.2024", date.GetDateInCbicString(), "Date in valid format");
		}
	}
}
