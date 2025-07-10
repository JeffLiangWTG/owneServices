using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCusCodeFilter : ModuleTextFilter
	{
		public OrgCusCodeFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgCusCodeFilterValidation(this);
		}

		#endregion
	}
}
