using System.Linq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class CodelistLoaderTests
	{
		[Test]
		public void LoadCodeList()
		{
			var data = Loader.GetLocations().Result.ToList();

			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count, Is.EqualTo(6));
		}

		[Test]
		public void CreateUrlQuery()
		{
			Assert.That(Loader.CreateUrlQuery(), Is.EqualTo("RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq 'GB' and ZZD_ZZK_NKCodeType eq 'FAC'"));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			VirtualRefLoader = new VirtualRefDataLoader();
			Loader = new CCSUKLocationLoaderTester(VirtualRefLoader);
		}

		VirtualRefDataLoader VirtualRefLoader;
		CCSUKLocationLoaderTester Loader;
	}
}
