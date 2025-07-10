using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartCategoryValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckOPC_OPC_Parent

		public void TestCheckOPC_OPC_Parent()
		{
			var parentCategory = (OrgPartCategory)Helper.CreateProductCategory("P-C1", "ParentCategory", ZGuid.Empty);
			var childCategory = (OrgPartCategory)Helper.CreateProductCategory("C-C1", "ChildCategory", parentCategory.PK);
			AssertNoErrors(parentCategory.OPC_OPC_ParentInfo);
			AssertNoErrors(childCategory.OPC_OPC_ParentInfo);

			parentCategory.OPC_OPC_Parent = childCategory.PK;
			AssertHasErrors("Invalid parent category.", parentCategory.OPC_OPC_ParentInfo);
		}

		#endregion

		#region Implementation

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory)); }
		}
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
