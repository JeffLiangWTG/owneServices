using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCarrierVesselPivot))]
	class RefCarrierVesselPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewCarrierVesselPivot(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewCarrierVesselPivot(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		RefCarrierVesselPivot GetNewCarrierVesselPivot(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var zzVessel = helper.CreateVesselZZ("LSC", "A", "CV", "GB");
			var zzCarrier = helper.CreateCarrierCode("DJC", "Blah", "GB");
			return helper.CreateCarrierVesselPivot(zzCarrier.PK, zzVessel.PK);
		}

		public void TestVessel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zzVessel = helper.CreateVesselZZ("LSC", "A", "CV", "GB");
			var zzCarrier = helper.CreateCarrierCode("DJC", "Blah", "GB");
			var pivot = helper.CreateCarrierVesselPivot(zzCarrier.PK, zzVessel.PK);
			Factory.Save();
			AssertEquals(zzVessel, pivot.Vessel);
			AssertType(typeof(RefVesselZZ), pivot.Vessel);
		}
	}
}
