using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementItemCollection))]
	sealed class OrgCommissionAgreementItemCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCommissionAgreementItemCollection>
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();

			AssertEquals(1, agreement.ProductItems.Count);
			var productItem = agreement.ProductItems[0];
			AssertEquals(true, productItem.CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllProductsCode, productItem.CAI_Code);

			AssertEquals(1, productItem.ChildServiceItems.Count);
			var serviceItem = productItem.ChildServiceItems[0];
			AssertEquals(true, serviceItem.CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllServicesCode, serviceItem.CAI_Code);

			AssertEquals(1, serviceItem.ChildSubModuleItems.Count);
			var subModuleItem = serviceItem.ChildSubModuleItems[0];
			AssertEquals(true, subModuleItem.CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllSubModulesCode, subModuleItem.CAI_Code);
		}

		#endregion

	}
}
