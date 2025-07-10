using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefAirlineDefaultCommodityCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRDC_RAR_NKProduct()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "110";
			Factory.Save();

			var productCode1 = Factory.New<RefAirlineProductCode>();
			productCode1.RAR_AirlineID = "110";
			productCode1.RAR_Code = "0001";
			productCode1.RAR_Description = "test1";

			var productCode2 = Factory.New<RefAirlineProductCode>();
			productCode2.RAR_AirlineID = "110";
			productCode2.RAR_Code = "0002";
			productCode2.RAR_Description = "test2";
			Factory.Save();

			var defaultCommodityCode = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode.RDC_RM = airline.PK;
			var list = defaultCommodityCode.Lookups.RDC_RAR_NKProductCode_List;

			AssertEquals(2, list.Count);
			AssertCodeDescription(list[0], "0001", "test1");
			AssertCodeDescription(list[1], "0002", "test2");
		}

		void AssertCodeDescription(ICodeDescription codeDescription, string expectedCode, string expectedDescription)
		{
			AssertEquals("Code", expectedCode, codeDescription.Code);
			AssertEquals("Description", expectedDescription, codeDescription.Description);
		}

		public void TestRDC_RAC_NKCommodity()
		{
			var refAirlineProductCode = Factory.New<RefAirlineProductCode>();
			refAirlineProductCode.RAR_AirlineID = "341";
			refAirlineProductCode.RAR_Code = "CO1";
			refAirlineProductCode.RAR_Description = "Product Code Description 1";
			Factory.Save();

			var refAirlineCommodityCode1 = Factory.New<RefAirlineCommodityCode>();
			refAirlineCommodityCode1.RAC_AirlineID = "341";
			refAirlineCommodityCode1.RAC_Code = "0001";
			refAirlineCommodityCode1.RAC_Description = "Commodity Code Description 1";

			var refAirlineCommodityCode2 = Factory.New<RefAirlineCommodityCode>();
			refAirlineCommodityCode2.RAC_AirlineID = "341";
			refAirlineCommodityCode2.RAC_Code = "0002";
			refAirlineCommodityCode2.RAC_Description = "Commodity Code Description 2";
			Factory.Save();

			var refAirlineProductCodeCommodityCodePivot1 = Factory.New<RefAirlineProductCodeCommodityCodePivot>();
			refAirlineProductCodeCommodityCodePivot1.RPC_AirlineID = "341";
			refAirlineProductCodeCommodityCodePivot1.RPC_RAR = refAirlineProductCode.PK;
			refAirlineProductCodeCommodityCodePivot1.RPC_RAC = refAirlineCommodityCode1.PK;
			Factory.Save();

			var airline1 = Factory.New<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "341";
			Factory.Save();

			var defaultCommodityCode = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode.RDC_RM = airline1.PK;
			var list = defaultCommodityCode.Lookups.RDC_RAC_NKCommodityCode_List;
			Assert(list is RefAirlineCommodityCodeCollection);
			Assert("RDC_RAC_NKCommodityCode_List count", list.Count == 0);

			defaultCommodityCode.RDC_RAR_NKProductCode = refAirlineProductCode.RAR_Code;
			list = defaultCommodityCode.Lookups.RDC_RAC_NKCommodityCode_List;

			Assert("RDC_RAC_NKCommodityCode_List count", list.Count == 1);
			Assert(list.FirstOrDefault(code => code.RAC_Code.Equals("0001")).RAC_Description.Equals("Commodity Code Description 1"));
		}
	}
}
