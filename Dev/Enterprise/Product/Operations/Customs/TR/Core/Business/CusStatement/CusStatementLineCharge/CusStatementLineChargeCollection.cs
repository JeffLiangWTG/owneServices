using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class CusStatementLineChargeCollection : ActiveBusinessObjectCollection<CusStatementLineCharge>
	{
		public CusStatementLineChargeCollection(CusStatementLine statementLine)
			: base(statementLine)
		{
		}

		public CusStatementLineCharge this[string chargeType] => this.SingleOrDefault(t => t.B4_ChargeType == chargeType);

		public ZDecimal GetChargeAmount(ZString type)
		{
			return this[type]?.B4_ChargeAmount ?? ZDecimal.Zero;
		}
	}
}
