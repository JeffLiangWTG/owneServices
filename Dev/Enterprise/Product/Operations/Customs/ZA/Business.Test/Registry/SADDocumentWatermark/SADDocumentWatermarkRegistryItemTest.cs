using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(SADDocumentWatermarkRegistryItem))]
	sealed class SADDocumentWatermarkRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<SADDocumentWatermarkCollection>
	{
		public override void TestCasting()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1", "Test1");
			testHelper.CreateCustomsStatusCusCodeEntry("2", "Test2");
			Factory.Save();

			base.TestCasting();
		}

		protected override StronglyTypedRegistryItem<SADDocumentWatermarkCollection, SADDocumentWatermarkCollection> GetNewRegistryItem()
		{
			return new SADDocumentWatermarkRegistryItem("", null, null, null);
		}

		protected override SADDocumentWatermarkCollection ValidValue
		{
			get
			{
				var collection = new SADDocumentWatermarkCollection();
				var watermark1 = collection.AddNew();
				var watermark2 = collection.AddNew();

				watermark1.EntryStatusCode = "1";
				watermark2.EntryStatusCode = "2";
				watermark2.WatermarkText = "Watermarked";

				return collection;
			}
		}
	}

	[TestedType(typeof(SADDocumentWatermarkRegistryDataType))]
	sealed class SADDocumentWatermarkRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SADDocumentWatermarkRegistryDataType>
	{
		public override void TestGetSetValidValues()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1", "Test1");
			testHelper.CreateCustomsStatusCusCodeEntry("2", "Test2");
			testHelper.CreateCustomsStatusCusCodeEntry("3", "Test3");
			factory.Save();

			base.TestGetSetValidValues();
		}

		protected override string ExpectedEditorName
		{
			get { return "SADDocumentWatermarkRegistryItemEditor"; }
		}

		protected override SADDocumentWatermarkRegistryDataType GetNewDataType()
		{
			return new SADDocumentWatermarkRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new SADDocumentWatermarkCollection();
			var watermark1 = collection1.AddNew();
			var watermark2 = collection1.AddNew();

			watermark1.EntryStatusCode = "1";
			watermark1.WatermarkText = "Watermarked1";
			watermark2.EntryStatusCode = "2";

			var collection2 = new SADDocumentWatermarkCollection();
			var watermark3 = collection2.AddNew();

			watermark3.EntryStatusCode = "3";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new SADDocumentWatermarkRegistryDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new SADDocumentWatermarkRegistryDataType().Serialise(collection2))
			};
		}
	}
}
