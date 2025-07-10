using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class RefContainerISOTypesFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}

		public string InitialCodeOnSearch { get; private set; }

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override void SetInitialCodeForSearchCore(ZString code, string propertyName)
		{
			base.SetInitialCodeForSearchCore(code, propertyName);
			InitialCodeOnSearch = code;
		}
	}
}
