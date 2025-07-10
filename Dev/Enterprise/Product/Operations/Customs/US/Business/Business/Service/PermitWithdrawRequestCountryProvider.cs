using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.US.Business.Service
{
	class PermitWithdrawRequestCountryProvider : IPermitWithdrawRequestCountryProvider
	{
		IPermitWithdrawRequestProvider IPermitWithdrawRequestCountryProvider.FindProviderFor(PermitType permitType)
		{
			IPermitWithdrawRequestProvider result = null;
			if (permitType == PermitType.FTZ)
			{
				result = new FTZPermitWithdrawRequestProvider();
			}
			return result;
		}
	}
}
