using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection))]
	sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new CusEntryInstructionCollection(testDeclaration);
		}

		public void TestSetDefaultsForNewChild()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var cei = dec.CustomsEntryInstructions.AddNew();
			AssertEquals("EntityCode should default to 'C' after effective date.", EntityTypeList.Codes.CustomsCode, cei.CEI_EntityType);
			AssertEquals(cei.CEI_IsUCROverridden, true);
		}
	}
}
