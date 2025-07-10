using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TariffDataGroupingDeclaration : BaseJobDeclaration
	{
		public TariffDataGroupingDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString TariffDataGrouping { get; set; }
		protected override ZString DefaultDataGroupingForTariffsCore => TariffDataGrouping.IsEmpty ? base.DefaultDataGroupingForTariffsCore : TariffDataGrouping;
	}
}
