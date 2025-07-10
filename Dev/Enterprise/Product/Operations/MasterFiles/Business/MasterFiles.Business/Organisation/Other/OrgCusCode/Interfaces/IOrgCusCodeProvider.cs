using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	interface IOrgCusCodeProvider
	{
		CodeDescriptionPairList AddOrgCusCodes(CodeDescriptionPairList list);
		HashSet<string> GetPrimaryCusCodes();
		HashSet<string> GetMainOrganizationNumberTypes();
	}
}
