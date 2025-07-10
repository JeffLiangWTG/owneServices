using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateMultiOrderPickActionMethod : OperationalActionMethod
	{
		public GenerateMultiOrderPickActionMethod()
			: base(new ZGuid("40e718bd-2853-43a5-8977-f23d72e519f1"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			var factoryService = ObjectFactory.New<IFactoryService>();
			factoryService.RegisterFactory(() => new BusinessObjectFactory { NameForDebugging = "Multi-Order Pick Creator Factory" });

			return new GenerateMultiOrderPickActionMethodApplicator(factoryService);
		}

		public override string Name => Res.GetString("928670a6-246a-46f9-aa53-5ce3ad2c0e84", "Generate Multi-Order Pick");

		public override string Description => Res.GetString("2ed69fae-1719-4904-8357-7285eaa8ef89", "Generate Pick For All Selected Orders");
	}
}
