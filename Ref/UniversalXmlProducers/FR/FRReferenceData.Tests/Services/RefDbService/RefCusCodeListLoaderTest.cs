using System;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	[TestFixture]
	class RefCusCodeListLoaderTest
	{
		[Test]
		public void GetRefCusCodeList()
		{
			var data = Loader.GetEuUomCodeList().Result.ToList();
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count > 0);

			data = Loader.GetRateCodeUsageCodeList("U167, U395, U397, U437").Result.ToList();
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count > 0);
		}

		[Test]
		public void CreateFilterQuery()
		{
			Assert.That(Loader.CreateFilterForQueryForEuUom(), Is.EqualTo($"RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq 'EUN' and ZZD_ZZK_NKCodeType eq 'CUSUQ'"));
			Assert.That(Loader.CreateFilterQueryForRateCodeUsage("U167,U395,    U397, U437"), Is.EqualTo($"RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq 'FR' and ZZD_ZZK_NKCodeType in ('U167', 'U395', 'U397', 'U437')"));
			Assert.That(Loader.CreateFilterQueryForRateCodeUsage(ApplicationConfig.Instance.NationalRateCodesRequiringUsageTracking), Is.EqualTo($"RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq 'FR' and ZZD_ZZK_NKCodeType in ('U167', 'U395', 'U397', 'U437')"));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			RefLoader = new RefDataLoaderForTest();
			Loader = new RefCusCodeListLoaderTester(RefLoader);
		}

		RefDataLoaderForTest RefLoader;
		RefCusCodeListLoaderTester Loader;
	}

	public class RefCusCodeListLoaderTester : RefCusCodeListLoader
	{
		public RefCusCodeListLoaderTester(IRefDataLoader refDataLoader) : base(refDataLoader)
		{
		}

		public new string CreateFilterForQueryForEuUom() => RefCusCodeListLoader.CreateFilterForQueryForEuUom();


		public new string CreateFilterQueryForRateCodeUsage(string rateCodes) => RefCusCodeListLoader.CreateFilterQueryForRateCodeUsage(rateCodes);

		public new IRefDataLoader GetRefDataLoader => base.GetRefDataLoader;
	}
}
