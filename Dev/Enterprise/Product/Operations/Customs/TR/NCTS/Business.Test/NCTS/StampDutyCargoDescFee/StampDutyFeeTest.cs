using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(StampDutyFee))]
	public class StampDutyFeeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = Factory.New<NctsHeader>();

			result.SetMovementType(NctsMovementType.Codes.Departure);
			var stampDutyCargoDescs = result.StampDutyCargoDescs.AddNew();

			var stampDutyFee = stampDutyCargoDescs.StampDutyFees.AddNew();

			stampDutyFee.BFE_MethodOfCalculation = StampDutyStatusCodeList.Codes.D3;

			return stampDutyFee;
		}
	}
}
