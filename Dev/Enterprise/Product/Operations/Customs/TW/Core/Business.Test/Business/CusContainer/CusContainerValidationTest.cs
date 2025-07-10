using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusContainerValidationTest : Customs.Business.Testing.CusContainerValidationTest<JobDeclaration>
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			CusContainer parent = Factory.New<CusContainer>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Validation.Parent));
		}

		public void TestCO_FCL_LCL_AIR()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "AA";
			AssertHasMessageErrorContaining(container.CO_FCL_LCL_AIRInfo, ListValidation.InvalidCodeMessageError);
			container.CO_FCL_LCL_AIR = "EMP";
			AssertNoMessageErrorContaining(container.CO_FCL_LCL_AIRInfo, ListValidation.InvalidCodeMessageError);
			container.CO_FCL_LCL_AIR = "";
			AssertHasMessageErrorContaining(container.CO_FCL_LCL_AIRInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCO_Seal()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_Seal = "123456789012345678";
			AssertHasMessageError(container.CO_SealInfo, "The length of Seal Number shouldn't be more than 17.");
			container.CO_Seal = "12345678901234567";
			AssertNoMessageErrors(container.CO_SealInfo);
			container.CO_FCL_LCL_AIR = "";
			AssertNoMessageErrors(container.CO_SealInfo);
		}

		[ExpectNoExceptions]
		public override void TestContainersRequirePackagesValidation()
		{
			var testDecl = (JobDeclaration)GetJobDeclaration();
			testDecl.JE_TransportMode = testDecl.TransportModeSeaCodeForTesting;
			testDecl.DisableDefaultPackingInformation = true;
			var testHouseBill = testDecl.Bills.AddNew();
			testHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			testHouseBill.CU_HouseBill = "TestHouseBill";
			var testContainer = testDecl.CusContainers.AddNew();
			testContainer.CO_ContainerNumber = "CRXU1234569";
			NUnit.Framework.Assert.That(testDecl.IsContainerPackingRequired, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(testContainer.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages), NUnit.Framework.Is.EqualTo(false), "HasMessageError(CusContainerValidation.ContainersRequirePackages)");
		}
	}
}
