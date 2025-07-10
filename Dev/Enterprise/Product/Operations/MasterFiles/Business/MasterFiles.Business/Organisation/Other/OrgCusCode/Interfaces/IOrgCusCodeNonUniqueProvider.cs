using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	interface IOrgCusCodeNonUniqueProvider
	{
		HashSet<string> GetNonUniqueCodes();
	}
}
