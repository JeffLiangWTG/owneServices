using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAccCommissionLineGroup
	{
		ZGuid CLG_AC { get; }
		ZGuid CLG_CH0 { get; }
		ZString CLG_RX_NKCommissionCurrency { get; }
		ZString CLG_RX_NKTransactionCurrency { get; }
		ZDecimal CLG_TransactionAmount { get; }
		ZDecimal CLG_TotalCommissionableAmount { get; }
		IEnumerable<IAccCommissionLine> Lines { get; }
	}
}
