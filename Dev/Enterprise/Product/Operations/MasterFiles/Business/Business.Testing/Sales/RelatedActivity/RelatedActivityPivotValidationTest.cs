using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatedActivityPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckToActivityTypeForBinding()
		{
			var communication = Factory.New<OrgSalesCall>();
			var pivot = communication.RelatedChildActivityPivotCollection.AddNew();

			var link = RelatedActivityLink.Get(pivot, communication);
			link.ToActivityTypeForBinding = RelatableActivityTypeList.Codes.CrmOpportunityManager;

			link.Validation.ValidateAll();
			AssertEquals("Can not create COP type.", expected: true, link.ToActivityTypeForBindingInfo.HasError("Enter a valid Activity Type."));

			link.HasChanges = false;
			link.Validation.ValidateAll();
			AssertEquals("Existing COP links should have no error.", expected: false, link.ToActivityTypeForBindingInfo.HasError("Enter a valid Activity Type."));
		}
	}
}
