using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.ForwardAir.Testing
{
	sealed class ForwardAirRatesFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var rate = new Xsd.Rate();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var embeddedResourceStream = resourceRetriever.GetStream("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.Test.csv"))
			using (var reader = new StreamReader(embeddedResourceStream))
			{
				var converter = new ForwardAirRatesFlatFileConverter(new NotificationBuffer(), Factory);
				converter.ImportFlatFile(rate, new ForwardAirRatesFlatFileFormat(Factory), reader);
			}

			var testCosting = Factory.NewWithValidTestData<Costing>();
			var importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var adapter = new ForwardAirRatesValueObjectDataAdapter();
			adapter.ImportFromValueObject(testCosting, rate, importContext);

			var rateEntries = testCosting.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;
			Factory.Save();
			AssertEquals(7, rateEntries.Count);
			AssertEquals(RatingConstants.RatingHeaderTypes.Costing, testCosting.TH_RateType);
			AssertEquals(true, testCosting.IsCosting());
			AssertEntry(rateEntries[0], "LRO", "USABQ", "USALB");
			AssertEntry(rateEntries[1], "LRO", "USABQ", "USATL");
			AssertEntry(rateEntries[2], "LRO", "USABQ", "USAUS");
		}

		void AssertEntry(RateEntry entry, string mode, string origin, string destination)
		{
			AssertEquals(mode, entry.TI_Mode);
			AssertEquals(origin, entry.TI_OriginLRC);
			AssertEquals(destination, entry.TI_DestinationLRC);
			AssertEquals(Env.Registry.FreightVolumeUnit, entry.Unit);

			AssertEquals(1, entry.RateLines.Count);
			AssertEquals(Env.Registry.FreightChargeCode, entry.RateLines[0].TL_AC);
			AssertEquals("USD", entry.RateLines[0].Currency.RX_Code);
			AssertEquals("DEF", entry.RateLines[0].TL_Rounding);
			AssertEquals("LB", entry.RateLines[0].TL_WeightVolume);
			AssertEquals(100m, entry.RateLines[0].TL_WeightVolumeMultiple);
		}
	}
}
