using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationLookups))]
	abstract class JobDeclarationLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentPartyList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.B, DutyPaymentTypeList.Descriptions.B),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.C, DutyPaymentTypeList.Descriptions.C),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.D, DutyPaymentTypeList.Descriptions.D),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.E, DutyPaymentTypeList.Descriptions.E),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.G, DutyPaymentTypeList.Descriptions.G),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.J, DutyPaymentTypeList.Descriptions.J),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.L, DutyPaymentTypeList.Descriptions.L),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.M, DutyPaymentTypeList.Descriptions.M),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.N, DutyPaymentTypeList.Descriptions.N),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.P, DutyPaymentTypeList.Descriptions.P),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.R, DutyPaymentTypeList.Descriptions.R),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.Y, DutyPaymentTypeList.Descriptions.Y),
				new CodeDescriptionPair(DutyPaymentTypeList.Codes.Z, DutyPaymentTypeList.Descriptions.Z),
			}, lookups.PaymentPartyList);
		}

		public void TestEntrySubStyleList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(EntrySubStyleList.Codes._1, EntrySubStyleList.Descriptions._1),
				new CodeDescriptionPair(EntrySubStyleList.Codes._2, EntrySubStyleList.Descriptions._2),
				new CodeDescriptionPair(EntrySubStyleList.Codes._3, EntrySubStyleList.Descriptions._3),
				new CodeDescriptionPair(EntrySubStyleList.Codes._4, EntrySubStyleList.Descriptions._4),
				new CodeDescriptionPair(EntrySubStyleList.Codes._5, EntrySubStyleList.Descriptions._5),
				new CodeDescriptionPair(EntrySubStyleList.Codes._8, EntrySubStyleList.Descriptions._8),
				new CodeDescriptionPair(EntrySubStyleList.Codes._10, EntrySubStyleList.Descriptions._10),
				new CodeDescriptionPair(EntrySubStyleList.Codes._11, EntrySubStyleList.Descriptions._11),
				new CodeDescriptionPair(EntrySubStyleList.Codes._12, EntrySubStyleList.Descriptions._12),
				new CodeDescriptionPair(EntrySubStyleList.Codes._16, EntrySubStyleList.Descriptions._16),
				new CodeDescriptionPair(EntrySubStyleList.Codes._17, EntrySubStyleList.Descriptions._17),
				new CodeDescriptionPair(EntrySubStyleList.Codes._18, EntrySubStyleList.Descriptions._18),
				new CodeDescriptionPair(EntrySubStyleList.Codes._19, EntrySubStyleList.Descriptions._19),
				new CodeDescriptionPair(EntrySubStyleList.Codes._20, EntrySubStyleList.Descriptions._20),
				new CodeDescriptionPair(EntrySubStyleList.Codes._22, EntrySubStyleList.Descriptions._22),
				new CodeDescriptionPair(EntrySubStyleList.Codes._34, EntrySubStyleList.Descriptions._34),
				new CodeDescriptionPair(EntrySubStyleList.Codes._35, EntrySubStyleList.Descriptions._35),
				new CodeDescriptionPair(EntrySubStyleList.Codes._36, EntrySubStyleList.Descriptions._36),
				new CodeDescriptionPair(EntrySubStyleList.Codes._111, EntrySubStyleList.Descriptions._111),
				new CodeDescriptionPair(EntrySubStyleList.Codes._112, EntrySubStyleList.Descriptions._112),
				new CodeDescriptionPair(EntrySubStyleList.Codes._113, EntrySubStyleList.Descriptions._113),
			}, lookups.EntrySubStyleList);
		}

		public void TestTRCustomsOfficeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Currency");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, Core.Constants.CurrencyCodes.EuropeanUnion, yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE003478", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR008734", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR009278", "TURKISH OFFICE", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities);
			Factory.Save();

			var customsOffices = lookups.TRCustomsOfficeList;
			customsOffices.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "TR008734", "TR009278" }, customsOffices.Select(x => x.ZZD_Code));
		}

		public void TestCustomsOffices()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Currency");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, Core.Constants.CurrencyCodes.EuropeanUnion, yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE003478", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR008734", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR009278", "TURKISH OFFICE", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities);
			Factory.Save();

			var customsOffices = lookups.CustomsOffices;
			customsOffices.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "TR008734", "TR009278" }, customsOffices.Select(x => x.ZZD_Code));
		}

		public void TestTRTransportModeInlandList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(TRTransportModeInland.Codes._10, TRTransportModeInland.Descriptions._10),
				new CodeDescriptionPair(TRTransportModeInland.Codes._12, TRTransportModeInland.Descriptions._12),
				new CodeDescriptionPair(TRTransportModeInland.Codes._16, TRTransportModeInland.Descriptions._16),
				new CodeDescriptionPair(TRTransportModeInland.Codes._17, TRTransportModeInland.Descriptions._17),
				new CodeDescriptionPair(TRTransportModeInland.Codes._18, TRTransportModeInland.Descriptions._18),
				new CodeDescriptionPair(TRTransportModeInland.Codes._20, TRTransportModeInland.Descriptions._20),
				new CodeDescriptionPair(TRTransportModeInland.Codes._23, TRTransportModeInland.Descriptions._23),
				new CodeDescriptionPair(TRTransportModeInland.Codes._30, TRTransportModeInland.Descriptions._30),
				new CodeDescriptionPair(TRTransportModeInland.Codes._40, TRTransportModeInland.Descriptions._40),
				new CodeDescriptionPair(TRTransportModeInland.Codes._50, TRTransportModeInland.Descriptions._50),
				new CodeDescriptionPair(TRTransportModeInland.Codes._70, TRTransportModeInland.Descriptions._70),
				new CodeDescriptionPair(TRTransportModeInland.Codes._80, TRTransportModeInland.Descriptions._80),
				new CodeDescriptionPair(TRTransportModeInland.Codes._90, TRTransportModeInland.Descriptions._90),
			}, lookups.TransportModeInland);
		}

		public void TestTRPortlist()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port List", "TR");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Fake List", "ZA");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Fake List", "TR");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "CDE", "CDE Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "XYZ", "XYZ Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "ZZZ", "ZZZ Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XXX", "Fake Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AAA", "Fake Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var trPortLists = lookups.TRCustomsPortList;
			AssertEquals("TR PortList Count", 4, trPortLists.Count);
			AssertEquals("TR PortList CodesAsString", "ABC, CDE, XYZ, ZZZ", trPortLists.CodesAsString);
		}

		public void TestJE_LocationOfGoods()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "TR Warehouses");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "Warehouse1", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "Warehouse2", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "Warehouse3", yesterday, tomorrow);

			Factory.Save();

			var locationList = (CodeDescriptionPairList)lookups.Locations;

			AssertContainsExactElementsInAnyOrder(new[] { "Warehouse1", "Warehouse2", "Warehouse3" }, locationList.GetAllCodes());
		}

		public void TestBondedWarehouseCodeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "Bonded Warehouse Codes List", "TR");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Fake List", "ZA");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Fake List", "TR");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "CDE", "CDE Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "XYZ", "XYZ Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ZZZ", "ZZZ Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XXX", "Fake Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AAA", "Fake Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var bwCodeList = lookups.BondedWarehouseCodeList;
			bwCodeList.Load();
			AssertEquals("BondedWarehouseCodeList Count", 4, bwCodeList.Count);
			Assert(bwCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "ABC"));
			Assert(bwCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "CDE"));
			Assert(bwCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "XYZ"));
			Assert(bwCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "ZZZ"));
		}

		public void TestJE_ExportGoodsTypeList()
		{
			var list = lookups.OrderTypesOfGoodsList;

			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "1, 2, 3, 9", list.CodesAsString);
				AssertSame(Factory.GetCachedValue<OrderTypesOfGoodsList>(), list);
			});
		}

		public void TestCargoIdTypeList()
		{
			var list = jobDeclaration.Lookups.CargoIdTypeList;
			AssertEquals(1, list.Count);
			AssertCargoIdType(list[0], Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
		}

		public void TestTransportMeansList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(TransportMeans.Codes._1, TransportMeans.Descriptions._1),
				new CodeDescriptionPair(TransportMeans.Codes._2, TransportMeans.Descriptions._2),
				new CodeDescriptionPair(TransportMeans.Codes._3, TransportMeans.Descriptions._3),
				new CodeDescriptionPair(TransportMeans.Codes._4, TransportMeans.Descriptions._4),
				new CodeDescriptionPair(TransportMeans.Codes._5, TransportMeans.Descriptions._5),
				new CodeDescriptionPair(TransportMeans.Codes._6, TransportMeans.Descriptions._6),
			}, lookups.TransportMeansList);
		}

		void AssertCargoIdType(ICodeDescription codeDescription, string code, string description)
		{
			AssertEquals("Code", code, codeDescription.Code);
			AssertEquals("Description", description, codeDescription.Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			lookups = GetLookups();
		}
		protected JobDeclaration jobDeclaration;
		protected JobDeclarationLookups lookups;

		protected abstract string MessageType { get; }
		protected abstract JobDeclarationLookups GetLookups();
	}
}
