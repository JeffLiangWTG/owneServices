using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IExternalValidationMessages
	{
		ZString ValidationProductWarningMessage { get; set; }
		ZString ValidationPalletIDWarningMessage { get; set; }
		void SetValidationPartAttribWarningMessage(int attributeNumber, string message);
		IReadOnlyList<ZString> ValidationPartAttribWarningMessage { get; }
	}
}
