using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.Builders
{
	sealed class DangerousGoodBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateNECWeight()
		{
			var context = new CommonContext(Factory);

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_UNNO = "A01";
			substance1.DG_Code = "A01";
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IATA;

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance1.PK;
			undg.DI_DGWeight = 1.1m;
			undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			undg.DI_PackageCount = 1;
			undg.DI_NECWeight = 1;
			undg.DI_NECWeightUQ = Core.Constants.Weight.Kilograms;

			var dangerousGood = new DangerousGoodBuilder().Build(undg, context);
			AssertEquals("NEC Weight Value", undg.DI_NECWeight, dangerousGood.NetExplosiveWeight.Value);
			AssertEquals("NEC Weight Unit", Core.Constants.Weight.Kilograms, dangerousGood.NetExplosiveWeight.Unit.Code);
		}
	}
}
