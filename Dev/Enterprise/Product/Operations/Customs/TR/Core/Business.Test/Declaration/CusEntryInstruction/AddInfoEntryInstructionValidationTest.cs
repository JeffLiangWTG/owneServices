using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class AddInfoEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_ExportUnionSecretaryCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.ZG_ExportUnionSecretaryCodeInfo);
		}

		public void TestCheckZG_ExportUnionCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.ZG_ExportUnionCodeInfo);
		}

		public void TestCheckZG_ExportUnionCountryCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.ZG_ExportUnionCountryCodeInfo);
		}

		public void TestCheckZG_InlandTransportType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.ZG_InlandTransportTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.FirstOrDefault() ?? (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
	}
}
