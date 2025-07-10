using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsList()
		{
			var header = Factory.New<CusInBondHeader>();
			var list = header.Lookups.TransportModeCodes;
			var list1 = Factory.GetCachedValue<InBondTransportModeCodes>();
			AssertEquals(list1.Count, list.Count);
			AssertEquals(list1, list);
		}

		public void TestCustomsOfficeList()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "KF", "Taipei office2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var lookups = cusInBondHeader.Lookups;
			var officeCode = lookups.CustomsOfficeList;
			AssertNotNull(officeCode);
			AssertEquals("Taipei office2", officeCode.GetDescriptionFromCode("KF"));
		}

		public void TestEntryStatusList()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var list = cusInBondHeader.Lookups.EntryStatusList;
			var entryStatusList = Factory.GetCachedValue<EntryStatusCodeList>();
			AssertEquals(entryStatusList, list);
		}

		public void TestTWMessageStatusCodeList()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var lookups = new CusInBondHeaderLookups(cusInBondHeader);
			var list = lookups.MessageStatusList;
			AssertSame(Factory.GetCachedValue<TWMessageStatusCodeList>(), list);
			AssertEquals("NOT, UNK, AWR, ERR, SNT, ACK", list.CodesAsString);
		}

		public void TestBoxNumberList()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var header = Factory.New<CusInBondHeader>();
			var lookups = header.Lookups;
			AssertEquals(3, lookups.BoxNumberList.Count);
		}
	}
}
