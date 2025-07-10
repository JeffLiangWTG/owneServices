using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class InsuranceWrapper : IInsurance
	{
		public InsuranceWrapper(Equipment conveyance)
		{
			this.conveyance = conveyance;
		}

		#region Implementation of IInsurance

		public ZString InsuranceName
		{
			get { return conveyance.BJ_InsuranceName; }
		}

		public ZString InsurancePolicyNumber
		{
			get { return conveyance.BJ_InsurancePolicyNumber; }
		}

		public ZInt InsuranceYearPolicyIssue
		{
			get { return conveyance.BJ_InsuranceYearPolicyIssue; }
		}

		public ZDecimal InsuranceAmount
		{
			get { return conveyance.BJ_InsuranceAmount; }
		}

		#endregion

		readonly Equipment conveyance;
	}
}
