using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AutoAllocateItemsActionMethod : OperationalActionMethod
	{
		public AutoAllocateItemsActionMethod()
			: base(new ZGuid("1420d726-0adf-488b-8c44-64271b41692d"))
		{
		}

		public override string Name => Res.GetString("3eb42f2e-ef85-4550-8f50-36e4cef15e4e", "Auto Allocate Remaining Items");

		public override string Description => Res.GetString("3eb42f2e-ef85-4550-8f50-36e4cef15e4e", "Auto Allocate Remaining Items");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AutoAllocateItemsActionMethodApplicator();
		}
	}
}
