using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExportDeclarationTypeList()
		{
			CombineAssertions(() =>
			{
				var exportDeclarationTypeList = lookups.ExportDeclarationTypeList;
				AssertEquals("Codes", "ASY, EXP, WHS", lookups.ExportDeclarationTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<NctsDeclarationTypeList>(), lookups.ExportDeclarationTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodItem = Factory.New<NctsDepartureCargoDesc>();
			lookups = new NctsDepartureCargoDescPhase5Lookups(goodItem);
		}
		NctsDepartureCargoDesc goodItem;
		NctsDepartureCargoDescPhase5Lookups lookups;
	}
}
