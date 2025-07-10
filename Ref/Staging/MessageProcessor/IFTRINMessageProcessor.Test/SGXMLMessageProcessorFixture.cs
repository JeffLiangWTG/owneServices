using System.IO;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.Test
{
	[TestFixture]
	public class SGXMLMessageProcessorFixture : BaseMessageProcessorFixture
	{
		[Test]
		public void ProcessExchangeRate()
		{
			AssertProcess("SGIFTRIN XML CustomsExchangeRate.xml", AssertExchangeRate);
		}

		void AssertExchangeRate(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(18, noOfUpdates);
			AssertSameDataAsDirectory("SGExchangeRateFromXML", xmlFiles);
		}

		protected override IFTRINMessageProcessor CreateProcessor(IStagingRepository staging, string outputPath)
		{
			return new SGIFTRINMessageProcessor(staging, outputPath);
		}

		protected override string ContentType => DataSourceConstants.ContentType.XML;

		protected override string CountryCode => DataSourceConstants.Country.Singapore;
	}
}
