using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobChargeTarget))]
	sealed class JobChargeTargetTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeTarget = Factory.New<JobChargeTarget>();
			chargeTarget.JRT_JR = charge.PK;
			chargeTarget.JRT_RelatedJobID = ZGuid.NewZGuid();
			chargeTarget.JRT_RelatedJobTableCode = "JS";

			return chargeTarget;
		}

		#endregion
	}
}
