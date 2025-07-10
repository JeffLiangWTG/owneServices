using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class CancelDocketActionMethod<T> : OperationalActionMethod
		where T : WhsDocket
	{
		protected CancelDocketActionMethod(ZGuid guid)
			: base(guid)
		{
		}

		public override string Name
		{
			get { return GetOperationalActionName(); }
		}

		public override string Description
		{
			get { return GetOperationalActionName(); }
		}

		protected abstract string GetOperationalActionName();
	}
}
