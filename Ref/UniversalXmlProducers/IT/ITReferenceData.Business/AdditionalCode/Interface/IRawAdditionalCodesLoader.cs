using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public interface IRawAdditionalCodesLoader
	{
		IEnumerable<IRawAdditionalCode> GetRawAdditionalCodes();
	}
}
