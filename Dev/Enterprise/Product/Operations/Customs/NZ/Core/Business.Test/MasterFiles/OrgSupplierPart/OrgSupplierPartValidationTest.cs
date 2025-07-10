using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	public class OrgSupplierPartValidationTest : TestCaseWithFactory
	{
		#region TestParent
		public void TestParent()
		{
			OrgSupplierPart productViaValidation = Product.Validation.Parent;
			AssertEquals("ProductViaValidation = Product", Product, productViaValidation);
		}
		#endregion

		#region Product
		OrgSupplierPart Product
		{
			get
			{
				if (fProduct == null)
				{
					fProduct = Factory.New<OrgSupplierPart>();
				}
				return fProduct;
			}
		}
		OrgSupplierPart fProduct;

		#endregion
	}
}
