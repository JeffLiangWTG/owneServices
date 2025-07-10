using System.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgSecondaryTypeModuleFilter : ModuleTextFilter
	{
		public OrgSecondaryTypeModuleFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}
	}
}
