using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CancelPickActionMethod : OperationalActionMethod
	{
		public CancelPickActionMethod()
			: base(new ZGuid("e842e4a6-bd4b-4a79-ab7b-7715357aee10"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			var factoryService = ObjectFactory.New<IFactoryService>();
			factoryService.RegisterFactory(() => new BusinessObjectFactory { NameForDebugging = "Cancel Picks Creator Factory" });

			return new CancelPickApplicator(factoryService);
		}

		public override string Name => Res.GetString("bd9b3c34-e8e8-4bbc-85ba-39d084e20045", "Cancel Picks");

		public override string Description => Res.GetString("c3e5f76a-1450-4fae-a7ca-a5f2919b6c21", "Cancel Picks");
	}
}
