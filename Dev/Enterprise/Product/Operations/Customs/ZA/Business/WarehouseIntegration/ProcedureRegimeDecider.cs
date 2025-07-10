using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public class ProcedureRegimeDecider : Customs.Business.ProcedureRegimeDecider
	{
		public override bool IsOutOfRegime(RefCusProcedure procedure) => procedure.IsOutOfWarehouse();
		public override bool IsIntoRegime(RefCusProcedure procedure) => procedure.IsIntoWarehouse();
	}
}
