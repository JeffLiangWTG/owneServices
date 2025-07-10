using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCommodityCode))]
	sealed class RefCommodityCodeTest : EnterpriseBusinessObjectTestCase
	{
		void CreateCommodityCodeMap(string commodityCode)
		{
			CreateCommodityCodeMap(commodityCode, $"{commodityCode}_Local", "DBH");
		}

		void CreateCommodityCodeMap(string commodityCode, string localCode, string localCodeProvider)
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();
			commodityCodeMap.LC_RH_NKCommodityCode = commodityCode;
			commodityCodeMap.LC_LocalCode = localCode;
			commodityCodeMap.LC_LocalCodeProvider = localCodeProvider;
			commodityCodeMap.LC_RN_NKCountry = "DE";
		}

		void CreateRefCommodityRatingCodeMap(string commodityCode, string commodityChildCode)
		{
			var commodityRatingCodeMap = Factory.New<RefCommodityRatingCodeMap>();
			commodityRatingCodeMap.RI_RH_NKCommodityParent = commodityCode;
			commodityRatingCodeMap.RI_RH_NKCommodityChild = commodityChildCode;
		}

		public void TestCommodityCodeMapCollection()
		{
			// Arrange
			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			CreateCommodityCodeMap(commodity1.RH_Code);

			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			CreateCommodityCodeMap(commodity2.RH_Code);

			Factory.Save();

			// Act
			var commodity1MapCodes = commodity1.RefCommodityCodeMaps.Select(commodityMap => commodityMap.LC_LocalCode).ToList();
			var commodity2MapCodes = commodity2.RefCommodityCodeMaps.Select(commodityMap => commodityMap.LC_LocalCode).ToList();

			// Assert
			AssertContainsExactElementsInAnyOrder(commodity1MapCodes, new List<string> { $"{commodity1.RH_Code}_Local" });
			AssertContainsExactElementsInAnyOrder(commodity2MapCodes, new List<string> { $"{commodity2.RH_Code}_Local" });
		}

		public void TestLocalCodesAsString()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();

			AssertEquals(ZString.Empty, commodity.LocalCodesAsString);

			CreateCommodityCodeMap(commodity.RH_Code, "localCode1", "AAA");
			CreateCommodityCodeMap(commodity.RH_Code, "localCode2", "BBB");

			var expectedString = "AAA:localCode1,BBB:localCode2";
			AssertEquals
			(
				"The LocalCodesAsString should be in the format of {LC_LocalCodeProvider}:{LC_LocalCode}.",
				expectedString,
				commodity.LocalCodesAsString
			);
		}

		public void TestRatingCodesAsString()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();

			AssertEquals(ZString.Empty, commodity.RatingCodesAsString);

			CreateRefCommodityRatingCodeMap(commodity.RH_Code, "AAA");
			CreateRefCommodityRatingCodeMap(commodity.RH_Code, "BBB");

			var expectedRatingCodesAsString = "AAA,BBB";
			AssertEquals
			(
				"The RatingCodesAsString should be in the format of {RI_RH_NKCommodityChild},{RI_RH_NKCommodityChild}.",
				expectedRatingCodesAsString,
				commodity.RatingCodesAsString
			);
		}

		public void TestMaxTempAndMinTemp()
		{
			// First Case - Min < Max
			CommodityCode.RH_ReeferMinTemperature = new ZDecimal(1);
			CommodityCode.RH_ReeferMaxTemperature = new ZDecimal(10);
			AssertEquals("The Min Temp is set properly", new ZDecimal(1), CommodityCode.RH_ReeferMinTemperature);
			AssertEquals("The Max Temp is set properly", new ZDecimal(10), CommodityCode.RH_ReeferMaxTemperature);

			Assert("Min should not have errors", !CommodityCode.RH_ReeferMinTemperatureInfo.HasErrors());
			Assert("Max should not have errors", !CommodityCode.RH_ReeferMaxTemperatureInfo.HasErrors());

			// Second Case - Min > Max
			CommodityCode.RH_ReeferMinTemperature = new ZDecimal(100);
			CommodityCode.RH_ReeferMaxTemperature = new ZDecimal(10);
			AssertEquals("The Min Temp is set properly", new ZDecimal(100), CommodityCode.RH_ReeferMinTemperature);
			AssertEquals("The Max Temp is set properly", new ZDecimal(10), CommodityCode.RH_ReeferMaxTemperature);
			Assert("Max should have errors", CommodityCode.RH_ReeferMaxTemperatureInfo.HasErrors());
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(CommodityCode.GetType()));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region GetCommodities

		public void TestGetCommodities_UniversalCommodityGroupField()
		{
			var comUniversalGroup = "COM";

			var commodity = CreateCommodity("CM1", "Commodity1");
			commodity.RH_UniversalCommodityGroup = comUniversalGroup;

			CreateCommodities(new[] { RefCommodityCode.HAZD, RefCommodityCode.PERS, RefCommodityCode.TIMB, RefCommodityCode.FLAM, RefCommodityCode.CNVT });

			AssertGetCommodities
			(
				universalCommodityGroup: comUniversalGroup,
				existingCommodityCodes: existingCommodityCodes,
				expectedCommodityCodes: new[] { "CM1" }
			);
		}

		public void TestGetCommodities_HAZD()
		{
			var commodity = CreateCommodity("CM1", "Commodity1");
			commodity.RH_IsHazardous = true;

			CreateCommodities(new[] { "COM", RefCommodityCode.PERS, RefCommodityCode.TIMB, RefCommodityCode.FLAM, RefCommodityCode.CNVT });

			AssertGetCommodities
			(
				universalCommodityGroup: RefCommodityCode.HAZD,
				existingCommodityCodes: existingCommodityCodes,
				expectedCommodityCodes: new[] { "CM1" }
			);
		}

		public void TestGetCommodities_PERS()
		{
			var commodity = CreateCommodity("CM1", "Commodity1");
			commodity.RH_IsPerishable = true;

			CreateCommodities(new[] { "COM", RefCommodityCode.HAZD, RefCommodityCode.TIMB, RefCommodityCode.FLAM, RefCommodityCode.CNVT });

			AssertGetCommodities
			(
				universalCommodityGroup: RefCommodityCode.PERS,
				existingCommodityCodes: existingCommodityCodes,
				expectedCommodityCodes: new[] { "CM1" }
			);
		}

		public void TestGetCommodities_TIMB()
		{
			var commodity = CreateCommodity("CM1", "Commodity1");
			commodity.RH_IsTimber = true;

			CreateCommodities(new[] { "COM", RefCommodityCode.HAZD, RefCommodityCode.PERS, RefCommodityCode.FLAM, RefCommodityCode.CNVT });

			AssertGetCommodities
			(
				universalCommodityGroup: RefCommodityCode.TIMB,
				existingCommodityCodes: existingCommodityCodes,
				expectedCommodityCodes: new[] { "CM1" }
			);
		}

		public void TestGetCommodities_FLAM()
		{
			var commodity = CreateCommodity("CM1", "Commodity1");
			commodity.RH_IsFlammable = true;

			CreateCommodities(new[] { "COM", RefCommodityCode.HAZD, RefCommodityCode.PERS, RefCommodityCode.TIMB, RefCommodityCode.CNVT });

			AssertGetCommodities
			(
				universalCommodityGroup: RefCommodityCode.FLAM,
				existingCommodityCodes: existingCommodityCodes,
				expectedCommodityCodes: new[] { "CM1" }
			);
		}

		public void TestGetCommodities_CNVT()
		{
			var commodity = CreateCommodity("CM1", "Commodity1");
			commodity.RH_ContainerVentRequired = true;

			CreateCommodities(new[] { "COM", RefCommodityCode.HAZD, RefCommodityCode.PERS, RefCommodityCode.TIMB, RefCommodityCode.FLAM });

			AssertGetCommodities
			(
				universalCommodityGroup: RefCommodityCode.CNVT,
				existingCommodityCodes: existingCommodityCodes,
				expectedCommodityCodes: new[] { "CM1" }
			);
		}

		public void TestRefAirlineCommodityCode()
		{
			var airlineCommodityCode = Factory.NewWithValidTestData<RefAirlineCommodityCode>();
			airlineCommodityCode.RAC_Code = "1234";
			airlineCommodityCode.RAC_Description = "Testing1";
			airlineCommodityCode.RAC_AirlineID = "123";

			var airlineCommodityCode2 = Factory.NewWithValidTestData<RefAirlineCommodityCode>();
			airlineCommodityCode2.RAC_Code = "1234";
			airlineCommodityCode2.RAC_Description = "Testing2";
			airlineCommodityCode2.RAC_AirlineID = ZString.Empty;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_IATACommodityItem = "1234";

			AssertType<RefAirlineCommodityCode>(commodity.RefIATACommodityCode);
			AssertEquals(commodity.RefIATACommodityCode.RAC_Code, "1234");
			AssertEquals(commodity.RefIATACommodityCode.RAC_Description, "Testing2");
			AssertEquals(commodity.RefIATACommodityCode.RAC_AirlineID, ZString.Empty);
			AssertNotEquals(commodity.RefIATACommodityCode.RAC_Description, "Testing1");
		}

		void AssertGetCommodities(string universalCommodityGroup, IEnumerable<string> existingCommodityCodes, IEnumerable<string> expectedCommodityCodes, string message = default)
		{
			var commodityCodes = RefCommodityCode.GetCommodities(Factory, universalCommodityGroup)
				.Select(commodity => commodity.RH_Code.ToString())
				.Where(commodity => !existingCommodityCodes.Contains(commodity));

			AssertContainsExactElementsInAnyOrder(message, expectedCommodityCodes, commodityCodes);
		}

		#endregion

		#region Implementation

		BusinessObjectFactory TestFactory;
		RefCommodityCode CommodityCode;
		IEnumerable<string> existingCommodityCodes;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			CommodityCode = TestFactory.New(typeof(RefCommodityCode)) as RefCommodityCode;

			var existingCommodities = Factory.Load<RefCommodityCode>(new ZQuery());
			existingCommodityCodes = existingCommodities.Select(commodity => commodity.RH_Code.ToString());
		}

		RefCommodityCode CreateCommodity(string code, string description)
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = code;
			commodity.RH_Description = description;
			return commodity;
		}

		void CreateCommodities(IEnumerable<string> commodityCodes)
		{
			foreach (var commodityCode in commodityCodes)
			{
				var commodity = Factory.New<RefCommodityCode>();
				commodity.RH_Code = commodityCode;
				commodity.RH_Description = $"{commodityCode} description";

				switch (commodityCode)
				{
					case RefCommodityCode.HAZD:
						commodity.RH_IsHazardous = true;
						break;
					case RefCommodityCode.PERS:
						commodity.RH_IsPerishable = true;
						break;
					case RefCommodityCode.TIMB:
						commodity.RH_IsTimber = true;
						break;
					case RefCommodityCode.FLAM:
						commodity.RH_IsFlammable = true;
						break;
					case RefCommodityCode.CNVT:
						commodity.RH_ContainerVentRequired = true;
						break;
					default:
						commodity.RH_UniversalCommodityGroup = commodityCode;
						break;
				}
			}
		}

		#endregion

	}
}
