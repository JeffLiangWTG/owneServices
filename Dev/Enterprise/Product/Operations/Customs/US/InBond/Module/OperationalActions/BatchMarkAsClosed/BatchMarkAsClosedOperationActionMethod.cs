using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions
{
	public class BatchMarkAsClosedOperationActionMethod : USOperationalActionMethod
	{
		public BatchMarkAsClosedOperationActionMethod()
			: base(new ZGuid("CE85E988-AED1-419E-8D35-3984A6825FE8"))
		{
		}
		public override string Name => "Batch Mark As Closed Operational Action";

		public override string Description => "Batch Mark As Closed";

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new BatchMarkAsClosedActionMethodApplicator(factory);
	}
}
