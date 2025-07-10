using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestAddInfoDataObjectReaderRead()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var schDArrivalName = USAddInfoSchema.US_SchDArrival.Name.Substring(3);
				var importConveyanceName = USAddInfoSchema.US_ImportConveyanceName.Name.Substring(3);
				var drwExportingCarrier = USAddInfoSchema.US_UI_NKCarrierSCAC.Name.Substring(3);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_AddInfo = string.Format("EXISTINGVALUE=DO NOT REMOVE*{0}=TBREM*{1}=Hi*{2}=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA", schDArrivalName, importConveyanceName, drwExportingCarrier);
				var addInfoManager = declaration as IAddInfoManager;
				var row = (IColumnIndexer)((IBusinessObjectInternals)declaration).Row;
				var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=Hello*{1}=", schDArrivalName, importConveyanceName)));
				using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, JobDeclarationSchema.JE_AddInfo, Array.Empty<string>());
					reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
					AssertEquals(string.Format("EXISTINGVALUE=DO NOT REMOVE*{0}=Hello*{1}=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA", schDArrivalName, drwExportingCarrier), declaration.JE_AddInfo);
				}
			}
		}

		public void TestAddInfoDataObjectReaderForBoolValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var schDArrivalName = USAddInfoSchema.US_SchDArrival.Name.Substring(3);
				var importConveyanceName = USAddInfoSchema.US_ImportConveyanceName.Name.Substring(3);
				var drwExportingCarrier = USAddInfoSchema.US_UI_NKCarrierSCAC.Name.Substring(3);
				var accLinq = USAddInfoSchema.US_AccLiqReq.Name.Substring(3);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_AddInfo = string.Format("EXISTINGVALUE=DO NOT REMOVE*{0}=TBREM*{1}=Hi*{2}=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA", schDArrivalName, importConveyanceName, drwExportingCarrier);
				var addInfoManager = declaration as IAddInfoManager;
				var row = (IColumnIndexer)((IBusinessObjectInternals)declaration).Row;
				var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("{0}=Hello*{1}=*{2}=T", schDArrivalName, importConveyanceName, accLinq)));
				using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, JobDeclarationSchema.JE_AddInfo, Array.Empty<string>());
					AssertNoExceptionThrown(() => { reader.ReadIntoRow(addInfoManager, row, shipmentData, null); });
				}
			}
		}

		public void TestTestAddInfoDataObjectReaderForDate_Long()
		{
			var dummy = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			dummy.Z0_VarCharMax = "*String=Hi*Guid=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA";
			var addInfo = dummy.AddInfo;
			addInfo.UZ_DateOnly = new ZDate(2021, 4, 15);
			addInfo.UZ_Long = 1234243233;
			var addInfoManager = dummy;
			var row = (IColumnIndexer)((IBusinessObjectInternals)dummy).Row;
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection("DateOnly=2021-05-21*Long=62342323423"));
			logger.ClearLogs();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, DummyBizoSchema.Z0_VarCharMax, Array.Empty<string>());
				reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
				CombineAssertions(() =>
				{
					AssertEquals("UZ_DateOnly", new ZDate(2021, 5, 21), addInfo.UZ_DateOnly);
					AssertEquals("UZ_Long", 62342323423, addInfo.UZ_Long);
				});
			}
		}

		public void TestAddInfoStringValueIsTruncated()
		{
			var dummy = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			dummy.Z0_VarCharMax = "String=Hi*Guid=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA";
			var addInfoManager = dummy;
			var row = (IColumnIndexer)((IBusinessObjectInternals)dummy).Row;
			var message = "HELLO".PadRight(TestAddInfo.Schema.UZ_StringMaxLength, 'A');
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("String={0}*Int=3", message + "B")));
			logger.ClearLogs();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, DummyBizoSchema.Z0_VarCharMax, Array.Empty<string>());
				reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
				AssertEquals(string.Format("String={0}*Guid=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA*Int=3", message), dummy.Z0_VarCharMax);
				AssertEquals("Warning - The maximum length of Add Info 'String' is 50 characters, but 'HELLOAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB' was specified; value has been truncated.", logger.Logs);

				shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection("String=Bye*Int=9"));
				logger.ClearLogs();
				reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
				AssertEquals("String=Bye*Guid=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA*Int=9", dummy.Z0_VarCharMax);
				AssertEquals("", logger.Logs);
			}
		}

		public void TestAddInfoThrowZTypeValueException()
		{
			var addInfoManager = Factory.New<DummyBusinessObjectWithIAddInfoManager>();
			addInfoManager.Z0_VarCharMax = "*String=Hi*Guid=C6F73353-B2E4-40A8-B0EB-FFEDAD550CDA";
			var row = (IColumnIndexer)((IBusinessObjectInternals)addInfoManager).Row;
			var message = "HELLO".PadRight(TestAddInfo.Schema.UZ_StringMaxLength, 'A');
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(string.Format("String={0}*Int=3", message + "B")));

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var mockReader = new Mock<AddInfoDataObjectReader>(logger, CurrentCompanyHelper, DummyBizoSchema.Z0_VarCharMax, Array.Empty<string>(), new Dictionary<string, SchemaColumn>());
				mockReader.CallBase = true;
				mockReader
					.Protected()
					.Setup("SetValue", ItExpr.IsAny<IAddInfoManager>(), ItExpr.IsAny<IColumnIndexer>(), ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>(), ItExpr.IsAny<string>(), ItExpr.IsAny<Dictionary<string, ValueSetter>>())
					.Throws(new ZTypeValueException("Cannot initialise a CargoWise.Types.ZBool with <> (System.String)."));
				var reader = mockReader.Object;

				shipmentData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection("String=Bye*Int=9"));
				AssertEquals(2, shipmentData.AddInfoCollection.Count);
				logger.ClearLogs();
				AssertNoExceptionThrown(() => reader.ReadIntoRow(addInfoManager, row, shipmentData, null));
				AssertContains("Cannot initialise a CargoWise.Types.ZBool with <> (System.String).", logger.Logs);
				mockReader
					.Protected()
					.Verify("SetValue", Times.Exactly(2), ItExpr.IsAny<IAddInfoManager>(), ItExpr.IsAny<IColumnIndexer>(), ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>(), ItExpr.IsAny<string>(), ItExpr.IsAny<Dictionary<string, ValueSetter>>());
			}
		}

		public void TestReadIntoRow_IAddInfoChildSupporter_IAddInfoWithSyncPropertySupporter()
		{
			var parent = Factory.New<BizObjAddInfoWithSyncPropertySupporter>();
			parent.Z0_AnotherDate = ZDateTime.Empty;
			parent.Z0_DateOnly = ZDate.Empty;
			parent.Z0_SmallDateTime = ZDate.Empty;
			var addInfoChild = (DummyBusinessObject)parent.AddInfoChild;
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
			"*DateOnly=2019-01-31 11:30:35.000" +
			"*DateTime2=2021-10-15 12:35:46" +
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
			"*IsSystem=Y"));

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var addInfoManager = parent;
				var row = (IColumnIndexer)((IBusinessObjectInternals)parent).Row;
				var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, DummyBizoSchema.Z0_VarCharMax, Array.Empty<string>());
				reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
				CombineAssertions(() =>
				{
					AssertEquals("Leaf table Z0_AnotherDate", new ZDateTime(2021, 6, 14, 16, 45, 26), addInfoChild.Z0_AnotherDate);
					AssertEquals("Leaf table Z0_AnotherDecimal", 150.567m, addInfoChild.Z0_AnotherDecimal);
					AssertEquals("Leaf table Z0_AnotherNumber", 4326, addInfoChild.Z0_AnotherNumber);
					AssertEquals("Leaf table Z0_BitFalse", ZBool.False, addInfoChild.Z0_BitFalse);
					AssertEquals("Leaf table Z0_BitFiltered", ZBool.True, addInfoChild.Z0_BitFiltered);
					AssertEquals("Leaf table Z0_BitTrue", ZBool.True, addInfoChild.Z0_BitTrue);
					AssertEquals("Leaf table Z0_Bool", ZBool.True, addInfoChild.Z0_Bool);
					AssertEquals("Leaf table Z0_Byte", (byte)8, addInfoChild.Z0_Byte);
					AssertEquals("Leaf table Z0_Code", "Z1K", addInfoChild.Z0_Code);
					AssertEquals("Leaf table Z0_Date - still exists in the AddInfo Schema", ZDateTime.Empty, addInfoChild.Z0_Date);
					AssertEquals("Leaf table Z0_DateOnly - still exists in the AddInfo Schema", ZDate.Empty, addInfoChild.Z0_DateOnly);
					AssertEquals("Leaf table Z0_Decimal - still exists in the AddInfo Schema", ZDecimal.Zero, addInfoChild.Z0_Decimal);
					AssertEquals("Leaf table Z0_Description", "HI BOB", addInfoChild.Z0_Description);
					AssertEquals("Leaf table Z0_FK_Code", "K%G", addInfoChild.Z0_FK_Code);
					AssertEquals("Leaf table Z0_Long - still exists in the AddInfo Schema", ZLong.Zero, addInfoChild.Z0_Long);
					AssertEquals("Leaf table Z0_Money", 15032.34m, addInfoChild.Z0_Money);
					AssertEquals("Leaf table Z0_Number", 78234, addInfoChild.Z0_Number);
					AssertEquals("Leaf table Z0_NVarChar", "NVARCHAR DATA", addInfoChild.Z0_NVarChar);
					AssertEquals("Leaf table Z0_NVarCharMax", "NVARCHARMAX DATA", addInfoChild.Z0_NVarCharMax);
					AssertEquals("Leaf table Z0_Short - still exists in the AddInfo Schema", ZShort.Zero, addInfoChild.Z0_Short);
					AssertEquals("Leaf table Z0_SmallDateTime", new ZDateTime(2021, 12, 28, 1, 36, 46), addInfoChild.Z0_SmallDateTime);
					AssertEquals("Leaf table Z0_VarBinaryMax", varBinaryMax, addInfoChild.Z0_VarBinaryMax);
					AssertEquals("Leaf table Z0_VarCharMax", "G'DAY", addInfoChild.Z0_VarCharMax);
					AssertEquals("Leaf table Z0_IsSystem - System Fields not updated", ZBool.False, addInfoChild.Z0_IsSystem);
					AssertEquals("parent Z0_AnotherDate updated from AddInfoSyncProperty - AnotherDate", new ZDateTime(2021, 6, 14, 16, 45, 26), parent.Z0_AnotherDate);
					AssertEquals("parent Z0_DateOnly updated from AddInfoSyncProperty - DateTime2", new ZDate(2021, 10, 15), parent.Z0_DateOnly);
					AssertEquals("parent Z0_SmallDateTime updated from AddInfoSyncProperty - SmallDateTime", new ZDateTime(2021, 12, 28, 1, 36, 46), parent.Z0_SmallDateTime);
				});
			}
		}

		public void TestReadIntoRow_IAddInfoChildSupporter_IAddInfoManagerWithSchema()
		{
			var dummy = Factory.New<BizObjWithIAddInfoManagerWithSchema>();
			var addInfoManager = dummy;
			var row = (IColumnIndexer)((IBusinessObjectInternals)dummy).Row;
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoCollection(CreateAddInfoData);
			var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, DummyBizoSchema.Z0_VarCharMax, Array.Empty<string>());
			reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
			CombineAssertions(() =>
			{
				AssertTestAddInfo(dummy.AddInfo, ZBool.False, ZDateTime.Empty, ZDate.Empty, ZDecimal.Zero, ZGuid.Empty, ZInt.Zero, ZLong.Zero, ZString.Empty, ZShort.Zero, ZString.Empty, ZString.Empty);
				AssertDummyBizo(dummy.AddInfoChild, new ZDateTime(2021, 6, 14, 16, 45, 26), 150.567m, 4326, ZBool.False, ZBool.True, ZBool.True,
					8, "Z2K", ZDateTime.Empty, ZDate.Empty, ZDateTimeOffset.Empty, ZDecimal.Zero, "HELLO WORLD", "K%G",
					ZGeography.Empty, ZLong.Zero, 15032.34m, 78234, "NVARCHAR DATA", "NVARCHARMAX DATA", ZShort.Zero, new ZDateTime(2021, 12, 28, 1, 36, 46),
					ZBlob.Empty, ZString.Empty, ZString.Empty);
			});
		}

		public void TestReadIntoRow_IAddInfoChildSupporter_IAddInfoManager()
		{
			var dummy = Factory.New<BizObjWithIAddInfoManager>();
			var addInfoManager = dummy;
			var row = (IColumnIndexer)((IBusinessObjectInternals)dummy).Row;
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.SetAddInfoCollection(CreateAddInfoData);
			var reader = new AddInfoDataObjectReader(logger, CurrentCompanyHelper, DummyBizoSchema.Z0_VarCharMax, Array.Empty<string>());
			reader.ReadIntoRow(addInfoManager, row, shipmentData, null);
			CombineAssertions(() =>
			{
				AssertTestAddInfo(dummy.AddInfo, ZBool.False, ZDateTime.Empty, ZDate.Empty, ZDecimal.Zero, ZGuid.Empty, ZInt.Zero, ZLong.Zero, ZString.Empty, ZShort.Zero, ZString.Empty, ZString.Empty);
				AssertDummyBizo(dummy.AddInfoChild, new ZDateTime(2021, 6, 14, 16, 45, 26), 150.567m, 4326, ZBool.False, ZBool.True, ZBool.True,
					8, "Z2K", ZDateTime.Empty, ZDate.Empty, ZDateTimeOffset.Empty, ZDecimal.Zero, "HELLO WORLD", "K%G",
					ZGeography.Empty, ZLong.Zero, 15032.34m, 78234, "NVARCHAR DATA", "NVARCHARMAX DATA", ZShort.Zero, new ZDateTime(2021, 12, 28, 1, 36, 46),
					ZBlob.Empty, ZString.Empty, ZString.Empty);
			});
		}

		void AssertDummyBizo(DummyBusinessObject bizObj, ZDateTime expectedAnotherDate, ZDecimal expectedAnotherDecimal, ZInt expectedAnotherNumber, ZBool expectedBitFalse, ZBool expectedBitFiltered, ZBool expectedBool,
			ZByte expectedByte, ZString expectedCode, ZDateTime expectedDate, ZDate expectedDateOnly, ZDateTimeOffset expectedDateTimeOffset, ZDecimal expectedDecimal, ZString expectedDescription, ZString expectedFK_Code,
			ZGeography expectedGeography, ZLong expectedLong, ZDecimal expectedMoney, ZInt expectedNumber, ZString expectedNVarChar, ZString expectedNVarCharMax, ZShort expectedShort, ZDateTime expectedSmallDateTime,
			ZBlob expectedVarBinaryMax, ZString expectedVarCharMax, ZString expectedXml)
		{
			AssertEquals("Z0_AnotherDate", expectedAnotherDate, bizObj.Z0_AnotherDate);
			AssertEquals("Z0_AnotherDecimal", expectedAnotherDecimal, bizObj.Z0_AnotherDecimal);
			AssertEquals("Z0_AnotherNumber", expectedAnotherNumber, bizObj.Z0_AnotherNumber);
			AssertEquals("Z0_BitFalse", expectedBitFalse, bizObj.Z0_BitFalse);
			AssertEquals("Z0_BitFiltered", expectedBitFiltered, bizObj.Z0_BitFiltered);
			AssertEquals("Z0_Bool", expectedBool, bizObj.Z0_Bool);
			AssertEquals("Z0_Byte", expectedByte, bizObj.Z0_Byte);
			AssertEquals("Z0_Code", expectedCode, bizObj.Z0_Code);
			AssertEquals("Z0_Date", expectedDate, bizObj.Z0_Date);
			AssertEquals("Z0_DateOnly", expectedDateOnly, bizObj.Z0_DateOnly);
			AssertEquals("Z0_DateTimeOffset", expectedDateTimeOffset, bizObj.Z0_DateTimeOffset);
			AssertEquals("Z0_Decimal", expectedDecimal, bizObj.Z0_Decimal);
			AssertEquals("Z0_Description", expectedDescription, bizObj.Z0_Description);
			AssertEquals("Z0_FK_Code", expectedFK_Code, bizObj.Z0_FK_Code);
			AssertEquals("Z0_Geography", expectedGeography, bizObj.Z0_Geography);
			AssertEquals("Z0_Long", expectedLong, bizObj.Z0_Long);
			AssertEquals("Z0_Money", expectedMoney, bizObj.Z0_Money);
			AssertEquals("Z0_Number", expectedNumber, bizObj.Z0_Number);
			AssertEquals("Z0_NVarChar", expectedNVarChar, bizObj.Z0_NVarChar);
			AssertEquals("Z0_NVarCharMax", expectedNVarCharMax, bizObj.Z0_NVarCharMax);
			AssertEquals("Z0_Short", expectedShort, bizObj.Z0_Short);
			AssertEquals("Z0_SmallDateTime", expectedSmallDateTime, bizObj.Z0_SmallDateTime);
			AssertEquals("Z0_VarBinaryMax", expectedVarBinaryMax, bizObj.Z0_VarBinaryMax);
			AssertEquals("Z0_VarCharMax", expectedVarCharMax, bizObj.Z0_VarCharMax);
			AssertEquals("Z0_Xml", expectedXml, bizObj.Z0_Xml);
		}

		void AssertTestAddInfo(TestAddInfo addInfo, ZBool expectedBoolean, ZDateTime expectedDate, ZDate expectedDateOnly, ZDecimal expectedDecimal,
			ZGuid expectedGuid, ZInt expectedInt, ZLong expectedLong, ZString expectedNString, ZShort expectedShort, ZString expectedString, ZString expectedSomeProperty)
		{
			AssertEquals("UZ_Boolean", expectedBoolean, addInfo.UZ_Boolean);
			AssertEquals("UZ_Date", expectedDate, addInfo.UZ_Date);
			AssertEquals("UZ_DateOnly", expectedDateOnly, addInfo.UZ_DateOnly);
			AssertEquals("UZ_Decimal", expectedDecimal, addInfo.UZ_Decimal);
			AssertEquals("UZ_Guid", expectedGuid, addInfo.UZ_Guid);
			AssertEquals("UZ_Int", expectedInt, addInfo.UZ_Int);
			AssertEquals("UZ_Long", expectedLong, addInfo.UZ_Long);
			AssertEquals("UZ_NString", expectedNString, addInfo.UZ_NString);
			AssertEquals("UZ_Short", expectedShort, addInfo.UZ_Short);
			AssertEquals("UZ_String", expectedString, addInfo.UZ_String);
			AssertEquals("UZ_SomeProperty", expectedSomeProperty, addInfo.UZ_SomeProperty);
		}

		List<AddInfo> CreateAddInfoData()
		{
			return AddInfoCollectionCreator.CreateCollection(
				"AnotherDate=2021-06-14 16:45:26.000"
				+ "*AnotherDecimal=150.567"
				+ "*AnotherNumber=4326"
				+ "*BitFalse=N"
				+ "*BitFiltered=Y"
				+ "*BitTrue=Y"
				+ "*Bool=Y"
				+ "*Boolean=Y"
				+ "*Byte=8"
				+ "*Code=Z2K"
				+ "*Date=2020-11-25 17:36:48.000"
				+ "*DateOnly=2019-01-31"
				+ "*DateTimeOffset=15/05/2017 12:00:00 AM +03:00"
				+ "*Decimal=873"
				+ "*Description=HELLO WORLD"
				+ "*FK_Code=K%G"
				+ "*Geography=POINT (-121 48)"
				+ "*Guid=089B1C39-1ADD-4502-8943-BC0F84DCA635"
				+ "*Int=47823"
				+ "*Long=1503024232323"
				+ "*Money=15032.34"
				+ "*NString=G'DAY"
				+ "*NVarChar=NVARCHAR DATA"
				+ "*NVarCharMax=NVARCHARMAX DATA"
				+ "*Number=78234"
				+ "*Short=845"
				+ "*SmallDateTime=2021-12-28 01:36:46.000"
				+ "*SomeProperty=BYE"
				+ "*String=HELLO"
				+ "*Xml=<GREETING>HI<GREETING/>"
			);
		}
	}
}
