using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CancelReceivesActionMethod : CancelDocketActionMethod<WhsReceive>
	{
		public CancelReceivesActionMethod()
			: base(new ZGuid("791CF078-BF8E-4F8A-9E2B-6A5CE3E85756"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CancelReceivesActionMethodApplicator(factory);
		}

		protected override string GetOperationalActionName()
		{
			return Res.GetString("6506A586-EDC5-483F-95F9-071B6384B2AB", "Cancel Receives");
		}
	}
}
