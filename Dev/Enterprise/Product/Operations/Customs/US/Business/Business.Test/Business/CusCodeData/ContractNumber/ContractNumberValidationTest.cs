using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ContractNumberValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			ContractNumber bizObj = Factory.New<ContractNumber>();
			bizObj.RunPreSaveValidation();
			AssertNoErrors(bizObj.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			ContractNumber contractNumber = Factory.New<ContractNumber>();
			contractNumber.CY_Data = "PENDING";
			contractNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "12-12345-123";
			contractNumber.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "12-12345-1234";
			contractNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "1212345123";
			contractNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "12-1234-123";
			contractNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "123-12345-123";
			contractNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "12-12345-A23";
			contractNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
			contractNumber.CY_Data = "12.12345.1234";
			contractNumber.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(contractNumber.CY_DataInfo, ContractNumberValidation.InvalidContractNumberMessage);
		}
	}
}
