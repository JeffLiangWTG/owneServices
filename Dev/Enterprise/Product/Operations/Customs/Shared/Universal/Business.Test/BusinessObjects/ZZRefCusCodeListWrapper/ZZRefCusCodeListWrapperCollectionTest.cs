using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(DummyRefCusCodeListWrapperCollection))]
	public class ZZRefCusCodeListWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ZZRefCusCodeListWrapperCollectionTest.DummyRefCusCodeListWrapperCollection>
	{
		public void TestLoad()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateNewOrGetExistingCusCodeType("ADDIN", "Additional Information");
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "CUSOF", "1111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "CUSOF", "2222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "ADDIN", "3333", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("EU", "CUSOF", "4444", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			otherFactory.Save();
			var locationCollection = new DummyRefCusCodeListWrapperCollection(Factory);
			locationCollection.Load();
			AssertEquals("DummyRefCusCodeListWrapperCollection.Count", 2, locationCollection.Count);
			AssertNotNull(locationCollection.Cast<ZZRefCusCodeListWrapper>().FirstOrDefault(x => x.Code == "1111"));
			AssertNotNull(locationCollection.Cast<ZZRefCusCodeListWrapper>().FirstOrDefault(x => x.Code == "2222"));
			var filter = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, "1111");
			AssertEquals("DummyRefCusCodeListWrapperCollection.Count", 1, locationCollection.GetEstimatedLoadCount(filter));
			locationCollection.Load(filter);
			AssertEquals("DummyRefCusCodeListWrapperCollection.Count", 1, locationCollection.Count);
			AssertNotNull(locationCollection.Cast<ZZRefCusCodeListWrapper>().FirstOrDefault(x => x.Code == "1111"));
		}

		public void TestZZRefCusCodeListFindBoxListProvider()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateNewOrGetExistingCusCodeType("ADDIN", "Additional Information");
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "CUSOF", "1111", "1111 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "CUSOF", "2222", "2222 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "CUSOF", "1122", "1122 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "ADDIN", "3333", "3333 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("EU", "CUSOF", "4444", "4444 DESC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			otherFactory.Save();
			var locationCollection = new DummyRefCusCodeListWrapperCollection(Factory);
			var findBoxListProvider = new ZZRefCusCodeListFindBoxListProvider<ZZRefCusCodeListWrapper>(locationCollection);
			AssertEquals("1111", findBoxListProvider.NearestMatch("11", true, -1).Item1);
			AssertEquals("2222", findBoxListProvider.NearestMatch("22", true, -1).Item1);
			AssertEquals("33", findBoxListProvider.NearestMatch("33", true, -1).Item1);
			AssertEquals("1111", findBoxListProvider.NearestMatch("", true, -1).Item1);
			AssertEquals("1111 DESC", findBoxListProvider.DescriptionFromCode("1111"));
			AssertEquals("1122 DESC", findBoxListProvider.DescriptionFromCode("1122"));
			AssertEquals("2222 DESC", findBoxListProvider.DescriptionFromCode("2222"));
			AssertEquals("", findBoxListProvider.DescriptionFromCode("3333"));
		}

		protected override DummyRefCusCodeListWrapperCollection GetCollectionToTest()
		{
			return new DummyRefCusCodeListWrapperCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_Code = "XXXX";
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);
			return new ZZRefCusCodeListWrapper(cusCodeList);
		}

		UniversalReferenceTestDataHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
		}

		public class DummyRefCusCodeListWrapperCollection : ZZRefCusCodeListWrapperCollection<ZZRefCusCodeListWrapper>
		{
			public DummyRefCusCodeListWrapperCollection(BusinessObjectFactory factory) : base(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now)
			{
			}
		}
	}
}
