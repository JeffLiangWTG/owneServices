using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	abstract class HsnTariffExcListDutyRatesLoaderBaseTestCase
	{
		protected abstract Tuple<string, string> DataFileName { get; }
		protected abstract HsnTariffEXCListDutyRatesLoaderBase RateLoader { get; }
		protected abstract int ExpectedLength { get; }
		protected abstract IEnumerable<HsnTariffEXCListDutyRate> ExpectedSampleData { get; }

		[Test]
		public void LoadData()
		{
			var loadData = RateLoader.GetRates();
			Assert.AreEqual(ExpectedLength, loadData.Length);

			foreach (var sample in ExpectedSampleData)
			{
				var result = loadData.FirstOrDefault(x => x.TariffNo == sample.TariffNo);
				Assert.IsNotNull(result);
				Assert.AreEqual(sample.Description, result.Description);
				Assert.AreEqual(sample.DutyAmount, result.DutyAmount);
				Assert.AreEqual(sample.UomCU3, result.UomCU3);
				Assert.AreEqual(sample.UomCU4, result.UomCU4);
				Assert.AreEqual(sample.UomCU5, result.UomCU5);
				Assert.AreEqual(sample.ExemptedTariffCodes, result.ExemptedTariffCodes);
				Assert.AreEqual(sample.AdditionalCode, result.AdditionalCode);
				Assert.AreEqual(sample.RateType, result.RateType);
				Assert.AreEqual(sample.RateCode, result.RateCode);
				Assert.AreEqual(sample.RateFormula, result.RateFormula);
				Assert.AreEqual(sample.StartDate, result.StartDate);
				Assert.AreEqual(sample.EndDate, result.EndDate);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);

			TestHelper.SimulateDownload(Path.Combine(tempFolder, DataFileName.Item1), DataFileName.Item2);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;
	}
}
