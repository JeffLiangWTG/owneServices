using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Universal.GUI
{
	internal class ModuleTariffFilter : ModuleTextFilter
	{
		public ModuleTariffFilter(ZString description, SchemaStringColumn filterColumn, Common.ITariffFormatter tariffFormatter)
			: base(description, filterColumn)
		{
			this.TariffFormatter = tariffFormatter;
		}

		public Common.ITariffFormatter TariffFormatter { get; set; }

		public override ZString Property { get => base.Property; set => base.Property = TariffFormatter?.Format(value) ?? value; }
	}
}
