using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Services.ServiceHost
{
	public interface IAPReconciliationService
	{
		IEnumerable<AccrualsForAPReconciliation<Charge>> GetChargeAccruals(List<JobParentInfo> jobParentsInfo, ZGuid companyPK);

		IEnumerable<AccrualsForAPReconciliation<JobConsolCost>> GetConsolCostAccruals(List<JobParentInfo> jobParentsInfo, ZGuid companyPK);
	}
}
