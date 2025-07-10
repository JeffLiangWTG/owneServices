using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ClassificationDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportClassification()
		{
			var packLine = Factory.New<PackLine>();
			AssertEquals(0, packLine.HarmonisedCodes.Count);

			AssertImportClassificationResult(packLine, CollectionContent.Complete);
			AssertImportClassificationResult(packLine, CollectionContent.Partial);
		}

		public void TestReadIntoCollection_Complete_OverrideExisting()
		{
			var packLine = Factory.New<PackLine>();

			var harmonisedCode = packLine.HarmonisedCodes.AddNew();
			harmonisedCode.JLH_Code = "T11";
			harmonisedCode.JLH_RN_NKCountry = "AU";

			var harmonisedCode2 = packLine.HarmonisedCodes.AddNew();
			harmonisedCode2.JLH_Code = "T22";
			harmonisedCode2.JLH_RN_NKCountry = "US";

			var harmonisedCode3 = packLine.HarmonisedCodes.AddNew();
			harmonisedCode3.JLH_Code = "T33";
			harmonisedCode3.JLH_RN_NKCountry = "HK";

			AssertEquals(3, packLine.HarmonisedCodes.Count);
			AssertImportClassificationResult(packLine, CollectionContent.Complete);
		}

		public void TestReadIntoCollection_Partial()
		{
			var packLine = Factory.New<PackLine>();

			var harmonisedCode = packLine.HarmonisedCodes.AddNew();
			harmonisedCode.JLH_Code = "XX11";
			harmonisedCode.JLH_RN_NKCountry = "AU";

			var harmonisedCode2 = packLine.HarmonisedCodes.AddNew();
			harmonisedCode2.JLH_Code = "US22";
			harmonisedCode2.JLH_RN_NKCountry = "US";

			AssertEquals(2, packLine.HarmonisedCodes.Count);

			var classification1 = UniversalTestHelper.CreateClassificationDataObject("AU11", "HSC", "Harmonized Code", "AU", "Australia");
			var classification2 = UniversalTestHelper.CreateClassificationDataObject("CN33", "HSC", "Harmonized Code", "CN", "China");

			var classificationDOs = new DataObjectList<Classification>() { Content = CollectionContent.Partial };
			classificationDOs.AddRange(new[] { classification1, classification2 });

			var reader = new ClassificationCollectionReader<JobPackLineHarmonisedCode, JobPackLineHarmonisedCodeCollection>(classificationDOs, logger, Factory, packLine);
			reader.ReadIntoCollection();

			AssertEquals(4, packLine.HarmonisedCodes.Count);
			Assert("Country matched with existing record, code is updated", packLine.HarmonisedCodes.Any(x => x.JLH_Code == "AU11" && x.JLH_RN_NKCountry == "AU"));
			Assert("Country not matched with existing record, do not update anything", packLine.HarmonisedCodes.Any(x => x.JLH_Code == "US22" && x.JLH_RN_NKCountry == "US"));
			Assert("Country not found, create new record", packLine.HarmonisedCodes.Any(x => x.JLH_Code == "CN33" && x.JLH_RN_NKCountry == "CN"));
		}

		void AssertImportClassificationResult(PackLine packLine, CollectionContent content)
		{
			var classification1 = UniversalTestHelper.CreateClassificationDataObject("HS11", "HSC", "Harmonized Code", "AU", "Australia");
			var classification2 = UniversalTestHelper.CreateClassificationDataObject("HS22", "HSC", "Harmonized Code", "NZ", "New Zealand");
			var classification3 = UniversalTestHelper.CreateClassificationDataObject("HS33", "HSC", "Harmonized Code", "", "");
			var classification4 = UniversalTestHelper.CreateClassificationDataObject("", "HSC", "Harmonized Code", "AU", "Australia");
			var classification5 = UniversalTestHelper.CreateClassificationDataObject("XXXX", "XXX", "XXX Code", "AU", "Australia");

			var classificationDOs = new DataObjectList<Classification>() { Content = content };
			classificationDOs.AddRange(new[] { classification1, classification2, classification3, classification4, classification5 });

			var reader = new ClassificationCollectionReader<JobPackLineHarmonisedCode, JobPackLineHarmonisedCodeCollection>(classificationDOs, logger, Factory, packLine);
			reader.ReadIntoCollection();

			AssertEquals(2, packLine.HarmonisedCodes.Count);
			Assert(packLine.HarmonisedCodes.Any(x => x.JLH_Code == "HS11"));
			Assert(packLine.HarmonisedCodes.Any(x => x.JLH_Code == "HS22"));
			Assert(logger.Logs.Contains("Data object does not contain Code."));
			Assert(logger.Logs.Contains("Data object does not contain Country/Region Code."));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
