using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase4LookupsTest : BusinessObjectLookupsTestCase
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
			lookups = new NctsDepartureCargoDescPhase4Lookups(goodItem);
		}
		NctsDepartureCargoDesc goodItem;
		NctsDepartureCargoDescPhase4Lookups lookups;
	}
}
