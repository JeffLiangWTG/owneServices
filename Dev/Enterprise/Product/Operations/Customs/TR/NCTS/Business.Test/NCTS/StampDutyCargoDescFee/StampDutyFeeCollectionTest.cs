using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(StampDutyFeeCollection))]
	public class StampDutyFeeCollectionTest : ActiveBusinessObjectCollectionTestCase<StampDutyFeeCollection>
	{
		protected override StampDutyFeeCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var stampDutyCargoDesc = nctsHeader.StampDutyCargoDescs.AddNew();
			return stampDutyCargoDesc.StampDutyFees;
		}
	}
}
