using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(StampDutyCargoDescCollection))]
	public class StampDutyCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<StampDutyCargoDescCollection>
	{
		protected override StampDutyCargoDescCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();

			return nctsHeader.StampDutyCargoDescs;
		}
	}
}
