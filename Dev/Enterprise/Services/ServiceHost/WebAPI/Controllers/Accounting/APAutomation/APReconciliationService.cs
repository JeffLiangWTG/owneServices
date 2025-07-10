using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Services.ServiceHost
{
	public class APReconciliationService : IAPReconciliationService
	{
		public IEnumerable<AccrualsForAPReconciliation<Charge>> GetChargeAccruals(List<JobParentInfo> jobParentsInfo, ZGuid companyPK)
		{
			var source = new InvoicingJobBasedAPReconciliationAccrualSource(jobParentsInfo.Select(ji => (new ZGuid(ji.ParentId), (ZString)ji.ParentTableCode)), companyPK);
			return source.GetChargeAccruals();
		}

		public IEnumerable<AccrualsForAPReconciliation<JobConsolCost>> GetConsolCostAccruals(List<JobParentInfo> jobParentsInfo, ZGuid companyPK)
		{
			var source = new InvoicingJobBasedAPReconciliationAccrualSource(jobParentsInfo.Select(ji => (new ZGuid(ji.ParentId), (ZString)ji.ParentTableCode)), companyPK);
			return source.GetConsolCostAccruals();
		}
	}
}
