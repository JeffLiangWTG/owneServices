using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseDeclarationLevelPackageCollectionNonInheritedTest : TestCaseWithFactory
	{
		public void TestDeletedPackLineTriggersValidationOnDeclaration()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			BaseJobDeclaration declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TotalNoOfPacks = 10;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HOUSEBILL1";
			BasePackage package = declaration.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_HouseBill = houseBill.CU_HouseBill;
			AssertEquals("HasMessageError(JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage)", false, declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
			declaration.Packages.RemoveAndDelete(package);
			AssertEquals("HasMessageError(JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage)", true, declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
		}

		public void TestGetElementWithNoHouseBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			var mockBill = Factory.NewMoq<Bill>();
			mockBill.Object.CU_BillType = BillTypeList.Codes.HouseBill;
			mockBill.Setup(m => m.CU_BillUniqueCode).Returns(new ZString("1"));
			declaration.Bills.Add(mockBill.Object);

			BasePackage package = declaration.Packages.AddNew();
			AssertEquals(package, declaration.Packages.GetElementWithNoHouseBill());

			package.CW_HouseBill = "1";
			AssertNull(declaration.Packages.GetElementWithNoHouseBill());
		}

		public void TestGetElementWithHouseBillAndContainer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			var mockBill = Factory.NewMoq<Bill>();
			mockBill.Object.CU_BillType = BillTypeList.Codes.HouseBill;
			mockBill.Setup(m => m.CU_BillUniqueCode).Returns(new ZString("1"));
			declaration.Bills.Add(mockBill.Object);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1111";

			BasePackage package = declaration.Packages.AddNew();
			AssertNull(declaration.Packages.GetElementWithHouseBillAndContainer("1", "CRUX1111"));

			package.CW_HouseBill = "1";
			AssertNull(declaration.Packages.GetElementWithHouseBillAndContainer("1", "CRUX1111"));

			package.CW_ContainerNoOrEquipmentNo = "CRUX1111";
			AssertEquals(package, declaration.Packages.GetElementWithHouseBillAndContainer("1", "CRUX1111"));
		}
	}
}
