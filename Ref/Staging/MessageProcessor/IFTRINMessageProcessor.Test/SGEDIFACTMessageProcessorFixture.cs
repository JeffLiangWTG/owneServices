using System.IO;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.Test
{
	[TestFixture]
	public class SGEDIFACTMessageProcessorFixture : BaseMessageProcessorFixture
	{
		[Test]
		public void ProcessExchangeRateWithUNA()
		{
			AssertProcess("SGIFTRIN ExchangeRate with UNA.txt", AssertExchangeRateWithUNA);
		}

		void AssertExchangeRateWithUNA(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(24, noOfUpdates);
			AssertSameDataAsDirectory("SGExchangeRateWithUNA", xmlFiles);
		}

		[Test]
		public void ProcessExchangeRateWithoutUNA()
		{
			AssertProcess("SGIFTRIN ExchangeRate without UNA.txt", AssertExchangeRateWithoutUNA);
		}

		void AssertExchangeRateWithoutUNA(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(24, noOfUpdates);
			AssertSameDataAsDirectory("SGExchangeRateWithoutUNA", xmlFiles);
		}

		protected override IFTRINMessageProcessor CreateProcessor(IStagingRepository staging, string outputPath)
		{
			return new SGIFTRINMessageProcessor(staging, outputPath);
		}

		protected override string ContentType => DataSourceConstants.ContentType.Edifact;

		protected override string CountryCode => DataSourceConstants.Country.Singapore;
	}
}
