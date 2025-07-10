using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectForREQDOC))]
	sealed class MessageSendingObjectForREQDOCTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new MessageSendingObjectForREQDOC(entryHeader);
		}

		public void TestLocalReferenceNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = ZDateTime.Now;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_BGMReference = "00505655KFN20160602000001";
			var messageSendingObject = new MessageSendingObjectForREQDOC(entryHeader);
			AssertEquals(entryHeader.CH_BGMReference, messageSendingObject.LocalReferenceNumber);
		}
	}
}
