using System.IO;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	public class TestBase
	{
		[SetUp]
		public virtual void SetUp()
		{
			GlobalOption.Instance.Setting = new Setting("CargoWise.RefDbRepo.CNReferenceData.Tests.config.json");
		}

		[TearDown]
		public virtual void TearDown()
		{
			TestHelper.ClearOutputFiles();

			if (File.Exists(TariffProducerTrace.TrackingFileLocation))
			{
				File.Delete(TariffProducerTrace.TrackingFileLocation);
			}
		}
	}
}
