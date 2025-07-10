using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CancelOrdersActionMethod : CancelDocketActionMethod<WhsOrder>
	{
		public CancelOrdersActionMethod()
			: base(new ZGuid("791CF078-BF8E-4F8A-9E2B-6A5CE3E85756"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CancelOrdersActionMethodApplicator(factory);
		}

		protected override string GetOperationalActionName()
		{
			return Res.GetString("4D446AF1-DE53-42DD-ABDF-69FDB690525A", "Cancel Orders");
		}
	}
}
