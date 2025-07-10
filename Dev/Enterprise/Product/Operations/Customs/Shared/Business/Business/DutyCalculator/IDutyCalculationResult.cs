using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public interface IDutyCalculationResult
	{
		decimal ResultAmount { get; }

		IEnumerable<IDutyCalculationIntermediateResult> IntermediateResults { get; }

		IEnumerable<ErrorInformation> Errors { get; }
	}
}
