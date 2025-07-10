using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	interface IOrgCusCodeUniqueValidation
	{
		HashSet<string>[] GetCodesCannotCoexist();
	}
}
