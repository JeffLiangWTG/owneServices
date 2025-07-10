using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UHelper = Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffUOMView))]
	class TariffUOMViewTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestCallsBaseSetDefaultValues() => Assert(true);

		public void TestDefaultValues()
		{
			var uom = Factory.New<TariffUOMView>();
			CombineAssertions(() =>
			{
				AssertEquals("Default value for ZZ8_DataSet", "O", uom.ZZ8_DataSet);
				AssertEquals("Default value for ZZ8_ParentTableType", "CR1", uom.ZZ8_ParentTableType);
			});
		}

		public void TestIsApplicable()
		{
			var tradeGroupES = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only ES");
			helper.AddCountry(tradeGroupES, CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffType = UHelper.CreateTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var uomTradeGroupES = helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM", tradeGroup: tradeGroupES);

			CombineAssertions(() =>
			{
				Assert("Spain", uomTradeGroupES.IsApplicable(CountryCodes.Spain));
				AssertEquals("Italy", false, uomTradeGroupES.IsApplicable(CountryCodes.Italy));
			});
		}

		public void TestReadonlys()
		{
			var testItem = (TariffUOMView)GetNewBusinessObject();
			Assert("ZZ8_DataSetInfo readonly", testItem.ZZ8_DataSetInfo.ReadOnly);
			Assert("ZZ8_IsSystemInfo readonly", testItem.ZZ8_IsSystemInfo.ReadOnly);
		}

		public void TestZZ8_TradeGroupDescription() => CombineAssertions(() =>
		{
			UOM.ZZ8_ZZA_TradeGroup = ZGuid.Empty;
			AssertEquals("Trade group empty description", ZString.Empty, UOM.TradeGroupDescription);

			const string testTradeGroupDescription = "Test description";
			var tradeGroup = Factory.New<CusRefTradeGroupView>();
			tradeGroup.ZZA_Description = testTradeGroupDescription;
			UOM.ZZ8_ZZA_TradeGroup = tradeGroup.PK;
			AssertEquals("Trade group test description", testTradeGroupDescription, UOM.TradeGroupDescription);
		});

		public void TestZZ8_TradeGroupDescription_Caption()
			=> AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TariffUOMView), nameof(TariffUOMView.TradeGroupDescription), false,
				resAttr => resAttr.Caption == "Group Description." && resAttr.ShortCaption == "Gr. Desc.");

		public void TestZZ8_ZZZ_NKDataGrouping_Caption()
			=> AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TariffUOMView), nameof(TariffUOMView.ZZ8_ZZZ_NKDataGrouping), false,
				resAttr => resAttr.Caption == "Country/Region or Grouping" && resAttr.ShortCaption == "Grouping");

		protected override BusinessObject GetNewBusinessObject() => UOM;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => UOM;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => UOM;
		protected override bool CanPersistedObjectBeDeleted => false;
		protected override void SetUp()
		{
			base.SetUp();
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		TariffUOMView UOM => uom ?? (uom = Helper.CreateTariffUOM(CusTariff, "CU1", "VWG"));
		TariffUOMView uom;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		UHelper Helper => helper ?? (helper = new UHelper(Factory));
		UHelper helper;
	}
}
