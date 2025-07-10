using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestedType(typeof(CusSCAHouseDataContextManager))]
	sealed class CusSCAHouseDataContextManagerTest : ShipmentDataContextManagerTestCase<CusSCAHouseDataContextManager, BaseCusSCAHouse>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CusSCAHouse doesn't have any unique jobnumber", true);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => string.Empty;

		protected override BaseCusSCAHouse GetNewBusinessObjectForTesting()
		{
			var oceanBill = Factory.NewWithValidTestData<TestCusSCAOceanBill>();
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_Code = "AUC";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU")).RL_Code;
			newBranch.GB_Code = "AUB";
			oceanBill.CB_GB = newBranch.PK;

			var houseBill = Factory.NewWithValidTestData<CusSCAHouseForTest>();
			houseBill.CA_CB = oceanBill.PK;
			return houseBill;
		}
	}
}
