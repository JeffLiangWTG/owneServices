using System;
using System.IO;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.DataTransfer.ForwardAir.Testing
{
	public sealed class ForwardAirRatesFlatFileDataImporterTest : FlatFileDataImporterTestCase
	{
		public void TestImportData()
		{
			using (var reader = new StreamReader(TestCsvPath))
			{
				var costing = Factory.New<Costing>();
				var importer = new ForwardAirRatesFlatFileDataImporter(costing);

				var result = importer.ImportData(reader, "TestFileName", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				AssertEquals(7, costing.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count);
			}
		}

		protected override string PathToTestFile => TestCsvPath;

		protected override FlatFileDataImporter GetDataImporter() => new ForwardAirRatesFlatFileDataImporter(Factory.New<Costing>());

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string testCsvPath;
		string TestCsvPath
		{
			get
			{
				if (string.IsNullOrEmpty(testCsvPath))
				{
					testCsvPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.Test.csv");
				}
				return testCsvPath;
			}
		}
	}
}
