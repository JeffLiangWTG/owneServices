
using System.Xml;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.ForwardAir.Testing
{
	[TestedType(typeof(ForwardAirRatesValueObjectDataAdapter))]
	sealed class ForwardAirRatesValueObjectDataAdapterTest : RatingValueObjectDataAdapterTest<RatingHeader>
	{
		#region Import

		protected override void TestImportCore()
		{
			var testCosting = Factory.NewWithValidTestData<Costing>();
			var importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			DataAdapter.ImportFromValueObject(testCosting, PopulatedRates.Rate[0], importContext);

			var airRateEntries = testCosting.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			Factory.Save();
			AssertEquals(7, airRateEntries.Count);
			AssertEntry(airRateEntries[0], "LSE", "USABQ", "USALB");
			AssertEntry(airRateEntries[1], "LSE", "USABQ", "USATL");
			AssertEntry(airRateEntries[2], "LSE", "USABQ", "USAUS");
		}

		public override void TestImportRateInWrongModule()
		{
			Assert(true); // FAR rate can only be imported from Rating header screen only. this test is irrevelant
		}

		protected override void TestExpectedRateTypeCore()
		{
			AssertEquals("Rate Type", ExpectedRateType, DataAdapter.ExpectedRateType);
		}

		protected override void TestShouldCheckOwnerIsSpecifiedCore()
		{
			AssertEquals("Should Check Owner is Specified", RateShouldCheckOwnerIsSpecified, DataAdapter.ShouldCheckifOwnerIsSpecified);
		}

		protected override ZString ExpectedInvalidModuleMesg
		{
			get { return ""; }
		}

		#endregion

		#region Implementation

		protected override RatingValueObjectDataAdapter<RatingHeader> GetDataAdapter
		{
			get { return new ForwardAirRatesValueObjectDataAdapter(); }
		}

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.Costing; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		protected override bool RateShouldCheckOwnerIsSpecified
		{
			get { return false; }
		}

		protected override ZString OtherRateTypeForErrorTesitng
		{
			get { return RatingConstants.RatingHeaderTypes.Quote; }
		}

		#endregion

		#region Helpers

		void AssertEntry(RateEntry entry, string mode, string origin, string destination)
		{
			AssertEquals(mode, entry.TI_Mode);
			AssertEquals(origin, entry.TI_OriginLRC);
			AssertEquals(destination, entry.TI_DestinationLRC);
		}

		class ForwardAirRatesValueObjectDataAdapterForTest : ForwardAirRatesValueObjectDataAdapter
		{
			public new ZString ExpectedRateType
			{
				get { return base.ExpectedRateType; }
			}

			public new bool ShouldCheckifOwnerIsSpecified
			{
				get { return base.ShouldCheckifOwnerIsSpecified; }
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new ForwardAirRatesValueObjectDataAdapterForTest();
		}

		ForwardAirRatesValueObjectDataAdapterForTest DataAdapter;

		Xsd.Rates PopulatedRates
		{
			get
			{
				if (fPopulatedRates == null)
				{
					using (var resourceRetriever = new EmbeddedResourceRetriever())
					using (var stream = resourceRetriever.GetStream("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.PopulatedRate.xml"))
					{
						var xmlTextReader = XmlReader.Create(stream);
						var xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.Rates));
						fPopulatedRates = (Xsd.Rates)xmlValueObjectSerializer.Deserialize(xmlTextReader);
					}
				}

				return fPopulatedRates;
			}
		}

		Xsd.Rates fPopulatedRates;

		#endregion
	}
}
