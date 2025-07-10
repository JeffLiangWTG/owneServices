using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffAttributeView))]
	internal class TariffAttributeViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalDescription()
		{
			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, "111");
			var tariffType2 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "222");
			Factory.Save();
			Helper.CreateCusCodeType("TWIR", "Taiwan Import Regulations");
			Helper.CreateCusCodeListWithAttribute("TW", "TWIR", "111", "description1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ImportRegulations", "Value");
			Helper.CreateCusCodeListWithAttribute("TW", "TWIR", "222", "description2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ImportRegulations", "Value");
			Helper.CreateCusCodeListWithAttribute("CN", "TWIR", "333", "description1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ImportRegulations", "Value");
			Helper.CreateCusCodeListWithAttribute("CN", "TWIR", "444", "description2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ImportRegulations", "Value");
			var tariffView1 = Helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType1.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Description 1");
			var tariffAttributeView1 = Helper.CreateTariffAttribute("333", "ImportRegulations", tariffView1);
			var tariffView2 = Helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType2.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Description 2");
			var tariffAttributeView2 = Helper.CreateTariffAttribute("333", "ImportRegulations", tariffView2);
			Factory.Save();
			AssertEquals("", tariffAttributeView1.AdditionalDescription);
			tariffAttributeView2.ZZ3_Value = "111";
			tariffAttributeView2.ZZ3_Name = "ImportRegulations";
			AssertEquals("description1", tariffAttributeView2.AdditionalDescription);
			tariffAttributeView2.ZZ3_Value = "";
			tariffAttributeView2.ZZ3_Name = "ImportRegulations";
			AssertEquals("", tariffAttributeView2.AdditionalDescription);
			tariffAttributeView2.ZZ3_Value = "111";
			tariffAttributeView2.ZZ3_Name = "";
			AssertEquals("", tariffAttributeView2.AdditionalDescription);
			tariffAttributeView2.ZZ3_Value = "";
			tariffAttributeView2.ZZ3_Name = "";
			AssertEquals("", tariffAttributeView2.AdditionalDescription);
			tariffAttributeView2.ZZ3_Value = "222";
			tariffAttributeView2.ZZ3_Name = "ImportRegulations";
			AssertEquals("description2", tariffAttributeView2.AdditionalDescription);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject() => Attribute;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override bool CanPersistedObjectBeDeleted => false;
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		TariffAttributeView Attribute => attribute ?? (attribute = Helper.CreateTariffAttribute("Name", "Value", CusTariff));
		TariffAttributeView attribute;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
