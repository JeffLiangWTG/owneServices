using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class USWHSPackLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceLineList()
		{
			var whsPackLine = Factory.New<WHSPackLine>();
			AssertNull(whsPackLine.AddInfoLookups.InvoiceLineList);

			var declaration = Factory.New<JobDeclaration>();
			whsPackLine.B7_ParentID = declaration.PK;
			AssertEquals(declaration.PackableInvoiceLines, whsPackLine.AddInfoLookups.InvoiceLineList);
		}

		public void TestWHSPackList()
		{
			var whsPackLine = Factory.New<WHSPackLine>();
			AssertNull(whsPackLine.AddInfoLookups.WHSPackList);

			var declaration = Factory.New<JobDeclaration>();
			whsPackLine.B7_ParentID = declaration.PK;
			AssertEquals(declaration.WHSPacks.UniquePackageReferenceList, whsPackLine.AddInfoLookups.WHSPackList);
		}
	}
}
