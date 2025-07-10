#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.DocRollupOrSort
{
	public partial class BaseDocRollupOrSortLoader<T>
		where T : BusinessObject
	{
		internal T BaseLoadForTestOnly(ZString serviceDirection, ZString transportMode, ZString containerMode, params ZString[] jobTypesInOrderOfPreference)
			=> Load(serviceDirection, transportMode, containerMode, jobTypesInOrderOfPreference);
	}
}

#endif
