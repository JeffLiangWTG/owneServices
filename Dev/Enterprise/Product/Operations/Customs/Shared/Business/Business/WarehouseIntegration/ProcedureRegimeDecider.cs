using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class ProcedureRegimeDecider
	{
		public virtual bool IsOutOfRegime(RefCusProcedure procedure) => procedure.IsOutOfRegime();
		public virtual bool IsIntoRegime(RefCusProcedure procedure) => procedure.IsIntoRegime();
	}
}
