using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestMRN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "456";
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = "MRN";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			entryNumber.CE_ParentID = bill.PK;
			entryNumber.CE_ParentTable = bill.TableName;
			Factory.Save();

			AssertEquals("123", bill.MRN);

			bill.MRN = "456";
			Factory.Save();

			bill = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals("456", bill.MRN);
		}

		public void TestLRN_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_Nature = NatureList.Codes.Export22;
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "456";

			AssertEquals(false, masterBill.LRNInfo.ReadOnly);

			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, masterBill.LRNInfo.ReadOnly);
		}

		public void TestLRN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "456";
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = Enterprise.Customs.Common.CusEntryNumberTypes.SouthAfrica.LocalReferenceNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			entryNumber.CE_ParentID = masterBill.PK;
			entryNumber.CE_ParentTable = masterBill.TableName;
			Factory.Save();

			AssertEquals("123", masterBill.LRN);
		}

		public void TestCustomsCPC_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_Nature = NatureList.Codes.Export22;
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "456";

			AssertEquals(false, masterBill.CustomsCPCInfo.ReadOnly);

			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, masterBill.CustomsCPCInfo.ReadOnly);
		}

		public void TestCustomsCPC()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "456";
			masterBill.CustomsCPC = "12";
			Factory.Save();

			masterBill = new BusinessObjectFactory().Load<AsycudaBill>(masterBill.PK);
			AssertEquals("12", masterBill.CustomsCPC);
		}

		public void TestABL_BillStatus_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(true, header.MasterBill.ABL_BillStatus_ReadOnly);
		}

		public void TestCountryCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, header.MasterBill.CountryCode);
		}

		public void TestBillCountry()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.MasterBill;
			AssertNotNull(bill);
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, bill.CountryCode);
		}

		public void TestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(header, bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0, bill.Packs.Count);
			bill.Packs.AddNew();
			AssertEquals(1, bill.Packs.Count);
			Factory.Save();
			var billReloaded = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals(1, billReloaded.Packs.Count);
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var mBill = header.MasterBill;
			var bill = header.Bills.AddNew();
			AssertType<AsycudaBillValidationForMasterChild>(mBill.Validation);
			AssertType<AsycudaBillValidationForRegularBill>(bill.Validation);
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillLookups>(header.MasterBill.Lookups);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new CusEntryNumberReconciliationLightValidationTester(bizObjToTest);

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			return header.MasterBill;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		sealed class CusEntryNumberReconciliationLightValidationTester : LightValidationTester
		{
			public CusEntryNumberReconciliationLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != CusEntryNumber.Schema.CE_Category
						&& propertyName != CusEntryNumber.Schema.CE_EntryNum
						&& propertyName != CusEntryNumber.Schema.CE_EntryType
						&& propertyName != CusEntryNumber.Schema.CE_ParentID
						&& propertyName != CusEntryNumber.Schema.CE_ParentTable
						&& propertyName != CusEntryNumber.Schema.CE_RN_NKCountryCode;
			}
		}
	}
}
