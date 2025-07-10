using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestsSubclassesOf(typeof(CusEntryInstructionLookups))]
abstract class CusEntryInstructionLookupsAbstractTest : BusinessObjectLookupsTestCase
{
	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		instruction = declaration.CustomsEntryInstructions.AddNew();
		lookups = instruction.Lookups;
	}

	protected JobDeclaration declaration;
	protected CusEntryInstruction instruction;
	protected CusEntryInstructionLookups lookups;

	protected abstract string MessageType { get; }
}
