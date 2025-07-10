using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	abstract class StatementMessageLine : NonPersistentBusinessObject
	{
		protected StatementMessageLine()
		{
		}
		protected IEnumerable<KeyValuePair<ZString, ZDecimal>> fees = new List<KeyValuePair<ZString, ZDecimal>>();

		public void AddFees(IStatementFees feesProvider)
		{
			((List<KeyValuePair<ZString, ZDecimal>>)fees).AddRange(feesProvider.Fees);
		}

		protected ZDecimal GetFee(ZString code)
		{
			return (from fee in fees
					where fee.Key == code
					select fee.Value).FirstOrDefault();
		}
	}
}
