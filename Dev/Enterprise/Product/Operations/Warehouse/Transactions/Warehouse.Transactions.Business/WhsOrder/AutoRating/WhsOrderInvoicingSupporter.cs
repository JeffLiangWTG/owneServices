using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderInvoicingSupporter : WhsPickableDocketInvoicingSupporter
	{
		public WhsOrderInvoicingSupporter(WhsOrder parent)
			: base(parent)
		{
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.WhsOrderJobInvoicing;
		}

		public override ZDecimal ActualChargeable { get { return Parent.WD_TotalWeight; } }

		public override ZString ActualChargeableUnit { get { return Parent.WD_TotalWeightUnit; } }
	}
}
