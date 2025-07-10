using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection<CusEntryInstruction>))]
	public class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryInstructionCollection<CusEntryInstruction>(Factory.New<BaseJobDeclaration>());
	}
}
