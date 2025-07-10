using System;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	[TestFixture]
	class RefCusProcedureLoaderTest
	{
		[Test]
		public void GetRefCusProcedure()
		{
			var data = Loader.GetRefCusProcedure().Result.ToList();

			Assert.That(data, Is.Not.Null);
			Assert.That(data.Count > 0);
		}

		[Test]
		public void CreateFilterQuery()
		{
			Assert.That(Loader.CreateFilterQuery(), Is.EqualTo("RefCusProcedureUpdate?$filter=ZZ6_ZZZ_NKDataGrouping eq 'FR'"));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			RefLoader = new RefDataLoaderForTest();
			Loader = new RefCusProcedureLoaderTester(RefLoader);
		}

		RefDataLoaderForTest RefLoader;
		RefCusProcedureLoaderTester Loader;
	}

	public class RefCusProcedureLoaderTester : RefCusProcedureLoader
	{
		public RefCusProcedureLoaderTester(IRefDataLoader refDataLoader) : base(refDataLoader)
		{
		}

		public new string CreateFilterQuery() => RefCusProcedureLoader.CreateFilterQuery();

		public new IRefDataLoader GetRefDataLoader => base.GetRefDataLoader;
	}
}
