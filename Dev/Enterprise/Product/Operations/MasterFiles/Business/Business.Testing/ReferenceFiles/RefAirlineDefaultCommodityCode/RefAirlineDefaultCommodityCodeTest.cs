using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirlineDefaultCommodityCode))]
	public class RefAirlineDefaultCommodityCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRDC_RAR_NKProductCode()
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
			defaultCommodityCode.RDC_RAR_NKProductCode = "0001";
			Assert(defaultCommodityCode.RDC_RAR_NKProductCode == "0001");

			defaultCommodityCode.RDC_RAR_NKProductCode = "0002";
			Assert(defaultCommodityCode.RDC_RAR_NKProductCode == "0002");
		}

		public void TestRDC_RAC_NKCommodityCodeAndRAC_Descriptio()
		{
			var commodityCode1 = Factory.New<RefAirlineCommodityCode>();
			commodityCode1.RAC_Code = "0001";
			commodityCode1.RAC_Description = "test1";
			var commodityCode2 = Factory.New<RefAirlineCommodityCode>();
			commodityCode2.RAC_Code = "0002";
			commodityCode2.RAC_Description = "test2";
			Factory.Save();

			var defaultCommodityCode = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode.RDC_RAC_NKCommodityCode = "0001";
			Assert(defaultCommodityCode.RDC_RAC_NKCommodityCode == "0001");
			Assert(defaultCommodityCode.RDC_Description == "test1");

			defaultCommodityCode.RDC_RAC_NKCommodityCode = "0002";
			Assert(defaultCommodityCode.RDC_RAC_NKCommodityCode == "0002");
			Assert(defaultCommodityCode.RDC_Description == "test2");

			defaultCommodityCode.RDC_RAC_NKCommodityCode = "0002";
			Assert(defaultCommodityCode.RDC_RAC_NKCommodityCode == "0002");
			Assert(defaultCommodityCode.RDC_Description == "test2");
		}

		public void TestRDC_RL_NKOrigin()
		{
			var defaultCommodityCode = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode.RDC_RL_NKOrigin = "A";
			Assert(defaultCommodityCode.RDC_RL_NKOrigin.Equals("A"));

			defaultCommodityCode.RDC_RL_NKOrigin = "AEAUH";
			Assert(defaultCommodityCode.RDC_RL_NKOrigin.Equals("AEAUH"));

			defaultCommodityCode.RDC_RL_NKOrigin = "AODRC";
			Assert(defaultCommodityCode.RDC_RL_NKOrigin.Equals("AODRC"));

			defaultCommodityCode.RDC_RL_NKOrigin = "PDD";
			Assert(defaultCommodityCode.RDC_RL_NKOrigin.Equals("PDD"));
		}

		public void TestRDC_RL_NKDestination()
		{
			var defaultCommodityCode = Factory.New<RefAirlineDefaultCommodityCode>();
			defaultCommodityCode.RDC_RL_NKDestination = "A";
			Assert(defaultCommodityCode.RDC_RL_NKDestination.Equals("A"));

			defaultCommodityCode.RDC_RL_NKDestination = "AEAUH";
			Assert(defaultCommodityCode.RDC_RL_NKDestination.Equals("AEAUH"));

			defaultCommodityCode.RDC_RL_NKDestination = "AODRC";
			Assert(defaultCommodityCode.RDC_RL_NKDestination.Equals("AODRC"));

			defaultCommodityCode.RDC_RL_NKDestination = "PDD";
			Assert(defaultCommodityCode.RDC_RL_NKDestination.Equals("PDD"));
		}

		protected override IZType GetValueFromBusinessObject(BusinessObject bizO, string name)
		{
			if (name != RefAirlineDefaultCommodityCodeSchema.Constants.RDC_RL_NKDestination && name != RefAirlineDefaultCommodityCodeSchema.Constants.RDC_RL_NKOrigin)
			{
				return (IZType)bizO[name];
			}
			return null;
		}
	}
}
