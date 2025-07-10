using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsDepartureMovementHeaderPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBM_CustomsOfficeAtBorderList()
		{
			var goodsShiptoCodeList = departureMovement.Lookups.GoodsShiptoCodeList;

			CombineAssertions("Making sure codes match and list is cached", () =>
			{
				AssertEquals("Codes", "ANTREPO, GUMRUK, IYGIA", goodsShiptoCodeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<GoodsShiptoCodeList>(), goodsShiptoCodeList);
			});
		}

		public void TestCustomsStatusList()
		{
			AssertSame(Factory.GetCachedValue<NCTSMovementHeaderCustomsStatusList>(), departureMovement.Lookups.CustomsStatusList);
		}

		public void TestSpecificCircumstanceIndicatorList()
		{
			AssertSame(Factory.GetCachedValue<SpesificCircumstanceIndicatorList>(), departureMovement.Lookups.SpecificCircumstanceIndicatorList);
		}

		public void TestDeclarationTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ncts = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "NCTS Declaration Types");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "ABC", "ABC Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var cde = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "CDE", "CDE Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var xyz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "XYZ", "XYZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var zzz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "ZZZ", "ZZZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var xxx = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Albania, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "XXX", "XXX Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var declarationTypeList = departureMovement.Lookups.DeclarationTypeList;
			CombineAssertions("For The Existing Codes Except 'XXX'", () =>
			{
				Assert(declarationTypeList.ContainsCode("ABC"));
				Assert(declarationTypeList.ContainsCode("CDE"));
				Assert(declarationTypeList.ContainsCode("XYZ"));
				Assert(declarationTypeList.ContainsCode("ZZZ"));

				Assert(!declarationTypeList.ContainsCode("XXX"));
			});
		}

		public void TestWeightUnitList()
		{
			AssertSame(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), departureMovement.Lookups.WeightUnitList);
		}

		public void TestTRWarehouseList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRCWH", "TRCWH");

			helper.CreateCusCodeList("TR", "TRCWH", "A0002", "EKOL ULUSŞLARARASI TİC. A.Ş.", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "TRCWH", "A0004", "İNT.İNTERNAS NAK.TURİZM A.Ş.", yesterday, tomorrow);

			Factory.Save();

			var list = departureMovement.Lookups.TRWarehouseList;
			list.Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0002"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0004"));
		}

		public void TestTankerStatusList()
		{
			AssertEquals(new CodeDescriptionPairList(), departureMovement.Lookups.TankerStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = header.MovementHeader;
		}
		NctsDepartureMovementHeader departureMovement;
	}
}
