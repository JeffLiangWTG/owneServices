using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public sealed class AddInfoDataTransferToolTest : TestCaseWithFactory
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
			new AddInfoDataTransferTool(TestAddInfo.Schema.PK.Substring(0, 3), (x) => true).ImportAddInfos(zPropertyInfo, addCustomsDetails, context);

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

		public void TestImportAddInfosFromIAddInfoSchemaProvider()
		{
			var dummy = Factory.New<DummyZZBizo>();
			var interchange = new Xsd.XmlInterchange();
			var addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();

			var addCustomsInfo = addCustomsDetails.AddNew();
			addCustomsInfo.CustomsDetailType = "AddInfoString35";
			addCustomsInfo.CustomsDetailValue = "String Imported";

			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			new AddInfoDataTransferTool(ZZDummyBizoSchema.Constants.Prefix, _ => true).ImportAddInfos(dummy, addCustomsDetails, context);

			AssertEquals("String Imported", dummy.Z0_AddInfoString35);
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
			new AddInfoDataTransferTool(TestAddInfo.Schema.PK.Substring(0, 3), (x) => true).ImportAddInfos(zPropertyInfo, addCustomsDetails, context);

			addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();
			AssertEquals("PreCondition: Values removed from Xml AddInfo object", 0, addCustomsDetails.Count);

			new AddInfoDataTransferTool(TestAddInfo.Schema.PK.Substring(0, 3), (x) => true).ExportAddInfos(addCustomsDetails, zPropertyInfo);

			AssertEquals("String", addCustomsDetails[1].CustomsDetailType);
			AssertEquals("String Imported", addCustomsDetails[1].CustomsDetailValue);
		}

		public void TestExportAddInfosFromIAddInfoSchemaProvider()
		{
			var dummy = Factory.New<DummyZZBizo>();
			var interchange = new Xsd.XmlInterchange();
			var addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();

			var addCustomsInfo = addCustomsDetails.AddNew();
			addCustomsInfo.CustomsDetailType = "AddInfoString35";
			addCustomsInfo.CustomsDetailValue = "String Imported";

			var context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			new AddInfoDataTransferTool(ZZDummyBizoSchema.Constants.Prefix, _ => true).ImportAddInfos(dummy, addCustomsDetails, context);

			addCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();
			AssertEquals("PreCondition: Values removed from Xml AddInfo object", 0, addCustomsDetails.Count);

			new AddInfoDataTransferTool(ZZDummyBizoSchema.Constants.Prefix, _ => true).ExportAddInfos(addCustomsDetails, dummy);

			AssertEquals("AddInfoString35", addCustomsDetails[1].CustomsDetailType);
			AssertEquals("String Imported", addCustomsDetails[1].CustomsDetailValue);
			foreach (Xsd.AdditionalCustomsInformation addCustomsDetail in addCustomsDetails)
			{
				AssertNotEquals("PK not included", "PK", addCustomsDetail.CustomsDetailType);
				AssertNotEquals("ClusterKey not included", "ClusterKey", addCustomsDetail.CustomsDetailType);
			}
		}

		public static bool ItemInXmlAddInfoCollection(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			var itemFound = false;

			foreach (Xsd.AdditionalCustomsInformation addCusInfo in addCusInfos)
			{
				if (addCusInfo.CustomsDetailType == itemType)
				{
					itemFound = true;
				}
			}

			return itemFound;
		}

		public static string GetXmlAddInfoValue(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			var itemValue = string.Empty;

			foreach (Xsd.AdditionalCustomsInformation addCusInfo in addCusInfos)
			{
				if (addCusInfo.CustomsDetailType == itemType)
				{
					itemValue = addCusInfo.CustomsDetailValue;
				}
			}

			return itemValue;
		}
	}
}
