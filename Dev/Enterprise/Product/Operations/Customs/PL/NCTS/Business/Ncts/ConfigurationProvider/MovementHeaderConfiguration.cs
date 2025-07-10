using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class MovementHeaderConfiguration : EU.NCTS.Business.MovementHeaderConfiguration
{
	protected override INctsDepartureMovementHeaderPhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsDepartureMovementHeaderPhase5ValidationDecider();
}
