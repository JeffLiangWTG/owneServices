using System;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	[TestFixture]
	class RefCusTariffLoaderTest
	{
		[Test]
		public void GetRefCusTariff()
		{
			var data = Loader.GetEUDeclarableTariffsFromRefDB(DateTime.Today).Result.ToList();

			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count > 0);
		}

		[Test]
		public void CreateFilterQuery()
		{
			Assert.That(Loader.CreateFilterQuery(new DateTime(2022,04,11)), Is.EqualTo("RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'EUN' and ZZ1_StartDate le 2022-04-11T12:00:00Z and ZZ1_EndDate ge 2022-04-15T12:00:00Z"));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			RefLoader = new RefDataLoaderForTest();
			Loader = new RefCusTariffLoaderTester(RefLoader);
		}

		RefDataLoaderForTest RefLoader;
		RefCusTariffLoaderTester Loader;
	}

	public class RefCusTariffLoaderTester : RefCusTariffLoader
	{
		public RefCusTariffLoaderTester(IRefDataLoader refDataLoader) : base(refDataLoader)
		{
		}

		public new string CreateFilterQuery(DateTime thatDay) => base.CreateFilterQuery(thatDay);

		public new IRefDataLoader GetRefDataLoader => base.GetRefDataLoader;

		protected override DateTime GetQueryEndDate() => new DateTime(2022, 04, 15);
	}
}
