using System.Collections.Generic;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.Business
{
	class PermitWithdrawRequestResponseResult : IPermitWithdrawRequestResponseResult
	{
		public bool Success { get; set; }

		public IEnumerable<IPermitWithdrawRequestResponse> Responses { get; set; }
	}
}
