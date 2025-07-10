using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFCallTest : TestCaseWithFactory
	{
		public void TestPlace_and_DateTime()
		{
			var pointInTime = new ZDateTime(2020, 05, 20, 14, 45, 0);
			var call = (ICALINFCallInformation)new CALINFCall("ZAJNB", "139", CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope, pointInTime);
			AssertEquals("ZAJNB", call.CallLocation);
			AssertEquals("139", call.LocationCodeListIdentificationCode);
			AssertEquals(CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope, call.LocationCodeListResponsibleAgencyCode);
			AssertEquals(pointInTime, call.CallDateTime);
		}
	}
}
