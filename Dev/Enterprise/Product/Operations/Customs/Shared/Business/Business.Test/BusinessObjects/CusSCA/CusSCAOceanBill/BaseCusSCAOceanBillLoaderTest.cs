using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusSCAOceanBill.Loader))]
	sealed class BaseCusSCAOceanBillLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BaseCusSCAOceanBill.Loader(Factory);
		}

		public void TestLoadFromConsolAndApplicationCode()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var testCusSCAOceanBill1 = Factory.New<TestCusSCAOceanBill>();
			testCusSCAOceanBill1.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testCusSCAOceanBill1.CB_ParentId = consol1.PK;
			testCusSCAOceanBill1.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var testCusSCAOceanBill2 = Factory.New<TestCusSCAOceanBill>();
			testCusSCAOceanBill2.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting;
			testCusSCAOceanBill2.CB_ParentId = consol1.PK;
			testCusSCAOceanBill2.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			var loader = new BaseCusSCAOceanBill.Loader(Factory);
			AssertEquals("Loads correct object", testCusSCAOceanBill1.PK, loader.LoadFromConsolAndApplicationCode(consol1, new ZString[] { "CMR" }).PK);
			AssertEquals("Loads correct object", testCusSCAOceanBill2.PK, loader.LoadFromConsolAndApplicationCode(consol1, new ZString[] { "TST" }).PK);
			AssertNull("Not found", loader.LoadFromConsolAndApplicationCode(consol1, new ZString[] { "CCC" }));
			AssertNull("Not found", loader.LoadFromConsolAndApplicationCode(consol2, new ZString[] { "CMR", "TST", "CCC" }));
			AssertEquals("Loads correct object", testCusSCAOceanBill2.PK, loader.LoadFromConsolAndApplicationCode(consol1, new ZString[] { "CCC", "TST" }).PK);
		}
	}
}
