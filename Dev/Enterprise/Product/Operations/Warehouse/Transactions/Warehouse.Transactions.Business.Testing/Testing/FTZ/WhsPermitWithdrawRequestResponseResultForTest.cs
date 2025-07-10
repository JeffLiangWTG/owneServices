using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPermitWithdrawRequestResponseResultForTest : IPermitWithdrawRequestResponseResult
	{
		public IEnumerable<IPermitWithdrawRequestResponse> Responses { get; set; }

		public bool Success => Responses != null && Responses.All(x => x.SuccessOrFailure == SuccessOrFailure.Success);
	}
}
