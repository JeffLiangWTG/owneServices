using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ITransactionLinesValidation
	{
		ResourceString ValidateInvoiceLines(IEnumerable<AccTransactionLines> accTransactionLines);

		string[] GetErrorMessages();
	}
}
