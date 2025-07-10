using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public interface IIndexFilterHandler
	{
		string[] RequiredIndexSearchFields { get; }

		bool IsApplicable();

		ModuleFilterCollection Add(ModuleFilterCollection filters);

		IEnumerable<ModuleFilter> GetFilters();
	}
}
