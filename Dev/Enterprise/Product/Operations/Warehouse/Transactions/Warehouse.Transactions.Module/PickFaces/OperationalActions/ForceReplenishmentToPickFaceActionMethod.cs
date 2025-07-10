using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ForceReplenishmentToPickFaceActionMethod : OperationalActionMethod
	{
		public ForceReplenishmentToPickFaceActionMethod() : base(new ZGuid("ae056c5a-6c3f-4ce6-be0a-23ecb29a4baf"))
		{
		}

		public override string Name => Res.GetString("f342f13a-22e9-42e7-8244-97f0d74d9c07", "Force Replenishment for Selected Pick Faces.");

		public override string Description => Name;

		public override bool IsRunAgainDisabled => true;

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ForceReplenishmentToPickFaceActionMethodApplicator(factory);
		}
	}
}
