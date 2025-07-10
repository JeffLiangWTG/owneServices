using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class AddInfoCollectionCreatorTest : TestCaseWithFactory
	{
		public void TestGuidValueShouldBeIgnored()
		{
			var collection = AddInfoCollectionCreator.CreateCollection("DUMMY=8108aee0-a46d-41a4-abc0-507ca0f16b9f*DUMMY2=1.3");
			AssertEquals(1, collection.Count);
			AssertContents(collection[0], "DUMMY2", "1.3");
		}

		public void TestCreateCollection_IAddInfoManagerWithSchema()
		{
			var dummyBizObj = Factory.New<BizObjWithIAddInfoManagerWithSchema>();
			SetupDummyBusinessObject(dummyBizObj.AddInfoChild);
			dummyBizObj.AddInfo.UZ_String = "HELLO WORLD";
			dummyBizObj.AddInfo.UZ_Long = 2563456;
			dummyBizObj.AddInfo.UZ_Date = new ZDateTime(2019, 10, 15);
			var collection = AddInfoCollectionCreator.CreateCollection(dummyBizObj, DummyBizoSchema.Z0_VarCharMax);
			var expectedExtraAddInfos = new[]
			{
				"Code=Z1K",
				"Date=2019-10-15 00:00:00.000",
				"DateOnly=2019-01-31",
				"Description=HI BOB",
				"Long=1503024232323",
				"Short=845"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollectionFromSyncAddInfosAndAddInfoChild_IAddInfoWithSyncPropertySupporter()
		{
			var dummyBizObj = Factory.New<BizObjAddInfoWithSyncPropertySupporter>();
			dummyBizObj.Z0_Date = new ZDateTime(2019, 11, 25, 10, 45, 20);
			dummyBizObj.Z0_AnotherDate = new ZDateTime(2019, 11, 26, 10, 45, 20);
			dummyBizObj.Z0_SmallDateTime = new ZDateTime(2019, 11, 27, 10, 45, 20);
			dummyBizObj.Z0_SparseDateTime = new ZDateTime(2019, 11, 28, 10, 45, 20);
			dummyBizObj.Z0_DateOnly = new ZDate(2019, 12, 30);
			dummyBizObj.Z0_SparseDate = new ZDate(2019, 12, 31);
			var addInfo = dummyBizObj.AddInfo;
			addInfo.UZ_Date = new ZDateTime(2017, 11, 25, 10, 45, 20);
			addInfo.UZ_DateOnly = new ZDate(2017, 12, 25);
			var addInfoChild = (DummyBusinessObject)dummyBizObj.AddInfoChild;
			addInfoChild.Z0_Date = new ZDateTime(2018, 11, 25, 10, 45, 20);
			addInfoChild.Z0_AnotherDate = new ZDateTime(2018, 11, 26, 10, 45, 20);
			addInfoChild.Z0_SmallDateTime = new ZDateTime(2018, 11, 27, 10, 45, 20);
			addInfoChild.Z0_SparseDateTime = new ZDateTime(2018, 11, 28, 10, 45, 20);
			addInfoChild.Z0_SparseSmallDateTime = new ZDateTime(2018, 11, 29, 10, 45, 20);
			addInfoChild.Z0_DateOnly = new ZDate(2018, 12, 30);
			addInfoChild.Z0_SparseDate = new ZDate(2018, 12, 31);

			var collection = AddInfoCollectionCreator.CreateCollection(dummyBizObj, DummyBizoSchema.Z0_VarCharMax);
			CombineAssertions(() =>
			{
				AssertEquals("dummyBizObj.Z0_AnotherDate -> AnotherDate", "2018-11-26", collection.GetZStringValue("AnotherDate"));
				AssertEquals("dummyBizObj.Z0_DateOnly -> DateTime2", "2019-12-30 00:00:00.000", collection.GetZStringValue("DateTime2"));
				AssertEquals("dummyBizObj.Z0_SmallDateTime -> SmallDateTime", "2019-11-27", collection.GetZStringValue("SmallDateTime"));
				AssertEquals("addInfo.UZ_Date -> Date", "2017-11-25 10:45:20.000", collection.GetZStringValue("Date"));
				AssertEquals("addInfoChild.Z0_DateOnly -> DateOnly", "2018-12-30 00:00:00.000", collection.GetZStringValue("DateOnly"));
				AssertEquals("addInfoChild.Z0_SparseDate -> SparseDate", "2018-12-31", collection.GetZStringValue("SparseDate"));
				AssertEquals("addInfoChild.Z0_SparseDateTime -> SparseDateTime", "2018-11-28 10:45:20.000", collection.GetZStringValue("SparseDateTime"));
				AssertEquals("addInfoChild.Z0_SparseSmallDateTime -> SparseSmallDateTime", "2018-11-29 10:45:20.000", collection.GetZStringValue("SparseSmallDateTime"));
			});
		}

		public void TestCreateCollectionFromSyncAddInfosAndAddInfoChild_IAddInfoManagerWithSchema()
		{
			var dummyBizObj = Factory.New<BizObjWithIAddInfoManagerWithSchema>();
			SetupDummyBusinessObject(dummyBizObj.AddInfoChild);
			var collection = AddInfoCollectionCreator.CreateCollectionFromSyncAddInfosAndAddInfoChild(dummyBizObj, null, dummyBizObj);
			var expectedExtraAddInfos = new[]
			{
				"Code=Z1K",
				"DateOnly=2019-01-31",
				"Description=HI BOB",
				"Long=1503024232323",
				"Short=845"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollection_IAddInfoManager()
		{
			var dummyBizObj = Factory.New<BizObjWithIAddInfoManager>();
			SetupDummyBusinessObject(dummyBizObj.AddInfoChild);
			dummyBizObj.AddInfo.UZ_String = "HELLO WORLD";
			dummyBizObj.AddInfo.UZ_Long = 2563456;
			dummyBizObj.AddInfo.UZ_Date = new ZDateTime(2019, 10, 15);
			var collection = AddInfoCollectionCreator.CreateCollection(dummyBizObj, DummyBizoSchema.Z0_VarCharMax);
			var expectedExtraAddInfos = new[]
			{
				"Boolean=N",
				"Code=Z1K",
				"Date=2019-10-15 00:00:00.000",
				"Description=HI BOB",
				"Long=2563456",
				"String=HELLO WORLD"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollectionFromSyncAddInfosAndAddInfoChild_IAddInfoManager()
		{
			var dummyBizObj = Factory.New<BizObjWithIAddInfoManager>();
			SetupDummyBusinessObject(dummyBizObj.AddInfoChild);
			var collection = AddInfoCollectionCreator.CreateCollectionFromSyncAddInfosAndAddInfoChild(dummyBizObj, null, dummyBizObj);
			var expectedExtraAddInfos = new[]
			{
				"Code=Z1K",
				"Description=HI BOB"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollectionFromSyncAddInfosAndAddInfoChild()
		{
			var dummyBizObj = CreateAndPopulateDummyBizObjWithAddInfoChildSupporter();
			var collection = AddInfoCollectionCreator.CreateCollectionFromSyncAddInfosAndAddInfoChild(dummyBizObj, null, dummyBizObj);
			var expectedExtraAddInfos = new[]
			{
				"Code=Z1K",
				"Date=2020-11-25 17:36:48.000",
				"DateOnly=2019-01-31",
				"Decimal=873",
				"Description=HI BOB",
				"Long=1503024232323",
				"Short=845"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollectionFromSyncAddInfosAndAddInfoChild_NullAddInfos()
		{
			var dummyBizObj = CreateAndPopulateDummyBizObjWithAddInfoChildSupporter();
			var collection = AddInfoCollectionCreator.CreateCollectionFromSyncAddInfosAndAddInfoChild(dummyBizObj, null, dummyBizObj, null);
			var expectedExtraAddInfos = new[]
			{
				"Code=Z1K",
				"Date=2020-11-25 17:36:48.000",
				"DateOnly=2019-01-31",
				"Decimal=873",
				"Description=HI BOB",
				"Long=1503024232323",
				"Short=845"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollectionFromSyncAddInfosAndAddInfoChild_ExistingData()
		{
			var dummyBizObj = CreateAndPopulateDummyBizObjWithAddInfoChildSupporter();
			var existingAddInfos = CreateAndPopulateExistingAddInfos();
			var collection = AddInfoCollectionCreator.CreateCollectionFromSyncAddInfosAndAddInfoChild(dummyBizObj, null, dummyBizObj, existingAddInfos);
			var expectedExtraAddInfos = new[]
			{
				"Code=Z2K",
				"Date=2020-11-25 17:36:48.000",
				"DateOnly=2019-01-31",
				"Decimal=873",
				"Description=HELLO WORLD",
				"Long=1503024232323",
				"Short=845"
			};
			AssertAddInfoCollection(expectedExtraAddInfos, collection);
		}

		public void TestCreateCollection_Indexer()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Australia))
			{
				var org = Factory.New<OrgHeader>();
				var declaration = Factory.New<Integration.Customs.AU.IJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_AddInfo = string.Format("{0}=61*{1}=IMP234", AUAddInfoSchema.Constants.ZA_TRN.Substring(3), AUAddInfoSchema.Constants.ZA_VAN.Substring(3));
				var invoice = Factory.New<Integration.Customs.AU.IJobComInvoiceHeader>();
				invoice.JZ_JE = declaration.PK;
				invoice.JZ_AddInfo = string.Format("{0}=IN*{1}=IMP567*{2}=", AUAddInfoSchema.Constants.ZA_TRN.Substring(3), AUAddInfoSchema.Constants.ZA_VAN.Substring(3), AUAddInfoSchema.Constants.ZA_STR.Substring(3));
				var indexer = (IColumnIndexer)invoice;
				var invoiceBizObj = (IBusinessObjectInternals)invoice;

				AssertEquals("PreCondition: Invoice should implement IAddInfoManager", true, invoice is IAddInfoManager);
				var collection = AddInfoCollectionCreator.CreateCollection(indexer, JobComInvoiceHeaderSchema.JZ_AddInfo);
				var matches = collection.FindAll(x => x.Key.GetValueOrDefault() == AUAddInfoSchema.Constants.ZA_UPEIndicator_Hidden.Substring(3));
				AssertEquals(1, matches.Count);
				AssertEquals("N", matches[0].Value.GetValueOrDefault());
				matches = collection.FindAll(x => x.Key.GetValueOrDefault() == AUAddInfoSchema.Constants.ZA_TRN.Substring(3));
				AssertEquals(1, matches.Count);
				AssertEquals("IN", matches[0].Value.GetValueOrDefault());
				matches = collection.FindAll(x => x.Key.GetValueOrDefault() == AUAddInfoSchema.Constants.ZA_VAN.Substring(3));
				AssertEquals(1, matches.Count);
				AssertEquals("IMP567", matches[0].Value.GetValueOrDefault());

				var invoiceRow = (IColumnIndexer)invoiceBizObj.Row;
				AssertEquals("PreCondition: invoiceRow should not implement IAddInfoManager", false, invoiceRow is IAddInfoManager);
				collection = AddInfoCollectionCreator.CreateCollection(invoiceRow, JobComInvoiceHeaderSchema.JZ_AddInfo);
				matches = collection.FindAll(x => x.Key.GetValueOrDefault() == AUAddInfoSchema.Constants.ZA_UPEIndicator_Hidden.Substring(3));
				AssertEquals(0, matches.Count);
				matches = collection.FindAll(x => x.Key.GetValueOrDefault() == AUAddInfoSchema.Constants.ZA_TRN.Substring(3));
				AssertEquals(1, matches.Count);
				AssertEquals("IN", matches[0].Value.GetValueOrDefault());
				matches = collection.FindAll(x => x.Key.GetValueOrDefault() == AUAddInfoSchema.Constants.ZA_VAN.Substring(3));
				AssertEquals(1, matches.Count);
				AssertEquals("IMP567", matches[0].Value.GetValueOrDefault());
			}
		}

		public void TestCreateCollection_AddInfoManagerWithSchema()
		{
			var org = Factory.New<OrgHeader>();
			var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_AddInfo = string.Format("{0}=61*{1}=IMP234", USAddInfoSchema.Constants.US_InbondType.Substring(3), USAddInfoSchema.Constants.US_ImportEntryNo.Substring(3));

			var collection = AddInfoCollectionCreator.CreateCollection((IColumnIndexer)declaration, JobDeclarationSchema.JE_AddInfo);
			var matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_IsLineGrouping.Substring(3));
			AssertEquals("US_IsLineGrouping is unavailable on JobDeclaration", 0, matches.Count);
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_GenAIIForSup.Substring(3));
			AssertEquals("US_GenAIIForSup is unavailable on JobDeclaration", 0, matches.Count);
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_IsFinalWHS.Substring(3));
			AssertEquals("US_IsFinalWHS is available on JobDeclaration", 1, matches.Count);

			var invoice = Factory.New<Integration.Customs.US.IJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			invoice.JZ_AddInfo = string.Format("{0}=IN*{1}=IMP567*{2}=Y*{3}=", USAddInfoSchema.Constants.US_InvoiceType.Substring(3), USAddInfoSchema.Constants.US_ImportEntryNo.Substring(3), USAddInfoSchema.Constants.US_GenAIIForSup.Substring(3), USAddInfoSchema.Constants.US_UC_NKCountryOfExport.Substring(3));
			var indexer = (IColumnIndexer)invoice;
			var invoiceBizObj = (IBusinessObjectInternals)invoice;

			AssertEquals("PreCondition: Invoice should implement IAddInfoManager", true, invoice is IAddInfoManager);
			collection = AddInfoCollectionCreator.CreateCollection(indexer, JobComInvoiceHeaderSchema.JZ_AddInfo);
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_InbondType.Substring(3));
			AssertEquals(1, matches.Count);
			AssertEquals("61", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_InvoiceType.Substring(3));
			AssertEquals(1, matches.Count);
			AssertEquals("IN", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_ImportEntryNo.Substring(3));
			AssertEquals(1, matches.Count);
			AssertEquals("IMP567", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_IsLineGrouping.Substring(3));
			AssertEquals("US_IsLineGrouping is available on JobComInvoiceHeader", 1, matches.Count);
			AssertEquals("N", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_GenAIIForSup.Substring(3));
			AssertEquals("US_GenAIIForSup is available on JobComInvoiceHeader", 1, matches.Count);
			AssertEquals("Y", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_IsFinalWHS.Substring(3));
			AssertEquals("US_IsFinalWHS is unavailable on JobComInvoiceHeader", 0, matches.Count);

			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_UC_NKCountryOfExport.Substring(3));
			AssertEquals("Empty fields should not be included", 0, matches.Count);

			var invoiceRow = (IColumnIndexer)invoiceBizObj.Row;
			AssertEquals("PreCondition: invoiceRow should not implement IAddInfoManager", false, invoiceRow is IAddInfoManager);
			collection = AddInfoCollectionCreator.CreateCollection(invoiceRow, JobComInvoiceHeaderSchema.JZ_AddInfo);
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_InbondType.Substring(3));
			AssertEquals(0, matches.Count);
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_InvoiceType.Substring(3));
			AssertEquals(1, matches.Count);
			AssertEquals("IN", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_ImportEntryNo.Substring(3));
			AssertEquals(1, matches.Count);
			AssertEquals("IMP567", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_IsLineGrouping.Substring(3));
			AssertEquals(0, matches.Count);
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_GenAIIForSup.Substring(3));
			AssertEquals(1, matches.Count);
			AssertEquals("Y", matches[0].Value.GetValueOrDefault());
			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_IsFinalWHS.Substring(3));
			AssertEquals(0, matches.Count);

			matches = collection.FindAll(x => x.Key.GetValueOrDefault() == USAddInfoSchema.Constants.US_UC_NKCountryOfExport.Substring(3));
			AssertEquals("Empty fields should be included", 1, matches.Count);
		}

		public void TestCreateCollection()
		{
			AssertContents(AddInfoCollectionCreator.CreateCollection(AddInfoStringForTesting));
			AssertNull(AddInfoCollectionCreator.CreateCollection(ZString.Empty));
		}

		internal const string AddInfoStringForTesting = "*String=Te¤st*Int=3*String=Why*";

		internal static void AssertContents(List<AddInfo> addInfoCollection)
		{
			AssertEquals(2, addInfoCollection.Count);
			AssertContents(addInfoCollection[0], "String", "Te*st^Why");
			AssertContents(addInfoCollection[1], "Int", "3");
		}

		internal static void AssertContents(AddInfo addInfo, ZString expectedKey, ZString expectedValue)
		{
			AssertEquals("addInfo.Key", expectedKey, addInfo.Key);
			AssertEquals("addInfo.Value", expectedValue, addInfo.Value);
		}

		static void AssertAddInfoCollection(string[] expectedExtraAddInfos, List<AddInfo> addInfoCollection)
		{
			var standardAddInfo = new List<string>
			{
				"AddInfo=AddInfo",
				"AnotherDate=2021-06-14 16:45:26.000",
				"AnotherDecimal=150.567",
				"AnotherNumber=4326",
				"BitFalse=N",
				"BitFiltered=Y",
				"BitTrue=Y",
				"Bool=Y",
				"Byte=8",
				"DateTimeOffset=2017-05-15 00:00:00.0000000 +03:00",
				"FK_Code=K%G",
				"Geography=POINT (-121 48)",
				"Money=15032.34",
				"NAddInfo=NAddInfo",
				"Number=78234",
				"NVarChar=NVARCHAR DATA",
				"NVarCharMax=NVARCHARMAX DATA",
				"SmallDateTime=2021-12-28 01:36:46.000",
				"SparseBit=N",
				"SparseByte=",
				"SparseChar=",
				"SparseDate=",
				"SparseDateTime=",
				"SparseDateTimeOffset=",
				"SparseDecimal=",
				"SparseLong=",
				"SparseMoney=",
				"SparseNumber=",
				"SparseNVarChar=",
				"SparseShort=",
				"SparseSmallDateTime=",
				"SparseTime=",
				"SparseVarChar=",
				"SparseXml=",
				"Time=",
				"VarCharMax=G'DAY",
				"Xml=<GREETING>HI<GREETING/>"
			};
			standardAddInfo.AddRange(expectedExtraAddInfos);

			AssertContainsExactElementsInAnyOrder(standardAddInfo, addInfoCollection.Select(x => $"{x.Key}={x.Value}"));
		}

		DummyBizObjWithAddInfoChildSupporter CreateAndPopulateDummyBizObjWithAddInfoChildSupporter()
		{
			var dummyBizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var child = (DummyBusinessObject)dummyBizObj.AddInfoChild;
			SetupDummyBusinessObject(child);
			child.MarkAsNeedingValidation();
			return dummyBizObj;
		}

		void SetupDummyBusinessObject(DummyBusinessObject child)
		{
			child.Z0_AddInfo = "AddInfo";
			child.Z0_AnotherDate = new ZDateTime(2021, 6, 14, 16, 45, 26);
			child.Z0_AnotherDecimal = 150.5665m;
			child.Z0_AnotherNumber = 4326;
			child.Z0_BitFalse = ZBool.False;
			child.Z0_BitFiltered = ZBool.True;
			child.Z0_BitTrue = ZBool.True;
			child.Z0_Bool = ZBool.True;
			child.Z0_Byte = 8;
			child.Z0_Code = "Z1K";
			child.Z0_Date = new ZDateTime(2020, 11, 25, 17, 36, 48);
			child.Z0_DateOnly = new ZDate(2019, 1, 31);
			child.Z0_DateTimeOffset = new ZDateTimeOffset(2017, 5, 15, 0, 0, 0, TimeSpan.FromHours(3));
			child.Z0_Decimal = 873.2974m;
			child.Z0_Description = "HI BOB";
			child.Z0_Geography = new ZGeography("-121 48");
			child.Z0_FK_Code = "K%G";
			child.Z0_Long = 1503024232323;
			child.Z0_Money = 15032.34m;
			child.Z0_NAddInfo = "NAddInfo";
			child.Z0_Number = 78234;
			child.Z0_NVarChar = "NVARCHAR DATA";
			child.Z0_NVarCharMax = "NVARCHARMAX DATA";
			child.Z0_Short = 845;
			child.Z0_SmallDateTime = new ZDateTime(2021, 12, 28, 1, 36, 46);
			child.Z0_VarBinaryMax = ZBlob.FromAscii("SOME OTHER VALUE");
			child.Z0_VarCharMax = "G'DAY";
			child.Z0_Xml = "<GREETING>HI<GREETING/>";
			child.Z0_IsSystem = ZBool.True;
		}

		List<AddInfo> CreateAndPopulateExistingAddInfos()
		{
			var existingAddInfos = new List<AddInfo>();
			existingAddInfos.Add(new AddInfo() { Key = new ZString("Code"), Value = new ZString("Z2K") });
			existingAddInfos.Add(new AddInfo() { Key = new ZString("Description"), Value = new ZString("HELLO WORLD") });
			return existingAddInfos;
		}
	}
}
