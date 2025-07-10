using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestAddInfoChildDataObjectReaderRead()
		{
			var dummy = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var addInfoChild = (DummyBusinessObject)dummy.AddInfoChild;
			addInfoChild.Z0_BitFalse = ZBool.True;
			addInfoChild.Z0_BitFiltered = ZBool.False;
			addInfoChild.Z0_BitTrue = ZBool.False;
			addInfoChild.Z0_Bool = ZBool.False;
			addInfoChild.Z0_IsSystem = ZBool.False;
			var varBinaryMax = ZBlob.FromAscii("OTHER VALUE");
			addInfoChild.Z0_VarBinaryMax = varBinaryMax;
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
			"*AnotherDate=2021-06-14 16:45:26" +
			"*AnotherDecimal=150.5665" +
			"*AnotherNumber=4326" +
			"*BitFalse=N" +
			"*BitFiltered=Y" +
			"*BitTrue=Y" +
			"*Bool=Y" +
			"*Byte=8" +
			"*Code=Z1K" +
			"*Date=2020-11-25 17:36:48" +
			"*DateOnly=2019-01-31" +
			"*Decimal=873.2974" +
			"*Description=HI BOB" +
			"*FK_Code=K%G" +
			"*Long=1503024232323" +
			"*Money=15032.34" +
			"*Number=78234" +
			"*NVarChar=NVARCHAR DATA" +
			"*NVarCharMax=NVARCHARMAX DATA" +
			"*Short=845" +
			"*SmallDateTime=2021-12-28 01:36:46" +
			"*VarBinaryMax=" + ZBlob.FromAscii("SOME OTHER VALUE") +
			"*VarCharMax=G'DAY" +
			"*Xml=<GREETING>HI<GREETING/>" +
			"*IsSystem=Y"));

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new AddInfoChildDataObjectReader(logger, CurrentCompanyHelper);
				reader.ReadIntoBusinessObject(dummy, shipmentData.AddInfoCollection, null);
				CombineAssertions(() =>
				{
					AssertEquals("Z0_AnotherDate", new ZDateTime(2021, 6, 14, 16, 45, 26), addInfoChild.Z0_AnotherDate);
					AssertEquals("Z0_AnotherDecimal", 150.567m, addInfoChild.Z0_AnotherDecimal);
					AssertEquals("Z0_AnotherNumber", 4326, addInfoChild.Z0_AnotherNumber);
					AssertEquals("Z0_BitFalse", ZBool.False, addInfoChild.Z0_BitFalse);
					AssertEquals("Z0_BitFiltered", ZBool.True, addInfoChild.Z0_BitFiltered);
					AssertEquals("Z0_BitTrue", ZBool.True, addInfoChild.Z0_BitTrue);
					AssertEquals("Z0_Bool", ZBool.True, addInfoChild.Z0_Bool);
					AssertEquals("Z0_Byte", (byte)8, addInfoChild.Z0_Byte);
					AssertEquals("Z0_Code", "Z1K", addInfoChild.Z0_Code);
					AssertEquals("Z0_Date", new ZDateTime(2020, 11, 25, 17, 36, 48), addInfoChild.Z0_Date);
					AssertEquals("Z0_DateOnly", new ZDate(2019, 1, 31), addInfoChild.Z0_DateOnly);
					AssertEquals("Z0_Decimal", 873m, addInfoChild.Z0_Decimal);
					AssertEquals("Z0_Description", "HI BOB", addInfoChild.Z0_Description);
					AssertEquals("Z0_FK_Code", "K%G", addInfoChild.Z0_FK_Code);
					AssertEquals("Z0_Long", 1503024232323, addInfoChild.Z0_Long);
					AssertEquals("Z0_Money", 15032.34m, addInfoChild.Z0_Money);
					AssertEquals("Z0_Number", 78234, addInfoChild.Z0_Number);
					AssertEquals("Z0_NVarChar", "NVARCHAR DATA", addInfoChild.Z0_NVarChar);
					AssertEquals("Z0_NVarCharMax", "NVARCHARMAX DATA", addInfoChild.Z0_NVarCharMax);
					AssertEquals("Z0_Short", (short)845, addInfoChild.Z0_Short);
					AssertEquals("Z0_SmallDateTime", new ZDateTime(2021, 12, 28, 1, 36, 46), addInfoChild.Z0_SmallDateTime);
					AssertEquals("Z0_VarBinaryMax", varBinaryMax, addInfoChild.Z0_VarBinaryMax);
					AssertEquals("Z0_VarCharMax", "G'DAY", addInfoChild.Z0_VarCharMax);
					AssertEquals("Z0_Xml", "", addInfoChild.Z0_Xml);
					AssertEquals("Z0_IsSystem", ZBool.False, addInfoChild.Z0_IsSystem);
					AssertEquals("Logs", "", logger.Logs);
				});
			}
		}

		public void TestAddInfoChildDataObjectReaderRead_IAddInfoWithSyncPropertySupporter()
		{
			var parent = Factory.New<BizObjAddInfoWithSyncPropertySupporter>();
			parent.Z0_AnotherDate = ZDateTime.Empty;
			parent.Z0_DateOnly = ZDate.Empty;
			parent.Z0_SmallDateTime = ZDateTime.Empty;
			var addInfoChild = (DummyBusinessObject)parent.AddInfoChild;
			addInfoChild.Z0_DateOnly = ZDate.Empty;
			addInfoChild.Z0_SmallDateTime = ZDateTime.Empty;
			addInfoChild.Z0_SparseDate = ZDate.Empty;
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
			"*AnotherDate=2021-06-14 16:45:26" +
			"*DateOnly=2019-01-31 11:30:35.000" +
			"*DateTime2=2021-10-15 12:35:46" +
			"*SmallDateTime=2021-12-28 01:36:46" +
			"*SparseDate2=2021-11-16 12:35:46"));

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new AddInfoChildDataObjectReader(logger, CurrentCompanyHelper);
				reader.ReadIntoBusinessObject(parent, shipmentData.AddInfoCollection, null);
				CombineAssertions(() =>
				{
					AssertEquals("Leaf table Z0_AnotherDate", new ZDateTime(2021, 6, 14, 16, 45, 26), addInfoChild.Z0_AnotherDate);
					AssertEquals("Leaf table Z0_DateOnly", new ZDate(2019, 1, 31), addInfoChild.Z0_DateOnly);
					AssertEquals("Leaf table Z0_SmallDateTime", new ZDateTime(2021, 12, 28, 1, 36, 46), addInfoChild.Z0_SmallDateTime);
					AssertEquals("Leaf table Z0_SparseDate Sync property = SparseDate2", new ZDate(2021, 11, 16), addInfoChild.Z0_SparseDate);
					AssertEquals("Parent Z0_AnotherDate updated as is wrapped property leaf.Z0_AnotherDate", new ZDateTime(2021, 6, 14, 16, 45, 26), parent.Z0_AnotherDate);
					AssertEquals("Parent Z0_DateOnly not updated as AddInfoChildDataObjectReader not responsible", ZDate.Empty, parent.Z0_DateOnly);
					AssertEquals("Parent Z0_SmallDateTime not updated as AddInfoChildDataObjectReader not responsible", ZDateTime.Empty, parent.Z0_SmallDateTime);
					AssertEquals("Parent Z0_SparseDateWrapped updated as is wrapped property leaf.Z0_SparseDate", new ZDateTime(2021, 11, 16), parent.Z0_SparseDateWrapped);
				});
			}
		}

		public void TestAddInfoChildDataObjectReaderStringValueIsTruncated()
		{
			var dummy = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var value = "Z1K34343".PadRight(DummyBusinessObject.Schema.Z0_NVarCharMaxLength + 1, '1');
			var truncatedValue = value.Substring(0, DummyBusinessObject.Schema.Z0_NVarCharMaxLength);
			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection($"*NVarChar={value}*Description=HI BOB"));
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new AddInfoChildDataObjectReader(logger, CurrentCompanyHelper);
				reader.ReadIntoBusinessObject(dummy, shipmentData.AddInfoCollection, null);
				CombineAssertions(() =>
				{
					var addInfoChild = (DummyBusinessObject)dummy.AddInfoChild;
					AssertEquals(truncatedValue, addInfoChild.Z0_NVarChar);
					AssertEquals("Warning - Attempted to insert 21 characters into Field [Z0_NVarChar] which has a maximum length of 20 characters. Field was truncated.", logger.Logs);
				});
			}
		}

		public void TestCommonAddInfoDataObjectReaderForDateValue()
		{
			var dummy = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			dummy.Z0_VarCharMax = "*String=Hi*Guid=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA";
			var addInfo = dummy.AddInfoChild;
			addInfo.Z0_DateOnly = new ZDate(2021, 4, 15);
			addInfo.Z0_Date = new ZDateTime(2021, 4, 15);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection("DateOnly=DateOnly*Date=Date"));
			logger.ClearLogs();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new AddInfoChildDataObjectReader(logger, CurrentCompanyHelper);
				reader.ReadIntoBusinessObject(dummy, shipmentData.AddInfoCollection, null);

				CombineAssertions(() =>
				{
					AssertEquals("UZ_DateOnly not changed", new ZDate(2021, 4, 15), addInfo.Z0_DateOnly);
					AssertEquals("UZ_Date not changed", new ZDateTime(2021, 4, 15), addInfo.Z0_Date);
					AssertContains("Invalid Date has error", "Error - Cannot set Invalid Date to Column DummyBizo.Z0_DateOnly.", logger.Logs);
					AssertContains("Invalid DateTime has warnning", "Warning - Cannot set Column DummyBizo.Z0_Date to '<Invalid>' - Value is out of range.", logger.Logs);
				});
			}
		}
	}
}
