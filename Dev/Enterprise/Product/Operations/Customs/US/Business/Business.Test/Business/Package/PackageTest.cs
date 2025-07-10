using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Package))]
	sealed class PackageTest : Customs.Business.Testing.BasePackageTest
	{
		public void TestIInBondPackageLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "2 boxes of toys to 'Toys R You'";
			AssertEquals("Marks and numbers", package.CW_MarksAndNos, ((IInBondContainerMarksAndNumbers)package).MarksAndNumbers);
		}

		public void TestDoNotDeleteEvenIfPackageQtyIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_MasterBill = "1";
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			Factory.Save();
			AssertEquals("package is not deleted", false, package.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CusContainers.AddNew();
			declaration.Bills.AddNew();
			var package = declaration.Packages.AddNew();
			return package;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_MasterBill = "1";
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			return package;
		}
	}
}
