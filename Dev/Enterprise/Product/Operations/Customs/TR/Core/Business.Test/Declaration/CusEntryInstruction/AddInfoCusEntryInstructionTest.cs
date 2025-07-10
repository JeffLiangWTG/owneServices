using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	class AddInfoCusEntryInstructionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>().CEI_AddInfoInfo);
	}
}
