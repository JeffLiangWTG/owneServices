using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	public class RefDataEntityLoaderTest
	{
		[Test]
		public void TesGenerateCompositeKey()
		{
			var helper = new RefDataEntityLoader(GetSafeRepository(nomenclatureInputData));
			AssertCompositeKey(helper, "0106419000", "01.01..06.4.1.9.0.0.0", "1111.9.0.0.0");
			AssertCompositeKey(helper, "1001110000", "02.10..01.1.1.0.0.0.0", "2222.0.0.0.0");
			AssertCompositeKey(helper, "2005511000", "04.20..05.5.1.1.0.0.0", "3333.1.0.0.0");	
			AssertCompositeKey(helper, "6904100000", "13.69.02.04.1.0.0.0.0", "4444.0.0.0.0");
			AssertCompositeKey(helper, "0205000000", "01.02..05.0.0.0.0.0.0", "5555.0.0.0.0.0.0");
			AssertCompositeKey(helper, "2424000000", "", "");
		}

		void AssertCompositeKey(RefDataEntityLoader helper, string tariffCode, string currentResult, string pastResult)
		{
			var tariff = new Common.UniversalXmlWriter.EntityType.RefCusTariff { ZZ1_TariffCode = tariffCode, ZZ1_EndDate = DateTime.Today };
			Assert.That(helper.GenerateCompositeKey(tariff), Is.EqualTo(currentResult));
			tariff.ZZ1_EndDate = new DateTime(2021, 12, 31);
			Assert.That(helper.GenerateCompositeKey(tariff), Is.EqualTo(pastResult));
		}

		[Test]
		public void TestGetNomenclature()
		{
			var helper = new RefDataEntityLoader(GetSafeRepository(nomenclatureInputData: nomenclatureInputData));
			Assert.AreEqual(1, helper.GetNomenclatureData("WCO", "5", new DateTime(2022,1,1)).Count());
			Assert.AreEqual(1, helper.GetNomenclatureData("WCO", "8", new DateTime(2022, 1, 1)).Count());
			Assert.AreEqual(2, helper.GetNomenclatureData("WCO", "I", new DateTime(2022, 1, 1)).Count());
			Assert.AreEqual(2, helper.GetNomenclatureData("WCO", "V", new DateTime(2022, 1, 1)).Count());
		}


		[Test]
		public void TestGetTariff()
		{
			var helper = new RefDataEntityLoader(GetSafeRepository(tariffInputData: tariffInputData));
			Assert.AreEqual(1, helper.GetTariffData("WCO", "1", new DateTime(2022, 1, 1)).Count());
			Assert.AreEqual(1, helper.GetTariffData("WCO", "2", new DateTime(2022, 1, 1)).Count());
		}

		public ISafeRepository GetSafeRepository(List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatureInputData = null,
												List<Tuple<string, string, string, string, DateTime, DateTime>> tariffInputData = null)
		{
			var repoMock = new MockRepository(MockBehavior.Default);
			var safeRepositoryMock = repoMock.Create<ISafeRepository>();
			if (nomenclatureInputData != null)
			{
				safeRepositoryMock.Setup(x => x.Get<RefCusNomenclatureGroup>()).Returns(() => LoadNomenclatureGroup(nomenclatureInputData));
			}

			if (tariffInputData != null)
			{
				safeRepositoryMock.Setup(x => x.Get<RefCusTariff>()).Returns(() => LoadTariffs(tariffInputData));
			}
			return safeRepositoryMock.Object;
		}

		IQueryable<RefCusNomenclatureGroup> LoadNomenclatureGroup(List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatures)
		{
			var nomenclatureGroups = new List<RefCusNomenclatureGroup>();

			foreach (var item in nomenclatures)
			{
				var nomenclature = new RefCusNomenclatureGroup
				{
					ZZ5_Value = item.Item1,
					ZZ5_Description = item.Item2,
					ZZ5_CompositeKey = item.Item3,
					ZZ5_StartDate = item.Item5,
					ZZ5_EndDate = item.Item6,
					ZZ5_ZZZ_NKDataGrouping = item.Item4,
					ZZ5_ZZ9_NKNomenclatureGroupType = item.Item4
				};
				nomenclatureGroups.Add(nomenclature);
			}
			return nomenclatureGroups.AsQueryable();
		}

		IQueryable<RefCusTariff> LoadTariffs(List<Tuple<string, string, string, string, DateTime, DateTime>> tariffs)
		{
			var result = new List<RefCusTariff>();
			foreach (var item in tariffs)
			{
				var tariff = new RefCusTariff
				{
					ZZ1_TariffCode = item.Item1,
					ZZ1_Description = item.Item2,
					ZZ1_CompositeKeyOnZZ5 = item.Item3,
					ZZ1_StartDate = item.Item5,
					ZZ1_EndDate = item.Item6,
					ZZ1_ZZZ_NKDataGrouping = item.Item4,
				};
				result.Add(tariff);
			}
			return result.AsQueryable();
		}

		List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatureInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
			new Tuple<string, string, string, string, DateTime, DateTime>("555", "WCO Nomenclature 1", "", "WCO", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("543", "CN Nomenclature 1", "", "CN", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("888", "WCO Nomenclature 2", "", "WCO", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("876", "CN Nomenclature 2", "", "CN", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("I", "WCO Nomenclature 3", "", "WCO", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("IV", "WCO Nomenclature 4", "", "WCO", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("II", "CN Nomenclature 3", "", "CN", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("V", "WCO Nomenclature 5", "", "WCO", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("VII", "WCO Nomenclature 6", "", "WCO", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("VI", "CN Nomenclature 4", "", "CN", new DateTime(2020, 1, 1), new DateTime(2079, 6, 6)),

			new Tuple<string, string, string, string, DateTime, DateTime>("010641", "Nom1 Past", "1111", "KR", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("100111", "Nom2 Past", "2222", "KR", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("200551", "Nom3 Past", "3333", "KR", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("690410", "Nom4 Past", "4444", "KR", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0205", "Nom5 Past", "5555", "KR", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("010641", "Nom1 Current", "01.01..06.4.1", "KR", new DateTime(2022, 1, 1), DateTime.Now.AddDays(5)),
			new Tuple<string, string, string, string, DateTime, DateTime>("100111", "Nom2 Current", "02.10..01.1.1", "KR", new DateTime(2022, 1, 1), DateTime.Now.AddDays(5)),
			new Tuple<string, string, string, string, DateTime, DateTime>("200551", "Nom3 Current", "04.20..05.5.1", "KR", new DateTime(2022, 1, 1), DateTime.Now.AddDays(5)),
			new Tuple<string, string, string, string, DateTime, DateTime>("690410", "Nom4 Current", "13.69.02.04.1", "KR", new DateTime(2022, 1, 1), DateTime.Now.AddDays(5)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0205", "Nom5 Current", "01.02..05", "KR", new DateTime(2022, 1, 1), DateTime.Now.AddDays(5))
		};

		List<Tuple<string, string, string, string, DateTime, DateTime>> tariffInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
			new Tuple<string, string, string, string, DateTime, DateTime>("111111", "WCO Tariff 1", "", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("123456", "CN Tariff 1", "", "CN", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("222222", "WCO Tariff 2", "", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
			new Tuple<string, string, string, string, DateTime, DateTime>("234567", "CN Tariff 2", "", "CN", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6))
		};
	}


}
