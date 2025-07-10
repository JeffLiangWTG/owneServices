using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(AddInfoCusEntryInstruction))]
class AddInfoCusEntryInstructionTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidation()
	{
		AssertType<AddInfoCusEntryInstructionValidation>(new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>()).Validation);

		var dec = Factory.New<JobDeclaration>();
		var entry = dec.CustomsEntryInstructions.AddNew();

		AssertType<ExportAddInfoCusEntryInstructionValidation>(entry.AddInfoValidation);

		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<AddInfoCusEntryInstructionValidation>(entry.AddInfoValidation);
	}

	public void TestLookups() => AssertType<AddInfoCusEntryInstructionLookups>(new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>()).Lookups);

	protected override BusinessObject GetNewBusinessObject()
	{
		return new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
	}
}
