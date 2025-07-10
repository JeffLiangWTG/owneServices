using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class CustomsValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestImportAddInfos()
		{
			var dummy = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			var testAddInfo = new TestAddInfo(dummy);
			var interchange = new Xsd.XmlInterchange();
			var zPropertyInfo = testAddInfo.ZPropertyInfoHash;
			var addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();

			var addCustomsInfo = addCustomsDetails.AddNew();
			addCustomsInfo.CustomsDetailType = "String";
			addCustomsInfo.CustomsDetailValue = "String Imported";

			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			customsValueObjectDataAdapter.ImportAddInfos(zPropertyInfo, addCustomsDetails, context);

			bool itemFound = false;
			foreach (ZPropertyInfo zPropertyInfoItem in zPropertyInfo)
			{
				if (zPropertyInfoItem is ZPropertyInfoString && zPropertyInfoItem.Name == "UZ_String")
				{
					AssertEquals("String Imported", ((ZPropertyInfoString)zPropertyInfoItem).Value);
					itemFound = true;
				}
			}

			Assert("String not imported", itemFound);
		}

		public void TestExportAddInfos()
		{
			var dummy = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			var testAddInfo = new TestAddInfo(dummy);
			var interchange = new Xsd.XmlInterchange();
			var zPropertyInfo = testAddInfo.ZPropertyInfoHash;
			var addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();

			var addCustomsInfo = addCustomsDetails.AddNew();
			addCustomsInfo.CustomsDetailType = "String";
			addCustomsInfo.CustomsDetailValue = "String Imported";

			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			customsValueObjectDataAdapter.ImportAddInfos(zPropertyInfo, addCustomsDetails, context);

			addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();
			AssertEquals("PreCondition: Values removed from Xml AddInfo object", 0, addCustomsDetails.Count);

			customsValueObjectDataAdapter.ExportAddInfos(addCustomsDetails, zPropertyInfo);

			AssertEquals("String", addCustomsDetails[1].CustomsDetailType);
			AssertEquals("String Imported", addCustomsDetails[1].CustomsDetailValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			customsValueObjectDataAdapter = new CustomsValueObjectDataAdapterTestClass<BaseJobDeclaration, Xsd.Consol>();
		}

		sealed class CustomsValueObjectDataAdapterTestClass<TBusinessObject, TValueObject> : CustomsBusinessObjectValueObjectDataAdapter<TBusinessObject, TValueObject>
			where TBusinessObject : BusinessObject
			where TValueObject : IValueObject
		{
			public new void ImportAddInfos(ZPropertyInfoHashtable zPropertyInfoHash, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
			{
				base.ImportAddInfos(zPropertyInfoHash, addCustomsDetails, context);
			}

			public new void ExportAddInfos(Xsd.AdditionalCustomsInformationCollection xmlAddInfo, ZPropertyInfoHashtable zPropertyInfoHash)
			{
				base.ExportAddInfos(xmlAddInfo, zPropertyInfoHash);
			}

			public override string RootElementName => "";

			public override string RootCollectionElementName => "";

			public override System.Xml.Schema.XmlSchema Schema => null;

			public override System.Xml.Schema.XmlSchema CollectionSchema => null;

			protected override string AddInfoPrefix => "UZ_";

			protected override TBusinessObject FindBusinessObject(TValueObject value, IValueObjectImportContext context) => null;

			protected override void ImportFromValueObjectCore(TBusinessObject bizObj, TValueObject value, IValueObjectImportContext context)
			{
			}

			protected override void ExportToValueObjectCore(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context)
			{
			}
		}

		CustomsValueObjectDataAdapterTestClass<BaseJobDeclaration, Xsd.Consol> customsValueObjectDataAdapter;
	}
}
