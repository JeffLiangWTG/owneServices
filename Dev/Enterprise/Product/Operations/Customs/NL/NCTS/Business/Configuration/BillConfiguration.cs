using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class BillConfiguration : EU.NCTS.Business.BillConfiguration
{
	protected override INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new NctsBillDeparturePhase5ValidationDecider();
}
