using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObject))]
	public class JobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public virtual void TestProperties()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_MessageType = "EX1";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CH_EntryStatus = "CDW";
			entry.CH_MessageType = "EX2";
			entry.CH_BGMReference = "CH_BGMReference";
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);

			Factory.Save();
			var testItem = new JobDeclarationMessageSendingObject(entry);

			AssertEquals("EX2", testItem.MessageType);
			Assert(testItem.MessageTypeInfo.ReadOnly);
			AssertEquals("11", testItem.DeclarationType);
			AssertEquals("CDW", testItem.EntryStatus);
			AssertEquals("CH_BGMReference", testItem.LocalReferenceNumber);
			Assert(testItem.LocalReferenceNumberInfo.ReadOnly);
			AssertEquals("ABC", testItem.MovementReferenceNumber);
			Assert(testItem.MovementReferenceNumberInfo.ReadOnly);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDeclarationMessageSendingObject(Factory.New<CusEntryHeader>());
		}

		#endregion
	}
}
