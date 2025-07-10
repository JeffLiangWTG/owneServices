using Enterprise.Integration.ComplianceWise;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceWiseEnabledDetail : IComplianceWiseEnabledDetail
	{
		public bool IsCustomsEnabledComplianceWise
		{
			get
			{
				return ComplianceRiskHelper.IsCustomsEnabledComplianceWise;
			}
		}

		public bool IsComplianceCommodityScreeningEnable
		{
			get
			{
				return ComplianceRiskHelper.IsComplianceCommodityScreeningEnable;
			}
		}
	}
}
