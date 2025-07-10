using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ClassificationCollectionReader<JobPackLineHarmonisedCode, JobPackLineHarmonisedCodeCollection>))]
	sealed class ClassificationCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var logger = new TestErrorLogger();
			var shipment = Factory.New<CommonShipment>();
			var packingLine = shipment.OuterPackLines.AddNew();
			var harmonisedCode = packingLine.HarmonisedCodes.AddNew();
			harmonisedCode.JLH_Code = "HS1";
			harmonisedCode.JLH_RN_NKCountry = "US";

			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.GoodsDescription = "Line001";

			var classification1 = UniversalTestHelper.CreateClassificationDataObject("HS1", "HSC", "Harmonized Code", "AU", "Australia");
			var classification2 = UniversalTestHelper.CreateClassificationDataObject("HS2", "HSC", "Harmonized Code", "SG", "Singapore");

			dataObject.SetClassificationCollection(() => new DataObjectList<Classification>());
			dataObject.ClassificationCollection.AddRange(new[] { classification1, classification2 });
			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => packingLine, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("PackingLine should be added", 1, shipment.OuterPackLines.Count);
			AssertEquals("Only one harmonised code should be added", 2, shipment.OuterPackLines[0].HarmonisedCodes.Count);

			var harmoisedCode1 = shipment.OuterPackLines[0].HarmonisedCodes[0];
			AssertEquals("HS1", harmoisedCode1.JLH_Code);
			AssertEquals("AU", harmoisedCode1.JLH_RN_NKCountry);

			var harmoisedCode2 = shipment.OuterPackLines[0].HarmonisedCodes[1];
			AssertEquals("HS2", harmoisedCode2.JLH_Code);
			AssertEquals("SG", harmoisedCode2.JLH_RN_NKCountry);
		}

		public void TestReadIntoCollection_Partial()
		{
			var logger = new TestErrorLogger();
			var shipment = Factory.New<CommonShipment>();
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.GoodsDescription = "Line001";

			var classification1 = UniversalTestHelper.CreateClassificationDataObject("HS1", "HSC", "Harmonized Code", "AU", "Australia");
			var classification2 = UniversalTestHelper.CreateClassificationDataObject("HS2", "HSC", "Harmonized Code", "AU", "Australia");
			var classification3 = UniversalTestHelper.CreateClassificationDataObject("HS3", "XXX", "XXX Code", "AU", "Australia");

			dataObject.SetClassificationCollection(() => new DataObjectList<Classification>());
			dataObject.ClassificationCollection.Content = CollectionContent.Partial;
			dataObject.ClassificationCollection.AddRange(new[] { classification1, classification2, classification3 });
			var readerToTest = new PackingLineDataObjectReader<PackLine, CommonShipment>(dataObject, logger, Factory, shipment, dataObj => null, shipment.OuterPackLines.AddNew);
			readerToTest.ReadIntoBusinessObject();

			AssertEquals("PackingLine should be added", 1, shipment.OuterPackLines.Count);
			AssertEquals("Harmonised codes should be added", 2, shipment.OuterPackLines[0].HarmonisedCodes.Count);

			var harmoisedCode1 = shipment.OuterPackLines[0].HarmonisedCodes[0];
			AssertEquals("HS1", harmoisedCode1.JLH_Code);
			AssertEquals("AU", harmoisedCode1.JLH_RN_NKCountry);

			var harmoisedCode2 = shipment.OuterPackLines[0].HarmonisedCodes[1];
			AssertEquals("HS2", harmoisedCode2.JLH_Code);
			AssertEquals("AU", harmoisedCode2.JLH_RN_NKCountry);
		}
	}
}
