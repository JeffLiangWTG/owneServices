using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Universal.Module
{
	class TariffTypeTextFilter : ModuleTextFilter
	{
		public TariffTypeTextFilter(ZString description, GetTextQueryWithOperator query, System.Collections.IList list) : base(description, query, list)
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new TariffTypeModuleTextFilterValidation(this);
		}
	}

	class TariffTypeModuleTextFilterValidation : ModuleTextFilterValidation
	{
		public TariffTypeModuleTextFilterValidation(TariffTypeTextFilter parent) : base(parent)
		{
		}

		protected override void CheckProperty()
		{
			if (Parent.SqlComparisonOperator == SQLComparisonOperator.Equal || Parent.SqlComparisonOperator == SQLComparisonOperator.NotEqual)
			{
				base.CheckProperty();
			}
		}
	}
}
