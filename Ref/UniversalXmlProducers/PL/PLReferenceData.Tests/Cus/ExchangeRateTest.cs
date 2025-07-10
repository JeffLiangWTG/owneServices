using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.PLReferenceData.Business.Cus;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Cus
{
	[TestFixture]
	sealed class ExchangeRateTest
	{
		[Test]
		public void TestTabela_kursowPozycja()
		{
			var position = new tabela_kursowPozycja
			{
				kod_waluty = "USD",
				kurs_sredni = "3.2342",
				nazwa_waluty = "dolar amerykanski",
				przelicznik = 10m
			};

			Assert.AreEqual("USD", position.kod_waluty);
			Assert.AreEqual("3.2342", position.kurs_sredni);
			Assert.AreEqual("dolar amerykanski", position.nazwa_waluty);
			Assert.AreEqual(10m, position.przelicznik);
		}

		[Test]
		public void TestTabela_kursow()
		{
			var dataTable = new tabela_kursow
			{
				pozycja = new List<tabela_kursowPozycja>()
			};

			var position1 = new tabela_kursowPozycja()
			{
				kod_waluty = "USD",
				kurs_sredni = "3.2342",
				nazwa_waluty = "dolar amerykanski",
				przelicznik = 10m
			};

			var position2 = new tabela_kursowPozycja()
			{
				kod_waluty = "EUR",
				kurs_sredni = "3.0342",
				nazwa_waluty = "euro",
				przelicznik = 1m
			};

			dataTable.data_publikacji = new DateTime(2020, 1, 1);
			dataTable.pozycja.Add(position1);
			dataTable.pozycja.Add(position2);

			Assert.AreEqual(new DateTime(2020, 1, 1), dataTable.data_publikacji);
			Assert.AreEqual(2, dataTable.pozycja.Count);

			Assert.AreEqual("USD", dataTable.pozycja[0].kod_waluty);
			Assert.AreEqual("3.2342", dataTable.pozycja[0].kurs_sredni);
			Assert.AreEqual("dolar amerykanski", dataTable.pozycja[0].nazwa_waluty);
			Assert.AreEqual(10m, dataTable.pozycja[0].przelicznik);

			Assert.AreEqual("EUR", dataTable.pozycja[1].kod_waluty);
			Assert.AreEqual("3.0342", dataTable.pozycja[1].kurs_sredni);
			Assert.AreEqual("euro", dataTable.pozycja[1].nazwa_waluty);
			Assert.AreEqual(1m, dataTable.pozycja[1].przelicznik);
		}
	}
}
