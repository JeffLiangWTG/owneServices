using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	class ModuleTariffFilter : ModuleTextFilter
	{
		public ModuleTariffFilter()
			: base(Constants.RefCusTariffFilters.TariffCode, GetTariffCodeQuery)
		{
			FilterColumn = TariffViewSchema.ZZ1_TariffCode;
		}

		static ZQuery GetTariffCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(TariffViewSchema.ZZ1_TariffCode, comparisonOperator, value.ExcludeChars("."));
		}

		public override ZString Property { get => base.Property; set => base.Property = value.ExcludeChars("."); }
	}
}
