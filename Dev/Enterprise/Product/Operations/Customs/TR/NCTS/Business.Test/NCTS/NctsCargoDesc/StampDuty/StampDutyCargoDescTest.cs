using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(StampDutyCargoDesc))]
	public class StampDutyCargoDescTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetStampDutyFeesChild()
		{
			var stampDutyCargoDescHeader = Factory.New<StampDutyCargoDesc>();
			AssertType<StampDutyFeeCollection>(stampDutyCargoDescHeader.StampDutyFees);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = Factory.New<NctsHeader>();

			result.SetMovementType(NctsMovementType.Codes.Departure);
			var stampDutyCargoDescs = result.StampDutyCargoDescs.AddNew();

			stampDutyCargoDescs.StampDutyStatus = StampDutyStatusCodeList.Codes.D3;

			return stampDutyCargoDescs;
		}
	}
}
