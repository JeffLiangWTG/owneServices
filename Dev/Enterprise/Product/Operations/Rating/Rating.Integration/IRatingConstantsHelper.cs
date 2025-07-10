using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Rating.Integration
{
	public interface IRatingConstantsHelper
	{
		IEnumerable<ZString> GetRateModes(ZString mode);
	}
}
