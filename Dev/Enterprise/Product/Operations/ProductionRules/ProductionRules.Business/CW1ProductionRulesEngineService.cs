using CargoWise.EntityFramework;
using WTG.ProductionRules.Service;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.ProductionRules.Business
{
	public class CW1ProductionRulesEngineService : CW1ProductionRulesEnginePullService
	{
		public CW1ProductionRulesEngineService(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		BusinessObjectFactory Factory { get; }

		protected override IProductionRulesLoader GetRulesLoader() => new CW1ProductionRulesLoader(Factory);
	}
}
