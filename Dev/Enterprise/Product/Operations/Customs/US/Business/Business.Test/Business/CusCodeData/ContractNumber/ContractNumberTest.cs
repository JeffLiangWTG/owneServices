using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ContractNumber))]
	sealed class ContractNumberTest : Customs.Business.Testing.CusCodeDataTest<ContractNumber>
	{
		public void TestSetDefaultValues()
		{
			ContractNumber contractNumber = (ContractNumber)GetNewBusinessObject();
			AssertEquals("default value", contractNumber.CY_Type, CusCodeDataTypeList.Codes.ContractNumber);
			AssertEquals("default value", contractNumber.CY_Code, CusCodeDataTypeList.Codes.ContractNumber);
		}

		public void TestGetsCorrectValidation()
		{
			ContractNumber contractNumber = (ContractNumber)GetNewBusinessObject();
			AssertEquals("Correct validation", typeof(ContractNumberValidation), contractNumber.Validation.GetType());
		}

		public void TestIDrawbackContractNumber()
		{
			ContractNumber contractNumber = (ContractNumber)GetNewBusinessObject();
			contractNumber.CY_Data = "CM123456";
			AssertEquals("ContractNumberCode", "CM123456", contractNumber.ContractNumberCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.ContractNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<ContractNumber>();

		protected override IEnumerable<ContractNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<ContractNumber>();
			var declaration = factory.New<JobDeclaration>();
			declaration.ContractNumbers.Add(result);
			yield return result;
		}
	}
}
