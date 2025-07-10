using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ApprovedCommissionAgreementCollection))]
	sealed class ApprovedCommissionAgreementCollectionTest : ActiveBusinessObjectCollectionTestCase<ApprovedCommissionAgreementCollection>
	{
		#region Default Values

		public void TestDefaultValues_IsApproved()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();

			AssertEquals(true, agreement.IsApproved);
			AssertNotEquals(ZDateTime.Empty, agreement.CA0_LastApprovedDateUtc);
		}

		#endregion

		#region Overrides

		protected override ApprovedCommissionAgreementCollection GetCollectionToTest()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			return new ApprovedCommissionAgreementCollection(opportunity);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (OrgCommissionAgreement)base.GetNewElementToAddToTheCollection();
			result.CA0_LastApprovedDateUtc = ZDateTime.Now;
			return result;
		}

		#endregion
	}
}
