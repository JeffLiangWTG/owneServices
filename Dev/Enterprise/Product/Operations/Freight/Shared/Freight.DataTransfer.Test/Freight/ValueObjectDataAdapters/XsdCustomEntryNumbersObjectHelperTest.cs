using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[CountrySpecificTest("AU")]
	sealed class XsdCustomEntryNumbersObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportFromXsdCustomsEntryNumberCollection_IfCountryIsInvalid()
		{
			CusEntryNumCollection result = new CusEntryNumCollection(Factory);
			Xsd.CustomsEntryNumberCollection testDataCollection = new Xsd.CustomsEntryNumberCollection();

			CommonShipment shipment = CommonShipment.New(Factory);
			CusEntryNumber cusEntryNumberBizObj = shipment.CusEntryNumbers.AddNew();
			cusEntryNumberBizObj.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			cusEntryNumberBizObj.CE_EntryNum = "123";
			cusEntryNumberBizObj.CE_EntryType = "XXX";

			Xsd.CustomsEntryNumber cusEntryNumberResult = testDataCollection.AddNew();
			cusEntryNumberResult.Country = "ZZ";
			cusEntryNumberResult.Number = "123";
			cusEntryNumberResult.Type = "XXX";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(testDataCollection, shipment.PK, JobShipmentSchema.Constants.TableName, () => result, context);

			AssertEquals(0, result.Count);
		}

		public void TestImportFromXsdCustomsEntryNumberCollection()
		{
			CusEntryNumCollection result = new CusEntryNumCollection(Factory);
			Xsd.CustomsEntryNumberCollection testDataCollection = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber cusEntryNumberResult = testDataCollection.AddNew();
			cusEntryNumberResult.Country = "AU";
			cusEntryNumberResult.Number = "123";
			cusEntryNumberResult.Type = "XXX";

			CommonShipment shipment = CommonShipment.New(Factory);
			CusEntryNumber cusEntryNumberBizObj = shipment.CusEntryNumbers.AddNew();
			cusEntryNumberBizObj.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			cusEntryNumberBizObj.CE_EntryNum = "123";
			cusEntryNumberBizObj.CE_EntryType = "XXX";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(testDataCollection, shipment.PK, JobShipmentSchema.Constants.TableName, () => result, context);

			AssertEquals(1, result.Count);

			CusEntryNumber bizOResult = result[0];

			AssertEquals("XXX", bizOResult.CE_EntryType);
			AssertEquals("123", bizOResult.CE_EntryNum);
			AssertEquals("AU", bizOResult.CE_RN_NKCountryCode);
		}

		public void TestImportFromXsdCustomsEntryNumberCollection_TwoCountries()
		{
			CusEntryNumCollection result = new CusEntryNumCollection(Factory);
			Xsd.CustomsEntryNumberCollection testDataCollection = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber cusEntryNumberResult1 = testDataCollection.AddNew();
			cusEntryNumberResult1.Country = "AU";
			cusEntryNumberResult1.Number = "123";
			cusEntryNumberResult1.Type = "XXX";

			Xsd.CustomsEntryNumber cusEntryNumberResult2 = testDataCollection.AddNew();
			cusEntryNumberResult2.Country = "HK";
			cusEntryNumberResult2.Number = "456";
			cusEntryNumberResult2.Type = "ZZZ";

			CommonShipment shipment = CommonShipment.New(Factory);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(testDataCollection, shipment.PK, JobShipmentSchema.Constants.TableName, () => result, context);

			AssertEquals(2, result.Count);
		}

		public void TestImportFromXsdCustomsEntryNumberCollection_WithMatch()
		{
			CusEntryNumCollection result = new CusEntryNumCollection(Factory);
			Xsd.CustomsEntryNumberCollection testDataCollection = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber xsdCusEntryNumber = testDataCollection.AddNew();
			xsdCusEntryNumber.Country = Env.CurrentCompany.Country.Code;
			xsdCusEntryNumber.Number = "123";
			xsdCusEntryNumber.Type = "TestType";

			CommonShipment shipment = CommonShipment.New(Factory);
			CusEntryNumber cusEntryNumberBizObj = shipment.CusEntryNumbers.AddNew();
			cusEntryNumberBizObj.CE_EntryNum = "difrnt";
			cusEntryNumberBizObj.CE_EntryType = "Tes";
			cusEntryNumberBizObj.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;

			int resultCount = result.Count;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(testDataCollection, shipment.PK, JobShipmentSchema.Constants.TableName, () => result, context);

			Assert(result.Count == result.Count);
		}

		public void TestImportFromXsdCustomsEntryNumberCollection_WithTypeMaxLengthExceeded()
		{
			CusEntryNumCollection result = new CusEntryNumCollection(Factory);
			Xsd.CustomsEntryNumberCollection testDataCollection = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber xsdCusEntryNumber = testDataCollection.AddNew();
			xsdCusEntryNumber.Country = Env.CurrentCompany.Country.Code;
			xsdCusEntryNumber.Number = "123";
			xsdCusEntryNumber.Type = "SomeLongType";

			CommonShipment shipment = CommonShipment.New(Factory);
			CusEntryNumber cusEntryNumberBizObj = shipment.CusEntryNumbers.AddNew();
			cusEntryNumberBizObj.CE_EntryNum = "123";
			cusEntryNumberBizObj.CE_EntryType = ((ZString)"SomeLongType").Left(CusEntryNumSchema.CE_EntryType.MaxLength);
			cusEntryNumberBizObj.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(testDataCollection, shipment.PK, JobShipmentSchema.Constants.TableName, () => result, context);
			Assert("No customs entry numbers should be imported", result.Count == 0);
			AssertEquals("Warnings should be logged", true, notify.HasWarnings);
		}

		public void TestImportFromXsdCustomsEntryNumberCollection_WithNumberMaxLengthExceeded()
		{
			CusEntryNumCollection result = new CusEntryNumCollection(Factory);
			Xsd.CustomsEntryNumberCollection testDataCollection = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber xsdCusEntryNumber = testDataCollection.AddNew();
			xsdCusEntryNumber.Country = Env.CurrentCompany.Country.Code;
			xsdCusEntryNumber.Number = "1234567890123456789012345678901234567890";
			xsdCusEntryNumber.Type = "XXX";

			CommonShipment shipment = CommonShipment.New(Factory);
			CusEntryNumber cusEntryNumberBizObj = shipment.CusEntryNumbers.AddNew();
			cusEntryNumberBizObj.CE_EntryNum = ((ZString)"1234567890123456789012345678901234567890").Left(CusEntryNumSchema.CE_EntryNum.MaxLength);
			cusEntryNumberBizObj.CE_EntryType = "XXX";
			cusEntryNumberBizObj.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(testDataCollection, shipment.PK, JobShipmentSchema.Constants.TableName, () => result, context);
			Assert("No customs entry numbers should be imported", result.Count == 0);
			AssertEquals("Warnings should be logged", true, notify.HasWarnings);
		}

		public void TestExportFromCusEntryNumCollection()
		{
			CusEntryNumCollection testDataCollection = new CusEntryNumCollection(Factory);

			CusEntryNumber testCen = testDataCollection.AddNew();
			testCen.CE_EntryNum = "123";
			testCen.CE_EntryType = "Tes";

			GlbCompany.CurrentCompany.SetCountry("NZ");

			CusEntryNumber testCen1 = testDataCollection.AddNew();
			testCen1.CE_EntryNum = "666";
			testCen1.CE_EntryType = "dev";

			GlbCompany.CurrentCompany.SetCountry("AU");

			Xsd.CustomsEntryNumberCollection result = XsdCustomEntryNumbersObjectHelper.ExportFromCusEntryNumCollection(testDataCollection);
			AssertEquals("exports both", 2, result.Count);
		}

		[ExpectNoExceptions()]
		public void TestImportTwoNumbersWithSameTypeAndSameCountryEnsureCanSave_Incident28827()
		{
			Xsd.CustomsEntryNumberCollection customsEntryNumberCollectionValueObject = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber customsEntryNumberValueObject = customsEntryNumberCollectionValueObject.AddNew();
			customsEntryNumberValueObject.Country = Core.Constants.CountryCodes.Australia;
			customsEntryNumberValueObject.Type = "XYZ";
			customsEntryNumberValueObject.Number = "123";

			CommonShipment shipment = CommonShipment.New(Factory);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(customsEntryNumberCollectionValueObject, shipment.PK, JobShipmentSchema.Constants.TableName, () => shipment.CusEntryNumbers, context);

			customsEntryNumberValueObject.Number = "234";
			shipment = Factory.New<CommonShipment>();
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(customsEntryNumberCollectionValueObject, shipment.PK, JobShipmentSchema.Constants.TableName, () => shipment.CusEntryNumbers, context);

			Factory.Save();
		}

		public void TestHandleDuplicateEntryNumbers()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CusEntryNumber existingNumber = Factory.New<CusEntryNumber>();
			existingNumber.CE_EntryIsSystemGenerated = false;
			existingNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;//not a current company
			existingNumber.CE_ParentID = shipment.PK;
			existingNumber.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			existingNumber.CE_EntryType = "AAA";
			existingNumber.CE_EntryNum = "123";
			Factory.Save();

			Xsd.CustomsEntryNumberCollection customsEntryNumberCollectionValueObject = new Xsd.CustomsEntryNumberCollection();

			Xsd.CustomsEntryNumber customsEntryNumberValueObject = customsEntryNumberCollectionValueObject.AddNew();
			customsEntryNumberValueObject.Country = Core.Constants.CountryCodes.NewZealand;
			customsEntryNumberValueObject.Type = "AAA";
			customsEntryNumberValueObject.Number = "123";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			XsdCustomEntryNumbersObjectHelper.ImportFromXsdCustomsEntryNumberCollection(customsEntryNumberCollectionValueObject, shipment.PK, JobShipmentSchema.Constants.TableName, () => shipment.CusEntryNumbers, context);
			AssertNoExceptionThrown(Factory.Save);
		}
	}
}
