using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class FinalizeDocketActionMethod<T> : OperationalActionMethod
		where T : WhsDocket
	{
		public FinalizeDocketActionMethod(ZGuid guid)
			: base(guid)
		{
		}

		public sealed override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return NewApplicatorCore(factory);
		}

		protected abstract FinalizeDocketsActionMethodApplicator<T> NewApplicatorCore(BusinessObjectFactory factory);

		public override string Name => GetOperationalActionName();

		public override string Description => GetOperationalActionName();

		protected abstract string GetOperationalActionName();
	}
}
