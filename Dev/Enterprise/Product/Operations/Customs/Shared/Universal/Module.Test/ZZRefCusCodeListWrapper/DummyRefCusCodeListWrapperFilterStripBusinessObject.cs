using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Universal.Module.Testing
{
	class DummyRefCusCodeListWrapperFilterStripBusinessObject : ZZRefCusCodeListWrapperFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			return result;
		}

		protected override Dictionary<string, string> AttributeFilters
		{
			get
			{
				return new Dictionary<string, string> { { "Type", "Type" } };
			}
		}
	}
}
