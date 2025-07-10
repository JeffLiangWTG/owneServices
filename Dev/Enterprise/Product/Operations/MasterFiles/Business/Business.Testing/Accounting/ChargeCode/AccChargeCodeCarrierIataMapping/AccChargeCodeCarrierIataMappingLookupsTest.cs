using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeCodeCarrierIataMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestACI_IATAChargeCodeMap_List()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			var svcLvl = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl.PL_Code = "ABC";
			svcLvl.PL_CarrierServiceLevelDescription = "TestLevel";
			Factory.Save();

			var accChargeCodeCarrierIataMapping = Factory.NewWithValidTestData<AccChargeCodeCarrierIataMapping>();
			accChargeCodeCarrierIataMapping.ACI_OH_Carrier = org.PK;

			var lookups = new AccChargeCodeCarrierIataMappingLookups(accChargeCodeCarrierIataMapping);
			AssertContainsExactElementsInAnyOrder(new UntranslatableCodeDescriptionPairList("IATA AWB values cannot be translated", OLookUpEditType.AWBChargeCodes), lookups.ACI_IATAChargeCodeMap_List);
		}
	}
}
