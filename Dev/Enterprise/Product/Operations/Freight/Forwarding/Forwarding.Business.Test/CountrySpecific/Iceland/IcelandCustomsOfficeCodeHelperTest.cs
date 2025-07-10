using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class IcelandCustomsOfficeCodeHelperTest : TestCaseWithFactory
	{
		CusEntryNumber GetCustomsHouseEntryNum(CusEntryNumAdditionalReferenceCollection numbers)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Iceland);
			CusEntryNumber[] results = (CusEntryNumber[])numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		public void TestProcessIcelandCustomsOfficeCode()
		{
			DummyBusinessObject bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			CusEntryNumAdditionalReferenceCollection numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", true);
			CusEntryNumber customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNull("COC", customsHouseEntryNum);
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNull("COC", customsHouseEntryNum);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.Iceland);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			customsHouseEntryNum = numbers.AddNew();
			customsHouseEntryNum.CE_EntryType = IcelandForwardingShipmentSupport.CustomsOfficeCode;
			customsHouseEntryNum.CE_EntryNum = "111";
			customsHouseEntryNum.CE_ParentTable = "JobDeclaration";
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", true);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(false, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("111", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			customsHouseEntryNum = numbers.AddNew();
			customsHouseEntryNum.CE_EntryType = IcelandForwardingShipmentSupport.CustomsOfficeCode;
			customsHouseEntryNum.CE_EntryNum = "111";
			customsHouseEntryNum.CE_ParentTable = "JobDeclaration";
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(true, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("111", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			bo.Z0_Code = "123";
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", true);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(false, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("asd", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			bo.Z0_Code = "123";
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(true, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("asd", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			Factory.Save();
			bo = new BusinessObjectFactory().Load<DummyBusinessObject>(bo.PK);
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			numbers.Load();
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC created on NumbersLoaded", customsHouseEntryNum);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			Factory.Save();
			bo = new BusinessObjectFactory().Load<DummyBusinessObject>(bo.PK);
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			numbers.Load();
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(numbers, "asd", true);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC created on NumbersLoaded", customsHouseEntryNum);
		}

		public void TestSetIcelandCustomsHouseCode()
		{
			DummyBusinessObject bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			CusEntryNumAdditionalReferenceCollection numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(numbers, "asd", "sssssss", true);
			CusEntryNumber customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNull("COC", customsHouseEntryNum);
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(numbers, "asd", "sssssss", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNull("COC", customsHouseEntryNum);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.SetCountry(Core.Constants.CountryCodes.Iceland);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			customsHouseEntryNum = numbers.AddNew();
			customsHouseEntryNum.CE_EntryType = IcelandForwardingShipmentSupport.CustomsOfficeCode;
			customsHouseEntryNum.CE_EntryNum = "111";
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(numbers, "asd", "sssssss", true);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(false, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("asd", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("sssssss", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			customsHouseEntryNum = numbers.AddNew();
			customsHouseEntryNum.CE_EntryType = IcelandForwardingShipmentSupport.CustomsOfficeCode;
			customsHouseEntryNum.CE_EntryNum = "111";
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(numbers, "asd", "sssssss", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(true, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("asd", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("sssssss", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(numbers, "asd", "sssssss", true);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(false, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("asd", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);

			bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			numbers = new CusEntryNumAdditionalReferenceCollection(bo);
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(numbers, "asd", "sssssss", false);
			customsHouseEntryNum = GetCustomsHouseEntryNum(numbers);
			AssertNotNull("COC", customsHouseEntryNum);
			AssertEquals(true, customsHouseEntryNum.CE_EntryType_ReadOnly);
			AssertEquals(true, customsHouseEntryNum.CE_EntryNum_ReadOnly);
			AssertEquals("asd", customsHouseEntryNum.CE_EntryNum);
			AssertEquals("", customsHouseEntryNum.AdditionalCustomLogReferenceSuffix);
		}
	}
}
